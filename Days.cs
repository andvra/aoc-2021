﻿using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.InteropServices;

public class Aoc2021
{
    public struct vec2d
    {
        public vec2d(long newx, long newy)
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
        public long x;
        public long y;
    }
    public struct vec3d
    {
        public vec3d(long newx, long newy, long newz)
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
        public long x;
        public long y;
        public long z;
    }

    public static long day1_part1(List<string> lines, bool is_real)
    {
        return lines
            .Zip(lines.Skip(1))
            .Select(x => int.Parse(x.First) < int.Parse(x.Second))
            .Count(x => x);
    }
    public static long day1_part2(List<string> lines, bool is_real)
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
    public static long day2_part1(List<string> lines, bool is_real)
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

    public static long day2_part2(List<string> lines, bool is_real)
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

    public static long day3_part1(List<string> lines, bool is_real)
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

    public static long day3_part2(List<string> lines, bool is_real)
    {
        var bin_arr = lines
            .Select(x => x.ToCharArray().Select(y => y - '0').ToList())
            .ToList();
        var all_lines = Enumerable.Range(0, lines.Count()).ToList();
        var lines_last_level = new List<List<int>> { all_lines, all_lines };
        var ratings = new List<int> { 0, 0 };
        foreach (var col in Enumerable.Range(0, bin_arr[0].Count()))
        {
            for (var idx_rating = 0; idx_rating < 2; idx_rating++)
            {
                var num_ones = lines_last_level[idx_rating]
                    .Select(x => bin_arr[x][col])
                    .Sum();
                var num_zeros = lines_last_level[idx_rating].Count() - num_ones;
                var to_keep = -1;
                if (idx_rating == 0)
                {
                    to_keep = num_ones >= num_zeros ? 1 : 0;
                }
                else
                {
                    to_keep = num_zeros <= num_ones ? 0 : 1;
                }
                lines_last_level[idx_rating] = lines_last_level[idx_rating]
                    .Where(x => bin_arr[x][col] == to_keep)
                    .ToList();
                if (lines_last_level[idx_rating].Count() == 1)
                {
                    var idx_line_win = lines_last_level[idx_rating][0];
                    var rating = Convert.ToInt32(lines[idx_line_win], 2);
                    ratings[idx_rating] = rating;
                }
            }
        }

        return ratings[0] * ratings[1];
    }

    class day4_row
    {
        public int idx_board;
        public int num_found;
        public List<int> vals = new List<int>();
    }
    public class day4_finish
    {
        public int idx_board;
        public int score;
        public int num_steps;
    }
    public static List<day4_finish> day4_common(List<string> lines, bool is_real, bool break_on_first)
    {
        var draws = lines[0].Split(',').Select(x => Int32.Parse(x)).ToList();
        var boards = lines
            .Skip(2)
            .Select((x, index) => new { index, x })
            .GroupBy(el => el.index / 6, i => i.x)
            .Select((group, idx) =>
                group
                .Take(5)
                .Select(y => y
                    .Trim()
                    .Replace("  ", " ")
                    .Split(" ")
                    .Select(z => Int32.Parse(z))
                    .ToList()
                    ))
            .ToList();

        var line_and_cols = new List<day4_row>();
        var idx_board = 0;
        foreach (var board in boards)
        {
            var rows = board
                 .Select(x => new day4_row { idx_board = idx_board, num_found = 0, vals = x })
                 .ToList();
            line_and_cols.AddRange(rows);
            foreach (var col in Enumerable.Range(0, 5))
            {
                var the_col = board
                    .Select(x => x[col])
                    .ToList();
                line_and_cols.Add(new day4_row { idx_board = idx_board, num_found = 0, vals = the_col });
            }
            idx_board++;
        }
        var done = false;
        var finishes = new List<day4_finish>();

        foreach (var idx_draw in Enumerable.Range(0, draws.Count()))
        {
            var draw = draws[idx_draw];
            var indices_with_draw = line_and_cols
                .Select((row, index) => new { row, index })
                .Where(x => x.row.vals.Contains(draw) && (finishes.FirstOrDefault(y => y.idx_board == x.row.idx_board) == null))
                .Select(x => x.index);
            foreach (var idx in indices_with_draw)
            {
                line_and_cols[idx].num_found++;
                if (line_and_cols[idx].num_found == 5)
                {
                    var idx_board_winner = line_and_cols[idx].idx_board;
                    var sum_not_marked = boards[idx_board_winner]
                        .Select(x =>
                            x.Select(y => draws.Take(idx_draw + 1).Contains(y) ? 0 : y).ToList())
                        .ToList()
                        .Sum(x => x.Sum());
                    finishes.Add(new day4_finish() { idx_board = idx_board_winner, num_steps = idx_draw + 1, score = sum_not_marked * draw });
                    if (break_on_first)
                    {
                        done = true;
                        break;
                    }
                }
            }
            if (done)
            {
                break;
            }
        }

        return finishes;
    }

