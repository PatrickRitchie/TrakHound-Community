﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Net.NetworkInformation;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Net.Sockets;

namespace TrakHound.Tools
{
    public static class Network_Functions
    {
        public static IPAddress GetHostIP()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip;
                }
            }

            return null;
        }

        public static IPAddress[] GetAddressList(IPAddress ipAddress)
        {
            var result = new List<IPAddress>();

            byte[] ipBytes = ipAddress.GetAddressBytes();

            var b = new byte[4];
            b[0] = ipBytes[0];
            b[1] = ipBytes[1];
            b[2] = ipBytes[2];

            for (var x = 0; x <= 255; x++)
            {
                b[3] = Convert.ToByte(x);

                var ip = new IPAddress(b);
                result.Add(ip);
            }

            return result.ToArray();
        }

        public static bool IsPortOpen(string host, int port, TimeSpan timeout)
        {
            try
            {
                using (var client = new TcpClient())
                {
                    var result = client.BeginConnect(host, port, null, null);
                    var success = result.AsyncWaitHandle.WaitOne(timeout);
                    if (!success)
                    {
                        return false;
                    }

                    client.EndConnect(result);
                }

            }
            catch
            {
                return false;
            }
            return true;
        }

        //public static IPAddress[] GetAddressList(IPAddress ipAddress)
        //{
        //    var result = new IPAddress[256];

        //    byte[] ipBytes = ipAddress.GetAddressBytes();

        //    var b = new byte[4];
        //    b[0] = ipBytes[0];
        //    b[1] = ipBytes[1];
        //    b[2] = ipBytes[2];

        //    for (var x = 0; x <= 255; x++)
        //    {
        //        b[3] = Convert.ToByte(x);

        //        var ip = new IPAddress(b);
        //        result[x] = ip;
        //    }

        //    return result;
        //}

        public class PingNodes
        {
            private int returnedIndexes = 0;
            private int expectedIndexes = 0;

            //private Ping ping;
            private int timeout = 2000;

            public delegate void PingSuccessful_Handler(PingReply reply);
            public event PingSuccessful_Handler PingSuccessful;

            public delegate void Finished_Handler();
            public event Finished_Handler Finished;

            public PingNodes()
            {

            }

            public PingNodes(int _timeout)
            {
                timeout = _timeout;
            }

            public void Start()
            {
                returnedIndexes = 0;
                expectedIndexes = 0;

                var ip = GetHostIP();
                if (ip != null)
                {
                    var list = GetAddressList(ip);
                    if (list.Length > 0)
                    {
                        expectedIndexes = list.Length;

                        for (var x = 0; x <= list.Length - 1; x++)
                        {
                            int index = x;
                            StartPing(list[x], index);
                        }
                    }
                }
            }

            public void Cancel()
            {
                foreach (var request in queuedPings) request.SendAsyncCancel();
            }

            private static IPAddress GetSubnetMask(IPAddress ip)
            {
                foreach (NetworkInterface adapter in NetworkInterface.GetAllNetworkInterfaces())
                {
                    foreach (UnicastIPAddressInformation unicastIPAddressInformation in adapter.GetIPProperties().UnicastAddresses)
                    {
                        if (unicastIPAddressInformation.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            if (ip.Equals(unicastIPAddressInformation.Address))
                            {
                                return unicastIPAddressInformation.IPv4Mask;
                            }
                        }
                    }
                }

                return null;
            }

            private static byte[] GetSubnetBytes(IPAddress ip, IPAddress subnetMask)
            {
                var ibytes = ip.GetAddressBytes();
                var sbytes = subnetMask.GetAddressBytes();

                var b = new List<byte>();

                for (var x = 0; x <= sbytes.Length - 1; x++)
                {
                    if (sbytes[x] == Convert.ToByte(0)) return b.ToArray();
                    else b.Add(ibytes[x]);
                }

                return null;
            }

            public List<Ping> queuedPings = new List<Ping>();

            private void StartPing(IPAddress ipAddress, int index)
            {
                var ping = new Ping();
                ping.PingCompleted += Ping_PingCompleted;
                queuedPings.Add(ping);
                ping.SendAsync(ipAddress, timeout, index);
            }

            private void Ping_PingCompleted(object sender, PingCompletedEventArgs e)
            {
                if (!e.Cancelled)
                {
                    var status = e.Reply.Status;
                    var ip = e.Reply.Address;
                    var index = e.UserState;

                    if (status == IPStatus.Success)
                    {
                        if (PingSuccessful != null) PingSuccessful(e.Reply);
                    }

                    returnedIndexes += 1;
                    if (returnedIndexes >= expectedIndexes) if (Finished != null) Finished();
                }
            }
        }

    }
}
