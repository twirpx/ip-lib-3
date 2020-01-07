using System;
using System.Net;
using System.Net.Sockets;
using IPLib3.Filtering;

namespace IPLib3 {
    public static class IPAddressConverter {
        
        public static UInt32 ToUInt32(this IPAddress ip) {
            byte[] bytes = ip.GetAddressBytes();
            return BitConverter.ToUInt32(bytes.Swap(), 0);
        }
        
        public static byte[] ToBytes(this IPAddress ip) {
            byte[] bytes = ip.GetAddressBytes();
            return bytes.Swap();
        }

        public static UInt128 ToUInt128(this IPAddress ip) {
            if (ip.AddressFamily == AddressFamily.InterNetwork) {
                ip = ip.MapToIPv6();
            } else if (ip.AddressFamily == AddressFamily.InterNetworkV6) {
                // OK
            } else {
                throw new IPFilterException("IPFilter does not support IPAddress.AddressFamily other than InterNetwork or InterNetworkV6");
            }
            
            byte[] bytes = ip.GetAddressBytes();
            
            return (UInt128)bytes.Swap();
        }

        public static IPAddress ToIPAddress(this UInt128 u) {
            byte[] bytes = u.GetBytes();
            
            IPAddress ip = new IPAddress(bytes.Swap());
            
            if (ip.IsIPv4MappedToIPv6) {
                return ip.MapToIPv4();
            } else {
                return ip;
            }
        }
        
        public static IPAddress ToIPAddress(this uint u) {
            byte[] bytes = BitConverter.GetBytes(u);
            return new IPAddress(bytes.Swap());
        }
        
        public static IPAddress ToIPAddress(this byte[] bytes) {
            return new IPAddress(bytes.Swap());
        }
        
    }
}