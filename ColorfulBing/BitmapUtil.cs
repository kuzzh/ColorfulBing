using Android.Graphics;
using System.Threading.Tasks;
using static Android.Graphics.Bitmap;
using System.IO;
using Android.Graphics.Drawables;
using Android.Content.Res;

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

        public static Drawable GetDrawable(Resources res, Bitmap bitmap) {
            return new BitmapDrawable(res, bitmap);
        }

        //public static Bitmap DrawableToBitmap(Drawable drawable) {
        //    // 取 drawable 的长宽
        //    int w = drawable.IntrinsicWidth;
        //    int h = drawable.IntrinsicHeight;

        //    // 取 drawable 的颜色格式
        //    Bitmap.Config config = drawable.Opacity != PixelFormat.OPAQUE ? Bitmap.Config.ARGB_8888
        //            : Bitmap.Config.RGB_565;
        //    // 建立对应 bitmap
        //    Bitmap bitmap = Bitmap.createBitmap(w, h, config);
        //    // 建立对应 bitmap 的画布
        //    Canvas canvas = new Canvas(bitmap);
        //    drawable.setBounds(0, 0, w, h);
        //    // 把 drawable 内容画到画布中
        //    drawable.draw(canvas);
        //    return bitmap;
        //}

        //public static Drawable ZoomDrawable(Drawable drawable, int w, int h) {
        //    int width = drawable.IntrinsicWidth;
        //    int height = drawable.IntrinsicHeight;
        //    // drawable转换成bitmap
        //    Bitmap oldbmp = drawableToBitmap(drawable);
        //    // 创建操作图片用的Matrix对象
        //    Matrix matrix = new Matrix();
        //    // 计算缩放比例
        //    float sx = ((float)w / width);
        //    float sy = ((float)h / height);
        //    // 设置缩放比例
        //    matrix.postScale(sx, sy);
        //    // 建立新的bitmap，其内容是对原bitmap的缩放后的图
        //    Bitmap newbmp = Bitmap.createBitmap(oldbmp, 0, 0, width, height,
        //            matrix, true);
        //    return new BitmapDrawable(newbmp);
        //}
    }
}