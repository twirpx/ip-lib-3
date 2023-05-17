namespace IPLib3; 

public static class UInt128Converter {
        
    public static UInt128 FromBytes(byte[] bytes) {
        if (bytes == null) throw new ArgumentNullException(nameof(bytes));
        if (bytes.Length != 16) throw new ArgumentOutOfRangeException(nameof(bytes));

        UInt128 value = 0;
        
        for (int i = 15; i >= 0; i--) {
            value = (value << 8) + bytes[i];
        }

        return value;
    }
        
}