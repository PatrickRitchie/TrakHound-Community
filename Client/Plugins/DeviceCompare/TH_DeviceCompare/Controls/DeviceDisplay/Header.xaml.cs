﻿// Copyright (c) 2015 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Threading;
using System.Windows.Media.Animation;

using TH_Configuration;
using TH_Global.Functions;
using TH_UserManagement.Management;

namespace TH_DeviceCompare.Controls.DeviceDisplay
{
    public enum HeaderViewType
    {
        Minimized,
        Small,
        Large
    }

    /// <summary>
    /// Interaction logic for Header.xaml
    /// </summary>
    public partial class Header : UserControl
    {
        public Header()
        {
            InitializeComponent();
            root.DataContext = this;
        }

        public Header(Configuration config)
        {
            InitializeComponent();
            root.DataContext = this;

            if (config != null) DeviceDescription = config.Description;

            // Load Device Logo
            if (config.FileLocations.Manufacturer_Logo_Path != null)
            {
                LoadManufacturerLogo(config.FileLocations.Manufacturer_Logo_Path);
            }

            // Load Device Image
            if (config.FileLocations.Image_Path != null)
            {
                LoadDeviceImage(config.FileLocations.Image_Path);
            }
        }

        public int Index { get; set; }

        public TH_DeviceCompare.DeviceDisplay ParentDisplay { get; set; }


        public Description_Settings DeviceDescription
        {
            get { return (Description_Settings)GetValue(DeviceDescriptionProperty); }
            set { SetValue(DeviceDescriptionProperty, value); }
        }

        public static readonly DependencyProperty DeviceDescriptionProperty =
            DependencyProperty.Register("DeviceDescription", typeof(Description_Settings), typeof(Header), new PropertyMetadata(null));



        const System.Windows.Threading.DispatcherPriority priority = System.Windows.Threading.DispatcherPriority.Background;


        #region "Data"

        /// <summary>
        /// Update data using the Snapshots Table
        /// </summary>
        /// <param name="snapshotData"></param>
        public void UpdateData_Snapshots(object snapshotData)
        {
            Update_Alert(snapshotData);

            Update_Idle(snapshotData);

            Update_Production(snapshotData);
        }

        /// <summary>
        /// Update data using the Variables Table
        /// </summary>
        /// <param name="varaibleData"></param>
        public void UpdateData_Variables(object variableData)
        {
            Update_Break(variableData);
        }

        void Update_Production(object snapshotData)
        {
            DataTable dt = snapshotData as DataTable;
            if (dt != null)
            {
                string value = DataTable_Functions.GetTableValue(dt, "name", "Production", "value");

                bool val = true;
                if (value != null) bool.TryParse(value, out val);

                Production = val;
            }
        }

        void Update_Idle(object snapshotData)
        {
            DataTable dt = snapshotData as DataTable;
            if (dt != null)
            {
                string value = DataTable_Functions.GetTableValue(dt, "name", "Idle", "value");

                bool val = true;
                if (value != null) bool.TryParse(value, out val);

                Idle = val;
            }
        }

        void Update_Alert(object snapshotData)
        {
            DataTable dt = snapshotData as DataTable;
            if (dt != null)
            {
                string value = DataTable_Functions.GetTableValue(dt, "name", "Alert", "value");

                bool val = true;
                if (value != null) bool.TryParse(value, out val);

                Alert = val;
            }
        }

        void Update_Break(object variableData)
        {
            DataTable dt = variableData as DataTable;
            if (dt != null)
            {
                string value = DataTable_Functions.GetTableValue(dt, "variable", "shift_type", "value");
                if (value != null)
                {
                    if (value.ToLower() == "break")
                    {
                        ScheduledDowntime = true;

                        ScheduledDowntime_Text = "Break";

                        string b = DataTable_Functions.GetTableValue(dt, "variable", "shift_begintime", "value");
                        string e = DataTable_Functions.GetTableValue(dt, "variable", "shift_endtime", "value");

                        DateTime begin = DateTime.MinValue;
                        DateTime end = DateTime.MinValue;

                        if (DateTime.TryParse(b, out begin) && DateTime.TryParse(e, out end))
                        {
                            string breakTimes = "(" + begin.ToShortTimeString() + " - " + end.ToShortTimeString() + ")";
                            ScheduledDowntime_Times = breakTimes;
                        }
                    }
                    else ScheduledDowntime = false;
                }
                else ScheduledDowntime = false;
            }
        }

        #endregion


        #region "Status"

        public bool Connected
        {
            get { return (bool)GetValue(ConnectedProperty); }
            set { SetValue(ConnectedProperty, value); }
        }

        public static readonly DependencyProperty ConnectedProperty =
            DependencyProperty.Register("Connected", typeof(bool), typeof(Header), new PropertyMetadata(false));



