﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TrakHound_Dashboard.Pages.TableManager.Controls
{
    /// <summary>
    /// Interaction logic for DeviceButton.xaml
    /// </summary>
    public partial class DeviceButton : UserControl
    {
        public DeviceButton()
        {
            InitializeComponent();
            DataContext = this;
        }

        public object Parent;


        public string Description
        {
            get { return (string)GetValue(DescriptionProperty); }
            set { SetValue(DescriptionProperty, value); }
        }

        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register("Description", typeof(string), typeof(DeviceButton), new PropertyMetadata(null));


        public string Manufacturer
        {
            get { return (string)GetValue(ManufacturerProperty); }
            set { SetValue(ManufacturerProperty, value); }
        }

        public static readonly DependencyProperty ManufacturerProperty =
            DependencyProperty.Register("Manufacturer", typeof(string), typeof(DeviceButton), new PropertyMetadata(null));


        public string Model
        {
            get { return (string)GetValue(ModelProperty); }
            set { SetValue(ModelProperty, value); }
        }

        public static readonly DependencyProperty ModelProperty =
            DependencyProperty.Register("Model", typeof(string), typeof(DeviceButton), new PropertyMetadata(null));


        public string Serial
        {
            get { return (string)GetValue(SerialProperty); }
            set { SetValue(SerialProperty, value); }
        }

        public static readonly DependencyProperty SerialProperty =
            DependencyProperty.Register("Serial", typeof(string), typeof(DeviceButton), new PropertyMetadata(null));


        public string Id
        {
            get { return (string)GetValue(IdProperty); }
            set { SetValue(IdProperty, value); }
        }

        public static readonly DependencyProperty IdProperty =
            DependencyProperty.Register("Id", typeof(string), typeof(DeviceButton), new PropertyMetadata(null));


        public delegate void Clicked_Handler(DeviceButton bt);
        public event Clicked_Handler Clicked;

        private void Grid_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Clicked != null) Clicked(this);
        }
        
    }
}
