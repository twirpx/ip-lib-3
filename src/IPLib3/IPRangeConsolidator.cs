using System.Linq;

namespace IPLib3;

public static class IPRangeConsolidator {

    public static List<IPRange> ExcludeRanges(List<IPRange> input, List<IPRange> excludes) {
        if (excludes.Count < 0) {
            return input;
        }

        List<IPRange> output = [];

        foreach (var range in input) {
            var full_excludes = excludes.FindAll(e => e.StartUI <= range.StartUI && e.EndUI >= range.EndUI);
            if (full_excludes.Count > 0) {
                continue;
            }

            var new_ranges = ExcludeRanges(range, excludes);
            if (new_ranges.Count > 0) {
                output.AddRange(new_ranges);
            }
        }

        return output;
    }

    private static readonly List<IPRange> EMPTY_RANGES = [];

    private static UInt128 Min(UInt128 a, UInt128 b) => a < b ? a : b;

    private static UInt128 Max(UInt128 a, UInt128 b) => a > b ? a : b;

    private static List<IPRange> Ranges(IPRange range) => [range];

    private static List<IPRange> ExcludeRanges(IPRange range, List<IPRange> excludes) {
        var remaining = range;

        var affected_excludes = excludes.FindAll(e => (e.StartUI >= remaining.StartUI && e.StartUI <= remaining.EndUI) || (e.EndUI >= remaining.StartUI && e.EndUI <= remaining.EndUI));
        if (affected_excludes.Count > 0) {
            var before_excludes = affected_excludes.FindAll(e => e.StartUI <= remaining.StartUI);
            if (before_excludes.Count > 0) {
                var start = before_excludes.Max(e => e.EndUI) + 1;
                if (start <= remaining.EndUI) {
                    remaining = new IPRange(start, remaining.EndUI);
                } else {
                    return EMPTY_RANGES;
                }

                foreach (var before_exclude in before_excludes) {
                    affected_excludes.Remove(before_exclude);
                }
            }

            var after_excludes = affected_excludes.FindAll(e => e.EndUI >= remaining.EndUI);
            if (after_excludes.Count > 0) {
                var end = after_excludes.Min(e => e.StartUI) - 1;
                if (end >= remaining.StartUI) {
                    remaining = new IPRange(remaining.StartUI, end);
                } else {
                    return EMPTY_RANGES;
                }

                foreach (var after_exclude in after_excludes) {
                    affected_excludes.Remove(after_exclude);
                }
            }

            if (affected_excludes.Count > 0) {
                var exclude = affected_excludes.Find(e => remaining.StartUI <= e.StartUI && remaining.EndUI >= e.EndUI);
                if (exclude != null) {
                    affected_excludes.Remove(exclude);

                    List<IPRange> ranges = [];

                    var preceding = new IPRange(remaining.StartUI, exclude.StartUI - 1);
                    if (preceding.StartUI <= preceding.EndUI) {
                        ranges.AddRange(ExcludeRanges(preceding, affected_excludes));
                    }

                    var succeeding = new IPRange(exclude.EndUI + 1, remaining.EndUI);
                    if (succeeding.StartUI <= succeeding.EndUI) {
                        ranges.AddRange(ExcludeRanges(succeeding, affected_excludes));
                    }

                    return ranges;
                }
            }
        }

        return Ranges(remaining);
    }

    public static List<IPRange> MergeRanges(List<IPRange> ranges) {
        List<IPRange> inputs = [..ranges];

        inputs.Sort((x, y) => {
            var xx = x.StartUI;
            var yy = y.StartUI;
            return xx.CompareTo(yy);
        });

        List<IPRange> outputs = [];

        IPRange output = null;

        foreach (var input in inputs) {
            if (output == null) {
                output = input;
            } else if (output.StartUI <= input.StartUI && output.EndUI + 1 >= input.StartUI) {
                output = new IPRange(
                    UInt128.Min(output.StartUI, input.StartUI),
                    UInt128.Max(output.EndUI, input.EndUI)
                );
            } else {
                outputs.Add(output);
                output = input;
            }
        }

        if (output != null) {
            outputs.Add(output);
        }

        return outputs;
    }

}