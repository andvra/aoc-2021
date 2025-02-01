using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Xml;
using System.Xml.Linq;
using Aoc_common;

public class Aoc2021
{
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

    public static long day9_part1(List<string> lines, bool is_real)
    {
        var num_rows = lines.Count;
        var num_cols = lines[0].Length;
        var heights = new int[num_rows + 2, num_cols + 2];
        for (var row = 0; row < num_rows + 2; row++)
        {
            for (var col = 0; col < num_cols + 2; col++)
            {
                heights[row, col] = int.MaxValue;
            }
        }
        for (var row = 0; row < num_rows; row++)
        {
            for (var col = 0; col < num_cols; col++)
            {
                heights[row + 1, col + 1] = (int)(lines[row][col] - '0');
            }
        }
        var diffs = new[]
            {
                new{r=1,c=0},
                new{r=-1,c=0},
                new{r=0,c=1},
                new{r=0,c=-1},
            };
        var nums = new List<int>();
        for (var row = 0; row < num_rows; row++)
        {
            for (var col = 0; col < num_cols; col++)
            {
                var cur = heights[row + 1, col + 1];
                var is_smaller = true;
                foreach (var diff in diffs)
                {
                    var row_eval = row + 1 + diff.r;
                    var col_eval = col + 1 + diff.c;
                    var eval = heights[row_eval, col_eval];
                    if (eval <= cur)
                    {
                        is_smaller = false;
                    }
                }
                if (is_smaller)
                {
                    nums.Add(cur);
                }
            }
        }

        return nums.Select(x => x + 1).Sum();
    }

    public static long day9_part2(List<string> lines, bool is_real)
    {
        var num_rows = lines.Count;
        var num_cols = lines[0].Length;
        var heights = new int[num_rows, num_cols];

        for (var row = 0; row < num_rows; row++)
        {
            for (var col = 0; col < num_cols; col++)
            {
                var val_orig = (int)(lines[row][col] - '0');
                var val = val_orig == 9 ? -1 : 0;
                heights[row, col] = val;
            }
        }

        var flooded = new int[num_rows, num_cols];
        var segment_sizes = new List<int>();

        if (!Functions.floodfill(in heights, ref flooded, ref segment_sizes))
        {
            return -1;
        }

        var ret = segment_sizes
            .OrderByDescending(x => x)
            .Take(3)
            .Aggregate((x, y) => x * y);

        return ret;
    }

    public static long day10_part1(List<string> lines, bool is_real)
    {
        var score = 0;
        var char_to_score = new Dictionary<char, int>
        {
            {')',3 },
            {']',57 },
            {'}',1197 },
            {'>',25137 },
        };
        var expected_open = new Dictionary<char, char>
        {
            {')','(' },
            {']','[' },
            {'}','{' },
            {'>','<' },
        };

        foreach (var line in lines)
        {
            var openers = new Stack<char>();
            openers.Push(line[0]);
            for (var idx_char = 1; idx_char < line.Length; idx_char++)
            {
                var c = line[idx_char];
                if (expected_open.ContainsKey(c))
                {
                    var last_opener = openers.Pop();
                    if (last_opener != expected_open[c])
                    {
                        score += char_to_score[c];
                        continue;
                    }
                }
                else
                {
                    openers.Push(c);
                }
            }
        }

        return score;
    }

    public static long day10_part2(List<string> lines, bool is_real)
    {
        var scores = new List<long>();
        var char_to_score = new Dictionary<char, int>
        {
            {'(',1 },
            {'[',2 },
            {'{',3 },
            {'<',4 },
        };
        var expected_open = new Dictionary<char, char>
        {
            {')','(' },
            {']','[' },
            {'}','{' },
            {'>','<' },
        };

        foreach (var line in lines)
        {
            var openers = new Stack<char>();
            openers.Push(line[0]);
            var is_valid = true;
            for (var idx_char = 1; idx_char < line.Length; idx_char++)
            {
                var c = line[idx_char];
                if (expected_open.ContainsKey(c))
                {
                    var last_opener = openers.Pop();
                    if (last_opener != expected_open[c])
                    {
                        is_valid = false;
                        continue;
                    }
                }
                else
                {
                    openers.Push(c);
                }
            }
            if (is_valid)
            {
                long score_line = 0;
                var num_items = openers.Count;
                for (var idx = 0; idx < num_items; idx++)
                {
                    var c = openers.Pop();
                    score_line = 5 * score_line + char_to_score[c];
                }
                scores.Add(score_line);
            }
        }

        scores.Sort();

        return scores[scores.Count / 2];
    }

