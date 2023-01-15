using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MMCCCore.Core.Model.Authenticator
{
    public class YggdrasilAuthResponseModel
    {
        [JsonProperty("availableProfiles")]
        public List<YggdrasilProfileModel> AvailableProfiles { get; set; } = new List<YggdrasilProfileModel>();
        [JsonProperty("selectedProfile")]
        public YggdrasilProfileModel SelectedProfile { get; set; }
        [JsonProperty("accessToken")]
        public string AccessToken { get; set; }
        [JsonProperty("clientToken")]
        public string ClientToken { get; set; }
        [JsonProperty("error")]
        public string ErrorMessage { get; set; }
    }
    public class YggdrasilProfileModel
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
    }
    public class SignoutResponse
    {
        public bool isSuccess { get; set; }
        public string ErrorMessage { get; set; }
    }
    public class SignoutRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
    public class YggdrasilErrorModel
    {
        [JsonProperty("error")]
        public string Error { get; set; }
        [JsonProperty("errorMessage")]
        public string ErrorMessage { get; set; }
    }
    public class YggdrasilProfileInfo
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("properties")]
        public List<YggdrasilProfilePropModel> Properties { get; set; } = new List<YggdrasilProfilePropModel>();
    }
    public class YggdrasilProfilePropModel
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("value")]
        public string Value { get; set; }
    }
    public class YggdrasilProfileTexturesModel
    {
        [JsonProperty("timestamp")]
        public long Timestamp { get; set; }
        [JsonProperty("profileId")]
        public string ProfileId { get; set; }
        [JsonProperty("profileName")]
        public string ProfileName { get; set; }
        [JsonProperty("isPublic")]
        public bool IsPublic { get; set; }
        [JsonProperty("textures")]
        public YggdrasilProfileSkinInfo Textures { get; set; }
    }
    public class YggdrasilProfileSkinInfo
    {
        [JsonProperty("SKIN")]
        public YggdrasilProfileSkinUrlModel Skin { get; set; }
    }
    public class YggdrasilProfileSkinUrlModel
    {
        [JsonProperty("url")]
        public string Url { get; set; }
    }
    public class YggdrasilResultModel
    {
        public bool isSuccess { get; set; }
        public string OtherInfo { get; set; }
        public Exception ErrorException { get; set; }
    }
}
