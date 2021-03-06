﻿// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Newtonsoft.Json;

namespace TrakHound.API
{
    public static partial class Data
    {
        public class TimersInfo
        {
            [JsonProperty("total")]
            public double Total { get; set; }

            // Device Status --------------------

            [JsonProperty("active")]
            public double Active { get; set; }

            [JsonProperty("idle")]
            public double Idle { get; set; }

            [JsonProperty("alert")]
            public double Alert { get; set; }

            // ----------------------------------

            // Production Status ----------------

            [JsonProperty("production")]
            public double Production { get; set; }

            [JsonProperty("setup")]
            public double Setup { get; set; }

            [JsonProperty("teardown")]
            public double Teardown { get; set; }

            [JsonProperty("maintenance")]
            public double Maintenance { get; set; }

            [JsonProperty("process_development")]
            public double ProcessDevelopment { get; set; }

            // ----------------------------------

            // Run Time -------------------------

            [JsonProperty("day_run")]
            public double DayRun { get; set; }

            [JsonProperty("day_operating")]
            public double DayOperating { get; set; }

            [JsonProperty("day_cutting")]
            public double DayCutting { get; set; }

            [JsonProperty("day_spindle")]
            public double DaySpindle { get; set; }


            [JsonProperty("total_run")]
            public double TotalRun { get; set; }

            [JsonProperty("total_operating")]
            public double TotalOperating { get; set; }

            [JsonProperty("total_cutting")]
            public double TotalCutting { get; set; }

            [JsonProperty("total_spindle")]
            public double TotalSpindle { get; set; }

            // ----------------------------------
        }
    }
}
