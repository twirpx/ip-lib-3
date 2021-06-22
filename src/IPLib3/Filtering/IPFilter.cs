using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace IPLib3.Filtering {
    public class IPFilter : IPFilterBase<string> {

        protected IPFilter(Node<string> root) : base(root) { }
        
        public void DumpValues(TextWriter writer) {
            DumpValues(Root, UInt128.Zero, LENGTH, writer);
        }

        private void DumpValues(Node<string> node, UInt128 start, UInt128 length, TextWriter writer) {
            if (node != null) {
                UInt128 l_start = start;
                UInt128 l_end = l_start + length - 1;

                if (node.LValue != null) {
                    if (node.LValue != "none") {
                        if (l_start == l_end) {
                            writer.Write("{0}", l_start.ToIPAddress());
                        } else {
                            writer.Write("{0} - {1}", l_start.ToIPAddress(), l_end.ToIPAddress());
                        }

                        writer.WriteLine(" = {0}", node.LValue);
                    }
                } else {
                    DumpValues(node.LPtr, l_start, length >> 1, writer);
                }

                UInt128 r_start = start + length;
                UInt128 r_end = r_start + length - 1;

                if (node.RValue != null) {
                    if (node.RValue != "none") {
                        if (r_start == r_end) {
                            writer.Write("{0}", r_start.ToIPAddress());
                        } else {
                            writer.Write("{0} - {1}", r_start.ToIPAddress(), r_end.ToIPAddress());
                        }

                        writer.WriteLine(" = {0}", node.RValue);
                    }
                } else {
                    DumpValues(node.RPtr, r_start, length >> 1, writer);
                }
            } else {
                throw new IPFilterException("Can't find node. IPFilter seems to be corrupted");
            }
        }
        
        
        public void DumpTree(TextWriter writer) {
            DumpTree(Root, UInt128.Zero, LENGTH, writer, 0);
        }

        private void DumpTree(Node<string> node, UInt128 start, UInt128 length, TextWriter writer, int level) {
            if (node != null) {
                UInt128 l_start = start;
                UInt128 l_end = l_start + length - 1;

                if (level > 0) {
                    writer.Write(new string(' ', level));
                }

                if (l_start == l_end) {
                    writer.Write("{0}", l_start.ToIPAddress());
                } else {
                    writer.Write("{0} - {1}", l_start.ToIPAddress(), l_end.ToIPAddress());
                }

                if (node.LValue != null) {
                    writer.WriteLine(" = {0}", node.LValue);
                } else {
                    writer.WriteLine();
                    DumpTree(node.LPtr, l_start, length >> 1, writer, level + 1);
                }

                UInt128 r_start = start + length;
                UInt128 r_end = r_start + length - 1;

                if (level > 0) {
                    writer.Write(new string(' ', level));
                }

                if (r_start == r_end) {
                    writer.Write("{0}", r_start.ToIPAddress());
                } else {
                    writer.Write("{0} - {1}", r_start.ToIPAddress(), r_end.ToIPAddress());
                }

                if (node.RValue != null) {
                    writer.WriteLine(" = {0}", node.RValue);
                } else {
                    writer.WriteLine();
                    DumpTree(node.RPtr, r_start, length >> 1, writer, level + 1);
                }
            } else {
                throw new IPFilterException("Can't find node. IPFilter seems to be corrupted");
            }
        }

        public void Optimize() {
            OptimizeNode(Root);
        }

        private string OptimizeNode(Node<string> node) {
            if (node != null) {
                if (node.LValue == null) {
                    string result = OptimizeNode(node.LPtr);
                    if (result != null) {
                        node.LValue = result;
                        node.LPtr = null;
                    }
                }
                if (node.RValue == null) {
                    string result = OptimizeNode(node.RPtr);
                    if (result != null) {
                        node.RValue = result;
                        node.RPtr = null;
                    }
                }
                if (node.LValue != null && node.RValue != null) {
                    if (node.LValue == node.RValue) {
                        return node.LValue;
                    } else {
                        return null;
                    }
                } else {
                    return null;
                }
            } else {
                throw new IPFilterException("Can't find node. IPFilter seems to be corrupted");
            }
        }

        public Dictionary<string, uint> EvaluateStats() {
            Dictionary<string, uint> stats = new Dictionary<string, uint>();
            EvaluateStats(Root, 2147483648u, stats);
            return stats;
        }

        private void EvaluateStats(Node<string> node, uint length, IDictionary<string, uint> stats) {
            if (node.LValue != null) {
                if (!stats.TryGetValue(node.LValue, out uint value)) {
                    value = 0;
                }
                stats[node.LValue] = value + length;
            } else {
                EvaluateStats(node.LPtr, length/2, stats);
            }
            if (node.RValue != null) {
                if (!stats.TryGetValue(node.RValue, out uint value)) {
                    value = 0;
                }
                stats[node.RValue] = value + length;
            } else {
                EvaluateStats(node.RPtr, length/2, stats);
            }
        }

        public void SaveTo(string file_name) {
            using (Stream stream = File.Open(file_name, FileMode.Create, FileAccess.Write, FileShare.Read)) {
                SaveTo(stream);
            }
        }

        public void SaveTo(Stream stream) {
            // IPF3
            stream.WriteByte(0x69);
            stream.WriteByte(0x70);
            stream.WriteByte(0x66);
            stream.WriteByte(0x33);
            WriteNode(Root, stream);
        }

        private static void WriteNode(Node<string> node, Stream stream) {
            if (node != null) {
                int flags = 0;

                byte[] lvalue_bytes;
                if (node.LValue != null) {
                    flags += 1 << 7;

                    lvalue_bytes = Encoding.UTF8.GetBytes(node.LValue);
                    if (lvalue_bytes.Length > 65536) {
                        throw new IPFilterException("Cannot save IPFilter values with more than 64KB length");
                    }
                    flags += (lvalue_bytes.Length > 256 ? 2 : 1) << 4;
                } else {
                    lvalue_bytes = null;
                }

                byte[] rvalue_bytes;
                if (node.RValue != null) {
                    flags += 1 << 3;

                    rvalue_bytes = Encoding.UTF8.GetBytes(node.RValue);
                    if (rvalue_bytes.Length > 65536) {
                        throw new IPFilterException("Cannot save IPFilter values with more than 64KB length");
                    }
                    flags += rvalue_bytes.Length > 256 ? 2 : 1;
                } else {
                    rvalue_bytes = null;
                }

                stream.WriteByte((byte)flags);

                if (lvalue_bytes != null) {
                    if (lvalue_bytes.Length < 256) {
                        stream.WriteByte((byte)lvalue_bytes.Length);
                    } else {
                        stream.WriteByte((byte)(lvalue_bytes.Length & 256));
                        stream.WriteByte((byte)((lvalue_bytes.Length >> 8) & 256));
                    }
                    stream.Write(lvalue_bytes, 0, lvalue_bytes.Length);
                } else {
                    WriteNode(node.LPtr, stream);
                }

                if (rvalue_bytes != null) {
                    if (rvalue_bytes.Length < 256) {
                        stream.WriteByte((byte)rvalue_bytes.Length);
                    } else {
                        stream.WriteByte((byte)(rvalue_bytes.Length & 256));
                        stream.WriteByte((byte)((rvalue_bytes.Length >> 8) & 256));
                    }
                    stream.Write(rvalue_bytes, 0, rvalue_bytes.Length);
                } else {
                    WriteNode(node.RPtr, stream);
                }
            } else {
                throw new IPFilterException("Can't find node. IPFilter seems to be corrupted");
            }
        }

        public static IPFilter CreateNew(string initial_value) {
            return new IPFilter(new Node<string> {
                LValue = initial_value,
                LPtr = null,
                RValue = initial_value,
                RPtr = null
            });
        }

        public static bool TryLoadFrom(string file_name, out IPFilter filter) {
            using (Stream stream = File.Open(file_name, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
                return TryLoadFrom(stream, out filter);
            }
        }

        public static bool TryLoadFrom(Stream stream, out IPFilter filter) {
            if (TryReadHeader(stream)) {
                if (TryLoadNode(stream, out Node<string> root)) {
                    filter = new IPFilter(root);
                    return true;
                }
            }

            filter = null;
            return false;
        }

        private static bool TryReadHeader(Stream stream) {
            int h1 = stream.ReadByte();
            if (h1 < 0) {
                return false;
            }

            int h2 = stream.ReadByte();
            if (h2 < 0) {
                return false;
            }

            int h3 = stream.ReadByte();
            if (h3 < 0) {
                return false;
            }

            int h4 = stream.ReadByte();
            if (h4 < 0) {
                return false;
            }

            // IPF3
            if (h1 != 0x69 || h2 != 0x70 || h3 != 0x66 || h4 != 0x33) {
                return false;
            }

            return true;
        }

        private static bool TryLoadNode(Stream stream, out Node<string> node) {
            int flags = stream.ReadByte();
            
            if (flags < 0) {
                node = null;
                return false;
            }
            
            node = new Node<string>();

            bool has_lvalue = (flags & 128) > 0;
            if (has_lvalue) {
                int lvalue_length_bytes = (flags >> 4) & 7;
                if (TryReadValue(stream, lvalue_length_bytes, out string value)) {
                    node.LValue = value;
                } else {
                    node = null;
                    return false;
                }
            } else {
                if (TryLoadNode(stream, out Node<string> ptr)) {
                    node.LPtr = ptr;
                } else {
                    node = null;
                    return false;
                }
            }

            bool has_rvalue = (flags & 8) > 0;
            if (has_rvalue) {
                int rvalue_length_bytes = flags & 7;
                if (TryReadValue(stream, rvalue_length_bytes, out string value)) {
                    node.RValue = value;
                } else {
                    node = null;
                    return false;
                }
            } else {
                if (TryLoadNode(stream, out Node<string> ptr)) {
                    node.RPtr = ptr;
                } else {
                    node = null;
                    return false;
                }
            }

            return true;
        }

        private static bool TryReadValue(Stream stream, int value_length_bytes, out string value) {
            int length;

            int i_0 = stream.ReadByte();
            if (i_0 < 0) {
                value = null;
                return false;
            }

            if (value_length_bytes > 1) {
                int i_1 = stream.ReadByte();
                if (i_1 < 0) {
                    value = null;
                    return false;
                }
                length = i_0 + (i_1 << 8);
            } else {
                length = i_0;
            }
            
            byte[] bytes = ArrayPool<byte>.Shared.Rent(length);
            try {
                int read = stream.Read(bytes, 0, length);
                if (read == length) {
                    value = String.Intern(Encoding.UTF8.GetString(bytes, 0, read));
                    return true;
                } else {
                    value = null;
                    return false;
                }
            } finally {
                ArrayPool<byte>.Shared.Return(bytes);
            }
        }

    }
}