    public static long day11_common(List<string> lines, bool run_until_all_flash)
    {
        var num_rows = lines.Count;
        var num_cols = lines[0].Length;
        var num_vals = num_rows * num_cols;
        var vals = lines
            .Select(x => x.ToCharArray().Select(c => (int)(c - '0')).ToList())
            .ToList();

        var num_iter = run_until_all_flash ? 100000 : 100;
        var num_flashes = 0;

        foreach (var iter in Enumerable.Range(0, num_iter))
        {
            var has_exploded = new bool[num_rows, num_cols];
            var heads = new List<vec2d>();
            foreach (var row in Enumerable.Range(0, num_rows))
            {
                foreach (var col in Enumerable.Range(0, num_cols))
                {
                    vals[row][col] += 1;
                    if (vals[row][col] > 9)
                    {
                        heads.Add(new vec2d(col, row));
                        has_exploded[row, col] = true;
                    }
                }
            }
            var idx_start = 0;
            var idx_end_excl = heads.Count;
            var done = false;

            while (!done)
            {
                var num_new = 0;
                foreach (var idx_head in Enumerable.Range(idx_start, idx_end_excl - idx_start))
                {
                    var cur_head = heads[idx_head];
                    foreach (var row_diff in Enumerable.Range(-1, 3))
                    {
                        foreach (var col_diff in Enumerable.Range(-1, 3))
                        {
                            var row_eval = (int)cur_head.y + row_diff;
                            var col_eval = (int)cur_head.x + col_diff;
                            if (Enumerable.Range(0, num_rows).Contains(row_eval)
                                && Enumerable.Range(0, num_cols).Contains(col_eval))
                            {
                                vals[row_eval][col_eval]++;
                                if (vals[row_eval][col_eval] > 9)
                                {
                                    if (!has_exploded[row_eval, col_eval])
                                    {
                                        heads.Add(new vec2d(col_eval, row_eval));
                                        num_new++;
                                    }
                                    has_exploded[row_eval, col_eval] = true;
                                }
                            }
                        }
                    }
                }

                if (num_new == 0)
                {
                    done = true;
                }
                idx_start = idx_end_excl;
                idx_end_excl = idx_start + num_new;
            }
            num_flashes += heads.Count;
            if (run_until_all_flash && (num_vals == heads.Count))
            {
                return iter + 1;
            }
            foreach (var row in Enumerable.Range(0, num_rows))
            {
                foreach (var col in Enumerable.Range(0, num_cols))
                {
                    if (has_exploded[row, col])
                    {
                        vals[row][col] = 0;
                    }
                }
            }
        }

        return num_flashes;
    }

    public static long day11_part1(List<string> lines, bool is_real)
    {
        return day11_common(lines, false);
    }

    public static long day11_part2(List<string> lines, bool is_real)
    {
        return day11_common(lines, true);
    }

    public static long day12_walk(in List<List<int>> connections, in List<bool> is_large, bool[] visited, int idx_prev, int idx_cur, int idx_start, int idx_end, bool used_double_access)
    {
        var visited_copy = new bool[visited.Length];
        visited.CopyTo(visited_copy, 0);
        visited_copy[idx_cur] = true;
        long num_paths = 0;

        foreach (var idx_eval in connections[idx_cur])
        {
            var new_used_double_access = used_double_access;
            bool node_not_blocked = is_large[idx_eval] || !visited_copy[idx_eval];

            if (!node_not_blocked && !used_double_access && (idx_eval != idx_start))
            {
                node_not_blocked = true;
                new_used_double_access = true;
            }

            bool is_at_end = idx_eval == idx_end;

            if (is_at_end)
            {
                num_paths++;
            }
            else if (node_not_blocked)
            {
                num_paths += day12_walk(connections, is_large, visited_copy, idx_cur, idx_eval, idx_start, idx_end, new_used_double_access);
            }
        }

        return num_paths;
    }

