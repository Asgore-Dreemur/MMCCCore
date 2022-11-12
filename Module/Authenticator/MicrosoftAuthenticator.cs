using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.IO;
using System.Threading;
using System.Diagnostics;
using MMCCCore.Model.Authenticator;
using MMCCCore.Wrapper;
using MMCCCore.Module.Minecraft;
using System.Runtime.Versioning;

namespace MMCCCore.Module.Authenticator
{
    public class MicrosoftAuthenticator : InstallerModel
    {
        private string ClientId = "00000000402b5328";
        private string CallBackUrl = "https://login.live.com/oauth20_desktop.srf/";
        private string ClientSecret;
        private HttpWrapper HttpWrapper = new HttpWrapper();
        private string RefreshToken;

        public MicrosoftAuthenticator() { }

        public MicrosoftAuthenticator(string ClientId, string CallBackUrl, string ClientSecret)
        {
            this.ClientId = ClientId;
            this.CallBackUrl = CallBackUrl;
            this.ClientSecret = ClientSecret;
        }

        public string GetAccessCodeWindows()
        {
            string url = "https://login.live.com/oauth20_authorize.srf" +
                $"?client_id={ClientId}" +
                "&response_type=code" +
                $"&redirect_uri={CallBackUrl}" +
                "&scope=XboxLive.signin%20offline_access";
            var listener = new HttpListener();
            listener.Prefixes.Add($"{CallBackUrl.TrimEnd('/')}/");
            listener.Start();
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    CreateNoWindow = true,
                    RedirectStandardInput = true,
                    UseShellExecute = false,
                    FileName = "cmd"
                },
            };
            process.Start();
            process.StandardInput.WriteLine($"start \"\" \"{url}\"");
            process.StandardInput.WriteLine("exit");
            process.WaitForExit();
            var res = listener.GetContext();
            string code =res.Request.Url.ToString().Split('=')[1];
            if (code.Contains("error"))
            {
                string ErrorMessage = "Error!Please Retry.";
                new StreamWriter(res.Response.OutputStream).WriteLine(ErrorMessage);
                res.Response.OutputStream.Flush();
                Task.Delay(1000).Wait();
                return null;
            }
            else
            {
                string CompleteMessage = "Complete!Please Back to see your Launcher";
                new StreamWriter(res.Response.OutputStream).WriteLine(CompleteMessage);
                res.Response.OutputStream.Flush();
                Task.Delay(1000).Wait();
                return code;
            }
        }

        public Account Authenticate(string code) { return AuthenticateTaskAsync(code).GetAwaiter().GetResult(); }

        public async Task<Account> AuthenticateTaskAsync(string code)
        {
            try
            {
                Dictionary<string, string> AuthDict = new Dictionary<string, string>
                {
                    {"client_id", this.ClientId },
                    {"code", code },
                    {"grant_type", "authorization_code" },
                    {"redirect_uri", CallBackUrl },
                };
                if (!string.IsNullOrWhiteSpace(ClientSecret)) AuthDict.Add("client_secret", ClientSecret);
                OnProgressChanged(0.2, "正在进行Token验证...");
                var AuthCodeRes = JsonConvert.DeserializeObject<AuthTokenResponseModel>(await (await HttpWrapper.HttpPostAsync("https://login.live.com/oauth20_token.srf", AuthDict)).Content.ReadAsStringAsync());
                if (AuthCodeRes.Error != null) throw new Exception($"Token验证时出现问题:{AuthCodeRes.ErrorDescription}({AuthCodeRes.ErrorDescription})");
                RefreshToken = AuthCodeRes.RefreshToken;
                XBLAuthRequestModel XblAuthRequest = new XBLAuthRequestModel();
                XblAuthRequest.Properites.RpsTicket = XblAuthRequest.Properites.RpsTicket.Replace("<access token>", AuthCodeRes.AccessToken);
                OnProgressChanged(0.4, "正在进行XBL验证...");
                var XBLAuthRes = JsonConvert.DeserializeObject<XBLAuthResponseModel>(await (await HttpWrapper.HttpPostAsync("https://user.auth.xboxlive.com/user/authenticate", content:JsonConvert.SerializeObject(XblAuthRequest))).Content.ReadAsStringAsync());
                if (XBLAuthRes.Error != null) throw new Exception($"XBL验证时出现问题:{XBLAuthRes.ErrorDescription}({XBLAuthRes.ErrorDescription})");
                XSTSAuthRequestModel XSTSAuthRequest = new XSTSAuthRequestModel();
                XSTSAuthRequest.Properties.UserTokens.Add(XBLAuthRes.Token);
                OnProgressChanged(0.8, "正在进行XSTS验证...");
                XSTSAuthResponseModel XSTSAuthResponse = JsonConvert.DeserializeObject<XSTSAuthResponseModel>(await (await HttpWrapper.HttpPostAsync("https://xsts.auth.xboxlive.com/xsts/authorize", content:JsonConvert.SerializeObject(XSTSAuthRequest))).Content.ReadAsStringAsync());
                if (XSTSAuthResponse.XErr != null) throw new Exception($"XSTS验证时出现问题:{XSTSAuthResponse.XErr}({XSTSAuthResponse.Message})");
                string AuthMinecraftPost = $"{{\"identityToken\":\"XBL3.0 x={XBLAuthRes.DisplayClaims.Xui[0]["uhs"]};{XSTSAuthResponse.Token}\"}}";
                string access_token = JObject.Parse(await (await HttpWrapper.HttpPostAsync("https://api.minecraftservices.com/authentication/login_with_xbox", content:AuthMinecraftPost)).Content.ReadAsStringAsync())["access_token"].ToString();
                var AuthTuple = new Tuple<string, string>("Bearer", access_token);
                OnProgressChanged(0.9, "正在验证该账号是否有Minecraft");
                var GameAuth = JsonConvert.DeserializeObject<AccountGamesModel>(await (await HttpWrapper.HttpGetAsync("https://api.minecraftservices.com/entitlements/mcstore", AuthTuple: AuthTuple)).Content.ReadAsStringAsync());
                if (GameAuth.Error != null) throw new Exception($"Minecraft API调用时出现问题:{GameAuth.ErrorDescription}({GameAuth.Error})");
                if (GameAuth.Items.Find(i => i.Name == "game_minecraft") == default) throw new Exception("此微软账号没有Minecaft");
                OnProgressChanged(0.95, "获取账号信息...");
                var profileRes = await HttpWrapper.HttpGetAsync("https://api.minecraftservices.com/minecraft/profile", AuthTuple: AuthTuple);
                var MicrosoftAuthRes = JsonConvert.DeserializeObject<MicrosoftAuthResponse>(await profileRes.Content.ReadAsStringAsync());
                if (MicrosoftAuthRes.Error != null) throw new Exception($"获取账号信息时出现问题:{MicrosoftAuthRes.ErrorDescription}({MicrosoftAuthRes.Error})");
                return new Account
                {
                    AccessToken = access_token,
                    LoginType = AccountType.Microsoft,
                    ClientToken = Guid.NewGuid().ToString("N"),
                    Name = MicrosoftAuthRes.Name,
                    Uuid = Guid.Parse(MicrosoftAuthRes.Id)
                };
            }catch(Exception e)
            {
                return new Account {ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message };
            }
        }
        public async Task<Account> RefreshAccountAsync()
        {
            if (RefreshToken == null) return null;
            try
            {
                Dictionary<string, string> AuthDict = new Dictionary<string, string>
                {
                    {"client_id", this.ClientId },
                    {"refresh_token", RefreshToken },
                    {"grant_type", "refresh_token" },
                    {"redirect_uri", CallBackUrl },
                };
                if (!string.IsNullOrWhiteSpace(ClientSecret)) AuthDict.Add("client_secret", ClientSecret);
                OnProgressChanged(0.2, "进行Token刷新...");
                var AuthCodeRes = JsonConvert.DeserializeObject<AuthTokenResponseModel>(await (await HttpWrapper.HttpPostAsync("https://login.live.com/oauth20_token.srf", AuthDict)).Content.ReadAsStringAsync());
                if (AuthCodeRes.Error != null) throw new Exception($"Token验证时出现问题:{AuthCodeRes.ErrorDescription}({AuthCodeRes.ErrorDescription})");
                XBLAuthRequestModel XblAuthRequest = new XBLAuthRequestModel();
                XblAuthRequest.Properites.RpsTicket = XblAuthRequest.Properites.RpsTicket.Replace("<access token>", AuthCodeRes.AccessToken);
                OnProgressChanged(0.4, "进行XBL认证...");
                var XBLAuthRes = JsonConvert.DeserializeObject<XBLAuthResponseModel>(await(await HttpWrapper.HttpPostAsync("https://user.auth.xboxlive.com/user/authenticate", content:JsonConvert.SerializeObject(XblAuthRequest))).Content.ReadAsStringAsync());
                if (XBLAuthRes.Error != null) throw new Exception($"XBL验证时出现问题:{XBLAuthRes.ErrorDescription}({XBLAuthRes.ErrorDescription})");
                XSTSAuthRequestModel XSTSAuthRequest = new XSTSAuthRequestModel();
                XSTSAuthRequest.Properties.UserTokens.Add(XBLAuthRes.Token);
                OnProgressChanged(0.6, "进行XSTS认证...");
                XSTSAuthResponseModel XSTSAuthResponse = JsonConvert.DeserializeObject<XSTSAuthResponseModel>(await(await HttpWrapper.HttpPostAsync("https://xsts.auth.xboxlive.com/xsts/authorize", content:JsonConvert.SerializeObject(XSTSAuthRequest))).Content.ReadAsStringAsync());
                if (XSTSAuthResponse.XErr != null) throw new Exception($"XSTS验证时出现问题:{XSTSAuthResponse.Message}({XSTSAuthResponse.XErr})");
                string AuthMinecraftPost = $"{{\"identityToken\":\"XBL3.0 x={XBLAuthRes.DisplayClaims.Xui[0]["uhs"]};{XSTSAuthResponse.Token}\"}}";
                string access_token = JObject.Parse(await(await HttpWrapper.HttpPostAsync("https://api.minecraftservices.com/authentication/login_with_xbox", content:AuthMinecraftPost)).Content.ReadAsStringAsync())["access_token"].ToString();
                var AuthTuple = new Tuple<string, string>("Bearer", access_token);
                OnProgressChanged(0.8, "验证该账号是否有Minecraft...");
                var GameAuth = JsonConvert.DeserializeObject<AccountGamesModel>(await (await HttpWrapper.HttpGetAsync("https://api.minecraftservices.com/entitlements/mcstore", AuthTuple: AuthTuple)).Content.ReadAsStringAsync());
                if (GameAuth.Error != null) throw new Exception($"Minecraft API调用时出现问题:{GameAuth.ErrorDescription}({GameAuth.Error})");
                if (GameAuth.Items.Find(i => i.Name == "game_minecraft") == default) throw new Exception("此微软账号没有Minecaft");
                OnProgressChanged(0.9, "获取账号信息...");
                var profileRes = await HttpWrapper.HttpGetAsync("https://api.minecraftservices.com/minecraft/profile", AuthTuple: AuthTuple);
                var MicrosoftAuthRes = JsonConvert.DeserializeObject<MicrosoftAuthResponse>(await profileRes.Content.ReadAsStringAsync());
                if (AuthCodeRes.Error != null) throw new Exception($"获取账号信息时出现问题:{AuthCodeRes.ErrorDescription}({AuthCodeRes.ErrorDescription})");
                return new Account
                {
                    AccessToken = access_token,
                    LoginType = AccountType.Microsoft,
                    ClientToken = Guid.NewGuid().ToString("N"),
                    Name = MicrosoftAuthRes.Name,
                    Uuid = Guid.Parse(MicrosoftAuthRes.Id)
                };
            }
            catch (Exception e)
            {
                return new Account { ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message };
            }
        }
    }
}
