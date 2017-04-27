using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Graphics;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using Android.Graphics.Drawables;
using Android.Provider;

namespace ColorfulBing
{
    [Activity(Label = "@string/ApplicationName", MainLauncher = true, Icon = "@drawable/bing")]
    public class MainActivity : Activity {
        private ImageView mImageView;
        private ImageView ivLocation;
        private BingData mBingData;

        protected override async void OnCreate(Bundle bundle) {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            mImageView = FindViewById<ImageView>(Resource.Id.imageView1);
            ivLocation = FindViewById<ImageView>(Resource.Id.ivLocation);

            RegisterForContextMenu(mImageView);

            var pd = new ProgressDialog(this);
            pd.SetProgressStyle(ProgressDialogStyle.Spinner);
            pd.SetTitle(Resource.String.WaitDlgTitle);
            pd.SetMessage(Resources.GetString(Resource.String.WaitDlgMsg));
            pd.Show();
            await LoadBingAsync();
            pd.Dismiss();
        }

        private async Task LoadBingAsync() {
            var tvCopyRight = FindViewById<TextView>(Resource.Id.tvCopyRight);
            var tvDescription = FindViewById<TextView>(Resource.Id.tvDescription);
            var tvCalendar = FindViewById<TextView>(Resource.Id.tvCalendar);
            var tvLocation = FindViewById<TextView>(Resource.Id.tvLocation);

            var size = new Point();
            WindowManager.DefaultDisplay.GetSize(size);
            var screenWidth = size.X;       // 屏幕宽（像素，如：480px）  
            var screenHeight = size.Y;      // 屏幕高（像素，如：800px）  

            var imageUrl = String.Format("https://bing.ioliu.cn/v1?w={0}&h={1}", screenWidth, screenHeight);
            var bitmap = await GetBitmapAsync(imageUrl);
            mImageView.SetImageBitmap(bitmap);

            var jsonString = await GetJsonDataAsync("https://bing.ioliu.cn/v1");
            mBingData = JsonConvert.DeserializeObject<BingData>(jsonString);

            tvCopyRight.Text = mBingData.Data.Copyright;
            tvDescription.Text = mBingData.Data.Description;
            var calendar = string.Format("{0}-{1}-{2}", mBingData.Data.EndDate.Substring(0, 4), mBingData.Data.EndDate.Substring(4, 2), mBingData.Data.EndDate.Substring(6, 2));
            tvCalendar.Text = calendar;
            var location = mBingData.Data.Continent;
            if (!string.IsNullOrEmpty(mBingData.Data.Country)) {
                location += "，" + mBingData.Data.Country;
            }
            if (!string.IsNullOrEmpty(mBingData.Data.City)) {
                location += "，" + mBingData.Data.City;
            }
            tvLocation.Text = location;

            ivLocation.Visibility = ViewStates.Visible;
        }

        public override void OnCreateContextMenu(IContextMenu menu, View v, IContextMenuContextMenuInfo menuInfo) {
            //menu.SetHeaderTitle("Edit");
            var menuItems = new List<String> {
                Resources.GetString(Resource.String.SaveToDisk),
                Resources.GetString(Resource.String.SetAsWallpaper)
            };
            for (int i = 0; i < menuItems.Count; i++) {
                menu.Add(Menu.None, i, i, menuItems[i]);
            }
            base.OnCreateContextMenu(menu, v, menuInfo);
        }

        public override bool OnContextItemSelected(IMenuItem item) {
            base.OnContextItemSelected(item);

            var bitmap = ((BitmapDrawable)mImageView.Drawable).Bitmap;
            var pd = new ProgressDialog(this);
            pd.SetProgressStyle(ProgressDialogStyle.Spinner);
            pd.SetTitle(Resources.GetString(Resource.String.WaitDlgTitle));

            switch (item.ItemId) {
                case 0: // 保存至磁盘
                    pd.SetMessage(Resources.GetString(Resource.String.WaitDlgMsgSave));
                    pd.Show();
                    MediaStore.Images.Media.InsertImage(ContentResolver, bitmap, mBingData.Data.Title, mBingData.Data.Description);
                    pd.Dismiss();

                    Toast.MakeText(this, "图片已保存至图库", ToastLength.Long).Show();

                    return true;
                case 1: // 设置为壁纸
                    pd.SetMessage("正在设置壁纸...");
                    pd.Show();
                    WallpaperManager.GetInstance(this).SetBitmap(bitmap);
                    pd.Dismiss();

                    Toast.MakeText(this, "壁纸设置成功", ToastLength.Long).Show();

                    return true;
            }
            return false;
        }

        private async Task<string> GetJsonDataAsync(string url) {
            var webRequest = WebRequest.CreateHttp(url);
            webRequest.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.79 Safari/537.36 Edge/14.14393";

            var response = await webRequest.GetResponseAsync();
            using (var sr = new StreamReader(response.GetResponseStream())) {
                return sr.ReadToEnd();
            }
        }

        private async Task<Bitmap> GetBitmapAsync(string url) {
            try {
                var webRequest = WebRequest.CreateHttp(url);
                webRequest.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.79 Safari/537.36 Edge/14.14393";

                var response = await webRequest.GetResponseAsync();
                var stream = response.GetResponseStream();
                var bitmap = await BitmapFactory.DecodeStreamAsync(stream);
                return bitmap;
            } catch (Exception ex) {
                Toast.MakeText(this, ex.Message, ToastLength.Long).Show();
                return null;
            }
        }
    }
}

