using Helper.Core.Library.QueryHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Helper.Core.Library.Translator
{
    internal class WhereTranslator : BaseTranslator
    {
        protected override Expression VisitBinary(BinaryExpression expression)
        {
            this.Visit(expression.Left);
            switch (expression.NodeType)
            {
                case ExpressionType.And:
                    this.stringBuilder.Append(" AND ");
                    break;
                case ExpressionType.Or:
                    this.stringBuilder.Append(" OR");
                    break;
                case ExpressionType.Equal:
                    this.stringBuilder.Append(" = ");
                    break;
                case ExpressionType.NotEqual:
                    this.stringBuilder.Append(" <> ");
                    break;
                case ExpressionType.LessThan:
                    this.stringBuilder.Append(" < ");
                    break;
                case ExpressionType.LessThanOrEqual:
                    this.stringBuilder.Append(" <= ");
                    break;
                case ExpressionType.GreaterThan:
                    this.stringBuilder.Append(" > ");
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    this.stringBuilder.Append(" >= ");
                    break;
                case ExpressionType.AndAlso:
                    this.stringBuilder.Append(" and ");
                    break;
                case ExpressionType.OrElse:
                    this.stringBuilder.Append(" or ");
                    break;
            }
            if (expression.Left.ToString() == expression.Right.ToString())
            {
                this.stringBuilder.Append("@");
            }
            this.Visit(expression.Right);
            return expression;
        }

        protected override Expression VisitConstant(ConstantExpression expression)
        {
            if (expression != null && expression.Value != null)
            {
                this.ConstantProcess(expression.Value, expression.Value.GetType());
            }
            else
            {
                this.ConstantProcess(null, null);
            }
            return expression;
        }

        protected void ConstantProcess(dynamic value, Type type)
        {
            if (value == null || type == null)
            {
                this.stringBuilder.Append("null");
                return;
            }
            TypeCode typeCode = Type.GetTypeCode(type);
            switch (typeCode)
            {
                case TypeCode.Boolean:
                    this.stringBuilder.Append(((bool)value) ? 1 : 0);
                    break;
                case TypeCode.String:
                case TypeCode.DateTime:
                    this.stringBuilder.Append("'");
                    this.stringBuilder.Append(value);
                    this.stringBuilder.Append("'");
                    break;
                default:
                    this.stringBuilder.Append(value);
                    break;
            }
        }

        protected override Expression VisitMethodCall(MethodCallExpression expression)
        {
            dynamic expressionArguments = expression.Arguments;
            if (expression.Method.Name == "Contains" || expression.Method.Name == "StartsWith" || expression.Method.Name == "EndsWith")
            {
                dynamic expressionData = expression.Object;
                if (expressionData != null && expressionData.Expression != null && expressionData.Member != null && expressionArguments.Count > 0)
                {
                    TranslatorMapperItem mapperItem = this.typeDict[expressionData.Expression.Name];
                    string fieldName = TQueryReflectionHelper.GetFieldName(mapperItem.TableType, expressionData.Member.Name);
                    this.stringBuilder.Append(string.Format(TQueryHelperTemplateEnum.TABLE_FIELD, mapperItem.TableName, fieldName));

                    string expressionParam = null;
                    if(expressionArguments[0].NodeType == ExpressionType.MemberAccess)
                    {
                        expressionParam = Expression.Lambda<Func<string>>(Expression.Convert(expressionArguments[0], typeof(string))).Compile().Invoke();
                    }
                    else
                    {
                        expressionParam = expressionArguments[0].Value.ToString().Trim();
                    }
                    if (expressionParam.IndexOf("@") >= 0)
                    {
                        this.stringBuilder.Append(" = ");
                        this.stringBuilder.Append(expressionParam);
                        this.stringBuilder.Append(" ");
                    }
                    else
                    {
                        this.stringBuilder.Append(" like '");
                        if (expression.Method.Name != "StartsWith") this.stringBuilder.Append("%");
                        this.stringBuilder.Append(expressionParam);
                        if (expression.Method.Name != "EndsWith") this.stringBuilder.Append("%");
                        this.stringBuilder.Append("' ");
                    }
                }
            }
            return expression;
        }

        protected override Expression VisitMember(MemberExpression expression)
        {
            if (expression.Expression == null && expression.Type == typeof(DateTime))
            {
                if (expression.Member is PropertyInfo)
                {
                    var value = (expression.Member as PropertyInfo).GetValue(expression);
                    this.ConstantProcess(value, expression.Type);
                }
            }
            else
            {
                if (expression.Expression != null)
                {
                    dynamic expressionMember = expression.Expression;
                    if (expression.Expression.NodeType == ExpressionType.Parameter)
                    {
                        TranslatorMapperItem mapperItem = this.typeDict[expressionMember.Name];
                        string fieldName = TQueryReflectionHelper.GetFieldName(mapperItem.TableType, expression.Member.Name);

                        if (!this.stringBuilder.ToString().EndsWith("@"))
                        {
                            this.stringBuilder.Append(string.Format(TQueryHelperTemplateEnum.TABLE_FIELD, mapperItem.TableName, fieldName));
                        }
                        else
                        {
                            this.stringBuilder.Append(expression.Member.Name);
                        }
                    }
                    else if (expression.Expression.NodeType == ExpressionType.Constant)
                    {
                        if (expression.Member is FieldInfo)
                        {
                            var value = (expression.Member as FieldInfo).GetValue(expressionMember.Value);
                            this.ConstantProcess(value, expression.Type);
                        }
                        if (expression.Member is PropertyInfo)
                        {
                            var value = (expression.Member as PropertyInfo).GetValue(expressionMember.Value);
                            this.ConstantProcess(value, expression.Type);
                        }
                    }
                }
            }
            return expression;
        }
    }
}
