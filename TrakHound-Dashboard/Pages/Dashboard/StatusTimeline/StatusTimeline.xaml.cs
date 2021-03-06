﻿using System;
using System.Collections.Generic;
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
using System.Collections.ObjectModel;
using TH_StatusTimeline.Controls;

using TrakHound.Configurations;

namespace TH_StatusTimeline
{
    /// <summary>
    /// Interaction logic for StatusTimeline.xaml
    /// </summary>
    public partial class StatusTimeline : UserControl
    {
        public StatusTimeline()
        {
            InitializeComponent();
            root.DataContext = this;
        }

        ObservableCollection<Row> _rows;
        public ObservableCollection<Row> Rows
        {
            get
            {
                if (_rows == null) _rows = new ObservableCollection<Row>();
                return _rows;
            }
            set
            {
                _rows = value;
            }
        }

        private Random rnd = new Random();

        private void AddRow(DeviceConfiguration config)
        {
            var row = new Row();
            row.Configuration = config;
            row.LastTimelineUpdate.Add(TimeSpan.FromSeconds(rnd.Next(5, 30)));
            Rows.Add(row);
        }

        private void AddRow(DeviceConfiguration config, int index)
        {
            var row = new Controls.Row();
            row.Configuration = config;
            row.LastTimelineUpdate.Add(TimeSpan.FromSeconds(rnd.Next(5, 30)));
            Rows.Insert(index, row);
        }

    }
}
