using System;

namespace IPLib3 {
    public struct UInt128 : IEquatable<UInt128>, IComparable, IComparable<UInt128> {
        
        private ulong m_0;

        public ulong UL0 => m_0;
        
        public uint UI0 => (uint)(m_0 & 0xFFFFFFFF);
        
        public uint UI1 => (uint)((m_0 >> 32) & 0xFFFFFFFF);

        private ulong m_1;

        public ulong UL1 => m_1;
        
        public uint UI2 => (uint)(m_1 & 0xFFFFFFFF);
        
        public uint UI3 => (uint)((m_1 >> 32) & 0xFFFFFFFF);

        public override bool Equals(object obj) {
            return obj is UInt128 u && Equals(u);
        }

        // ReSharper disable NonReadonlyMemberInGetHashCode
        public override int GetHashCode() {
            return m_0.GetHashCode() ^ m_1.GetHashCode();
        }
        // ReSharper restore NonReadonlyMemberInGetHashCode

        public static void Create(out UInt128 c, long a)  {
            c.m_0 = (ulong)a;
            c.m_1 = a < 0 ? ulong.MaxValue : 0;
        }
    
        public static explicit operator UInt128(int a)  {
            Create(out UInt128 c, a);
            return c;
        } 
    
        public static explicit operator UInt128(byte[] a)  {
            Create(out UInt128 c, a);
            return c;
        } 
        
        public static bool operator ==(UInt128 a, UInt128 b) => a.Equals(b);
        
        public static bool operator !=(UInt128 a, UInt128 b) => !a.Equals(b);

        public bool Equals(UInt128 other) => m_0 == other.m_0 && m_1 == other.m_1;

        public static bool operator <(UInt128 a, UInt128 b) => LessThan(ref a, ref b);

        public static bool operator <=(UInt128 a, UInt128 b) => !LessThan(ref b, ref a);

        public static bool operator >(UInt128 a, UInt128 b) => LessThan(ref b, ref a);

        public static bool operator >=(UInt128 a, UInt128 b) => !LessThan(ref a, ref b);

        private static bool LessThan(ref UInt128 a, ref UInt128 b) => a.m_1 != b.m_1 ? a.m_1 < b.m_1 : a.m_0 < b.m_0;

        public static bool operator <(UInt128 a, ulong b) => LessThan(ref a, b);
        
        public static bool operator <=(UInt128 a, ulong b) => !LessThan(b, ref a);
        
        public static bool operator >(UInt128 a, ulong b) => LessThan(b, ref a);
        
        public static bool operator >=(UInt128 a, ulong b) => !LessThan(ref a, b);

        private static bool LessThan(ref UInt128 a, ulong b) => a.m_1 == 0 && a.m_0 < b;
        
        private static bool LessThan(ulong a, ref UInt128 b) => b.m_1 != 0 || a < b.m_0;

        public static UInt128 Min(UInt128 a, UInt128 b) => LessThan(ref a, ref b) ? a : b;

        public static UInt128 Max(UInt128 a, UInt128 b) => LessThan(ref b, ref a) ? a : b;       
        
        public static UInt128 operator ~(UInt128 a) {
            Not(out UInt128 c, ref a);
            return c;
        }
        
        public static void Not(out UInt128 c, ref UInt128 a)  {
            c.m_0 = ~a.m_0;
            c.m_1 = ~a.m_1;
        }
        
        public static UInt128 operator &(UInt128 a, UInt128 b)  {
            And(out UInt128 c, ref a, ref b);
            return c;
        }

        public static void And(out UInt128 c, ref UInt128 a, ref UInt128 b) {
            c.m_0 = a.m_0 & b.m_0;
            c.m_1 = a.m_1 & b.m_1;
        }        
        
        public static UInt128 operator +(UInt128 a, UInt128 b) {
            Add(out UInt128 c, ref a, ref b);
            return c;
        }

        public static void Add(out UInt128 c, ref UInt128 a, ref UInt128 b)  {
            c.m_0 = a.m_0 + b.m_0;
            if (c.m_0 < a.m_0 && c.m_0 < b.m_0) {
                c.m_1 = a.m_1 + b.m_1 + 1;
            } else {
                c.m_1 = a.m_1 + b.m_1;
            }
        }

        public static UInt128 operator +(UInt128 a, ulong b) {
            Add(out UInt128 c, ref a, b);
            return c;
        }

