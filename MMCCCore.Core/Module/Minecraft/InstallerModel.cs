using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMCCCore.Core.Module.Minecraft
{
    public abstract class InstallerModel
    {
        public event EventHandler<(double, string)> ProgressChanged;
        protected virtual void OnProgressChanged(double status, string message) => ProgressChanged?.Invoke(this, (status, message));
    }
    public class InstallerResponse
    {
        public bool isSuccess { get; set; }
        public Exception Exception { get; set; }
        public string OtherMessage { get; set; }
    }
}
