string path_root = "C:\\Users\\andre\\source\\repos\\aoc-2021\\input";

foreach (var idx_day in Enumerable.Range(1, 25))
{
    foreach (var idx_part in Enumerable.Range(1, 2))
    {
        var func = typeof(Aoc2021).GetMethod($"day{idx_day}_part{idx_part}");
        if (func == null)
        {
            continue;
        }

        Int64 ret_test = -1;
        Int64 ret_real = -1;

        foreach (var idx_type in Enumerable.Range(1, 2))
        {
            var is_real = idx_type == 2;
            var fn = Path.Join(path_root, $"day{idx_day}_{(is_real ? "real" : "test")}.txt");
            if (!File.Exists(fn))
            {
                continue;
            }
            var lines = File.ReadAllLines(fn);
            if (lines != null && lines.Any())
            {
                var ret = func.Invoke(null, [lines.ToList(), is_real]);
                if (ret != null)
                {
                    var int_val = (long)ret;
                    if (is_real)
                    {
                        ret_real = int_val;
                    }
                    else
                    {
                        ret_test = int_val;
                    }
                }
            }
        }
        Console.WriteLine($"Day {idx_day} part {idx_part}: {ret_test} / {ret_real}");
    }
}