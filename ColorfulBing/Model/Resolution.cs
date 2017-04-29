namespace ColorfulBing.Model {
    public sealed class Resolution {
        public int Width { get; set; }
        public int Height { get; set; }

        public Resolution(int w, int h) {
            Width = w;
            Height = h;
        }
    }
}