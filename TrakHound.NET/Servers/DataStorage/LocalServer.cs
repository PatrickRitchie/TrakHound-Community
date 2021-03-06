﻿// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Web;

using TrakHound.API;
using TrakHound.Logging;
using TrakHound.Tools.Web;

namespace TrakHound.Servers.DataStorage
{
    public class LocalStorageServer
    {
        /// <summary>
        /// API Server Port
        /// </summary>
        public const int PORT = 8472; // ASCII Dec for 'T' and 'H'

        private HttpListener listener;

        private System.Timers.Timer backupTimer;

        private const int BACKUP_INTERVAL = 300000; // 5 Minutes


        public LocalStorageServer()
        {
            var apiMonitor = new ApiConfiguration.Monitor();
            apiMonitor.ApiConfigurationChanged += ApiMonitor_ApiConfigurationChanged;
        }

        private void ApiMonitor_ApiConfigurationChanged(ApiConfiguration config)
        {
            if (config != null && config.DataHost != ApiConfiguration.LOCAL_API_HOST) Stop();
            else Start();
        }

        public void Start()
        {
            listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:" + PORT + "/api/");
            listener.Start();

            ThreadPool.QueueUserWorkItem((o) =>
            {
                Console.WriteLine("TrakHound Data Server Started...");
                try
                {
                    while (listener.IsListening)
                    {
                        ThreadPool.QueueUserWorkItem((c) =>
                        {
                            var ctx = c as HttpListenerContext;

                            try
                            {
                                string rstr = ProcessRequest(ctx);
                                byte[] buf = Encoding.UTF8.GetBytes(rstr);
                                ctx.Response.ContentLength64 = buf.Length;
                                ctx.Response.OutputStream.Write(buf, 0, buf.Length);
                            }
                            catch { } // suppress any exceptions
                            finally
                            {
                                // always close the stream
                                ctx.Response.OutputStream.Close();
                            }
                        }, listener.GetContext());
                    }
                }
                catch { } // suppress any exceptions
            });

            StartBackupTimer();
        }

        public void Stop()
        {
            listener.Stop();
            listener.Close();

            StopBackupTimer();
        }


        private string ProcessRequest(HttpListenerContext context)
        {
            string result = null;

            try
            {
                Console.WriteLine("Processing '" + context.Request.Url + "'..");

                string path = context.Request.Url.AbsolutePath;
                if (!string.IsNullOrEmpty(path) && path.Length > 1)
                {
                    // Remove beginning forward slash
                    path = path.Substring(1);

                    path = path.Substring("api/".Length);

                    // Split path by forward slashes
                    var paths = path.Split('/');
                    if (paths.Length > 1 && paths[0] != "/" && !string.IsNullOrEmpty(paths[0]))
                    {
                        switch (paths[0].ToLower())
                        {
                            case "data":

                                // Remove 'data' from path
                                path = path.Substring(paths[0].Length);

                                // Remove first forward slash
                                path = path.TrimStart('/');

                                // Process the Data Request and return response
                                if (context.Request.HttpMethod == "GET")
                                {
                                    paths = path.Split('/');
                                    if (paths.Length > 0)
                                    {
                                        switch (paths[0].ToLower())
                                        {
                                            case "get": result = Data.Get(context.Request.Url.ToString()); break;
                                        }
                                    }
                                }
                                else if (context.Request.HttpMethod == "POST")
                                {
                                    paths = path.Split('/');
                                    if (paths.Length > 0)
                                    {
                                        switch (paths[0].ToLower())
                                        {
                                            case "update": result = Data.Update(context); break;
                                        }
                                    }
                                }
                                else result = "Incorrect REQUEST METHOD";

                                break;

                            case "config":

                                var apiConfig = ApiConfiguration.Read();
                                if (apiConfig != null)
                                {
                                    string json = JSON.FromObject(apiConfig);
                                    if (json != null)
                                    {
                                        result = json;
                                    }
                                    else
                                    {
                                        result = "Error Reading API Configuration";
                                    }
                                }

                                break;
                        }
                    }
                }
            }
            catch (Exception ex) { Logger.Log("Error Processing Local Server Request :: " + ex.Message, LogLineType.Error); }

            return result;
        }


        private void StartBackupTimer()
        {
            try
            {
                var backupInfos = Backup.Load();
                if (backupInfos != null && backupInfos.Count > 0)
                {
                    Data.DeviceInfos.AddRange(backupInfos);
                }

                if (backupTimer != null) backupTimer.Stop();
                else
                {
                    backupTimer = new System.Timers.Timer();
                    backupTimer.Interval = BACKUP_INTERVAL;
                    backupTimer.Elapsed += BackupTimer_Elapsed;
                }

                backupTimer.Start();
            }
            catch (Exception ex) { Logger.Log("Error Starting Local Server Backup Timer :: " + ex.Message, LogLineType.Error); }
        }

        private void BackupTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Backup.Create(Data.DeviceInfos.ToList());
        }

        private void StopBackupTimer()
        {
            if (backupTimer != null) backupTimer.Stop();
        }


        private static class Data
        {
            public static List<API.Data.DeviceInfo> DeviceInfos = new List<API.Data.DeviceInfo>();

