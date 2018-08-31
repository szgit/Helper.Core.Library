/*
 * 作用：利用 MSScriptControl 组件实现公式计算，添加引用之后，右键【属性】并设置嵌入互操作类型为 False。
 * */
using Helper.Core.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helper.Core.Library
{
    public class CalcHelper
    {
        #region 私有属性常量
        private const string FormulaNullException = "formula 数据为空！";
        #endregion

        #region 对外公开方法
        /// <summary>
        /// 公式计算
        /// </summary>
        /// <typeparam name="T">普通类型数据，例：int</typeparam>
        /// <param name="formula">公式，例：function Calc(x, y) { return x + y; }</param>
        /// <param name="parameterList">参数列表，Dictionary&lt;string, object&gt; 或 new {}</param>
        /// <returns></returns>
        public static T Calc<T>(string formula, object parameterList = null)
        {
            if (string.IsNullOrWhiteSpace(formula)) throw new Exception(FormulaNullException);

            #region 参数数据映射
            Dictionary<string, object> paramDict = CommonHelper.GetParameterDict(parameterList);
            #endregion

            #region 公式参数映射
            // 如果存在参数信息
            if (paramDict != null)
            {
                foreach(KeyValuePair<string, object> keyValueItem in paramDict)
                {
                    formula = formula.Replace(keyValueItem.Key, keyValueItem.Value.ToString());
                }
            }
            #endregion

            #region 计算公式结果
            // 使用 COM 组件计算公式
            MSScriptControl.ScriptControl scriptControl = new MSScriptControl.ScriptControlClass()
            {
                Language = "JavaScript"
            };
            object value = scriptControl.Eval(formula);
            #endregion

            return (T)Convert.ChangeType(value, typeof(T));
        }
        #endregion
    }
}