        public bool Production
        {
            get { return (bool)GetValue(ProductionProperty); }
            set { SetValue(ProductionProperty, value); }
        }

        public static readonly DependencyProperty ProductionProperty =
            DependencyProperty.Register("Production", typeof(bool), typeof(Header), new PropertyMetadata(false));


        public bool Idle
        {
            get { return (bool)GetValue(IdleProperty); }
            set { SetValue(IdleProperty, value); }
        }

        public static readonly DependencyProperty IdleProperty =
            DependencyProperty.Register("Idle", typeof(bool), typeof(Header), new PropertyMetadata(false));


        public bool Alert
        {
            get { return (bool)GetValue(AlertProperty); }
            set { SetValue(AlertProperty, value); }
        }

        public static readonly DependencyProperty AlertProperty =
            DependencyProperty.Register("Alert", typeof(bool), typeof(Header), new PropertyMetadata(false));



        public bool ScheduledDowntime
        {
            get { return (bool)GetValue(ScheduledDowntimeProperty); }
            set { SetValue(ScheduledDowntimeProperty, value); }
        }

        public static readonly DependencyProperty ScheduledDowntimeProperty =
            DependencyProperty.Register("ScheduledDowntime", typeof(bool), typeof(Header), new PropertyMetadata(false));


        public string ScheduledDowntime_Text
        {
            get { return (string)GetValue(ScheduledDowntime_TextProperty); }
            set { SetValue(ScheduledDowntime_TextProperty, value); }
        }

        public static readonly DependencyProperty ScheduledDowntime_TextProperty =
            DependencyProperty.Register("ScheduledDowntime_Text", typeof(string), typeof(Header), new PropertyMetadata(null));


        public string ScheduledDowntime_Times
        {
            get { return (string)GetValue(ScheduledDowntime_TimesProperty); }
            set { SetValue(ScheduledDowntime_TimesProperty, value); }
        }

        public static readonly DependencyProperty ScheduledDowntime_TimesProperty =
            DependencyProperty.Register("ScheduledDowntime_Times", typeof(string), typeof(Header), new PropertyMetadata(null));



        public string LastUpdatedTimestamp
        {
            get { return (string)GetValue(LastUpdatedTimestampProperty); }
            set { SetValue(LastUpdatedTimestampProperty, value); }
        }

        public static readonly DependencyProperty LastUpdatedTimestampProperty =
            DependencyProperty.Register("LastUpdatedTimestamp", typeof(string), typeof(Header), new PropertyMetadata("Never"));

        #endregion

        #region "Minimize / Collapse"

        public HeaderViewType ViewType
        {
            get { return (HeaderViewType)GetValue(ViewTypeProperty); }
            set
            {
                int requested = (int)value;

                int safe = Math.Max(0, requested);
                safe = Math.Min(2, safe);

                SetValue(ViewTypeProperty, (HeaderViewType)safe);

                switch (safe)
                {
                    case 0: AnimateHeight(30); break;
                    case 1: AnimateHeight(150); break;
                    case 2: AnimateHeight(300); break;
                }
            }
        }

        public static readonly DependencyProperty ViewTypeProperty =
            DependencyProperty.Register("ViewType", typeof(HeaderViewType), typeof(Header), new PropertyMetadata(HeaderViewType.Large));
        

        private void AnimateHeight(double to)
        {
            DoubleAnimation animation = new DoubleAnimation();

            animation.From = root.RenderSize.Height;
            animation.To = to;
            animation.Duration = new Duration(TimeSpan.FromMilliseconds(200));
            root.BeginAnimation(HeightProperty, animation);
        }

        #endregion

        #region "Images"

        public ImageSource Device_Image
        {
            get { return (ImageSource)GetValue(Device_ImageProperty); }
            set { SetValue(Device_ImageProperty, value); }
        }

        public static readonly DependencyProperty Device_ImageProperty =
            DependencyProperty.Register("Device_Image", typeof(ImageSource), typeof(Header), new PropertyMetadata(null));


        public ImageSource Manufacturer_Logo
        {
            get { return (ImageSource)GetValue(Manufacturer_LogoProperty); }
            set { SetValue(Manufacturer_LogoProperty, value); }
        }

        public static readonly DependencyProperty Manufacturer_LogoProperty =
            DependencyProperty.Register("Manufacturer_Logo", typeof(ImageSource), typeof(Header), new PropertyMetadata(null));


        #region "Manufacturer Logo"

        public bool ManufacturerLogoLoading
        {
            get { return (bool)GetValue(ManufacturerLogoLoadingProperty); }
            set { SetValue(ManufacturerLogoLoadingProperty, value); }
        }

