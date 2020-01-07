using System;
using System.Net;

namespace IPLib3 {
    public static class IPHelper {

        public static string FormatIPs(IPAddress[] ips) {
            string[] strings = Array.ConvertAll(ips, ip => ip.ToString());

            return String.Join(" ", strings);
        }

        public static IPAddress[] ExcludePrivateAndLocals(IPAddress[] ips) {
            throw new NotImplementedException();
        }
        
    }
}