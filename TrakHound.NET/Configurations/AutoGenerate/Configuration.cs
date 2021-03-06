﻿// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.


using System.Data;

using TrakHound.Configurations;
using TrakHound.Tools;
using MTConnect.Application.Components;

namespace TrakHound.Configurations.AutoGenerate
{
    public static class Configuration
    {
        public class ProbeData
        {
            public string Address { get; set; }
            public string Port { get; set; }
            public Device Device { get; set; }
        }

        public static DeviceConfiguration Create(ProbeData probeData)
        {
            if (probeData != null && probeData.Device != null)
            {
                var dt = new DataTable();
                dt.Columns.Add("address");
                dt.Columns.Add("name");
                dt.Columns.Add("value");
                dt.Columns.Add("attributes");

                var items = probeData.Device.GetAllDataItems();

                SetIds(dt);
                SetEnabled(dt);
                Description.Add(dt, probeData.Device);
                Agent.Add(dt, probeData.Address, probeData.Port, probeData.Device.Name);
                //Databases.Add(dt);
                InstanceData.Add(dt, items);
                SnapshotData.Add(dt, items);
                GeneratedEvents.DeviceStatus.Add(dt, items);
                GeneratedEvents.ProductionStatus.Add(dt, items);
                GeneratedEvents.CycleExecution.Add(dt, items);
                GeneratedEvents.PartsCount.Add(dt, items);
                Shifts.Add(dt, items);
                Cycles.Add(dt, items);
                Parts.Add(dt);

                // Manufacturer Specific Processing
                Manufacturers.Okuma.Process(dt, probeData.Device);

                var xml = Converters.DeviceConfigurationConverter.TableToXML(dt);

                return DeviceConfiguration.Read(xml);
            }

            return null;
        }

        private static void SetIds(DataTable dt)
        {
            DataTable_Functions.UpdateTableValue(dt, "address", "/UniqueId", "value", DeviceConfiguration.GenerateUniqueID());
            DataTable_Functions.UpdateTableValue(dt, "address", "/tUpdateId", "value", DeviceConfiguration.GenerateUniqueID());
        }

        private static void SetEnabled(DataTable dt)
        {
            DataTable_Functions.UpdateTableValue(dt, "address", "/Enabled", "value", "True");
        }

    }
}
