using System.Linq;

namespace IPLib3;

public static class IPRangeConsolidator {
    
    public static List<IPRange> ExcludeRanges(List<IPRange> input, List<IPRange> excludes) {
        if (excludes.Count < 0) {
            return input;
        }
        
        List<IPRange> output = new List<IPRange>();

        foreach (IPRange range in input) {
            List<IPRange> full_excludes = excludes.FindAll(e => e.StartUI <= range.StartUI && e.EndUI >= range.EndUI);
            if (full_excludes.Count > 0) {
                continue;
            }

            List<IPRange> new_ranges = ExcludeRanges(range, excludes);
            if (new_ranges.Count > 0) {
                output.AddRange(new_ranges);
            }
        }

        return output;
    }

    private static readonly List<IPRange> EMPTY_RANGES = new List<IPRange>();

    private static UInt128 Min(UInt128 a, UInt128 b) => a < b ? a : b;
    
    private static UInt128 Max(UInt128 a, UInt128 b) => a > b ? a : b;
    
    private static List<IPRange> Ranges(IPRange range) => new List<IPRange> { range };
    
    private static List<IPRange> ExcludeRanges(IPRange range, List<IPRange> excludes) {
        IPRange remaining = range;

        List<IPRange> affected_excludes = excludes.FindAll(e => (e.StartUI >= remaining.StartUI && e.StartUI <= remaining.EndUI) || (e.EndUI >= remaining.StartUI && e.EndUI <= remaining.EndUI));
        if (affected_excludes.Count > 0) {
            List<IPRange> before_excludes = affected_excludes.FindAll(e => e.StartUI <= remaining.StartUI);
            if (before_excludes.Count > 0) {
                UInt128 start = before_excludes.Max(e => e.EndUI) + 1;
                if (start <= remaining.EndUI) {
                    remaining = new IPRange(start, remaining.EndUI);
                } else {
                    return EMPTY_RANGES;
                }

                foreach (IPRange before_exclude in before_excludes) {
                    affected_excludes.Remove(before_exclude);
                }
            }

            List<IPRange> after_excludes = affected_excludes.FindAll(e => e.EndUI >= remaining.EndUI);
            if (after_excludes.Count > 0) {
                UInt128 end = after_excludes.Min(e => e.StartUI) - 1;
                if (end >= remaining.StartUI) {
                    remaining = new IPRange(remaining.StartUI, end);
                } else {
                    return EMPTY_RANGES;
                }

                foreach (IPRange after_exclude in after_excludes) {
                    affected_excludes.Remove(after_exclude);
                }
            }

            if (affected_excludes.Count > 0) {
                IPRange exclude = affected_excludes.Find(e => remaining.StartUI <= e.StartUI && remaining.EndUI >= e.EndUI);
                if (exclude != null) {
                    affected_excludes.Remove(exclude);
                    
                    List<IPRange> ranges = new List<IPRange>();

                    IPRange preceding = new IPRange(remaining.StartUI, exclude.StartUI - 1);
                    if (preceding.StartUI <= preceding.EndUI) {
                        ranges.AddRange(ExcludeRanges(preceding, affected_excludes));
                    }

                    IPRange succeeding = new IPRange(exclude.EndUI + 1, remaining.EndUI);
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
        List<IPRange> inputs = new List<IPRange>(ranges);
        
        inputs.Sort((x, y) => {
            UInt128 xx = x.StartUI;
            UInt128 yy = y.StartUI;
            return xx.CompareTo(yy);
        });
        
        List<IPRange> outputs = new List<IPRange>();

        IPRange output = null;
        
        foreach (IPRange input in inputs) {
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