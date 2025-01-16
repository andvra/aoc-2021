using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
public class Aoc2021
{
    static string path_root = "C:\\Users\\andre\\source\\repos\\aoc-2021\\input";

    public struct vec2d
    {
        public vec2d(Int64 newx, Int64 newy)
        {
            x = newx;
            y = newy;
        }
        public static vec2d operator *(vec2d v, int m)
        {
            return new vec2d(v.x * m, v.y * m);
        }
        public static vec2d operator +(vec2d v1, vec2d v2)
        {
            return new vec2d(v1.x + v2.x, v1.y + v2.y);
        }
        public Int64 x;
        public Int64 y;
    }
    public struct vec3d
    {
        public vec3d(Int64 newx, Int64 newy, Int64 newz)
        {
            x = newx;
            y = newy;
            z = newz;
        }
        public static vec3d operator *(vec3d v, int m)
        {
            return new vec3d(v.x * m, v.y * m, v.z * m);
        }
        public static vec3d operator +(vec3d v1, vec3d v2)
        {
            return new vec3d(v1.x + v2.x, v1.y + v2.y, v1.z + v2.z);
        }
        public Int64 x;
        public Int64 y;
        public Int64 z;
    }


    public static void run()
    {
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
                            var int_val = (Int64)ret;
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
    }
    public static Int64 day1_part1(List<string> lines, bool is_real)
    {
        return lines
            .Zip(lines.Skip(1))
            .Select(x => int.Parse(x.First) < int.Parse(x.Second))
            .Count(x => x);
    }
    public static Int64 day1_part2(List<string> lines, bool is_real)
    {
        var sums = lines
            .Zip(lines.Skip(1), lines.Skip(2))
            .Select(x => int.Parse(x.First) + int.Parse(x.Second) + int.Parse(x.Third))
            .ToList();
        return sums
            .Zip(sums.Skip(1))
            .Select(x => x.First < x.Second)
            .Count(x => x);
    }
    public static Int64 day2_part1(List<string> lines, bool is_real)
    {
        var c2v = new Dictionary<char, vec2d>
        {
            ['f'] = new vec2d(1, 0),
            ['u'] = new vec2d(0, -1),
            ['d'] = new vec2d(0, 1)
        };
        var v = lines
            .Select(x => x.Split(" ").Take(2))
            .Select(x => c2v[x.First()[0]] * int.Parse(x.Last()))
            .Aggregate((v1, v2) => v1 + v2);
        return v.x * v.y;
    }

    public static Int64 day2_part2(List<string> lines, bool is_real)
    {
        var c2v = new Dictionary<char, vec3d>
        {
            ['f'] = new vec3d(1, 0, 0),
            ['u'] = new vec3d(0, 0, -1),
            ['d'] = new vec3d(0, 0, 1)
        };
        var v = lines
            .Select(x => x.Split(" ").Take(2))
            .Select(x => c2v[x.First()[0]] * int.Parse(x.Last()))
            .Aggregate((v1, v2) => new vec3d(v1.x + v2.x, v1.y + v1.z * v2.x, v1.z + v2.z));
        return v.x * v.y;
    }

    public static Int64 day3_part1(List<string> lines, bool is_real)
    {
        var bin_arr = lines
            .Select(x => x.ToCharArray().Select(x => x - '0'))
            .Aggregate((x, y) => x.Zip(y).Select(x => x.First + x.Second))
            .Select(x => x > lines.Count() / 2 ? 1 : 0)
            .ToList();
        var bin_chars = string.Join(null, bin_arr);
        var num = Convert.ToInt64(bin_chars, 2);
        var num_inv = (1 << lines[0].Length) - 1 - num;
        return num * num_inv;
    }
}
