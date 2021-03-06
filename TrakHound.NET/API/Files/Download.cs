﻿// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Drawing;
using System.IO;
using System.Linq;

using TrakHound.API.Users;
using TrakHound.Logging;
using TrakHound.Tools.Web;

namespace TrakHound.API
{
    public static partial class Files
    {

        public static bool Download(UserConfiguration userConfig, string fileId, string destinationPath)
        {
            bool result = false;

            if (userConfig != null && !string.IsNullOrEmpty(destinationPath))
            {
                if (!File.Exists(destinationPath))
                {
                    Uri apiHost = ApiConfiguration.AuthenticationApiHost;

                    string url = new Uri(apiHost, "files/download/index.php").ToString();

                    string format = "{0}?token={1}&sender_id={2}&file_id={3}";

                    string token = userConfig.SessionToken;
                    string senderId = UserManagement.SenderId.Get();

                    url = string.Format(format, url, token, senderId, fileId);

                    var response = HTTP.GET(url, true);
                    if (response != null && response.Body != null && response.Headers != null)
                    {
                        string filename = Path.GetFileName(destinationPath);
                        string dir = destinationPath;

                        // Make sure destination directory exists
                        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

                        if (string.IsNullOrEmpty(filename))
                        {
                            // Get filename from HTTP Header
                            var header = response.Headers.ToList().Find(x => x.Key == "Content-Disposition");
                            if (header != null && header.Value != null)
                            {
                                try
                                {
                                    if (header.Value.Contains("filename"))
                                    {
                                        string s = header.Value;
                                        int i = s.IndexOf("filename");
                                        if (i >= 0) i = s.IndexOf("=", i);
                                        if (i >= 0) i = s.IndexOf("\"", i);
                                        if (i >= 0)
                                        {
                                            i++;
                                            int x = s.IndexOf("\"", i);
                                            if (x >= 0)
                                            {
                                                filename = s.Substring(i, x - i);
                                            }
                                        }
                                    }
                                } catch (Exception ex) { Logger.Log("Error Reading Filename from Download Response Header"); }
                            }
                        }

                        if (filename != null && dir != null)
                        {
                            string dest = Path.Combine(dir, filename);

                            try
                            {
                                using (var fileStream = new FileStream(dest, FileMode.CreateNew, FileAccess.ReadWrite))
                                using (var memStream = new MemoryStream())
                                {
                                    memStream.Write(response.Body, 0, response.Body.Length);
                                    memStream.WriteTo(fileStream);
                                }

                                Logger.Log("Download File Successful", LogLineType.Notification);
                                result = true;
                            }
                            catch (Exception ex) { Logger.Log("Download File Error : Exception : " + ex.Message); }
                        }
                    }
                }
                else Logger.Log("Download File Failed : File Already Exists @ " + destinationPath, LogLineType.Error);
            }

            return result;
        }

        public static Image DownloadImage(UserConfiguration userConfig, string fileId)
        {
            Image result = null;

            if (userConfig != null)
            {
                Uri apiHost = ApiConfiguration.AuthenticationApiHost;

                string url = new Uri(apiHost, "files/download/index.php").ToString();

                string format = "{0}?token={1}&sender_id={2}&file_id={3}";

                string token = userConfig.SessionToken;
                string senderId = UserManagement.SenderId.Get();

                url = string.Format(format, url, token, senderId, fileId);

                var response = HTTP.GET(url, true);
                if (response != null && response.Body != null && response.Headers != null)
                {
                    bool success = false;

                    // Takes forever to process an image
                    if (response.Body.Length < 500)
                    {
                        success = ApiError.ProcessResponse(response, "Download File");
                    }
                    else success = true;
                    
                    if (success)
                    {
                        try
                        {
                            using (var memStream = new MemoryStream())
                            {
                                memStream.Write(response.Body, 0, response.Body.Length);
                                result = Image.FromStream(memStream);
                                Logger.Log("Download File Successful", LogLineType.Notification);
                            }
                        }
                        catch (Exception ex) { Logger.Log("Response Not an Image : Exception : " + ex.Message); }
                    }
                }
            }

            return result;
        }

    }
}
