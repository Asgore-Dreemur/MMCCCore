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

namespace MMCCCore.Module.Authenticator
{
    public class MicrosoftAuthenticator : InstallerModel
    {
        private string ClientId = "15635053-8ffa-429d-b922-746236c6c199";
        private string CallbackUrl = "http://localhost:36285/mmcc/microsoft/auth/";
        private HttpWrapper HttpWrapper = new HttpWrapper();
        private string RefreshToken;
        public string GetAccessCode()
        {
            string url = "https://login.live.com/oauth20_authorize.srf" +
                $"?client_id={ClientId}" +
                "&response_type=code" +
                $"&redirect_uri={CallbackUrl}" +
                "&scope=XboxLive.signin%20offline_access";
            var listener = new HttpListener();
            listener.Prefixes.Add(CallbackUrl);
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
                res.Response.OutputStream.Write(Encoding.Default.GetBytes(ErrorMessage), 0, Encoding.Default.GetBytes(ErrorMessage).Length);
                res.Response.OutputStream.Flush();
                Task.Delay(1000).Wait();
                return null;
            }
            else
            {
                string CompleteMessage = "Complete!Please Back to see your Launcher";
                res.Response.OutputStream.Write(Encoding.Default.GetBytes(CompleteMessage), 0, Encoding.Default.GetBytes(CompleteMessage).Length);
                res.Response.OutputStream.Flush();
                Task.Delay(1000).Wait();
                return code;
            }
        }
        public async Task<Account> AuthenticateAsync(string code)
        {
            try
            {
                Dictionary<string, string> AuthDict = new Dictionary<string, string>
                {
                    {"client_id", this.ClientId },
                    {"code", code },
                    {"grant_type", "authorization_code" },
                    {"redirect_uri", CallbackUrl },
                    {"client_secret", "oo88Q~hA6.lHsiE4Ah9Mxjs_VftyQl~8MRcSTb.1" }
                };
                OnProgressChanged(0.2, "正在进行Token验证...");
                var AuthCodeRes = JsonConvert.DeserializeObject<AuthTokenResponseModel>(await (await HttpWrapper.HttpPostAsync("https://login.live.com/oauth20_token.srf", AuthDict)).Content.ReadAsStringAsync());
                RefreshToken = AuthCodeRes.RefreshToken;
                XBLAuthRequestModel XblAuthRequest = new XBLAuthRequestModel();
                XblAuthRequest.Properites.RpsTicket = XblAuthRequest.Properites.RpsTicket.Replace("<access token>", AuthCodeRes.AccessToken);
                OnProgressChanged(0.4, "正在进行XBL验证...");
                string tmp = await (await HttpWrapper.HttpPostAsync("https://user.auth.xboxlive.com/user/authenticate", content:JsonConvert.SerializeObject(XblAuthRequest))).Content.ReadAsStringAsync();
                var XBLAuthRes = JsonConvert.DeserializeObject<XBLAuthResponseModel>(await (await HttpWrapper.HttpPostAsync("https://user.auth.xboxlive.com/user/authenticate", content:JsonConvert.SerializeObject(XblAuthRequest))).Content.ReadAsStringAsync());
                XSTSAuthRequestModel XSTSAuthRequest = new XSTSAuthRequestModel();
                XSTSAuthRequest.Properties.UserTokens.Add(XBLAuthRes.Token);
                OnProgressChanged(0.8, "正在进行XSTS验证...");
                XSTSAuthResponseModel XSTSAuthResponse = JsonConvert.DeserializeObject<XSTSAuthResponseModel>(await (await HttpWrapper.HttpPostAsync("https://xsts.auth.xboxlive.com/xsts/authorize", content:JsonConvert.SerializeObject(XSTSAuthRequest))).Content.ReadAsStringAsync());
                string AuthMinecraftPost = $"{{\"identityToken\":\"XBL3.0 x={XBLAuthRes.DisplayClaims.Xui[0]["uhs"]};{XSTSAuthResponse.Token}\"}}";
                string access_token = JObject.Parse(await (await HttpWrapper.HttpPostAsync("https://api.minecraftservices.com/authentication/login_with_xbox", content:AuthMinecraftPost)).Content.ReadAsStringAsync())["access_token"].ToString();
                var AuthTuple = new Tuple<string, string>("Bearer", access_token);
                OnProgressChanged(0.9, "正在验证该账号是否有Minecraft");
                var GameAuth = JsonConvert.DeserializeObject<AccountGamesModel>(await (await HttpWrapper.HttpGetAsync("https://api.minecraftservices.com/entitlements/mcstore", AuthTuple: AuthTuple)).Content.ReadAsStringAsync());
                List<string> ProductsInfo = new List<string>();
                foreach (AccountItemModel ItemInfo in GameAuth.Items) ProductsInfo.Add(ItemInfo.Name);
                if (!ProductsInfo.Contains("game_minecraft")) throw new Exception("此微软账号没有Minecaft");
                OnProgressChanged(0.95, "获取账号信息...");
                var profileRes = await HttpWrapper.HttpGetAsync("https://api.minecraftservices.com/minecraft/profile", AuthTuple: AuthTuple);
                var MicrosoftAuthRes = JsonConvert.DeserializeObject<MicrosoftAuthResponse>(await profileRes.Content.ReadAsStringAsync());
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
                    {"redirect_uri", CallbackUrl },
                    {"client_secret", "oo88Q~hA6.lHsiE4Ah9Mxjs_VftyQl~8MRcSTb.1" }
                };
                OnProgressChanged(0.2, "进行Token刷新...");
                var AuthCodeRes = JsonConvert.DeserializeObject<AuthTokenResponseModel>(await (await HttpWrapper.HttpPostAsync("https://login.live.com/oauth20_token.srf", AuthDict)).Content.ReadAsStringAsync());
                XBLAuthRequestModel XblAuthRequest = new XBLAuthRequestModel();
                XblAuthRequest.Properites.RpsTicket = XblAuthRequest.Properites.RpsTicket.Replace("<access token>", AuthCodeRes.AccessToken);
                OnProgressChanged(0.4, "进行XBL认证...");
                var XBLAuthRes = JsonConvert.DeserializeObject<XBLAuthResponseModel>(await(await HttpWrapper.HttpPostAsync("https://user.auth.xboxlive.com/user/authenticate", content:JsonConvert.SerializeObject(XblAuthRequest))).Content.ReadAsStringAsync());
                XSTSAuthRequestModel XSTSAuthRequest = new XSTSAuthRequestModel();
                XSTSAuthRequest.Properties.UserTokens.Add(XBLAuthRes.Token);
                OnProgressChanged(0.6, "进行XSTS认证...");
                XSTSAuthResponseModel XSTSAuthResponse = JsonConvert.DeserializeObject<XSTSAuthResponseModel>(await(await HttpWrapper.HttpPostAsync("https://xsts.auth.xboxlive.com/xsts/authorize", content:JsonConvert.SerializeObject(XSTSAuthRequest))).Content.ReadAsStringAsync());
                string AuthMinecraftPost = $"{{\"identityToken\":\"XBL3.0 x={XBLAuthRes.DisplayClaims.Xui[0]["uhs"]};{XSTSAuthResponse.Token}\"}}";
                string access_token = JObject.Parse(await(await HttpWrapper.HttpPostAsync("https://api.minecraftservices.com/authentication/login_with_xbox", content:AuthMinecraftPost)).Content.ReadAsStringAsync())["access_token"].ToString();
                var AuthTuple = new Tuple<string, string>("Bearer", access_token);
                OnProgressChanged(0.8, "验证该账号是否有Minecraft...");
                var GameAuth = JObject.Parse(await(await HttpWrapper.HttpGetAsync("https://api.minecraftservices.com/entitlements/mcstore", AuthTuple: AuthTuple)).Content.ReadAsStringAsync());
                List<string> ProductsInfo = new List<string>();
                foreach (JObject ItemInfo in GameAuth["items"]) ProductsInfo.Add(ItemInfo["name"].ToString());
                if (!ProductsInfo.Contains("game_minecraft")) throw new Exception("此微软账号没有Minecaft");
                OnProgressChanged(0.9, "获取账号信息...");
                var profileRes = await HttpWrapper.HttpGetAsync("https://api.minecraftservices.com/minecraft/profile", AuthTuple: AuthTuple);
                var MicrosoftAuthRes = JsonConvert.DeserializeObject<MicrosoftAuthResponse>(await profileRes.Content.ReadAsStringAsync());
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
