using System;
using Android.Graphics;

namespace ColorfulBing.Model {
    public sealed class BData {
        public string Title { get; set; }
        public string Copyright { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public string ImageUrl { get; set; }
        public Bitmap Bitmap { get; set; }
        public DateTime Calendar { get; set; }
    }
}