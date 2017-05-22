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
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ColorfulBing {
    public sealed class ImagePagerAdapter : PagerAdapter {
        private const int MinCount = 10;
        private readonly MainActivity mMainActivity;

        private bool mFirstFetchData = true;
        private readonly ProgressDialog mProgressDialog;

        private int mCount;
        private BData mCurBData;
        private int mCurPos = -1;
        private DateTime mCurDate = DateTime.Today;

        private readonly List<CacheItem> mBDataCache = new List<CacheItem>();

        public BData CurBData { get { return mCurBData; } }

        public ImagePagerAdapter(MainActivity activity) {
            mMainActivity = activity;

            mProgressDialog = new ProgressDialog(activity);
            mProgressDialog.SetProgressStyle(ProgressDialogStyle.Spinner);
            mProgressDialog.SetCanceledOnTouchOutside(false);
        }

        public override int Count {
            get {
                if (mCount <= 0) {
                    mCount = Math.Max(SQLiteHelper.GetCount(), MinCount);
                    var bdata = SQLiteHelper.GetBDataAsync(DateTime.Today).Result;
                    if (null == bdata) { // 当天的壁纸还未存入数据库
                        mCount += 1;
                    }
                }
                return mCount;
            }
        }


        public override bool IsViewFromObject(View view, Java.Lang.Object objectValue) {
            return view == ((ImageView)objectValue);
        }

        public override Java.Lang.Object InstantiateItem(ViewGroup container, int position) {
            var imageView = new ImageView(mMainActivity);
            imageView.SetScaleType(ImageView.ScaleType.FitXy);

            mMainActivity.RegisterForContextMenu(imageView);

            var dt = mCurDate.Subtract(new TimeSpan(position, 0, 0, 0));

            if (mFirstFetchData) {
                mProgressDialog.SetProgressStyle(ProgressDialogStyle.Spinner);
                mProgressDialog.SetTitle(Resource.String.PleaseWait);
                mProgressDialog.SetMessage(mMainActivity.Resources.GetString(Resource.String.OnLoading));
                mProgressDialog.Show();
            }

            Task.Run(() => {
                try {
                    var bdata = GetBDataAsync(dt, position).Result;
                    mMainActivity.RunOnUiThread(() => {
                        imageView.SetImageBitmap(bdata.Bitmap);
                        ((ViewPager)container).AddView(imageView);

                        if (mFirstFetchData) {
                            mProgressDialog.Dismiss();

                            mFirstFetchData = false;
                        }
                    });
                } catch (Exception ex) {
                    LogUtil.WriteLogAsync(ex).Wait();
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
                    LogUtil.WriteLogAsync(ex).Wait();
                    Toast.MakeText(mMainActivity, ex.Message, ToastLength.Long).Show();
                }
            });
        }

        private async Task<BData> GetBDataAsync(DateTime dt, int d) {
            var cacheItem = mBDataCache.FirstOrDefault(data => data.BData.Calendar.Date.Equals(dt.Date));
            if (cacheItem != null) {
                cacheItem.LastAccessedTime = DateTime.Now;
                return cacheItem.BData;
            }
            var bdata = await SQLiteHelper.GetBDataAsync(dt);
            if (bdata != null) {
                mBDataCache.Add(new CacheItem(bdata));
                return bdata;
            }

            mMainActivity.RunOnUiThread(() => {
                mProgressDialog.SetTitle(Resource.String.PleaseWait);
                mProgressDialog.SetMessage(mMainActivity.Resources.GetString(Resource.String.OnLoading));
                mProgressDialog.Show();
            });

            bdata = await GetBDataFromRemoteAsync(d);

            SQLiteHelper.InsertBDataAsync(bdata);

            mBDataCache.Add(new CacheItem(bdata));

            mMainActivity.RunOnUiThread(() => mProgressDialog.Dismiss());

            return bdata;
        }

        private async Task<BData> GetBDataFromRemoteAsync(int d) {
            var size = new Point();
            mMainActivity.WindowManager.DefaultDisplay.GetSize(size);
            var screenWidth = size.X;       // 屏幕宽（像素，如：768px）  
            var screenHeight = size.Y;      // 屏幕高（像素，如：1184px）  

            var res = Utils.GetSuitableResolution(screenWidth, screenHeight);
            var bData = new BData();

            var html = await GetHtmlDataAsync(string.Format(Consts.BingLifeUrl, DateTime.Today.Subtract(new TimeSpan(d, 0, 0, 0)).ToString("yyyyMMdd")));

            bData.Title = ExtractText(html, "<div class=\"hplaTtl\">(.*?)</div>");
            bData.Location = ExtractText(html, "<span class=\"hplaAttr\">(.*?)</span");
            bData.Description = ExtractText(html, "<div id=\"hplaSnippet\">(.*?)</div>");

            var json = await GetHtmlDataAsync(string.Format(Consts.BingHPImageArchiveUrl, d, DateUtil.ConvertFromLocalDateTimeToSeconds(DateTime.Now)));
            var JBData = JsonConvert.DeserializeObject<JBData>(json);

            bData.Copyright = JBData.Images[0].Copyright;
            bData.ImageUrl = Consts.BingBaseUrl + JBData.Images[0].Urlbase + "_" + res.Width + "x" + res.Height + ".jpg";

            bData.Bitmap = await GetBitmapAsync(bData.ImageUrl);

            if (bData.Bitmap.Width != screenWidth || bData.Bitmap.Height != screenHeight) {
                bData.Bitmap = BitmapUtil.ZoomBitmap(bData.Bitmap, screenWidth, screenHeight);
            }

            bData.Calendar = FormatCalendar(JBData);

            return bData;
        }

        private string ExtractText(string html, string pattern) {
            var regex = new Regex(pattern);
            if (regex.IsMatch(html)) {
                return regex.Match(html).Groups[1].Value;
            }
            return "";
        }

        private DateTime FormatCalendar(JBData jbdata) {
            var dtStr = string.Format("{0}-{1}-{2}", jbdata.Images[0].EndDate.Substring(0, 4), jbdata.Images[0].EndDate.Substring(4, 2), jbdata.Images[0].EndDate.Substring(6, 2));
            return DateTime.Parse(dtStr);
        }

        private async Task<string> GetHtmlDataAsync(string url) {
            var webRequest = WebRequest.CreateHttp(url);
            webRequest.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.79 Safari/537.36 Edge/14.14393";

            var response = await webRequest.GetResponseAsync();
            using (var sr = new StreamReader(response.GetResponseStream())) {
                return sr.ReadToEnd();
            }
        }

        private async Task<Bitmap> GetBitmapAsync(string url) {
            var webRequest = WebRequest.CreateHttp(url);
            webRequest.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.79 Safari/537.36 Edge/14.14393";

            var response = await webRequest.GetResponseAsync();
            var stream = response.GetResponseStream();
            var bitmap = await BitmapFactory.DecodeStreamAsync(stream);
            return bitmap;
        }
    }
}