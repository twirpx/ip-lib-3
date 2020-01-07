namespace IPLib3 {
    internal static class BytesHelper {

        public static byte[] Swap(this byte[] bytes) {
            byte[] result = new byte[bytes.Length];
            
            for (int i = 0; i < bytes.Length; i++) {
                result[i] = bytes[bytes.Length - 1 - i];
            }

            return result;
        }
        
    }
}