using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aoc_common
{
    public struct vec2d
    {
        public vec2d(long newx, long newy)
        {
            x = newx;
            y = newy;
        }

        public static bool operator ==(vec2d a, vec2d b)
        {
            return (a.x == b.x) && (a.y == b.y);
        }

        public static bool operator !=(vec2d a, vec2d b)
        {
            return !(a == b);
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
    public class Functions
    {

        /// <summary>
        /// Copies a slice of one vector into another. Always starting at row/col 0/0 in destination vector
        /// </summary>
        /// <param name="vals_in"></param>
        /// <param name="row_offset"></param>
        /// <param name="col_offset"></param>
        /// <param name="rows"></param>
        /// <param name="cols"></param>
        /// <param name="vals_out"></param>
        /// <returns></returns>
        public static bool slice(ref int[,] vals_in, int row_offset, int col_offset, int rows, int cols, ref int[,] vals_out)
        {
            var num_rows = vals_in.GetLength(0);
            var num_cols = vals_in.GetLength(1);

            if (row_offset + rows >= num_rows)
            {
                return false;
            }

            if (col_offset + cols >= num_cols)
            {
                return false;
            }

            vals_out = new int[rows, cols];

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    vals_out[row, col] = vals_in[row + row_offset, col + col_offset];
                }
            }

            return true;
        }

        public static bool slice(ref List<List<int>> vals_in, int row_offset, int col_offset, int rows, int cols, ref List<List<int>> vals_out)
        {
            var num_rows = vals_in.Count;
            var num_cols = vals_in[0].Count;

            if (row_offset + rows > num_rows)
            {
                return false;
            }

            if (col_offset + cols > num_cols)
            {
                return false;
            }

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    vals_out[row][col] = vals_in[row + row_offset][col + col_offset];
                }
            }

            return true;
        }

        /// <summary>
        /// Input should be -1 for inaccessible squares and zero for squares to segment
        /// </summary>
        /// <param name="vals_in"></param>
        /// <param name="vals_out"></param>
        /// <returns></returns>
        public static bool floodfill(in int[,] vals_in, ref int[,] vals_out, ref List<int> segment_sizes)
        {
            var num_rows_orig = vals_in.GetLength(0);
            var num_cols_orig = vals_in.GetLength(1);
            var segments_with_border = new int[num_rows_orig + 2, num_cols_orig + 2];
            var val_inaccessible = -10;
            var val_unassigned = -1;

            for (var row = 0; row < num_rows_orig + 2; row++)
            {
                for (var col = 0; col < num_cols_orig + 2; col++)
                {
                    var on_edge = (row == 0) || (row == num_rows_orig + 1) || (col == 0) || (col == num_cols_orig + 1);
                    var val = val_inaccessible;
                    if (!on_edge)
                    {
                        val = vals_in[row - 1, col - 1] == 0 ? val_unassigned : val_inaccessible;
                    }
                    segments_with_border[row, col] = val;
                }
            }

            var num_segments = 0;

            for (var row = 1; row < num_rows_orig + 1; row++)
            {
                for (var col = 1; col < num_cols_orig + 1; col++)
                {
                    if (segments_with_border[row, col] != val_unassigned)
                    {
                        continue;
                    }
                    var diffs = new List<vec2d>
                {
                    new vec2d(-1,0),
                    new vec2d(1,0),
                    new vec2d(0,-1),
                    new vec2d(0,1),
                };
                    segments_with_border[row, col] = num_segments;
                    var heads = new List<vec2d> { new vec2d(col, row) };
                    var idx_start = 0;
                    var idx_end_excl = heads.Count;
                    var segment_size = heads.Count;
                    var done = false;
                    while (!done)
                    {
                        var num_added = 0;
                        for (var idx = idx_start; idx < idx_end_excl; idx++)
                        {
                            var el = heads[idx];
                            foreach (var diff in diffs)
                            {
                                var row_eval = el.y + diff.y;
                                var col_eval = el.x + diff.x;
                                var val = segments_with_border[row_eval, col_eval];

                                if (val == val_unassigned)
                                {
                                    segments_with_border[row_eval, col_eval] = num_segments;
                                    segment_size++;
                                    heads.Add(new vec2d(col_eval, row_eval));
                                    num_added++;
                                }
                            }
                        }
                        done = num_added == 0;
                        idx_start = idx_end_excl;
                        idx_end_excl = idx_start + num_added;
                    }
                    segment_sizes.Add(segment_size);
                    num_segments++;
                }
            }

            vals_out = new int[num_rows_orig, num_cols_orig];
            return slice(ref segments_with_border, 1, 1, num_rows_orig, num_cols_orig, ref vals_out);
        }

        public static long a_start(vec2d pos_start, vec2d pos_end, List<List<int>> costs)
        {
            var num_rows = costs.Count;
            var num_cols = costs[0].Count;
            var least_costs = costs
                .Select(x => x.Select(y => int.MaxValue).ToList())
                .ToList();
            var queue = new PriorityQueue<vec2d, int>();
            queue.Enqueue(pos_start, 0);
            var max_iter = 10000000;
            var pos_cur = new vec2d();
            var cur_priority = 0;

            foreach (var iter in Enumerable.Range(0, max_iter))
            {
                if (!queue.TryDequeue(out pos_cur, out cur_priority))
                {
                    // We did not reach the end
                    return -1;
                }

                if (pos_cur == pos_end)
                {
                    return cur_priority;
                }

                var to_eval = new List<vec2d>();

                if (pos_cur.x > 0)
                {
                    to_eval.Add(new vec2d(pos_cur.x - 1, pos_cur.y));
                }
                if (pos_cur.x < num_cols - 1)
                {
                    to_eval.Add(new vec2d(pos_cur.x + 1, pos_cur.y));
                }
                if (pos_cur.y > 0)
                {
                    to_eval.Add(new vec2d(pos_cur.x, pos_cur.y - 1));
                }
                if (pos_cur.y < num_rows - 1)
                {
                    to_eval.Add(new vec2d(pos_cur.x, pos_cur.y + 1));
                }
                foreach (var v_eval in to_eval)
                {
                    var prio_eval = cur_priority + costs[(int)v_eval.y][(int)v_eval.x];

                    if (least_costs[(int)v_eval.y][(int)v_eval.x] > prio_eval)
                    {
                        queue.Enqueue(v_eval, prio_eval);
                        least_costs[(int)v_eval.y][(int)v_eval.x] = prio_eval;
                    }
                }
            }

            return 0;
        }
    }
}
