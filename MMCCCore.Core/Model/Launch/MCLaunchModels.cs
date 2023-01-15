using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MMCCCore.Core.Wrapper;
using MMCCCore.Core.Model.Core;
using MMCCCore.Core.Model.Authenticator;
using System.Diagnostics;

namespace MMCCCore.Core.Model.Launch
{
    public class MCLaunchResponse
    {
        public string ErrorMessage { get; set; }
        public LocalGameInfoModel LaunchCore { get; set; }
        public Account LaunchAccount { get; set; }
        public LauncherSettings LaunchArgs { get; set; }
        public LaunchStatus LaunchResult { get; set; }
        public Process MCProcess { get; set; }
        public string Arguments { get; set; }
    }
    public class MCRunCompleteEventArgs
    {
        public string Logs { get; set; }
        public int ExitCode { get; set; } = 0;
    }
    public enum LaunchStatus
    {
        Error = 1,
        Success = 0
    }
}
