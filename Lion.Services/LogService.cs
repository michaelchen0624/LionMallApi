using System;
using System.IO;
using LionMall.Tools;

namespace Lion.Services
{
    /// <summary>
    /// </summary>
    public class LogService : ILogService
    {
        /// <summary>
        /// </summary>
        /// <param name="catLog"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public bool D(string catLog, string msg)
        {
            WriteLog(msg, logType.debug, catLog);
            return true;
        }

        /// <summary>
        /// </summary>
        /// <param name="catLog"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public bool E(string catLog, string msg)
        {
            WriteLog(msg, logType.err, catLog);
            return true;
        }

        /// <summary>
        /// </summary>
        /// <param name="catLog"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public bool I(string catLog, string msg)
        {
            WriteLog(msg, logType.info, catLog);
            return true;
        }

        private void WriteLog(string ss, logType tp, string catlog)
        {
            //if (tp == logType.debug ) return;//不开启DEBUG，直接退出


            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            if (!baseDirectory.EndsWith("/")) baseDirectory += "/";

            var di_log = new DirectoryInfo(baseDirectory + @"log/");
            if (!di_log.Exists) di_log.Create();


            var di = new DirectoryInfo(di_log.FullName + @"/" + DateTime.Now.ToString("yyyy_MM_dd") + @"/");
            if (!di.Exists) di.Create();

            try
            {
                var str_logType = Enum.GetName(typeof(logType), tp);

                var fileNmae = $"{catlog}_{str_logType}";

                var filePath = di.FullName + @"/" + fileNmae + ".log";
                var fileInfo = new FileInfo(filePath);

                var fs = fileInfo.Open(FileMode.Append, FileAccess.Write);

                var sw = new StreamWriter(fs);
                try
                {
                    sw.WriteLine(DateTime.Now.ToString("HH:mm:ss:ffff") + " " + ss);
                    sw.WriteLine();
                    sw.Flush();
                }
                catch
                {
                }
                finally
                {
                    fs.Close();
                    sw.Close();
                }
            }
            catch
            {
            }
        }


        private enum logType
        {
            err,
            info,
            warn,
            debug
        }
    }
}