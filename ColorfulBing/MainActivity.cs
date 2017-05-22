using System;
using Android.App;
using Android.Views;
using Android.Widget;
using Android.OS;
using System.Threading.Tasks;
using System.Collections.Generic;
using Android.Provider;
using ColorfulBing.Model;
using Android.Support.V4.View;
using Android.Runtime;
using System.IO;

namespace ColorfulBing {
    [Activity(Label = "@string/ApplicationName", MainLauncher = true, Icon = "@drawable/bing")]
    public class MainActivity : Activity {
        
        protected override void OnCreate(Bundle bundle) {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            var viewPager = FindViewById<ViewPager>(Resource.Id.view_pager);
            if (null == viewPager.Adapter) {
                viewPager.Adapter = new ImagePagerAdapter(this);
            }

            AndroidEnvironment.UnhandledExceptionRaiser += UnhandledExceptionHandler;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
            TaskScheduler.UnobservedTaskException += TaskSchedulerOnUnobservedTaskException;
        }

        protected override void Dispose(bool disposing) {
            AndroidEnvironment.UnhandledExceptionRaiser -= UnhandledExceptionHandler;
            AppDomain.CurrentDomain.UnhandledException -= CurrentDomainOnUnhandledException;
            TaskScheduler.UnobservedTaskException -= TaskSchedulerOnUnobservedTaskException;

            base.Dispose(disposing);
        }

        private void TaskSchedulerOnUnobservedTaskException(object sender,
            UnobservedTaskExceptionEventArgs unobservedTaskExceptionEventArgs) {
            var ex = unobservedTaskExceptionEventArgs.Exception;
            LogUtil.WriteLogAsync(ex).Wait();
            Toast.MakeText(this, ex.Message, ToastLength.Long).Show();
        }

        private void CurrentDomainOnUnhandledException(object sender,
            UnhandledExceptionEventArgs unhandledExceptionEventArgs) {
            var ex = unhandledExceptionEventArgs.ExceptionObject as Exception;
            LogUtil.WriteLogAsync(ex).Wait();
            Toast.MakeText(this, ex.Message, ToastLength.Long).Show();
        }

        private void UnhandledExceptionHandler(object sender, RaiseThrowableEventArgs e) {
            LogUtil.WriteLogAsync(e.Exception).Wait();
            Toast.MakeText(this, e.Exception.Message, ToastLength.Long).Show();
        }

        public void UpdateData(BData bdata) {
            var tvCopyRight = FindViewById<TextView>(Resource.Id.tvCopyRight);
            var tvDescription = FindViewById<TextView>(Resource.Id.tvDescription);
            var tvCalendar = FindViewById<TextView>(Resource.Id.tvCalendar);
            var tvLocation = FindViewById<TextView>(Resource.Id.tvLocation);
            var locIcon = FindViewById<ImageView>(Resource.Id.ivLocation);

            tvCopyRight.Text = bdata.Copyright;
            tvDescription.Text = bdata.Description;
            tvCalendar.Text = bdata.Calendar.ToShortDateString();
            tvLocation.Text = bdata.Location;

            locIcon.Visibility = ViewStates.Visible;
        }
        
        public override void OnCreateContextMenu(IContextMenu menu, View v, IContextMenuContextMenuInfo menuInfo) {
            var menuItems = new List<String> {
                Resources.GetString(Resource.String.SaveToDisk),
                Resources.GetString(Resource.String.SetAsWallpaper),
                Resources.GetString(Resource.String.GoToToday)
            };
            for (int i = 0; i < menuItems.Count; i++) {
                menu.Add(Menu.None, i, i, menuItems[i]);
            }
            base.OnCreateContextMenu(menu, v, menuInfo);
        }

        public override bool OnContextItemSelected(IMenuItem item) {
            base.OnContextItemSelected(item);

            var viewPager = FindViewById<ViewPager>(Resource.Id.view_pager);
            var imagePagerAdapter = (ImagePagerAdapter)viewPager.Adapter;
            var bdata = imagePagerAdapter.CurBData;
            var bitmap = bdata.Bitmap;
            var pd = new ProgressDialog(this);
            pd.SetProgressStyle(ProgressDialogStyle.Spinner);
            pd.SetCanceledOnTouchOutside(false);
            pd.SetTitle(Resources.GetString(Resource.String.PleaseWait));

            switch (item.ItemId) {
                case 0: // 保存至磁盘
                    pd.SetMessage(Resources.GetString(Resource.String.OnSaving));
                    pd.Show();
                    Task.Run(() => MediaStore.Images.Media.InsertImage(ContentResolver, bitmap, bdata.Title, bdata.Description)).ContinueWith(t => {
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
                case 2: // 转到今天
                    viewPager.SetCurrentItem(0, true);
                    return true;
            }
            return false;
        }
    }
}

