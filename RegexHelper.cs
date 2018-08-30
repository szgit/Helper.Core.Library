/*
 * 作用：常用正则表达式验证。
 * */
using System.Text.RegularExpressions;

namespace Helper.Core.Library
{
    #region 逻辑处理辅助枚举
    public enum RegexTypeEnum : int
    {
        /// <summary>
        /// 成功
        /// </summary>
        Success = 0,

        /// <summary>
        /// 范围不正确
        /// </summary>
        Range = 1,

        /// <summary>
        /// 长度
        /// </summary>
        Length = 2,

        /// <summary>
        /// 失败
        /// </summary>
        Failed = 3
    }
    #endregion

    public class RegexHelper
    {
        #region 对外公开方法
        /// <summary>
        /// 匹配，不区分大小写
        /// </summary>
        /// <param name="input"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public static bool IsMatch(string input, string pattern)
        {
            return Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase);
        }

        #region RegexTypeEnum

        /// <summary>
        /// 验证整数
        /// </summary>
        /// <param name="input">要验证的字符串</param>
        /// <param name="min">最小值/长度</param>
        /// <param name="max">最大值/长度</param>
        /// <param name="isRange">是否范围，false 按长度检查，true 按数值范围检查</param>
        /// <returns></returns>
        public static RegexTypeEnum Int(string input, int min = 0, int max = 0, bool isRange = false)
        {
            bool matchStatus = IsMatch(input, @"^-?[0-9]+$");
            if (!matchStatus) return RegexTypeEnum.Failed;
            if (max > 0 && max >= min)
            {
                if (!isRange)
                {
                    matchStatus = IsMatch(input, @"^-?[0-9]{m,n}$".Replace("m", min.ToString()).Replace("n", max.ToString()));
                    if (!matchStatus) return RegexTypeEnum.Length;
                }
                else
                {
                    if (int.Parse(input) < min || int.Parse(input) > max) return RegexTypeEnum.Range;
                }
            }
            return RegexTypeEnum.Success;
        }
        /// <summary>
        /// 验证浮点数
        /// </summary>
        /// <param name="input">要验证的字符串</param>
        /// <param name="dotMin">小数点最小长度</param>
        /// <param name="dotMax">小数点最大长度</param>
        /// <param name="min">最小值/长度</param>
        /// <param name="max">最大值/长度</param>
        /// <param name="isRange">是否范围，false 按长度检查，true 按数值范围检查</param>
        /// <returns></returns>
        public static RegexTypeEnum Float(string input, int dotMin = 0, int dotMax = 0, float min = 0f, float max = 0f, bool isRange = false)
        {
            string pattern = @"^-?[0-9]+([.][0-9]+)?$";
            if (dotMax > 0 && dotMin > 0 && dotMin <= dotMax) pattern = @"^-?[0-9]+[.][0-9]{m,n}$".Replace("m", dotMin.ToString()).Replace("n", dotMax.ToString());
            bool matchStatus = IsMatch(input, pattern);
            if (!matchStatus) return RegexTypeEnum.Failed;
            if (max > 0f && max >= min)
            {
                if (!isRange)
                {
                    pattern = @"^-?[0-9]{m,n}([.][0-9]+)?$".Replace("m", min.ToString("f0")).Replace("n", max.ToString("f0"));
                    if (dotMax > 0 && dotMin > 0 && dotMin <= dotMax) pattern = @"^-?[0-9]{m,n}[.][0-9]{a,b}$".Replace("m", min.ToString("f0")).Replace("n", max.ToString("f0")).Replace("a", dotMin.ToString()).Replace("b", dotMax.ToString());
                    matchStatus = IsMatch(input, pattern);
                    if (!matchStatus) return RegexTypeEnum.Length;
                }
                else
                {
                    if (float.Parse(input) < min || float.Parse(input) > max) return RegexTypeEnum.Range;
                }
            }
            return RegexTypeEnum.Success;
        }
        /// <summary>
        /// 验证字母
        /// </summary>
        /// <param name="input">要验证的字符串</param>
        /// <param name="min">最小长度</param>
        /// <param name="max">最大长度</param>
        /// <returns></returns>
        public static RegexTypeEnum Alphabet(string input, int min = 0, int max = 0)
        {
            bool matchStatus = IsMatch(input, @"^[a-zA-Z]+$");
            if (!matchStatus) return RegexTypeEnum.Failed;
            if (max > 0 && max >= min)
            {
                matchStatus = IsMatch(input, @"^[a-zA-Z]{m,n}$".Replace("m", min.ToString()).Replace("n", max.ToString()));
                if (!matchStatus) return RegexTypeEnum.Length;
            }
            return RegexTypeEnum.Success;
        }
        /// <summary>
        /// 验证字母/特殊字符
        /// </summary>
        /// <param name="input">要验证的字符串</param>
        /// <param name="spec">特殊字符</param>
        /// <param name="min">最小长度</param>
        /// <param name="max">最大长度</param>
        /// <returns></returns>
        public static RegexTypeEnum Alphabet(string input, string spec, int min = 0, int max = 0)
        {
            bool matchStatus = IsMatch(input, @"^[a-zA-Z*]+$".Replace("*", spec));
            if (!matchStatus) return RegexTypeEnum.Failed;
            if (max > 0 && max >= min)
            {
                matchStatus = IsMatch(input, @"^[a-zA-Z*]{m,n}$".Replace("*", spec).Replace("m", min.ToString()).Replace("n", max.ToString()));
                if (!matchStatus) return RegexTypeEnum.Length;
            }
            return RegexTypeEnum.Success;
        }
        /// <summary>
        /// 验证字母数字
        /// </summary>
        /// <param name="input">要验证的字符串</param>
        /// <param name="min">最小长度</param>
        /// <param name="max">最大长度</param>
        /// <returns></returns>
        public static RegexTypeEnum AlphabetDigit(string input, int min = 0, int max = 0)
        {
            bool matchStatus = IsMatch(input, @"^[a-zA-Z0-9]+$");
            if (!matchStatus) return RegexTypeEnum.Failed;
            if (max > 0 && max >= min)
            {
                matchStatus = IsMatch(input, @"^[a-zA-Z0-9]{m,n}$".Replace("m", min.ToString()).Replace("n", max.ToString()));
                if (!matchStatus) return RegexTypeEnum.Length;
            }
            return RegexTypeEnum.Success;
        }
        /// <summary>
        /// 验证字母数字/特殊字符
        /// </summary>
        /// <param name="input">要验证的字符串</param>
        /// <param name="spec">特殊字符</param>
        /// <param name="min">最小长度</param>
        /// <param name="max">最大长度</param>
        /// <returns></returns>
        public static RegexTypeEnum AlphabetDigit(string input, string spec, int min = 0, int max = 0)
        {
            bool matchStatus = IsMatch(input, @"^[a-zA-Z0-9*]+$".Replace("*", spec));
            if (!matchStatus) return RegexTypeEnum.Failed;
            if (max > 0 && max >= min)
            {
                matchStatus = IsMatch(input, @"^[a-zA-Z0-9*]{m,n}$".Replace("*", spec).Replace("m", min.ToString()).Replace("n", max.ToString()));
                if (!matchStatus) return RegexTypeEnum.Length;
            }
            return RegexTypeEnum.Success;
        }
        /// <summary>
        /// 验证汉字
        /// </summary>
        /// <param name="input">要验证的字符串</param>
        /// <param name="min">最小长度</param>
        /// <param name="max">最大长度</param>
        /// <returns></returns>
        public static RegexTypeEnum Chinese(string input, int min = 0, int max = 0)
        {
            bool matchStatus = IsMatch(input, @"^[\u4e00-\u9fa5]+$");
            if (!matchStatus) return RegexTypeEnum.Failed;
            if (max > 0 && max >= min)
            {
                matchStatus = IsMatch(input, @"^[\u4e00-\u9fa5]{m,n}$".Replace("m", min.ToString()).Replace("n", max.ToString()));
                if (!matchStatus) return RegexTypeEnum.Length;
            }
            return RegexTypeEnum.Success;
        }
        /// <summary>
        /// 验证汉字/特殊字符
        /// </summary>
        /// <param name="input">要验证的字符串</param>
        /// <param name="spec">特殊字符</param>
        /// <param name="min">最小长度</param>
        /// <param name="max">最大长度</param>
        /// <returns></returns>
        public static RegexTypeEnum Chinese(string input, string spec, int min = 0, int max = 0)
        {
            bool matchStatus = IsMatch(input, @"^[\u4e00-\u9fa5*]+$".Replace("*", spec));
            if (!matchStatus) return RegexTypeEnum.Failed;
            if (max > 0 && max >= min)
            {
                matchStatus = IsMatch(input, @"^[\u4e00-\u9fa5*]{m,n}$".Replace("*", spec).Replace("m", min.ToString()).Replace("n", max.ToString()));
                if (!matchStatus) return RegexTypeEnum.Length;
            }
            return RegexTypeEnum.Success;
        }
        /// <summary>
        /// 验证双字节
        /// </summary>
        /// <param name="input">要验证的字符串</param>
        /// <param name="min">最小长度</param>
        /// <param name="max">最大长度</param>
        /// <returns></returns>
        public static RegexTypeEnum DoubleByte(string input, int min = 0, int max = 0)
        {
            bool matchStatus = IsMatch(input, @"^[^\x00-\xff]+$");
            if (!matchStatus) return RegexTypeEnum.Failed;
            if (max > 0 && max >= min)
            {
                matchStatus = IsMatch(input, @"^[^\x00-\xff]{m,n}$".Replace("m", min.ToString()).Replace("n", max.ToString()));
                if (!matchStatus) return RegexTypeEnum.Length;
            }
            return RegexTypeEnum.Success;
        }
        /// <summary>
        /// 验证中文英文数字
        /// </summary>
        /// <param name="input">要验证的字符串</param>
        /// <param name="min">最小长度</param>
        /// <param name="max">最大长度</param>
        /// <returns></returns>
        public static RegexTypeEnum Word(string input, int min = 0, int max = 0)
        {
            bool matchStatus = IsMatch(input, @"^[\u4e00-\u9fa5A-Za-z0-9]+$");
            if (!matchStatus) return RegexTypeEnum.Failed;
            if (max > 0 && max >= min)
            {
                matchStatus = IsMatch(input, @"^[\u4e00-\u9fa5A-Za-z0-9]{m,n}$".Replace("m", min.ToString()).Replace("n", max.ToString()));
                if (!matchStatus) return RegexTypeEnum.Length;
            }
            return RegexTypeEnum.Success;
        }
        /// <summary>
        /// 验证中文英文数字/特殊字符
        /// </summary>
        /// <param name="input">要验证的字符串</param>
        /// <param name="spec">特殊字符</param>
        /// <param name="min">最小长度</param>
        /// <param name="max">最大长度</param>
        /// <returns></returns>
        public static RegexTypeEnum Word(string input, string spec, int min = 0, int max = 0)
        {
            bool matchStatus = IsMatch(input, @"^[\u4e00-\u9fa5A-Za-z0-9*]+$".Replace("*", spec));
            if (!matchStatus) return RegexTypeEnum.Failed;
            if (max > 0 && max >= min)
            {
                matchStatus = IsMatch(input, @"^[\u4e00-\u9fa5A-Za-z0-9*]{m,n}$".Replace("*", spec).Replace("m", min.ToString()).Replace("n", max.ToString()));
                if (!matchStatus) return RegexTypeEnum.Length;
            }
            return RegexTypeEnum.Success;
        }