    public struct Day12_data
    {
        public List<List<int>> connections;
        public List<bool> is_large;
        public bool[] visited;
        public int idx_start;
        public int idx_end;
    }

    public static Day12_data day12_get_data(List<string> lines)
    {
        Day12_data data = new Day12_data();

        var nodes = lines
             .SelectMany(x => x.Split("-"))
             .Distinct()
             .ToList();

        var input_connections = lines
            .Select(x => x.Split("-").Select(y => nodes.IndexOf(y)).ToList())
            .ToList();

        data.connections = Enumerable.Range(0, nodes.Count)
            .Select(x => new List<int>())
            .ToList();

        input_connections.ForEach(x =>
        {
            data.connections[x[0]].Add(x[1]);
            data.connections[x[1]].Add(x[0]);
        });

        data.idx_start = nodes
            .Select((x, index) => new { val = x, index })
            .Where(x => x.val == "start")
            .Select(x => x.index)
            .First();

        data.idx_end = nodes
            .Select((x, index) => new { val = x, index })
            .Where(x => x.val == "end")
            .Select(x => x.index)
            .First();

        data.is_large = nodes
            .Select(x => x[0] < 'a')
            .ToList();

        // visited is whether or not a specific node is visited
        // used_edge controls whether or not we made a specific
        //  movement - this is here to avoid loops. Eg. A->B->A->B..
        //  We are allowed to move A->B->A, but not back to a again
        data.visited = new bool[nodes.Count];

        return data;
    }

    public static long day12_part1(List<string> lines, bool is_real)
    {
        var data = day12_get_data(lines);
        return day12_walk(data.connections, data.is_large, data.visited, data.idx_start, data.idx_start, data.idx_start, data.idx_end, true);
    }

    public static long day12_part2(List<string> lines, bool is_real)
    {
        var data = day12_get_data(lines);
        return day12_walk(data.connections, data.is_large, data.visited, data.idx_start, data.idx_start, data.idx_start, data.idx_end, false);
    }

    public static List<vec2d> day13_fold(List<string> lines, int num_max_folds)
    {
        var idx_separator = lines
             .Select((x, index) => new { val = x, index })
             .Where(x => string.IsNullOrEmpty(x.val))
             .Select(x => x.index)
             .First();

        var coords = lines
            .Take(idx_separator)
            .Select(x => x.Split(","))
            .Select(x => new vec2d(int.Parse(x[0]), int.Parse(x[1])))
            .ToList();

        var folds = lines
            .Skip(idx_separator + 1)
            .Select(x => x.Substring("fold along ".Length))
            .Select(x => x.Split("="))
            .Select(x => new { axis = x[0], index = int.Parse(x[1]) })
            .ToList();

        var cnt = 0;

        foreach (var fold in folds)
        {
            if (fold.axis == "x")
            {
                coords = coords
                    .Select(v => v.x > fold.index ? new vec2d(2 * fold.index - v.x, v.y) : v)
                    .ToList();
            }
            else
            {
                coords = coords
                    .Select(v => v.y > fold.index ? new vec2d(v.x, 2 * fold.index - v.y) : v)
                    .ToList();
            }
            if (++cnt == num_max_folds)
            {
                break;
            }
        }

        return coords;
    }

    public static long day13_part1(List<string> lines, bool is_real)
    {
        var coords = day13_fold(lines, 1);
        var unique = new HashSet<long>();

        foreach (var coord in coords)
        {
            unique.Add(coord.x * 100000 + coord.y);
        }

        return unique.Count;
    }