            public static string Get(string url)
            {
                string response = null;

                try
                {
                    var uri = new Uri(url);

                    // Get Devices Parameter
                    string json = HttpUtility.ParseQueryString(uri.Query).Get("devices");
                    if (!string.IsNullOrEmpty(json))
                    {
                        var devices = JSON.ToType<List<string>>(json);
                        if (devices != null)
                        {
                            var deviceInfos = new List<API.Data.DeviceInfo>();

                            foreach (var device in devices)
                            {
                                var deviceInfo = DeviceInfos.Find(o => o.UniqueId == device);
                                if (deviceInfo != null) deviceInfos.Add(deviceInfo);
                            }

                            if (deviceInfos.Count > 0) response = JSON.FromList<API.Data.DeviceInfo>(deviceInfos);
                        }
                    }
                    else
                    {
                        response = JSON.FromList<API.Data.DeviceInfo>(DeviceInfos);
                    }
                }
                catch (Exception ex) { Logger.Log("Error Getting Local Server Data :: " + ex.Message, LogLineType.Error); }

                return response;
            }

            public static string Update(HttpListenerContext context)
            {
                string response = null;

                try
                {
                    using (var reader = new StreamReader(context.Request.InputStream))
                    {
                        var body = reader.ReadToEnd();

                        string json = HTTP.GetPostValue(body, "devices");
                        if (!string.IsNullOrEmpty(json))
                        {
                            var devices = JSON.ToType<List<API.Data.DeviceInfo>>(json);
                            if (devices != null && devices.Count > 0)
                            {
                                foreach (var device in devices)
                                {
                                    bool newInfo = false;

                                    int i = DeviceInfos.FindIndex(o => o.UniqueId == device.UniqueId);
                                    if (i < 0)
                                    {
                                        DeviceInfos.Add(device);
                                        i = DeviceInfos.FindIndex(o => o.UniqueId == device.UniqueId);
                                        newInfo = true;
                                    }

                                    var info = DeviceInfos[i];

                                    info.Status = device.Status;
                                    info.Controller = device.Controller;
                                    info.Timers = device.Timers;

                                    // Get HourInfos for current day
                                    //info.Hours = info.Hours.FindAll(o => o.Date == DateTime.UtcNow.ToString(API.Data.HourInfo.DateFormat));
                                    info.Hours = info.Hours.FindAll(o => TestHourDate(o));

                                    // Add new HourInfo objects and then combine them into the current list
                                    info.Hours.AddRange(device.Hours);
                                    info.Hours = API.Data.HourInfo.CombineHours(info.Hours);

                                    // Set DeviceInfo's OeeInfo to new values
                                    info.Oee = API.Data.HourInfo.GetOeeInfo(info.Hours);

                                    // Set DeviceInfo's TimersInfo to new values
                                    info.Timers.Total = info.Hours.Select(o => o.TotalTime).Sum();

                                    info.Timers.Active = info.Hours.Select(o => o.Active).Sum();
                                    info.Timers.Idle = info.Hours.Select(o => o.Idle).Sum();
                                    info.Timers.Alert = info.Hours.Select(o => o.Alert).Sum();

                                    info.Timers.Production = info.Hours.Select(o => o.Production).Sum();
                                    info.Timers.Setup = info.Hours.Select(o => o.Setup).Sum();
                                    info.Timers.Teardown = info.Hours.Select(o => o.Teardown).Sum();
                                    info.Timers.Maintenance = info.Hours.Select(o => o.Maintenance).Sum();
                                    info.Timers.ProcessDevelopment = info.Hours.Select(o => o.ProcessDevelopment).Sum();

                                    // Update Part Count
                                    for (var x = 0; x < info.Hours.Count; x++)
                                    {
                                        if (newInfo)
                                        {
                                            if (x < info.Hours.Count - 1 || info.Hours[x].TotalPieces == 0) info.Status.PartCount += info.Hours[x].TotalPieces;
                                        }
                                        else
                                        {
                                            info.Status.PartCount += info.Hours[x].TotalPieces;
                                        }
                                    }
                                    //info.Status.PartCount = info.Hours.Select(o => o.TotalPieces).Sum();
                                }

                                response = "Devices Updated Successfully";
                            }
                        }
                    }
                }
                catch (Exception ex) { Logger.Log("Error Updating Local Server Data :: " + ex.Message, LogLineType.Error); }

                return response;
            }

            private static bool TestHourDate(API.Data.HourInfo hourInfo)
            {
                // Probably a more elegant way of getting the Time Zone Offset could be done here
                int timeZoneOffset = (DateTime.UtcNow - DateTime.Now).Hours;

                string currentLocalDay = DateTime.Now.ToString(API.Data.HourInfo.DateFormat);
                string currentUtcDay = DateTime.UtcNow.ToString(API.Data.HourInfo.DateFormat);

                if (currentLocalDay != currentUtcDay)
                {
                    return hourInfo.Date == currentUtcDay || (hourInfo.Date == currentLocalDay && hourInfo.Hour > 24 - timeZoneOffset);
                }
                else return hourInfo.Date == currentUtcDay;
            }
        }

    }
}
