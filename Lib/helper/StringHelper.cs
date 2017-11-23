using System;
using System.Text;
using System.Collections;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace Lib.helper
{
    /// <summary>
    /// 字符串帮助类
    /// </summary>
    public static class StringHelper
    {
        /// <summary>
        /// join 不会返回空
        /// </summary>
        public static string Join(string spliter, IList<string> list)
        {
            string str = string.Empty;
            if (!ValidateHelper.IsPlumpList(list)) { return str; }
            list.ToList().ForEach(delegate (string s)
            {
                s = ConvertHelper.GetString(s);
                str += ((str == string.Empty) ? string.Empty : spliter) + s;
            });
            return str;
        }

        /// <summary>
        /// 可能返回空
        /// </summary>
        /// <param name="str"></param>
        /// <param name="spliter"></param>
        /// <returns></returns>
        public static List<string> Split(string str, params char[] spliter)
        {
            return ConvertHelper.GetString(str).Split(spliter).ToList();
        }

        #region 截取字符串

        /// <summary>
        /// 截取字符串
        /// </summary>
        public static string SubString(string str, int startIndex, int length)
        {
            str = ConvertHelper.GetString(str);
            if (str.Length > (startIndex + length))
            {
                return str.Substring(startIndex, length);
            }
            return str;
        }

        /// <summary>
        /// 截取字符串
        /// </summary>
        public static string SubString(string str, int length)
        {
            return SubString(str, 0, length);
        }

        #endregion

        #region 移除前导/后导字符串

        /// <summary>
        /// 移除前导字符串
        /// </summary>
        public static string TrimStart(string str, string trimStr, bool ignoreCase = true)
        {
            str = ConvertHelper.GetString(str);
            trimStr = ConvertHelper.GetString(trimStr);

            while (str.StartsWith(trimStr, ignoreCase, CultureInfo.CurrentCulture))
            {
                str = str.Remove(0, trimStr.Length);
            }

            return str;
        }

        /// <summary>
        /// 移除后导字符串
        /// </summary>
        public static string TrimEnd(string str, string trimStr, bool ignoreCase = true)
        {
            str = ConvertHelper.GetString(str);
            trimStr = ConvertHelper.GetString(trimStr);

            while (str.EndsWith(trimStr, ignoreCase, CultureInfo.CurrentCulture))
            {
                str = str.Substring(0, str.Length - trimStr.Length);
            }

            return str;
        }

        /// <summary>
        /// 移除前导和后导字符串
        /// </summary>
        public static string Trim(string str, string trimStr = " ", bool ignoreCase = true)
        {
            return TrimStart(TrimEnd(str, trimStr, ignoreCase), trimStr, ignoreCase);
        }

        #endregion
        
        #region 字符串相似度比较

        /// <summary>
        /// 计算字符相似度
        /// </summary>
        /// <param name="str1"></param>
        /// <param name="str2"></param>
        /// <returns></returns>
        private static int Levenshtein_Distance(string str1, string str2)
        {
            int[,] Matrix;
            int n = str1.Length;
            int m = str2.Length;

            int temp = 0;
            char ch1;
            char ch2;
            int i = 0;
            int j = 0;
            if (n == 0)
            {
                return m;
            }
            if (m == 0)
            {
                return n;
            }
            Matrix = new int[n + 1, m + 1];
            for (i = 0; i <= n; i++)
            {
                //初始化第一列
                Matrix[i, 0] = i;
            }
            for (j = 0; j <= m; j++)
            {
                //初始化第一行
                Matrix[0, j] = j;
            }
            for (i = 1; i <= n; i++)
            {
                ch1 = str1[i - 1];
                for (j = 1; j <= m; j++)
                {
                    ch2 = str2[j - 1];
                    if (ch1.Equals(ch2))
                    {
                        temp = 0;
                    }
                    else
                    {
                        temp = 1;
                    }
                    Matrix[i, j] = new int[] { Matrix[i - 1, j] + 1, Matrix[i, j - 1] + 1, Matrix[i - 1, j - 1] + temp }.Min();
                }
            }
            return Matrix[n, m];

        }

        /// <summary>
        /// 计算字符串相似度
        /// </summary>
        /// <param name="str1"></param>
        /// <param name="str2"></param>
        /// <returns></returns>
        public static decimal LevenshteinDistancePercent(string str1, string str2)
        {
            int val = Levenshtein_Distance(str1, str2);
            return 1 - (decimal)val / new int[] { str1.Length, str2.Length }.Max();
        }

        #endregion

        #region 计算匹配率/相似度
        /// <summary>
        /// 计算相似度。
        /// </summary>
        public static SimilarityResult SimilarityRate(string str1, string str2)
        {
            var result = new SimilarityResult();
            var arrChar1 = str1.ToCharArray();
            var arrChar2 = str2.ToCharArray();
            var computeTimes = 0;
            var row = arrChar1.Length + 1;
            var column = arrChar2.Length + 1;
            var matrix = new int[row, column];
            //初始化矩阵的第一行和第一列
            for (var i = 0; i < column; i++)
            {
                matrix[0, i] = i;
            }
            for (var i = 0; i < row; i++)
            {
                matrix[i, 0] = i;
            }
            for (var i = 1; i < row; i++)
            {
                for (var j = 1; j < column; j++)
                {
                    var intCost = 0;
                    intCost = arrChar1[i - 1] == arrChar2[j - 1] ? 0 : 1;
                    //关键步骤，计算当前位置值为左边+1、上面+1、左上角+intCost中的最小值 
                    //循环遍历到最后_Matrix[_Row - 1, _Column - 1]即为两个字符串的距离
                    matrix[i, j] = new int[] { matrix[i - 1, j] + 1, matrix[i, j - 1] + 1, matrix[i - 1, j - 1] + intCost }.Min();
                    computeTimes++;
                }
            }
            //相似率 移动次数小于最长的字符串长度的20%算同一题
            var intLength = row > column ? row : column;
            //_Result.Rate = (1 - (double)_Matrix[_Row - 1, _Column - 1] / intLength).ToString().Substring(0, 6);
            result.Rate = (1 - (double)matrix[row - 1, column - 1] / (intLength - 1));
            result.ComputeTimes = computeTimes.ToString() + " 距离为：" + matrix[row - 1, column - 1].ToString();
            return result;
        }

        /// <summary>
        /// 计算结果
        /// </summary>
        public struct SimilarityResult
        {
            /// <summary>
            /// 相似度，0.54即54%。
            /// </summary>
            public double Rate;
            /// <summary>
            /// 对比次数
            /// </summary>
            public string ComputeTimes;
        }
        #endregion
    }
}
