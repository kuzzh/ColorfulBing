using Android.Graphics;
using System.Threading.Tasks;
using static Android.Graphics.Bitmap;
using System.IO;

namespace ColorfulBing {
    public static class BitmapUtil {
        public static async Task<Bitmap> GetBitmapAsync(byte[] buffer) {
            return await BitmapFactory.DecodeByteArrayAsync(buffer, 0, buffer.Length);
        }

        public static async Task<byte[]> GetBitmapBufferAsync(Bitmap bitmap) {

            using (var stream = new MemoryStream()) {
                await bitmap.CompressAsync(CompressFormat.Jpeg, 100, stream);

                return stream.GetBuffer();
            }
        }
    }
}