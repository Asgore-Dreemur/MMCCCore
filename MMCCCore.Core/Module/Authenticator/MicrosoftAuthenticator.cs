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
using MMCCCore.Core.Model.Authenticator;
using MMCCCore.Core.Wrapper;
using MMCCCore.Core.Module.Minecraft;
using System.Runtime.Versioning;
using System.Net.Http;

namespace MMCCCore.Core.Module.Authenticator
{
    public class MicrosoftAuthenticator : InstallerModel
    {
        private string ClientId { get; set; }

        private HttpWrapper HttpWrapper = new HttpWrapper();
        private string RedirectUri { get; set; }
        public OAuth2TokenResult OAuth2Response { get; set; }

        public MicrosoftAuthenticator(string ClientId)
        {
            this.ClientId = ClientId;
        }

        public MicrosoftAccount Authenticate(bool isRefresh) { return AuthenticateTaskAsync(isRefresh).GetAwaiter().GetResult(); }

        public async Task<MicrosoftAccount> AuthenticateTaskAsync(bool isRefresh)
        {
            try
            {
                if (OAuth2Response == null) throw new Exception("请先进行设备代码流验证");
                OAuth2TokenResult token = new OAuth2TokenResult();
                if (isRefresh)
                {
                    Dictionary<string, string> RequestDict = new Dictionary<string, string>
                    {
                        {"client_id", ClientId },
                        {"refresh_token",  OAuth2Response.RefreshToken},
                        {"grant_type", "refresh_token" }
                    };
                    var authCodePostRes = await HttpWrapper.HttpPostAsync($"https://login.live.com/oauth20_token.srf", RequestDict);
                    token = JsonConvert.DeserializeObject<OAuth2TokenResult>(await authCodePostRes.Content.ReadAsStringAsync());
                    if (token == null) throw new Exception("无法获取token,没有详细的信息,我们只有一个HTTP状态码:" + authCodePostRes.StatusCode.ToString());
                    else if (token.Error != null) throw new Exception(token.ErrorDescription);
                }
                else token = OAuth2Response;
                OnProgressChanged(0.4, "XBL验证中");
                var xblauth = await XBLAuthTaskAsync(token.AccessToken);
                if (xblauth == null || xblauth.Error != null) throw new Exception(xblauth.ErrorDescription);
                OnProgressChanged(0.6, "XSTS验证中");
                var xstsauth = await XSTSAuthTaskAsync(xblauth.Token);
                if (xstsauth == null || xstsauth.XErr != null) throw new Exception(xstsauth.Message);
                OnProgressChanged(0.8, "登录Xbox...");
                var xboxlogin = await LoginMinecraftWithXbox(xstsauth);
                if (xboxlogin == null) throw new Exception(xblauth.ErrorDescription);
                OnProgressChanged(0.9, "验证购买...");
                bool isHaveMC = await isTheAccountHasMinecraft(xboxlogin.AccessToken);
                if (!isHaveMC) throw new Exception("无法验证您的微软账号是否存在MC,您或许并没有购买MC,或者是一些内部错误");
                var profile = await GetPlayerProfile(xboxlogin.AccessToken);
                if (profile.Error != null) throw new Exception(profile.ErrorDescription);
                return new MicrosoftAccount
                {
                    Name = profile.Name,
                    Uuid = Guid.Parse(profile.Id),
                    AccessToken = xboxlogin.AccessToken,
                    RefreshToken = token.RefreshToken,
                    ClientToken = Guid.NewGuid().ToString("N"),
                    ErrorMessage = null,
                    LoginType = AccountType.Microsoft,
                    Skins = profile.Skins
                };
            } catch (Exception e)
            {
                return new MicrosoftAccount { ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message };
            }
        }


        private async Task<XBLAuthResponseModel> XBLAuthTaskAsync(string AccessToken)
        {
            XBLAuthRequestModel model = new XBLAuthRequestModel()
            {
                Properites = new XBLAuthRequestProperitesModel
                {
                    AuthMethod = "RPS",
                    SiteName = "user.auth.xboxlive.com",
                    RpsTicket = $"d={AccessToken}"
                },
                RelyingParty = "http://auth.xboxlive.com",
                TokenType = "JWT"
            };
            var result = HttpWrapper.HttpPostAsync("https://user.auth.xboxlive.com/user/authenticate",
                content: JsonConvert.SerializeObject(model, Formatting.Indented)).GetAwaiter().GetResult();
            var str = result.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            return JsonConvert.DeserializeObject<XBLAuthResponseModel>(result.Content.ReadAsStringAsync().GetAwaiter().GetResult());
        }

