using System.Linq;
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
    public static long day4_part1(List<string> lines, bool is_real)
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
        var ret = 0;
        foreach (var idx_draw in Enumerable.Range(0,draws.Count()))
        {
            var draw = draws[idx_draw];
            var indices_with_draw = line_and_cols
                .Select((x, index) => new { x, index })
                .Where(x => x.x.vals.Contains(draw))
                .Select(x => x.index);
            foreach (var idx in indices_with_draw)
            {
                line_and_cols[idx].num_found++;
                if (line_and_cols[idx].num_found == 5)
                {
                    var idx_board_winner=line_and_cols [idx].idx_board;
                    var sum_not_marked = boards[idx_board_winner]
                        .Select(x =>
                            x.Select(y => draws.Take(idx_draw + 1).Contains(y) ? 0 : y).ToList())
                        .ToList()
                        .Sum(x => x.Sum());
                    ret = sum_not_marked * draw;
                    done = true;
                    break;
                }
            }
            if (done)
            {
                break;
            }
        }

        return ret;
    }
}
