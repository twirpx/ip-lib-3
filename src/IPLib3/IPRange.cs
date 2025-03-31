using System.Text.RegularExpressions;

namespace IPLib3;

public sealed class IPRange {

    public IPRange(IPAddress start, IPAddress end) {
        Start = start;
        StartUI = start.ToUInt128();
        End = end;
        EndUI = end.ToUInt128();
    }

    public IPRange(UInt128 start, UInt128 end) {
        Start = start.ToIPAddress();
        StartUI = start;
        End = end.ToIPAddress();
        EndUI = end;
    }

    public IPAddress Start { get; }

    public IPAddress End { get; }

    public UInt128 StartUI { get; }

    public UInt128 EndUI { get; }

    private static readonly Regex REGEX_V4_RANGE = new Regex(@"(?<i>[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3})\s*-\s*(?<j>[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3})", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);
    private static readonly Regex REGEX_V4_CIDR = new Regex(@"(?<i>[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3})\s*/\s*(?<m>[0-9]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);
    private static readonly Regex REGEX_V4_MASK = new Regex(@"(?<i>[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3})\.(?<j>[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3})", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);

    private static readonly Regex REGEX_V6_RANGE = new Regex(@"(?<i>[0-9a-f:]+)\s*-\s*(?<j>[0-9a-f:]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);
    private static readonly Regex REGEX_V6_CIDR = new Regex(@"(?<i>[0-9a-f:]+)\s*/\s*(?<m>[0-9]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);

#pragma warning disable 618
    public static bool TryParse(string str, out IPRange range) {
        range = null;

        var match = REGEX_V4_RANGE.Match(str);
        if (match.Success) {
            if (IPAddress.TryParse(match.Groups["i"].Value, out var i) && IPAddress.TryParse(match.Groups["j"].Value, out var j)) {
                range = new IPRange(i, j);
                return true;
            }
        }

        match = REGEX_V4_MASK.Match(str);
        if (match.Success) {
            if (IPAddress.TryParse(match.Groups["i"].Value, out var i) && IPAddress.TryParse(match.Groups["j"].Value, out var j)) {
                range = new IPRange(i, j);
                return true;
            }
        }

        match = REGEX_V4_CIDR.Match(str);
        if (match.Success) {
            if (IPAddress.TryParse(match.Groups["i"].Value, out var i) && Int32.TryParse(match.Groups["m"].Value, out var m) && m <= 32) {
                var k = m == 0 ? 0 : UInt32.MaxValue << (32 - m);

                var ii = i.ToUInt32();
                var jj = k;
                ii &= jj;
                jj = ~jj;
                jj = ii + jj;

                range = new IPRange(ii.ToIPAddress(), jj.ToIPAddress());
                return true;
            }
        }

        match = REGEX_V6_RANGE.Match(str);
        if (match.Success) {
            if (IPAddress.TryParse(match.Groups["i"].Value, out var i) && IPAddress.TryParse(match.Groups["j"].Value, out var j)) {
                range = new IPRange(i, j);
                return true;
            }
        }

        match = REGEX_V6_CIDR.Match(str);
        if (match.Success) {
            if (IPAddress.TryParse(match.Groups["i"].Value, out var i) && Int32.TryParse(match.Groups["m"].Value, out var m) && m <= 128) {
                var k = m == 0 ? 0 : UInt128.MaxValue << (128 - m);

                var ii = i.ToUInt128();
                var jj = k;
                ii &= jj;
                jj = ~jj;
                jj = ii + jj;

                range = new IPRange(ii, jj);
                return true;
            }
        }

        return range != null;
    }
#pragma warning restore 618

    public override string ToString() => $"{Start} - {End}";

    public bool ContainsIP(IPAddress ip) {
        var u = ip.ToUInt128();
        return StartUI <= u && u <= EndUI;
    }

    public bool ContainsIP(UInt128 ui) {
        return StartUI <= ui && ui <= EndUI;
    }

}