        private async Task<XSTSAuthResponseModel> XSTSAuthTaskAsync(string XBLToken)
        {
            XSTSAuthRequestModel model = new XSTSAuthRequestModel()
            {
                Properties = new XSTSAuthenticatePropertiesModels
                {
                    SandboxId = "RETAIL",
                    UserTokens = new List<string>() { XBLToken }
                },
                RelyingParty = "rp://api.minecraftservices.com/",
                TokenType = "JWT"
            };
            var result = HttpWrapper.HttpPostAsync("https://xsts.auth.xboxlive.com/xsts/authorize",
                content: JsonConvert.SerializeObject(model)).GetAwaiter().GetResult();
            return JsonConvert.DeserializeObject<XSTSAuthResponseModel>(result.Content.ReadAsStringAsync().GetAwaiter().GetResult());
        }

        private async Task<XboxLoginResponse> LoginMinecraftWithXbox(XSTSAuthResponseModel model)
        {
            JObject keyValuePairs = new JObject();
            keyValuePairs["identityToken"] = $"XBL3.0 x={model.DisplayClaims.Xui[0]["uhs"]};{model.Token}";
            var result = HttpWrapper.HttpPostAsync("https://api.minecraftservices.com/authentication/login_with_xbox",
                content: JsonConvert.SerializeObject(keyValuePairs)).GetAwaiter().GetResult();
            return JsonConvert.DeserializeObject<XboxLoginResponse>(result.Content.ReadAsStringAsync().GetAwaiter().GetResult());
        }

        private async Task<bool> isTheAccountHasMinecraft(string AuthToken)
        {
            var result = HttpWrapper.HttpGetAsync("https://api.minecraftservices.com/entitlements/mcstore",
                AuthTuple: new Tuple<string, string>("Bearer", AuthToken)).GetAwaiter().GetResult();
            var info = JsonConvert.DeserializeObject<AccountGamesModel>(result.Content.ReadAsStringAsync().GetAwaiter().GetResult());
            if (info.Error != null) return false;
            else
            {
                var game = info.Items.Find(i => i.Name == "game_minecraft");
                if (game == null) return false;
                else return true;
            }
        }

        private async Task<MicrosoftAuthResponse> GetPlayerProfile(string AuthToken)
        {
            var result = HttpWrapper.HttpGetAsync("https://api.minecraftservices.com/minecraft/profile",
                AuthTuple: new Tuple<string, string>("Bearer", AuthToken)).GetAwaiter().GetResult();
            var info = JsonConvert.DeserializeObject<MicrosoftAuthResponse>(result.Content.ReadAsStringAsync().GetAwaiter().GetResult());
            return info;
        }

        public async Task<OAuth2Result> OAuth2AuthenticateTaskAsync()
        {
            Dictionary<string, string> RequestDict = new Dictionary<string, string>()
            {
                {"client_id", ClientId },
                {"scope", "XboxLive.signin offline_access"}
            };
            var result = HttpWrapper.HttpPostAsync("https://login.microsoftonline.com/consumers/oauth2/v2.0/devicecode",
                RequestDict).GetAwaiter().GetResult();
            return JsonConvert.DeserializeObject<OAuth2Result>(result.Content.ReadAsStringAsync().GetAwaiter().GetResult());
        }

        public async Task<OAuth2TokenResult> OAuth2TokenAuthenticateTaskAsync(OAuth2Result result)
        {
            var watch = Stopwatch.StartNew();
            while (watch.Elapsed < TimeSpan.FromSeconds(result.ExpiresIn))
            {
                await Task.Delay(result.Interval * 1000);
                Dictionary<string, string> RequestDict = new Dictionary<string, string>()
                {
                    {"grant_type", "urn:ietf:params:oauth:grant-type:device_code" },
                    {"client_id", ClientId },
                    {"device_code", result.DeviceCode }
                };
                var res = HttpWrapper.HttpPostAsync("https://login.microsoftonline.com/consumers/oauth2/v2.0/token",
                    RequestDict).GetAwaiter().GetResult();
                var postres = JsonConvert.DeserializeObject<OAuth2TokenResult>(res.Content.ReadAsStringAsync().GetAwaiter().GetResult());
                if (res.IsSuccessStatusCode)
                {
                    return postres;
                }
                else
                {
                    if (!string.IsNullOrEmpty(postres.Error))
                    {
                        if (postres.Error != "authorization_pending") return null;
                    }
                }
            }
            return null;
        }
    }
}
