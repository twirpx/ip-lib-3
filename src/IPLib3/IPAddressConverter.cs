using IPLib3.Filtering;

namespace IPLib3;

public static class IPAddressConverter {

    public static UInt32 ToUInt32(this IPAddress ip) {
        byte[] bytes = ip.GetAddressBytes();
        Array.Reverse(bytes, 0, bytes.Length);
        return BitConverter.ToUInt32(bytes, 0);
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
        Array.Reverse(bytes, 0, bytes.Length);
        return UInt128Converter.FromBytes(bytes);
    }

    public static IPAddress ToIPAddress(this UInt128 u) {
        byte[] bytes = UInt128Converter.GetBytes(u);
        Array.Reverse(bytes, 0, bytes.Length);
        IPAddress ip = new IPAddress(bytes);

        if (ip.IsIPv4MappedToIPv6) {
            return ip.MapToIPv4();
        } else {
            return ip;
        }
    }

    public static IPAddress ToIPAddress(this UInt32 u) {
        byte[] bytes = BitConverter.GetBytes(u);
        Array.Reverse(bytes, 0, bytes.Length);
        return new IPAddress(bytes);
    }

}