﻿// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Windows.Controls;
using System.Collections.ObjectModel;
using TrakHound_Dashboard.Pages.Dashboard.OeeHourTimeline.Controls;

using TrakHound.Configurations;

namespace TrakHound_Dashboard.Pages.Dashboard.OeeHourTimeline
{
    /// <summary>
    /// Interaction logic for StatusTimeline.xaml
    /// </summary>
    public partial class Page : UserControl
    {
        public Page()
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

        private void AddRow(DeviceConfiguration config)
        {
            var row = new Row();
            row.Configuration = config;
            Rows.Add(row);
        }

        private void AddRow(DeviceConfiguration config, int index)
        {
            var row = new Controls.Row();
            row.Configuration = config;
            Rows.Insert(index, row);
        }

    }
}