        public static void Add(out UInt128 c, ref UInt128 a, ulong b)  {
            c.m_0 = a.m_0 + b;
            if (c.m_0 < a.m_0 && c.m_0 < b) {
                c.m_1 = a.m_1 + 1;
            } else {
                c.m_1 = a.m_1;    
            }
        }

        public static UInt128 operator -(UInt128 a, UInt128 b) {
            Subtract(out UInt128 c, ref a, ref b);
            return c;
        }

        public static void Subtract(out UInt128 c, ref UInt128 a, ref UInt128 b)  {
            c.m_0 = a.m_0 - b.m_0;
            if (a.m_0 < b.m_0) {
                c.m_1 = a.m_1 - b.m_1 - 1;
            } else {
                c.m_1 = a.m_1 - b.m_1;
            }
        }
        
        public static UInt128 operator -(UInt128 a, ulong b) {
            Subtract(out UInt128 c, ref a, b);
            return c;
        }
        public static void Subtract(out UInt128 c, ref UInt128 a, ulong b)  {
            c.m_0 = a.m_0 - b;
            if (a.m_0 < b) {
                c.m_1 = a.m_1 - 1;
            } else {
                c.m_1 = a.m_1;    
            }
        }

        public static UInt128 operator <<(UInt128 a, int b) {
            LeftShift(out UInt128 c, ref a, b);
            return c;
        }

        public static void LeftShift(out UInt128 c, ref UInt128 a, int b)  {
            if (b == 0) {
                c.m_0 = a.m_0;
                c.m_1 = a.m_1;
            } else if (b < 64) {
                c.m_1 = a.m_1 << b | a.m_0 >> (b - 64);
                c.m_0 = a.m_0 << b;
            } else if (b == 64) {
                c.m_0 = 0;
                c.m_1 = a.m_0;
            } else {
                c.m_0 = 0;
                c.m_1 = a.m_0 << (b - 64);
            }
        }

        public static UInt128 operator >>(UInt128 a, int b) {
            RightShift(out UInt128 c, ref a, b);
            return c;
        }

        public static void RightShift(out UInt128 c, ref UInt128 a, int b) {
            if (b == 0) {
                c.m_0 = a.m_0;
                c.m_1 = a.m_1;
            } else if (b < 64) {
                c.m_0 = a.m_0 >> b | a.m_1 << (64 - b);
                c.m_1 = a.m_1 >> b;
            } else if (b == 64) {
                c.m_0 = a.m_1;
                c.m_1 = 0;
            } else {
                c.m_0 = a.m_1 >> (b - 64);
                c.m_1 = 0;
            }
        }
        
        private static readonly UInt128 ZERO = (UInt128)0;

        private static readonly UInt128 MAX_VALUE = ~ZERO;

        public static UInt128 MinValue => ZERO;
        
        public static UInt128 MaxValue => MAX_VALUE;

        public static UInt128 Zero => ZERO; 

        public static void Create(out UInt128 c, byte[] b) {
            if (b == null) throw new ArgumentNullException(nameof(b));
            if (b.Length != 16) throw new ArgumentOutOfRangeException(nameof(b));

            c.m_0 = 0; 
            for (int i = 7; i >= 0; i--) {
                c.m_0 = (c.m_0 << 8) + b[i];
            }
            c.m_1 = 0;
            for (int i = 15; i >= 8; i--) {
                c.m_1 = (c.m_1 << 8) + b[i];
            }
        }

        public byte[] GetBytes() {
            byte[] bytes = new byte[16];

            ulong u = m_0;
            for (int i = 0; i <= 7; i++) {
                bytes[i] = (byte)(u & 0xFF);
                u = u >> 8;
            }
            
            u = m_1;
            for (int i = 8; i <= 15; i++) {
                bytes[i] = (byte)(u & 0xFF);
                u = u >> 8;
            }

            return bytes;
        }

        public int CompareTo(UInt128 other) => m_1 != other.m_1 ? m_1.CompareTo(other.m_1) : m_0.CompareTo(other.m_0);

        public int CompareTo(object obj) {
            if (obj == null) {
                return 1;
            }

            if (!(obj is UInt128)) {
                throw new ArgumentException();
            }

            return CompareTo((UInt128)obj);
        }
        
    }
}