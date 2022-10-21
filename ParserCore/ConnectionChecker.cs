﻿using ParserCore.Helpers;
using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.InteropServices;
using System.Text;

namespace ParserCore
{
    public class ConnectionChecker
    {
        public int CheckInterval { get; set; }
        public List<string> Websites = new() { "www.google.com", "www.yandex.ru", "www.godaddy.com" };
        public ConnectionChecker(int interval)
        {
            CheckInterval = interval;
        }

        public async Task<bool> CheckIfConnected()
        {
            //TODO: Use win32 api
            var ping = new Ping();
            try
            {
                foreach (string website in Websites)
                {
                    var reply = await ping.SendPingAsync(website);
                    if (reply.Status == IPStatus.Success)
                    {
                        return true;
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
            return false;
        }
    }
}
