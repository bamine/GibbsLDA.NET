using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GibbsLDA.NET
{
    public class ArrayInitializers
    {
        public static double[][] Empty(int m, int n)
        {
            var r = new double[m][];
            for (int i = 0; i < m; i++)
            {
                r[i] = new double[n];
            }
            return r;
        }

        public static double[][] Zeros(int m, int n)
        {
            var r = new double[m][];
            for (int i = 0; i < m; i++)
            {
                r[i] = new double[n];
                for (int j = 0; j < n; j++)
                {
                    r[i][j] = 0.0;
                }
            }
            return r;
        }

        public static int[][] EmptyInt(int m, int n)
        {
            var r = new int[m][];
            for (int i = 0; i < m; i++)
            {
                r[i] = new int[n];
            }
            return r;
        }

        public static int[][] ZerosInt(int m, int n)
        {
            var r = new int[m][];
            for (int i = 0; i < m; i++)
            {
                r[i] = new int[n];
                for (int j = 0; j < n; j++)
                {
                    r[i][j] = 0;
                }
            }
            return r;
        }

        public static double[] Zeros(int m)
        {
            var r = new double[m];
            for (int i = 0; i < m; i++)
            {
                r[i] = 0.0;
            }
            return r;
        }

        public static int[] ZerosInt(int m)
        {
            var r = new int[m];
            for (int i = 0; i < m; i++)
            {
                r[i] = 0;
            }
            return r;
        }


    }
}
