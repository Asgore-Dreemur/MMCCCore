using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMCCCore.Core.Model.Authenticator
{
    public class Account
    {
        public string AccessToken { get; set; }
        public string ClientToken { get; set; }
        public AccountType LoginType { get; set; }
        public string ErrorMessage { get; set; } = null;
        public string RefreshToken { get; set; }
        public string Name { get; set; }
        public Guid Uuid { get; set; }
    }
    public enum AccountType
    {
        Microsoft = 0,
        Yggdrasil = 1,
        Offline = 2
    }
}
