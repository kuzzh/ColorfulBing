using ColorfulBing.Model;
using System.Collections.Generic;
using System.IO;

namespace ColorfulBing {
    internal static class Consts {
        internal static string DBFile;
        internal static string LogFile;
        internal static List<Resolution> SupportedResolutions;
        internal const string BingBaseUrl = "http://cn.bing.com";
        internal const string BingLifeUrl = "http://cn.bing.com/cnhp/life?currentDate={0}";
        internal const string BingHPImageArchiveUrl = "http://cn.bing.com/HPImageArchive.aspx?format=js&idx={0}&n=1&nc={1}&pid=hp";

        static Consts() {
            var documents = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDocuments);

            var rootDir = Path.Combine(documents.AbsolutePath, "ColorfulBing");
            if (!Directory.Exists(rootDir)) {
                Directory.CreateDirectory(rootDir);
            }

            DBFile = Path.Combine(rootDir, "local.db");

            LogFile = Path.Combine(rootDir, "log.txt");
            if (!File.Exists(LogFile)) {
                File.Create(LogFile);
            }

            SupportedResolutions = new List<Resolution> {
                new Resolution(240, 320),
                new Resolution(320, 240),
                new Resolution(400, 240),
                new Resolution(480, 800),
                new Resolution(640, 480),
                new Resolution(720, 1280),
                new Resolution(768, 1280),
                new Resolution(800, 480),
                new Resolution(800, 600),
                new Resolution(1024, 768),
                new Resolution(1280,768),
                new Resolution(1366,768),
                new Resolution(1920,1080),
                new Resolution(1920,1200)
            };
        }
    }
}