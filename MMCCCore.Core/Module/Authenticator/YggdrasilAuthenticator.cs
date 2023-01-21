using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MMCCCore.Core.Model.Authenticator;
using MMCCCore.Core.Wrapper;
using System.Security.Cryptography;

namespace MMCCCore.Core.Module.Authenticator
{
    public class YggdrasilAuthenticator
    {
        private string YggdrasilServerAddr;
        private string Username;
        private string Password;
        private HttpWrapper HttpWrapper = new HttpWrapper();
        public YggdrasilAuthenticator(string YggdrasilServerAddr, string Username, string Password)
        {
            this.YggdrasilServerAddr = YggdrasilServerAddr;
            this.Username = Username;
            this.Password = Password;
        }

        public YggdrasilAccount Authenticate() => AuthenticateAsync().GetAwaiter().GetResult();

        public async Task<YggdrasilAccount> AuthenticateAsync()
        {
            try
            {
                string ClientToken = Guid.NewGuid().ToString("N");
                string AuthAddr = YggdrasilServerAddr + (YggdrasilServerAddr.Last().Equals('/') ? "authserver/authenticate" : "/authserver/authenticate");
                string AuthResult = await (await HttpWrapper.HttpPostAsync(AuthAddr + $"?clientToken={ClientToken}&username={Username}&password={Password}")).Content.ReadAsStringAsync();
                YggdrasilAuthResponseModel ResponseRes = JsonConvert.DeserializeObject<YggdrasilAuthResponseModel>(AuthResult);
                if (ResponseRes.ErrorMessage != null) throw new Exception("登录失败,请检查你的用户名和密码,错误:" + ResponseRes.ErrorMessage);
                return new YggdrasilAccount
                {
                    AccessToken = ResponseRes.AccessToken,
                    ClientToken = ResponseRes.ClientToken,
                    Uuid = ResponseRes.SelectedProfile == null ? Guid.Parse(ResponseRes.AvailableProfiles[0].Id) : Guid.Parse(ResponseRes.SelectedProfile.Id),
                    Name = ResponseRes.SelectedProfile == null ? ResponseRes.AvailableProfiles[0].Name : ResponseRes.SelectedProfile.Name,
                    LoginType = AccountType.Yggdrasil,
                    AvailableProfiles = ResponseRes.AvailableProfiles,
                    SelectedProfile = ResponseRes.SelectedProfile,
                    ServerAddr = YggdrasilServerAddr
                };
            }catch(Exception e)
            {
                return new YggdrasilAccount { ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message };
            }
        }
        public async Task<YggdrasilAccount> RefreshAccountAsync(string AccessToken)
        {
            try
            {
                string ClientToken = Guid.NewGuid().ToString("N");
                string AuthAddr = YggdrasilServerAddr + (YggdrasilServerAddr.Last().Equals('/') ? "authserver/refresh" : "/authserver/refresh");
                string AuthResult = await (await HttpWrapper.HttpPostAsync(AuthAddr + $"?accessToken={AccessToken}")).Content.ReadAsStringAsync();
                YggdrasilAuthResponseModel ResponseRes = JsonConvert.DeserializeObject<YggdrasilAuthResponseModel>(AuthResult);
                if (ResponseRes.ErrorMessage != null) throw new Exception("刷新失败,错误:" + ResponseRes.ErrorMessage);
                return new YggdrasilAccount
                {
                    AccessToken = ResponseRes.AccessToken,
                    ClientToken = ResponseRes.ClientToken,
                    Uuid = ResponseRes.SelectedProfile == null ? Guid.Parse(ResponseRes.AvailableProfiles[0].Id) : Guid.Parse(ResponseRes.SelectedProfile.Id),
                    Name = ResponseRes.SelectedProfile == null ? ResponseRes.AvailableProfiles[0].Name : ResponseRes.SelectedProfile.Name,
                    AvailableProfiles = ResponseRes.AvailableProfiles,
                    SelectedProfile = ResponseRes.SelectedProfile,
                    ServerAddr = YggdrasilServerAddr
                };
            }
            catch (Exception e)
            {
                return new YggdrasilAccount { ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message };
            }
        }
        public async Task<SignoutResponse> SignoutAsync()
        {
            try
            {
                string RequestAddr = YggdrasilServerAddr.TrimEnd('/') + "/authserver/signout";
                string RequestBody = JsonConvert.SerializeObject(new SignoutRequest { Username = Username, Password = Password });
                var result = await HttpWrapper.HttpPostAsync(RequestAddr, content: RequestBody);
                if (result.StatusCode == System.Net.HttpStatusCode.NoContent) return new SignoutResponse { isSuccess = true };
                else throw new Exception(JObject.Parse(await result.Content.ReadAsStringAsync())["errorMessage"].ToString());
            }catch(Exception e)
            {
                return new SignoutResponse { ErrorMessage = e.Message, isSuccess = false };
            }
        }
        public static async Task<YggdrasilResultModel> GetPlayerSkin(string Uuid, string ServerAddr)
        {
            try
            {
                string RequestAddr = ServerAddr.TrimEnd('/') + $"/sessionserver/session/minecraft/profile/{Uuid}";
                var result = await new HttpWrapper().HttpPostAsync(RequestAddr);
                string ResultStr = await result.Content.ReadAsStringAsync();
                if (ResultStr.Contains("error")) throw new Exception(JsonConvert.DeserializeObject<YggdrasilErrorModel>(ResultStr).ErrorMessage);
                YggdrasilProfileInfo info = JsonConvert.DeserializeObject<YggdrasilProfileInfo>(ResultStr);
                foreach(var item in info.Properties)
                {
                    if(item.Name == "textures")
                    {
                        YggdrasilProfileTexturesModel model = JsonConvert.DeserializeObject<YggdrasilProfileTexturesModel>(OtherTools.Base64Decode(item.Value));
                        if (model.Textures.Skin == null) return null;
                        else return new YggdrasilResultModel { isSuccess = true, OtherInfo = model.Textures.Skin.Url };
                    }
                }
                return null;
            }
            catch(Exception e)
            {
                return new YggdrasilResultModel { isSuccess = false, ErrorException = e };
            }
        }

        public static async Task<List<string>> GenerateYggdrasilLaunchArgs(string AuthlibPath, string ServerAddr)
        {
            List<string> args = new List<string>();
            string base64sec = Convert.ToBase64String(await (await new HttpWrapper().HttpGetAsync(ServerAddr)).Content.ReadAsByteArrayAsync());
            args.Add($"-javaagent:{AuthlibPath}={ServerAddr}");
            args.Add($"-Dauthlibinjector.yggdrasil.prefetched={base64sec}");
            return args;
        }

        public static async Task<string> GetAuthlibDownloadUrl()
        {
            return JObject.Parse(await (await (new HttpWrapper().HttpGetAsync("https://bmclapi2.bangbang93.com/mirrors/authlib-injector/artifact/latest.json"))).Content.ReadAsStringAsync())["download_url"].ToString();
        }
    }
}
