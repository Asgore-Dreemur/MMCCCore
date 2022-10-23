using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MMCCCore.Model.Authenticator;

namespace MMCCCore.Module.Authenticator
{
    public static class OfflineAuthenticator
    {
        public static Account OfflineAuthenticate(string Username)
        {
            return new Account
            {
                ClientToken = Guid.NewGuid().ToString("N"),
                Name = Username,
                LoginType = AccountType.Offline,
                AccessToken = Guid.NewGuid().ToString("N"),
                Uuid = Guid.NewGuid()
            };
        }
    }
}
