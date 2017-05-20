using ColorfulBing.Model;

namespace ColorfulBing {
    public static class Utils {
        public static Resolution GetSuitableResolution(int w, int h) {
            foreach (var res in Consts.SupportedResolutions) {
                if (res.Width >= w && res.Height >= h) {
                    return res;
                }
            }
            return new Resolution(w, h);
        }
    }
}