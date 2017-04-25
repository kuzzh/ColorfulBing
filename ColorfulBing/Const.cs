namespace ColorfulBing {
    internal static class Const {
        internal static string WallpaperDir = "Wallpaper";

        static Const() {
            var appRoot = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDcim);
            
            WallpaperDir = System.IO.Path.Combine(appRoot.AbsolutePath, "çÍ·×±ØÓ¦", WallpaperDir);
        }
    }
}