        #endregion

        #region Boolean

        /// <summary>
        /// 验证是否整数
        /// </summary>
        /// <param name="input">要验证的字符串</param>
        /// <returns></returns>
        public static bool IsInt(string input)
        {
            return Int(input) == RegexTypeEnum.Success;
        }
        /// <summary>
        /// 验证是否符点数
        /// </summary>
        /// <param name="input">要验证的字符串</param>
        /// <returns></returns>
        public static bool IsFloat(string input)
        {
            return Float(input) == RegexTypeEnum.Success;
        }
        /// <summary>
        /// 验证是否字母/特殊字符
        /// </summary>
        /// <param name="input">要验证的字符串</param>
        /// <param name="spec">特殊字符</param>
        /// <returns></returns>
        public static bool IsAlphabet(string input, string spec = null)
        {
            if (string.IsNullOrEmpty(spec)) return Alphabet(input) == RegexTypeEnum.Success;
            return Alphabet(input, spec) == RegexTypeEnum.Success;
        }
        /// <summary>
        /// 验证是否字母数字/特殊字符
        /// </summary>
        /// <param name="input">要验证的字符串</param>
        /// <param name="spec">特殊字符</param>
        /// <returns></returns>
        public static bool IsAlphabetDigit(string input, string spec = null)
        {
            if (string.IsNullOrEmpty(spec)) return AlphabetDigit(input) == RegexTypeEnum.Success;
            return AlphabetDigit(input, spec) == RegexTypeEnum.Success;
        }
        /// <summary>
        /// 验证是否汉字/特殊字符
        /// </summary>
        /// <param name="input">要验证的字符串</param>
        /// <param name="spec">特殊字符</param>
        /// <returns></returns>
        public static bool IsChinese(string input, string spec = null)
        {
            if (string.IsNullOrEmpty(spec)) return Chinese(input) == RegexTypeEnum.Success;
            return Chinese(input, spec) == RegexTypeEnum.Success;
        }
        /// <summary>
        /// 验证是否双字节
        /// </summary>
        /// <param name="input">要验证的字符串</param>
        /// <returns></returns>
        public static bool IsDoubleByte(string input)
        {
            return DoubleByte(input) == RegexTypeEnum.Success;
        }
        /// <summary>
        /// 验证是否中文英文数字/特殊字符
        /// </summary>
        /// <param name="input">要验证的字符串</param>
        /// <param name="spec">特殊字符</param>
        /// <returns></returns>
        public static bool IsWord(string input, string spec = null)
        {
            if (string.IsNullOrEmpty(spec)) return Word(input) == RegexTypeEnum.Success;
            return Word(input, spec) == RegexTypeEnum.Success;
        }
        /// <summary>
        /// 验证是否邮件
        /// </summary>
        /// <param name="input">要验证的字符串</param>
        /// <returns></returns>
        public static bool IsEmail(string input)
        {
            return IsMatch(input, @"^[a-z0-9]+([._-]*[a-z0-9]+)*@[a-z0-9]+([.][a-z0-9]+)*$");
        }
        /// <summary>
        /// 验证是否短日期 yyyy-MM-dd
        /// </summary>
        /// <param name="input">要验证的字符串</param>
        /// <returns></returns>
        public static bool IsDate(string input)
        {
            string pattern = @"^(((01[0-9]{2}|0[2-9][0-9]{2}|[1-9][0-9]{3})-(0?[13578]|1[02])-(0?[1-9]|[12]\d|3[01]))|((01[0-9]{2}|0[2-9][0-9]{2}|[1-9][0-9]{3})-(0?[13456789]|1[012])-(0?[1-9]|[12]\d|30))|((01[0-9]{2}|0[2-9][0-9]{2}|[1-9][0-9]{3})-0?2-(0?[1-9]|1\d|2[0-8]))|(((1[6-9]|[2-9]\d)(0[48]|[2468][048]|[13579][26])|((04|08|12|16|[2468][048]|[3579][26])00))-0?2-29))$";
            return IsMatch(input, pattern);
        }
        /// <summary>
        /// 验证是否长日期 yyyy-MM-dd hh:mm:ss
        /// </summary>
        /// <param name="input">要验证的字符串</param>
        /// <returns></returns>
        public static bool IsFullDate(string input)
        {
            string pattern = @"^(((01[0-9]{2}|0[2-9][0-9]{2}|[1-9][0-9]{3})-(0?[13578]|1[02])-(0?[1-9]|[12]\d|3[01]))|((01[0-9]{2}|0[2-9][0-9]{2}|[1-9][0-9]{3})-(0?[13456789]|1[012])-(0?[1-9]|[12]\d|30))|((01[0-9]{2}|0[2-9][0-9]{2}|[1-9][0-9]{3})-0?2-(0?[1-9]|1\d|2[0-8]))|(((1[6-9]|[2-9]\d)(0[48]|[2468][048]|[13579][26])|((04|08|12|16|[2468][048]|[3579][26])00))-0?2-29)) (20|21|22|23|[0-1]?\d):[0-5]?\d:[0-5]?\d$";
            return IsMatch(input, pattern);
        }
        /// <summary>
        /// 验证是否是脚本
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool IsScript(string input)
        {
            string pattern = @"[<>*]+";
            return IsMatch(input, pattern);
        }
        
        #endregion

        #endregion
    }
}
