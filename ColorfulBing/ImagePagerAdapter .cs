using System;

using Android.App;
using Android.Views;
using Android.Widget;
using Android.Support.V4.View;
using ColorfulBing.Model;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using Android.Graphics;
using Newtonsoft.Json;

namespace ColorfulBing {
    public sealed class ImagePagerAdapter : PagerAdapter {
        private const int MinCount = 10;
        private readonly MainActivity mMainActivity;

        private BData mCurBData;
        private int mCurPos = -1;
        private DateTime mCurDate = DateTime.Today;

        public BData CurBData;

        public ImagePagerAdapter(MainActivity activity) {
            mMainActivity = activity;
        }

        public override int Count {
            get {
                var count = SQLiteHelper.GetCount();
                if (count < MinCount) {
                    return MinCount;
                }
                return count;
            }
        }


        public override bool IsViewFromObject(View view, Java.Lang.Object objectValue) {
            return view == ((ImageView)objectValue);
        }

        public override Java.Lang.Object InstantiateItem(ViewGroup container, int position) {
            var imageView = new ImageView(mMainActivity);
            imageView.SetScaleType(ImageView.ScaleType.FitXy);

            var dt = mCurDate.Subtract(new TimeSpan(position, 0, 0, 0));

            var pd = new ProgressDialog(mMainActivity);
            pd.SetProgressStyle(ProgressDialogStyle.Spinner);
            pd.SetTitle(Resource.String.PleaseWait);
            pd.SetMessage(mMainActivity.Resources.GetString(Resource.String.OnLoading));
            pd.Show();
            Task.Run(() => {
                try {
                    var bdata = GetBDataAsync(dt, position).Result;
                    mMainActivity.RunOnUiThread(() => {
                        imageView.SetImageBitmap(bdata.Bitmap);
                        ((ViewPager)container).AddView(imageView);
                        pd.Dismiss();
                    });
                } catch (System.Exception ex) {
                    Toast.MakeText(mMainActivity, ex.Message, ToastLength.Long).Show();
                }
            });



            return imageView;
        }

        public override void DestroyItem(ViewGroup container, int position, Java.Lang.Object objectValue) {
            ((ViewPager)container).RemoveView((ImageView)objectValue);
        }

        public override void StartUpdate(ViewGroup container) {
            base.StartUpdate(container);

            var viewPager = mMainActivity.FindViewById<ViewPager>(Resource.Id.view_pager);
            if (mCurPos == viewPager.CurrentItem) return;

            Task.Run(() => {
                try {
                    mCurBData = GetBDataAsync(mCurDate.Subtract(new TimeSpan(viewPager.CurrentItem, 0, 0, 0)), viewPager.CurrentItem).Result;

                    mMainActivity.RunOnUiThread(() => {
                        mMainActivity.UpdateData(mCurBData);
                        mCurPos = viewPager.CurrentItem;
                    });
                } catch (System.Exception ex) {
                    Toast.MakeText(mMainActivity, ex.Message, ToastLength.Long).Show();
                }
            });
        }

        private async Task<BData> GetBDataAsync(DateTime dt, int d) {
            var bdata = await SQLiteHelper.GetBDataAsync(dt);
            if (bdata != null) {
                return bdata;
            }
            bdata = await GetBDataFromRemoteAsync(d);

            SQLiteHelper.InsertBDataAsync(bdata);

            return bdata;
        }

        private async Task<BData> GetBDataFromRemoteAsync(int d) {
            var size = new Point();
            mMainActivity.WindowManager.DefaultDisplay.GetSize(size);
            var screenWidth = size.X;       // 屏幕宽（像素，如：768px）  
            var screenHeight = size.Y;      // 屏幕高（像素，如：1184px）  

            var res = Utils.GetSuitableResolution(screenWidth, screenHeight);
            var imageUrl = System.String.Format("https://bing.ioliu.cn/v1?w={0}&h={1}&d={2}", res.Width, res.Height, d);
            var bitmap = await GetBitmapAsync(imageUrl);

            var jsonString = await GetJsonDataAsync(System.String.Format("https://bing.ioliu.cn/v1?d={0}", d));
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
            } catch (System.Exception ex) {
                Toast.MakeText(mMainActivity, ex.Message, ToastLength.Long).Show();
                return null;
            }
        }
    }
}