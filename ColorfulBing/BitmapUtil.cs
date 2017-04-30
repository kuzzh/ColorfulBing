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

        public static Bitmap DrawableToBitmap(Drawable drawable) {
            // 取 drawable 的长宽
            var w = drawable.IntrinsicWidth;
            var h = drawable.IntrinsicHeight;

            // 取 drawable 的颜色格式
            var config = drawable.Opacity != (int)Format.Opaque ? Config.Argb8888 : Config.Rgb565;
            // 建立对应 bitmap
            var bitmap = CreateBitmap(w, h, config);
            // 建立对应 bitmap 的画布
            var canvas = new Canvas(bitmap);
            drawable.SetBounds(0, 0, w, h);
            // 把 drawable 内容画到画布中
            drawable.Draw(canvas);
            return bitmap;
        }

        public static Drawable ZoomDrawable(Drawable drawable, int w, int h) {
            //var width = drawable.IntrinsicWidth;
            //var height = drawable.IntrinsicHeight;
            //// drawable转换成bitmap
            //var oldbmp = DrawableToBitmap(drawable);
            //// 创建操作图片用的Matrix对象
            //var matrix = new Matrix();
            //// 计算缩放比例
            //var sx = ((float)w / width);
            //var sy = ((float)h / height);
            //// 设置缩放比例
            //matrix.PostScale(sx, sy);
            //// 建立新的bitmap，其内容是对原bitmap的缩放后的图
            //var newbmp = CreateBitmap(oldbmp, 0, 0, width, height, matrix, true);
            //return new BitmapDrawable(newbmp);

            var oldbmp = DrawableToBitmap(drawable);
            var newbmp = ZoomBitmap(oldbmp, w, h);
            return new BitmapDrawable(newbmp);
        }

        public static Bitmap ZoomBitmap(Bitmap bitmap, int w, int h) {
            var width = bitmap.Width;
            var height = bitmap.Height;

            // 创建操作图片用的Matrix对象
            var matrix = new Matrix();
            // 计算缩放比例
            var sx = ((float)w / width);
            var sy = ((float)h / height);
            // 设置缩放比例
            matrix.PostScale(sx, sy);
            // 建立新的bitmap，其内容是对原bitmap的缩放后的图
            return CreateBitmap(bitmap, 0, 0, width, height, matrix, true);
        }
    }
}