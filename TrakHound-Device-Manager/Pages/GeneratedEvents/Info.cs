﻿// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using TrakHound.Plugins.Server;

namespace TrakHound_Device_Manager.Pages.GeneratedEvents
{
    public class Info : IConfigurationInfo
    {
        public string Title { get { return "Generated Events"; } }

        private BitmapImage _image;
        public ImageSource Image
        {
            get
            {
                if (_image == null)
                {
                    _image = new BitmapImage(new Uri("pack://application:,,,/TrakHound-Device-Manager;component/Resources/Group_01.png"));
                    _image.Freeze();
                }

                return _image;
            }
        }

        public Type ConfigurationPageType { get { return typeof(Page); } }

    }
}
