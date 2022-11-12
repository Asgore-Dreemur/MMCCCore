using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MMCCCore.Model.Authenticator
{
    public class AuthErrorModel
    {
        [JsonProperty("error")]
        public string Error { get; set; }
        [JsonProperty("error_description")]
        public string ErrorDescription { get; set; }
        [JsonProperty("correlation_id")]
        public string CorrelationId { get; set; }
    }
    public class AuthTokenResponseModel :AuthErrorModel
    {
        [JsonProperty("token_type")]
        public string TokenType { get; set; }
        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }
        [JsonProperty("scope")]
        public string Scope { get; set; }
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }
        [JsonProperty("user_id")]
        public string Uuid { get; set; }
    }
    public class XBLAuthRequestModel
    {
        [JsonProperty("Properties")]
        public XBLAuthRequestProperitesModel Properites { get; set; } = new XBLAuthRequestProperitesModel();
        [JsonProperty("RelyingParty")]
        public string RelyingParty { get; set; } = "http://auth.xboxlive.com";
        [JsonProperty("TokenType")]
        public string TokenType { get; set; } = "JWT";
    }
    public class XBLAuthRequestProperitesModel
    {
        [JsonProperty("AuthMethod")]
        public string AuthMethod { get; set; } = "RPS";

        [JsonProperty("SiteName")]
        public string SiteName { get; set; } = "user.auth.xboxlive.com";

        [JsonProperty("RpsTicket")]
        public string RpsTicket { get; set; } = "d=<access token>";
    }
    public class XBLAuthResponseModel : AuthErrorModel
    {
        [JsonProperty("IssueInstant")]
        public string IssueInstant { get; set; }

        [JsonProperty("NotAfter")]
        public string NotAfter { get; set; }

        [JsonProperty("Token")]
        public string Token { get; set; }

        [JsonProperty("DisplayClaims")]
        public DisplayClaimsModel DisplayClaims { get; set; }
    }
    public class DisplayClaimsModel
    {
        [JsonProperty("xui")]
        public List<JObject> Xui { get; set; }
    }
    public class XSTSAuthRequestModel
    {
        [JsonProperty("Properties")]
        public XSTSAuthenticatePropertiesModels Properties { get; set; } = new XSTSAuthenticatePropertiesModels();

        [JsonProperty("RelyingParty")]
        public string RelyingParty { get; set; } = "rp://api.minecraftservices.com/";

        [JsonProperty("TokenType")]
        public string TokenType { get; set; } = "JWT";
    }

    public class XSTSAuthResponseModel : XSTSAuthenticateErrorModel
    {
        [JsonProperty("IssueInstant")]
        public string IssueInstant { get; set; }

        [JsonProperty("NotAfter")]
        public string NotAfter { get; set; }

        [JsonProperty("Token")]
        public string Token { get; set; }

        [JsonProperty("DisplayClaims")]
        public DisplayClaimsModel DisplayClaims { get; set; }
    }

    public class XSTSAuthenticateErrorModel
    {
        [JsonProperty("Identity")]
        public string Identity { get; set; }

        [JsonProperty("XErr")]
        public string XErr { get; set; }

        [JsonProperty("Message")]
        public string Message { get; set; }

        [JsonProperty("Redirect")]
        public string Redirect { get; set; }
    }

    public class XSTSAuthenticatePropertiesModels
    {
        [JsonProperty("SandboxId")]
        public string SandboxId { get; set; } = "RETAIL";

        [JsonProperty("UserTokens")]
        public List<string> UserTokens { get; set; } = new List<string>();
    }
    public class MicrosoftAuthResponse : AuthErrorModel
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("skins")]
        public List<PlayerSkinModel> Skins { get; set; }

        [JsonProperty("capes")]
        public JArray Capes { get; set; }
    }
    public class AccountGamesModel : AuthErrorModel
    {
        [JsonProperty("items")]
        public List<AccountItemModel> Items { get; set; } = new List<AccountItemModel>();
        [JsonProperty("signature")]
        public string Signature { get; set; }
        [JsonProperty("keyId")]
        public string KeyId { get; set; }
    }
    public class AccountItemModel
    {
        [JsonProperty("signature")]
        public string Signature { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class PlayerSkinModel
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("variant")]
        public string Variant { get; set; }

        [JsonProperty("alias")]
        public string Alias { get; set; }
    }
}