    public static long day4_part1(List<string> lines, bool is_real)
    {
        return day4_common(lines, is_real, true).Max(x => x.score);
    }

    public static long day4_part2(List<string> lines, bool is_real)
    {
        var score = day4_common(lines, is_real, false).MaxBy(x => x.num_steps)?.score;
        return score.HasValue ? score.Value : 0;
    }

    public static long day5_common(List<string> lines, bool is_real, bool include_diag)
    {
        var ints = lines
            .Select(x => x.Split(" -> "))
            .SelectMany(x => x)
            .Select(x => x.Split(","))
            .SelectMany(x => x)
            .Select(x => int.Parse(x));
        var coords = ints
            .Where((val, idx) => idx % 2 == 0)
            .Zip(ints.Where((val, idx) => idx % 2 == 1))
            .Select(x => new vec2d(x.First, x.Second));
        var pairs = coords
            .Where((val, idx) => idx % 2 == 0)
            .Zip(coords.Where((val, idx) => idx % 2 == 1))
            .Select(x => new { pos_start = x.First, pos_end = x.Second })
            .Where(el => include_diag || ((el.pos_start.x == el.pos_end.x) || (el.pos_start.y == el.pos_end.y)))
            .ToList();
        var hashes_first = new HashSet<long>();
        var hashes_multi = new HashSet<long>();
        foreach (var pair in pairs)
        {
            var cur = pair.pos_start;
            var end = pair.pos_end;
            var diff = new vec2d(Math.Sign(end.x - cur.x), Math.Sign(end.y - cur.y));
            var cur_hash = cur.x + cur.y * 10000;
            if (hashes_first.Contains(cur_hash))
            {
                hashes_multi.Add(cur_hash);
            }
            hashes_first.Add(cur_hash);
            while ((cur.x != end.x) || (cur.y != end.y))
            {
                cur += diff;
                cur_hash = cur.x + cur.y * 10000;
                if (hashes_first.Contains(cur_hash))
                {
                    hashes_multi.Add(cur_hash);
                }
                hashes_first.Add(cur_hash);
            }
        }
        return hashes_multi.Count();
    }

    public static long day5_part1(List<string> lines, bool is_real)
    {
        return day5_common(lines, is_real, false);
    }

    public static long day5_part2(List<string> lines, bool is_real)
    {
        return day5_common(lines, is_real, true);
    }

    public static long day6_common(List<string> lines, int num_days)
    {
        var timers = lines[0]
            .Split(",")
            .Select(x => int.Parse(x))
            .ToList();

        var num_per_day = Enumerable.Range(0, 9).Select(x => new long()).ToArray();

        timers.ForEach(x => num_per_day[x]++);

        foreach (var day in Enumerable.Range(1, num_days))
        {
            var num_zero = num_per_day[0];
            foreach (var pos in Enumerable.Range(0, 8))
            {
                num_per_day[pos] = num_per_day[pos + 1];
            }
            num_per_day[8] = num_zero;
            num_per_day[6] += num_zero;
        }

        return num_per_day.Sum();
    }

    public static long day6_part1(List<string> lines, bool is_real)
    {
        return day6_common(lines, 80);
    }

    public static long day6_part2(List<string> lines, bool is_real)
    {
        return day6_common(lines, 256);
    }

    public static long day7_part1(List<string> lines, bool is_real)
    {
        var pos = lines[0].Split(",").Select(x => int.Parse(x)).ToList();
        pos.Sort();
        var mean = pos[pos.Count / 2];
        return pos.Select(x => Math.Abs(x - mean)).Sum();
    }

    public static long day7_part2(List<string> lines, bool is_real)
    {
        var pos = lines[0].Split(",").Select(x => int.Parse(x)).ToList();
        var min_vals = new long[2] { long.MaxValue, long.MaxValue };
        var last_vals = new long[2] { long.MaxValue, long.MaxValue };
        var done = new bool[2] { false, false };
        var avg = pos.Sum() / pos.Count();
        foreach (var level_diff in Enumerable.Range(0, pos.Max()))
        {
            var levels = new long[2] { avg - level_diff, avg + level_diff };
            var vals = new long[2];
            for (var idx = 0; idx < 2; idx++)
            {
                if (done[idx])
                {
                    continue;
                }
                vals[idx] = pos.Select(x => Math.Abs(levels[idx] - x)).Select(x => (long)(x * ((x + 1) / 2.0f))).Sum();
                min_vals[idx] = Math.Min(min_vals[idx], vals[idx]);
                if (vals[idx] > last_vals[idx])
                {
                    done[idx] = true;
                }
                last_vals[idx] = vals[idx];
            }
        }
        return min_vals.Min();
    }

