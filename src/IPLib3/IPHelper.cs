using System;
using System.Net;
using System.Net.Sockets;

namespace IPLib3 {
    public static class IPHelper {

        public static string FormatIPs(IPAddress[] ips) {
            string[] strings = Array.ConvertAll(ips, ip => ip.ToString());

            return String.Join(" ", strings);
        }

        public static IPAddress[] ExcludePrivateAndLocals(IPAddress[] ips) {
            return Array.FindAll(ips, ip => !IsPrivateOrLocal(ip));
        }

        private static bool IsPrivateOrLocal(IPAddress ip) {
            if (ip.IsIPv4MappedToIPv6) {
                ip = ip.MapToIPv4();
            }

            if (ip.AddressFamily == AddressFamily.InterNetwork) {
                uint u = ip.ToUInt32();
                if (u >= 167772160 && u <= 184549375) {
                    // 10.0.0.0 - 10.255.255.255
                    return true;
                } else if (u >= 2886729728 && u <= 2887778303) {
                    // 172.16.0.0 - 172.31.255.255
                    return true;
                } else if (u >= 3232235520 && u <= 3232301055) {
                    // 192.168.0.0 - 192.168.255.255
                    return true;
                } else if (u >= 2851995648 && u <= 2852061183) {
                    // 169.254.0.0 - 169.254.255.255
                    return true;
                } else if (u >= 2130706432 && u <= 2147483647) {
                    // 127.0.0.0 - 127.255.255.255
                    return true;
                } else {
                    return false;
                }
            } else if (ip.AddressFamily == AddressFamily.InterNetworkV6) {
                if (IPAddress.IsLoopback(ip)) {
                    return true;
                }
                if (ip.IsIPv6LinkLocal || ip.IsIPv6SiteLocal) {
                    return true;
                }
            }

            return false;
        }
        
    }
}