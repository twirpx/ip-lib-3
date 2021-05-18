using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace IPLib3.Filtering {
    public class IPFilter {

        private IPFilter() { }

        private class Node {

            public string LValue;

            public Node LPtr;

            public string RValue;

            public Node RPtr;

        }

        private Node m_Root;
        
        public string GetValue(IPAddress ip) => GetValue(ip.ToUInt128());
        
        private static readonly UInt128 LENGTH = (UInt128.MaxValue >> 1) + 1;

        private string GetValue(UInt128 u) {
            Node node = m_Root;

            UInt128 half_position = LENGTH;
            UInt128 half_length = LENGTH;

            check_node:
            if (u < half_position) {
                if (node.LValue != null) {
                    return node.LValue;
                } else if (half_length > 1) {
                    node = node.LPtr;
                    half_length = half_length >> 1;
                    half_position -= half_length;
                    goto check_node;
                } else {
                    throw new IPFilterException("Can't check range. IPFilter seems to be corrupted");
                }
            } else {
                if (node.RValue != null) {
                    return node.RValue;
                } else if (half_length > 1) {
                    node = node.RPtr;
                    half_length = half_length >> 1;
                    half_position += half_length;
                    goto check_node;
                } else {
                    throw new IPFilterException("Can't check range. IPFilter seems to be corrupted");
                }
            }
        }

        public void SetValue(IPAddress ip, string value) {
            SetValue(ip.ToUInt128(), value);
        }

        private void SetValue(UInt128 u, string value) {
            SetValue(m_Root, UInt128.Zero, LENGTH, u, u, value);
        }

        public void SetValue(IPAddress ip_from, IPAddress ip_to, string value) {
            SetValue(ip_from.ToUInt128(), ip_to.ToUInt128(), value);
        }

        private void SetValue(UInt128 u_from, UInt128 u_to, string value) {
            SetValue(m_Root, UInt128.Zero, LENGTH, u_from, u_to, value);
        }

        private void SetValue(Node node, UInt128 start, UInt128 length, UInt128 ip_from, UInt128 ip_to, string value) {
            if (ip_from > ip_to) throw new ArgumentException("Invalid range", nameof(ip_from));

            if (node != null) {
                UInt128 l_start = start;
                UInt128 l_end = l_start + length - 1;

                if (l_start <= ip_from && l_end >= ip_from) {
                    if (l_start == ip_from && l_end <= ip_to) {
                        node.LValue = value;
                        node.LPtr = null;
                    } else {
                        if (node.LValue != null) {
                            node.LPtr = new Node {
                                LValue = node.LValue,
                                LPtr = null,
                                RValue = node.LValue,
                                RPtr = null
                            };
                            node.LValue = null;
                        }
                        SetValue(node.LPtr, l_start, length >> 1, ip_from, UInt128.Min(l_end, ip_to), value);
                    }
                }

                UInt128 r_start = start + length;
                UInt128 r_end = r_start + length - 1;

                if (r_start <= ip_to && r_end >= ip_to) {
                    if (r_start >= ip_from && r_end == ip_to) {
                        node.RValue = value;
                        node.RPtr = null;
                    } else {
                        if (node.RValue != null) {
                            node.RPtr = new Node {
                                LValue = node.RValue,
                                LPtr = null,
                                RValue = node.RValue,
                                RPtr = null
                            };
                            node.RValue = null;
                        }
                        SetValue(node.RPtr, r_start, length >> 1, UInt128.Max(r_start, ip_from), ip_to, value);
                    }
                }
            } else {
                throw new IPFilterException("Can't find node. IPFilter seems to be corrupted");
            }
        }
        
        
        public void DumpValues(TextWriter writer) {
            DumpValues(m_Root, UInt128.Zero, LENGTH, writer);
        }

        private void DumpValues(Node node, UInt128 start, UInt128 length, TextWriter writer) {
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
            DumpTree(m_Root, UInt128.Zero, LENGTH, writer, 0);
        }

        private void DumpTree(Node node, UInt128 start, UInt128 length, TextWriter writer, int level) {
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
            OptimizeNode(m_Root);
        }

        private string OptimizeNode(Node node) {
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
            EvaluateStats(m_Root, 2147483648u, stats);
            return stats;
        }

        private void EvaluateStats(Node node, uint length, IDictionary<string, uint> stats) {
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
            SaveNode(m_Root, stream);
        }

        private void SaveNode(Node node, Stream stream) {
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
                    SaveNode(node.LPtr, stream);
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
                    SaveNode(node.RPtr, stream);
                }
            } else {
                throw new IPFilterException("Can't find node. IPFilter seems to be corrupted");
            }
        }


        public static IPFilter CreateNew(string initial_value) {
            return new IPFilter {
                m_Root = new Node {
                    LValue = initial_value,
                    LPtr = null,
                    RValue = initial_value,
                    RPtr = null
                }
            };
        }

        public static bool TryLoadFrom(string file_name, out IPFilter filter) {
            using (Stream stream = File.Open(file_name, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
                return TryLoadFrom(stream, out filter);
            }
        }

        public static bool TryLoadFrom(Stream stream, out IPFilter filter) {
            int h1 = stream.ReadByte();
            if (h1 < 0) {
                filter = null;
                return false;
            }
            int h2 = stream.ReadByte();
            if (h2 < 0) {
                filter = null;
                return false;
            }
            int h3 = stream.ReadByte();
            if (h3 < 0) {
                filter = null;
                return false;
            }
            int h4 = stream.ReadByte();
            if (h4 < 0) {
                filter = null;
                return false;
            }
            // IPF3
            if (h1 != 0x69 || h2 != 0x70 || h3 != 0x66 || h4 != 0x33) {
                filter = null;
                return false;
            }

            if (TryLoadNode(stream, out Node root)) {
                filter = new IPFilter {
                    m_Root = root
                };
                return true;
            } else {
                filter = null;
                return false;
            }
        }

        private static bool TryLoadNode(Stream stream, out Node node) {
            int flags = stream.ReadByte();
            if (flags < 0) {
                node = null;
                return false;
            } else {
                node = new Node();

                bool lvalue_has = (flags & 128) > 0;
                int lvalue_length_bytes = (flags >> 4) & 7;

                bool rvalue_has = (flags & 8) > 0;
                int rvalue_length_bytes = flags & 7;

                if (lvalue_has) {
                    int length;

                    int i0 = stream.ReadByte();
                    if (i0 < 0) {
                        node = null;
                        return false;
                    }

                    if (lvalue_length_bytes > 1) {
                        int i1 = stream.ReadByte();
                        if (i1 < 0) {
                            node = null;
                            return false;
                        }
                        length = i0 + i1 << 8;
                    } else {
                        length = i0;
                    }

                    byte[] bytes = new byte[length];
                    int read = stream.Read(bytes, 0, length);

                    if (read == length) {
                        node.LValue = String.Intern(Encoding.UTF8.GetString(bytes));
                    } else {
                        node = null;
                        return false;
                    }
                } else {
                    if (TryLoadNode(stream, out Node ptr)) {
                        node.LPtr = ptr;
                    } else {
                        node = null;
                        return false;
                    }
                }

                if (rvalue_has) {
                    int length;

                    int i0 = stream.ReadByte();
                    if (i0 < 0) {
                        node = null;
                        return false;
                    }

                    if (rvalue_length_bytes > 1) {
                        int i1 = stream.ReadByte();
                        if (i1 < 0) {
                            node = null;
                            return false;
                        }
                        length = i0 + i1 << 8;
                    } else {
                        length = i0;
                    }

                    byte[] bytes = new byte[length];
                    int read = stream.Read(bytes, 0, length);
                    if (read == length) {
                        node.RValue = String.Intern(Encoding.UTF8.GetString(bytes));
                    } else {
                        node = null;
                        return false;
                    }
                } else {
                    if (TryLoadNode(stream, out Node ptr)) {
                        node.RPtr = ptr;
                    } else {
                        node = null;
                        return false;
                    }
                }

                return true;
            }
        }

    }
}