    public static long day13_part2(List<string> lines, bool is_real)
    {
        var coords = day13_fold(lines, 10000);
        var unique = new HashSet<long>();

        // NB Set this to true to print the answer. It is non-numeric :/
        var do_print = false;
        if (do_print)
        {
            foreach (var coord in coords)
            {
                unique.Add(coord.x * 100000 + coord.y);
            }

            var coords_extent = coords
                .Aggregate((v1, v2) => new vec2d(Math.Max(v1.x, v2.x), Math.Max(v1.y, v2.y)));

            var num_rows = coords_extent.y + 1;
            var num_cols = coords_extent.x + 1;
            var dots = new bool[num_rows, num_cols];

            foreach (var v in unique)
            {
                var x = v / 100000;
                var y = v % 100000;
                dots[(int)y, (int)x] = true;
            }

            foreach (var row in Enumerable.Range(0, (int)num_rows))
            {
                foreach (var col in Enumerable.Range(0, (int)num_cols))
                {
                    if (dots[row, col])
                    {
                        Console.Write("#");
                    }
                    else
                    {
                        Console.Write(".");
                    }
                }
                Console.WriteLine();
            }
        }

        return 0;
    }

    public static long day14_part1(List<string> lines, bool is_real)
    {
        var polymer_last = lines[0].ToCharArray();
        var mapping = lines
            .Skip(2)
            .Select(x => x.Split(" -> "))
            .Select(x => new
            {
                pair = x[0].ToCharArray(),
                res = x[1][0]
            })
            .Select(x => new { hash = x.pair[0] * 1000 + x.pair[1], res = x.res })
            .ToDictionary(x => x.hash, x => x.res);
        var num_iter = 10;

        foreach (var iter in Enumerable.Range(1, num_iter))
        {
            var polymer_cur = new char[polymer_last.Length * 2 - 1];
            polymer_cur[0] = polymer_last[0];
            foreach (var idx_last in Enumerable.Range(0, polymer_last.Length - 1))
            {
                var hash_eval = polymer_last[idx_last] * 1000 + polymer_last[idx_last + 1];
                var res = mapping[hash_eval];
                polymer_cur[idx_last * 2 + 1] = res;
                polymer_cur[(idx_last + 1) * 2] = polymer_last[idx_last + 1];
            }
            polymer_last = polymer_cur;
        }

        var s = new string(polymer_last);
        var unique = s.Distinct().ToList();
        var occurences = unique
            .Select(x => s.Count(y => y == x))
            .ToList();

        return occurences.Max() - occurences.Min();
    }

    public static long day14_part2(List<string> lines, bool is_real)
    {
        var polymer_initial = lines[0];
        var mapping = lines
            .Skip(2)
            .Select(x => x.Split(" -> "))
            .Select(x => new
            {
                pair = x[0],
                res = x[1]
            })
            .ToDictionary(x => x.pair, x => x.res);
        var pairs = mapping
            .Select(x => x.Key)
            .ToList();
        var chars = mapping
            .Select(x => x.Value)
            .Distinct()
            .ToList();
        var num_per_char = chars
            .ToDictionary(x => x, x => (long)0);
        var num_per_pair = pairs
            .ToDictionary(x => x, x => (long)0);
        foreach (var c in polymer_initial)
        {
            num_per_char[c.ToString()]++;
        }
        foreach (var pair in polymer_initial.Zip(polymer_initial.Skip(1)))
        {
            var key = pair.First.ToString() + pair.Second.ToString();
            num_per_pair[key]++;
        }

        var num_iter = 40;

        foreach (var iter in Enumerable.Range(0, num_iter))
        {
            var num_per_pair_new = num_per_pair.ToDictionary(x => x.Key, x => (long)0);
            foreach (var pair in num_per_pair)
            {
                var char_from_mapping = mapping[pair.Key];
                num_per_char[char_from_mapping] += pair.Value;
                var key1 = pair.Key[0] + char_from_mapping;
                var key2 = char_from_mapping + pair.Key[1];
                num_per_pair_new[key1] += pair.Value;
                num_per_pair_new[key2] += pair.Value;
            }
            num_per_pair = num_per_pair_new;
        }

        return num_per_char.Select(x => x.Value).Max() - num_per_char.Select(x => x.Value).Min();
    }


    public static long day15_part1(List<string> lines, bool is_real)
    {
        var num_rows = lines.Count;
        var num_cols = lines[0].Length;
        var costs = lines
            .Select(x => x.Select(y => (int)(y - '0')).ToList())
            .ToList();

        var pos_start = new vec2d(0, 0);
        var pos_end = new vec2d(num_cols - 1, num_rows - 1);

        return Functions.a_start(pos_start, pos_end, costs);
    }
}
