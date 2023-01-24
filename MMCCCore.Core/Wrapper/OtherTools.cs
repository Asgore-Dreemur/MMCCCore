using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;
using System.Threading;
using MMCCCore.Core.Model.MinecraftFiles;

namespace MMCCCore.Core.Wrapper
{
    public static class OtherTools
    {

        public static char JavaCPSeparatorChar
        {
            get
            {
                return Environment.OSVersion.Platform == PlatformID.Win32NT ? ';' : ':';
            }
        }

        public static string FormatPath(string path) => path.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
        public static HashVaildateResult VaildateSha1(string FilePath, string Sha1)
        {
            try
            {
                Sha1 = Sha1.Trim('\n').Trim('\r').Trim();
                if (!File.Exists(FilePath)) return new HashVaildateResult { 
                    isSuccess = false,
                    isVaildated = false,
                    ErrorException = new Exception($"Sha1校验失败:给定的文件({FilePath})不存在")
                };
                HashAlgorithm algorithm = SHA1.Create();
                FileStream fs = new FileStream(FilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
                string SHA1Str = BitConverter.ToString(algorithm.ComputeHash(fs)).Replace("-", "").ToLower();
                fs.Close();
                if (SHA1Str != Sha1.ToLower()) return new HashVaildateResult { 
                    isSuccess = true,
                    isVaildated = false,
                    FileSha1 = SHA1Str,
                    ErrorException = new Exception($"Sha1校验失败:文件Sha1(\"{SHA1Str}\")与给定Sha1(\"{Sha1}\")不符")
                };
                return new HashVaildateResult { isVaildated = true, FileSha1 = SHA1Str, ErrorException = null, isSuccess = true };
            }
            catch(Exception e)
            {
                return new HashVaildateResult { isSuccess = false, isVaildated = false, ErrorException = e };
            }
        }
        public static string GetFileSha1(string FilePath)
        {
            if (!File.Exists(FilePath)) return "";
            HashAlgorithm algorithm = SHA1.Create();
            FileStream fs = new FileStream(FilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
            string SHA1Str = BitConverter.ToString(algorithm.ComputeHash(fs)).Replace("-", "").ToLower();
            fs.Close();
            return SHA1Str;
        }
        public static void CreateDir(string path)
        {
            path = FormatPath(path);
            string[] pathtree = path.Split(Path.DirectorySeparatorChar);
            if (pathtree.Count() != 1)
            {
                string pathd = pathtree[0] + Path.DirectorySeparatorChar;
                for (int i = 1; i < pathtree.Count(); i++)
                {
                    if (!Directory.Exists(Path.Combine(pathd, pathtree[i]))) Directory.CreateDirectory(Path.Combine(pathd, pathtree[i]));
                    pathd = Path.Combine(pathd, pathtree[i]);
                }
            }
        }
        public static void WaitForAllThreadExit(Queue<Thread> ThreadQueue)
        {
            while(ThreadQueue.Count > 0)
            {
                if (ThreadQueue.Peek().ThreadState == ThreadState.Aborted
                    || ThreadQueue.Peek().ThreadState == ThreadState.Stopped) ThreadQueue.Dequeue();
            }
        }
        public static void WaitAllTaskExit(List<Task> TaskList)
        {
            bool isFinished = false;
            while (!isFinished)
            {
                isFinished = true;
                foreach (Task task in TaskList)
                {
                    if (task.Status != TaskStatus.RanToCompletion)
                    {
                        isFinished = false;
                        break;
                    }
                }
            }
        }
        public static string GetSystemPlatformName()
        {
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32S:
                    return "win16";
                case PlatformID.Win32Windows:
                    return "win98/95";
                case PlatformID.Win32NT:
                    return "windows";
                case PlatformID.WinCE:
                    return "wince";
                case PlatformID.Unix:
                    return "linux";
                case PlatformID.Xbox:
                    return "xbox";
                case PlatformID.MacOSX:
                    return "osx";
                default:
                    return null;
            }
        }
        public static int GetArch() => Environment.Is64BitOperatingSystem ? 64 : 32;
        public static string Base64Decode(string Base64Str) => Encoding.Default.GetString(Convert.FromBase64String(Base64Str));
    }
}
