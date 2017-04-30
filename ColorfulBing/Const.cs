using ColorfulBing.Model;
using System.Collections.Generic;
using System.IO;

namespace ColorfulBing {
    internal static class Const {
        internal static string DatabaseFile;
        internal static List<Resolution> SupportedResolutions;
        static Const() {
            var documents = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDocuments);

            var databaseDir = Path.Combine(documents.AbsolutePath, "ColorfulBing");
            if (!Directory.Exists(databaseDir)) {
                Directory.CreateDirectory(databaseDir);
            }

            DatabaseFile = Path.Combine(databaseDir, "local.db");

            //if (File.Exists(DatabaseFile)) {
            //    File.Delete(DatabaseFile);
            //}

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