    public static long day8_part1(List<string> lines, bool is_real)
    {
        var wire_pos_per_char = new List<HashSet<int>> {
            new HashSet<int>{0,1,2,4,5,6 },
            new HashSet<int>{2,5 },
            new HashSet<int>{0,2,3,4,6},
            new HashSet<int>{0,2,3,5,6},
            new HashSet<int>{1,2,3,5},
            new HashSet<int>{0,1,3,5,6},
            new HashSet<int>{0,1,3,4,5,6},
            new HashSet<int>{0,2,5 },
            new HashSet<int>{0,1,2,3,4,5,6},
            new HashSet<int>{0,1,2,3,5,6}
        };
        var wires_unique_count = wire_pos_per_char
            .Where(x => wire_pos_per_char
                        .Select(y => y.Count)
                        .Where(y => y == x.Count)
                        .Count() == 1)
            .ToList();
        var unique_counts = wires_unique_count
            .Select(x => x.Count)
            .ToList();
        var ret = 0;
        foreach (var line in lines)
        {
            var parts = line.Split(" | ");
            var inputs = parts[0]
                .Split(" ")
                .Select(x => x.ToCharArray())
                .Select(x => x.Select(y => (int)(y - 'a')).ToList())
                .ToList();
            var outputs = parts[1]
                .Split(" ")
                .Select(x => x.ToCharArray())
                .Select(x => x.Select(y => (int)(y - 'a')).ToList())
                .ToList();
            ret += outputs.Where(x => unique_counts.Contains(x.Count)).Count();
        }

        return ret;
    }

    public static long day8_part2(List<string> lines, bool is_real)
    {
        // Instead of labeling the wires, we give them numberic IDs. 
        //  Starting with 0 at the top, going down/right.
        var wire_pos_per_char = new List<HashSet<int>> {
            new HashSet<int>{0,1,2,4,5,6 },
            new HashSet<int>{2,5 },
            new HashSet<int>{0,2,3,4,6},
            new HashSet<int>{0,2,3,5,6},
            new HashSet<int>{1,2,3,5},
            new HashSet<int>{0,1,3,5,6},
            new HashSet<int>{0,1,3,4,5,6},
            new HashSet<int>{0,2,5 },
            new HashSet<int>{0,1,2,3,4,5,6},
            new HashSet<int>{0,1,2,3,5,6}
        };

        var ret = 0;

        foreach (var line in lines)
        {
            var letter_per_pos = new char[7];
            var parts = line.Split(" | ");
            var inputs = parts[0]
                .Split(" ")
                .Select(x => x.ToCharArray())
                .Select(x => x.Select(y => y).ToList())
                .ToList();
            var outputs = parts[1]
                .Split(" ")
                .Select(x => x.ToCharArray())
                .Select(x => x.Select(y => y).ToList())
                .ToList();
            var cnt_per_char = new List<int> { 0, 0, 0, 0, 0, 0, 0 };
            foreach (var input in inputs)
            {
                foreach (var c in input)
                {
                    cnt_per_char[c - 'a']++;
                }
            }
            var input_1 = inputs.First(x => x.Count == 2);
            var input_4 = inputs.First(x => x.Count == 4);
            var input_7 = inputs.First(x => x.Count == 3);
            var input_8 = inputs.First(x => x.Count == 7);
            letter_per_pos[0] = input_7.Except(input_1).ToList()[0];
            letter_per_pos[1] = (char)(cnt_per_char
                .Select((x, index) => new { val = x, index })
                .Where(x => x.val == 6)
                .Select(x => x.index)
                .ToList()[0] + 'a');
            letter_per_pos[4] = (char)(cnt_per_char
                .Select((x, index) => new { val = x, index })
                .Where(x => x.val == 4)
                .Select(x => x.index)
                .ToList()[0] + 'a');
            letter_per_pos[5] = (char)(cnt_per_char
                .Select((x, index) => new { val = x, index })
                .Where(x => x.val == 9)
                .Select(x => x.index)
                .ToList()[0] + 'a');
            letter_per_pos[2] = input_1[0] == letter_per_pos[5] ? input_1[1] : input_1[0];
            var not_pos_3_or_char_4 = new List<char>
            {
                letter_per_pos[1],
                letter_per_pos[2],
                letter_per_pos[5],
            };
            letter_per_pos[3] = input_4.Except(not_pos_3_or_char_4).First();
            letter_per_pos[6] = Enumerable.Range(0, 7).Select(x => (char)(x + 'a')).Except(letter_per_pos).First();
            var pos_per_letter = new Dictionary<char, int>();

            for (var pos = 0; pos < letter_per_pos.Count(); pos++)
            {
                pos_per_letter[letter_per_pos[pos]] = pos;
            }

            var nums = new List<int>();

            foreach (var output in outputs)
            {
                var set = new HashSet<int>();
                foreach (var c in output)
                {
                    set.Add(pos_per_letter[c]);
                }
                var num = wire_pos_per_char
                    .Select((x, index) => new { val = x, index })
                    .Where(x => (set.Count == x.val.Count) && (x.val.Except(set).Count() == 0))
                    .Select(x => x.index)
                    .First();
                nums.Add(num);
            }
            ret += nums.Aggregate((x, y) => x * 10 + y);
        }

        return ret;
    }
}
