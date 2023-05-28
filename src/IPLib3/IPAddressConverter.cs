using System.Buffers;
using IPLib3.Filtering;

namespace IPLib3;

public static class IPAddressConverter {

    public static UInt32 ToUInt32(this IPAddress ip) {
        byte[] bytes = ip.GetAddressBytes();
        Array.Reverse(bytes, 0, bytes.Length);
        return BitConverter.ToUInt32(bytes, 0);
    }

    private const int UINT128_LENGTH = 16;

    public static UInt128 ToUInt128(this IPAddress ip) {
        if (ip.AddressFamily == AddressFamily.InterNetwork) {
            ip = ip.MapToIPv6();
        } else if (ip.AddressFamily == AddressFamily.InterNetworkV6) {
            // OK
        } else {
            throw new IPFilterException("IPFilter does not support IPAddress.AddressFamily other than InterNetwork or InterNetworkV6");
        }

        byte[] bytes = ip.GetAddressBytes();
        
        UInt128 value = 0;
        
        for (int i = 0; i < UINT128_LENGTH; i++) {
            value = (value << 8) + bytes[i];
        }

        return value;
    }
    
    public static IPAddress ToIPAddress(this UInt128 u) {
        Span<byte> bytes = stackalloc byte[UINT128_LENGTH];
        
        for (int i = UINT128_LENGTH - 1; i >= 0; i--) {
            bytes[i] = (byte)(u & 0xFF);
            u >>= 8;
        }
            
        IPAddress ip = new(bytes);

        if (ip.IsIPv4MappedToIPv6) {
            return ip.MapToIPv4();
        } else {
            return ip;
        }
    }

    private const int UINT32_LENGTH = 4;

    public static IPAddress ToIPAddress(this UInt32 u) {
        Span<byte> bytes = stackalloc byte[UINT32_LENGTH];
        
        for (int i = UINT32_LENGTH - 1; i >= 0; i--) {
            bytes[i] = (byte)(u & 0xFF);
            u >>= 8;
        }
        
        IPAddress ip = new(bytes);

        return ip;
    }

}