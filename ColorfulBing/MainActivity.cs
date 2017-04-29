using System;
using Android.App;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Graphics;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using Android.Graphics.Drawables;
using Android.Provider;
using ColorfulBing.Model;

namespace ColorfulBing {
    [Activity(Label = "@string/ApplicationName", MainLauncher = true, Icon = "@drawable/bing")]
    public class MainActivity : Activity {
        private ImageView mImageView;
        private ImageView mLocIcon;
        private BData mBData;

        private DateTime mCurDate = DateTime.Today;

        protected override async void OnCreate(Bundle bundle) {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            mImageView = FindViewById<ImageView>(Resource.Id.imageView1);
            mLocIcon = FindViewById<ImageView>(Resource.Id.ivLocation);

            RegisterForContextMenu(mImageView);

            var pd = new ProgressDialog(this);
            pd.SetProgressStyle(ProgressDialogStyle.Spinner);
            pd.SetTitle(Resource.String.PleaseWait);
            pd.SetMessage(Resources.GetString(Resource.String.OnLoading));
            pd.Show();
            try {
                mBData = await GetBDataAsync(mCurDate);
            } catch (Exception ex) {
                Toast.MakeText(this, ex.Message, ToastLength.Long).Show();
                return;
            }

            var tvCopyRight = FindViewById<TextView>(Resource.Id.tvCopyRight);
            var tvDescription = FindViewById<TextView>(Resource.Id.tvDescription);
            var tvCalendar = FindViewById<TextView>(Resource.Id.tvCalendar);
            var tvLocation = FindViewById<TextView>(Resource.Id.tvLocation);

            tvCopyRight.Text = mBData.Copyright;
            tvDescription.Text = mBData.Description;
            tvCalendar.Text = mBData.Calendar.ToShortDateString();
            tvLocation.Text = mBData.Location;
            mImageView.SetImageBitmap(mBData.Bitmap);
            mLocIcon.Visibility = ViewStates.Visible;

            pd.Dismiss();
        }

        private async Task<BData> GetBDataAsync(DateTime dt) {
            var bdata = await SQLiteHelper.GetBDataAsync(mCurDate);
            if (bdata != null) {
                return bdata;
            }
            bdata = await GetBDataFromRemoteAsync();

            SQLiteHelper.InsertBDataAsync(bdata);

            return bdata;
        }

        private async Task<BData> GetBDataFromRemoteAsync() {
            var size = new Point();
            WindowManager.DefaultDisplay.GetSize(size);
            var screenWidth = size.X;       // 屏幕宽（像素，如：768px）  
            var screenHeight = size.Y;      // 屏幕高（像素，如：1184px）  

            var res = Utils.GetSuitableResolution(screenWidth, screenHeight);
            var imageUrl = String.Format("https://bing.ioliu.cn/v1?w={0}&h={1}", res.Width, res.Height);
            var bitmap = await GetBitmapAsync(imageUrl);

            var jsonString = await GetJsonDataAsync("https://bing.ioliu.cn/v1");
            var JBData = JsonConvert.DeserializeObject<JBData>(jsonString);

            return new BData {
                Id = Guid.NewGuid().ToString(),
                Title = JBData.Data.Title,
                Description = JBData.Data.Description,
                Copyright = JBData.Data.Copyright,
                Location = FormatLocation(JBData),
                Calendar = FormatCalendar(JBData),
                Bitmap = bitmap
            };
        }

        private DateTime FormatCalendar(JBData jbdata) {
            var dtStr = string.Format("{0}-{1}-{2}", jbdata.Data.EndDate.Substring(0, 4), jbdata.Data.EndDate.Substring(4, 2), jbdata.Data.EndDate.Substring(6, 2));
            return DateTime.Parse(dtStr);
        }

        private string FormatLocation(JBData jbdata) {
            var location = jbdata.Data.Continent;
            if (!string.IsNullOrEmpty(jbdata.Data.Country)) {
                location += "，" + jbdata.Data.Country;
            }
            if (!string.IsNullOrEmpty(jbdata.Data.City)) {
                location += "，" + jbdata.Data.City;
            }
            return location;
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
            pd.SetTitle(Resources.GetString(Resource.String.PleaseWait));

            switch (item.ItemId) {
                case 0: // 保存至磁盘
                    pd.SetMessage(Resources.GetString(Resource.String.OnSaving));
                    pd.Show();
                    Task.Run(() => MediaStore.Images.Media.InsertImage(ContentResolver, bitmap, mBData.Title, mBData.Description)).ContinueWith(t => {
                        pd.Dismiss();

                        Toast.MakeText(this, Resource.String.SaveToDiskSuccess, ToastLength.Long).Show();

                    });

                    return true;
                case 1: // 设置为壁纸
                    pd.SetMessage(Resources.GetString(Resource.String.SettingWallpaper));
                    pd.Show();
                    Task.Run(() => WallpaperManager.GetInstance(this).SetBitmap(bitmap)).ContinueWith(t => {
                        pd.Dismiss();

                        Toast.MakeText(this, Resource.String.SetWallpaperSuccess, ToastLength.Long).Show();
                    });                    

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

