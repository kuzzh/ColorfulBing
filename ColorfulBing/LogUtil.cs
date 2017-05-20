using System;
using System.IO;
using System.Threading.Tasks;

namespace ColorfulBing {
    internal static class LogUtil {
        internal static async Task WriteLogAsync(string msg) {
            msg = string.Format("[{0}]{1}\r\n", DateTime.Now.ToString("G"), msg);
            using (var sw = new StreamWriter(Consts.LogFile)) {
                await sw.WriteLineAsync(msg);
            }
        }

        internal static async Task WriteLogAsync(Exception ex) {
            var msg = string.Format("[{0}]{1}\r\n{2}\r\n", DateTime.Now.ToString("G"), ex.Message, ex.StackTrace);
            await WriteLogAsync(msg);
        }
    }
}