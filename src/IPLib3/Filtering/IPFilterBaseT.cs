namespace IPLib3.Filtering; 

public class IPFilterBase<T> where T : class {

    protected IPFilterBase(T no_value) {
        None = no_value;

        Root = new Node<T> {
            LValue = no_value,
            LPtr = null,
            RValue = no_value,
            RPtr = null,
        };
    }

    protected IPFilterBase(T no_value, Node<T> root) {
        None = no_value;
        
        Root = root;
    }

    protected class Node<TV> where TV : class {

        public TV LValue;

        public Node<TV> LPtr;

        public TV RValue;

        public Node<TV> RPtr;

    }

    public T None { get; }
    
    protected Node<T> Root { get; }
        
    protected static readonly UInt128 LENGTH = (UInt128.MaxValue >> 1) + 1;

    public T GetValue(IPAddress ip) => GetValue(ip.ToUInt128());

    private T GetValue(UInt128 u) {
        Node<T> node = Root;

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

    public void SetValue(IPAddress ip, T value) => SetValue(ip.ToUInt128(), value);

    private void SetValue(UInt128 u, T value) => SetValue(Root, UInt128.Zero, LENGTH, u, u, value);

    public void SetValue(IPAddress ip_from, IPAddress ip_to, T value) => SetValue(ip_from.ToUInt128(), ip_to.ToUInt128(), value);

    private void SetValue(UInt128 u_from, UInt128 u_to, T value) => SetValue(Root, UInt128.Zero, LENGTH, u_from, u_to, value);

    private void SetValue(Node<T> node, UInt128 start, UInt128 length, UInt128 ip_from, UInt128 ip_to, T value) {
        if (ip_from > ip_to) throw new ArgumentException("Invalid range", nameof(ip_from));
        if (node == null) throw new IPFilterException("Can't find node. IPFilter seems to be corrupted");
        
        UInt128 l_start = start;
        UInt128 l_end = l_start + length - 1;

        if (l_start <= ip_from && l_end >= ip_from) {
            if (l_start == ip_from && l_end <= ip_to) {
                node.LValue = value;
                node.LPtr = null;
            } else {
                if (node.LValue != null) {
                    node.LPtr = new Node<T> {
                        LValue = node.LValue,
                        LPtr = null,
                        RValue = node.LValue,
                        RPtr = null
                    };
                    node.LValue = default;
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
                    node.RPtr = new Node<T> {
                        LValue = node.RValue,
                        LPtr = null,
                        RValue = node.RValue,
                        RPtr = null
                    };
                    node.RValue = default;
                }

                SetValue(node.RPtr, r_start, length >> 1, UInt128.Max(r_start, ip_from), ip_to, value);
            }
        }
    }

    public int GetCount() => GetCount(Root);

    private int GetCount(Node<T> node) {
        int count = 0;
        
        if (node.LValue == null) {
            count += GetCount(node.LPtr);
        } else if (node.LValue != None) {
            count += 1;
        }
        
        if (node.RValue == null) {
            count += GetCount(node.RPtr);
        } else if (node.RValue != None) {
            count += 1;
        }

        return count;
    }
    
    public void ForEach(IPAddress ip_from, IPAddress ip_to, Action<T> action) => ForEach(ip_from.ToUInt128(), ip_to.ToUInt128(), action);

    private void ForEach(UInt128 u_from, UInt128 u_to, Action<T> action) => ForEach(Root, UInt128.Zero, LENGTH, u_from, u_to, action);

    private void ForEach(Node<T> node, UInt128 start, UInt128 length, UInt128 ip_from, UInt128 ip_to, Action<T> action) {
        if (ip_from > ip_to) throw new ArgumentException("Invalid range", nameof(ip_from));
        
        if (node == null) {
            return;
        }
        
        UInt128 l_start = start;
        UInt128 l_end = l_start + length - 1;

        if (l_start <= ip_from && l_end >= ip_from) {
            if (l_start == ip_from && l_end <= ip_to) {
                if (node.LValue != null) {
                    if (node.LValue != None) {
                        action(node.LValue);
                    }
                } else {
                    ForEach(node.LPtr, action);
                }
            } else {
                ForEach(node.LPtr, l_start, length >> 1, ip_from, UInt128.Min(l_end, ip_to), action);
            }
        }

        UInt128 r_start = start + length;
        UInt128 r_end = r_start + length - 1;

        if (r_start <= ip_to && r_end >= ip_to) {
            if (r_start >= ip_from && r_end == ip_to) {
                if (node.RValue != null) {
                    if (node.RValue != None) {
                        action(node.RValue);
                    }
                } else {
                    ForEach(node.RPtr, action);
                }
            } else {
                ForEach(node.RPtr, r_start, length >> 1, UInt128.Max(r_start, ip_from), ip_to, action);
            }
        }
    }

    private void ForEach(Node<T> node, Action<T> action) {
        if (node == null) {
            return;
        }
        
        if (node.LValue != null) {
            if (node.LValue != None) {
                action(node.LValue);
            }
        } else {
            ForEach(node.LPtr, action);
        }

        if (node.RValue != null) {
            if (node.RValue != None) {
                action(node.RValue);
            }
        } else {
            ForEach(node.RPtr, action);
        }
    }

}