        public static readonly DependencyProperty ManufacturerLogoLoadingProperty =
            DependencyProperty.Register("ManufacturerLogoLoading", typeof(bool), typeof(Header), new PropertyMetadata(false));


        Thread LoadManufacturerLogo_THREAD;

        public void LoadManufacturerLogo(string filename)
        {
            ManufacturerLogoLoading = true;

            if (LoadManufacturerLogo_THREAD != null) LoadManufacturerLogo_THREAD.Abort();

            LoadManufacturerLogo_THREAD = new Thread(new ParameterizedThreadStart(LoadManufacturerLogo_Worker));
            LoadManufacturerLogo_THREAD.Start(filename);
        }

        void LoadManufacturerLogo_Worker(object o)
        {
            if (o != null)
            {
                string filename = o.ToString();

                System.Drawing.Image img = Images.GetImage(filename);

                this.Dispatcher.BeginInvoke(new Action<System.Drawing.Image>(LoadManufacturerLogo_GUI), priority, new object[] { img });
            }
            else this.Dispatcher.BeginInvoke(new Action<System.Drawing.Image>(LoadManufacturerLogo_GUI), priority, new object[] { null });
        }

        void LoadManufacturerLogo_GUI(System.Drawing.Image img)
        {
            if (img != null)
            {
                System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(img);

                IntPtr bmpPt = bmp.GetHbitmap();
                BitmapSource bmpSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bmpPt, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

                bmpSource.Freeze();

                if (bmpSource.PixelWidth > bmpSource.PixelHeight)
                {
                    Manufacturer_Logo = TH_WPF.Image_Functions.SetImageSize(bmpSource, 180);
                }
                else
                {
                    Manufacturer_Logo = TH_WPF.Image_Functions.SetImageSize(bmpSource, 0, 80);
                }
            }
            else
            {
                Manufacturer_Logo = null;
            }

            ManufacturerLogoLoading = false;
        }

        #endregion

        #region "Device Image"

        public bool DeviceImageLoading
        {
            get { return (bool)GetValue(DeviceImageLoadingProperty); }
            set { SetValue(DeviceImageLoadingProperty, value); }
        }

        public static readonly DependencyProperty DeviceImageLoadingProperty =
            DependencyProperty.Register("DeviceImageLoading", typeof(bool), typeof(Header), new PropertyMetadata(false));


        Thread LoadDeviceImage_THREAD;

        public void LoadDeviceImage(string filename)
        {
            ManufacturerLogoLoading = true;

            if (LoadDeviceImage_THREAD != null) LoadDeviceImage_THREAD.Abort();

            LoadDeviceImage_THREAD = new Thread(new ParameterizedThreadStart(LoadDeviceImage_Worker));
            LoadDeviceImage_THREAD.Start(filename);
        }

        void LoadDeviceImage_Worker(object o)
        {
            if (o != null)
            {
                string filename = o.ToString();

                System.Drawing.Image img = Images.GetImage(filename);

                this.Dispatcher.BeginInvoke(new Action<System.Drawing.Image>(LoadDeviceImage_GUI), priority, new object[] { img });
            }
            else this.Dispatcher.BeginInvoke(new Action<System.Drawing.Image>(LoadDeviceImage_GUI), priority, new object[] { null });
        }

        void LoadDeviceImage_GUI(System.Drawing.Image img)
        {
            if (img != null)
            {
                System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(img);

                IntPtr bmpPt = bmp.GetHbitmap();
                BitmapSource bmpSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bmpPt, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

                bmpSource.Freeze();

                if (bmpSource.PixelWidth > bmpSource.PixelHeight)
                {
                    Device_Image = TH_WPF.Image_Functions.SetImageSize(bmpSource, 250);
                }
                else
                {
                    Device_Image = TH_WPF.Image_Functions.SetImageSize(bmpSource, 0, 250);
                }
            }
            else
            {
                Device_Image = null;
            }

            ManufacturerLogoLoading = false;
        }

        #endregion

        #endregion

        #region "IsSelected"

        public delegate void Clicked_Handler(int index);
        public event Clicked_Handler Clicked;

        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(Header), new PropertyMetadata(false));

        private void Control_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Clicked != null) Clicked(Index);
        }

        #endregion

    }

    public class DesignTime_Header : Header
    {
        public DesignTime_Header()
        {
            root.DataContext = this;

            Connected = true;
            Production = true;
            Idle = false;
            Alert = false;
            ScheduledDowntime = true;
            LastUpdatedTimestamp = DateTime.Now.ToString();
           
            var d = new Description_Settings();
            d.Description = "Device Description";
            d.Manufacturer = "Manufacturer";
            d.Model = "Model";
            d.Serial = "Serial";
            d.Device_ID = "01";

            DeviceDescription = d;
        }
    }
}