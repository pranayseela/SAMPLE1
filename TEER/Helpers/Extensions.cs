using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Web.UI.WebControls;
using TEER.Model;
using NLog;
using Oracle.ManagedDataAccess.Client;

namespace TEER.Helpers
{
    public static class Extensions
    {
        private static readonly Logger _logger = LogManager.GetLogger("Extensions");
        public static bool EqualsEx(this string s, string value)
        {
            return s.Equals(value, StringComparison.OrdinalIgnoreCase);
        }
        public static OracleCommand CreateCommand(this OracleConnection con, string commandText, CommandType commandType = CommandType.StoredProcedure, int? commandTimeout = null)
        {
            OracleCommand cmd = con.CreateCommand();
            cmd.CommandType = commandType;
            cmd.CommandText = commandText;
            if (commandTimeout != null)
            {
                cmd.CommandTimeout = commandTimeout.Value;
            }
            else if (Config.CommandTimeout != null)
            {
                cmd.CommandTimeout = Config.CommandTimeout.Value;
            }
            else
            {
                // default to 60 seconds
                cmd.CommandTimeout = 60;
            }
            cmd.BindByName = true;

            return cmd;
        }
        public static OracleDataReader ExecuteReaderEx(this OracleCommand com, bool logPerformance = false)
        {
            Stopwatch st = null;

            if (logPerformance)
            {
                st = new Stopwatch();
                st.Start();
            }

            try
            {
                OracleDataReader reader = com.ExecuteReader();

                if (logPerformance)
                {
                    st.Stop();
                }

                return reader;
            }
            catch (Exception ex)
            {
                // do not log custom exception raised from oracle
                if (ex is OracleException && (((OracleException)ex).Number >= 20000 && ((OracleException)ex).Number <= 20999))
                {
                    // do nothing
                }
                else
                {
                    _logger.Error("ExecuteReader error. Command {0}. Parameters {1}. Error {2}",
                        com.CommandText,
                        com.GetCommandParametersString(),
                        ex);
                }

                throw;
            }
        }
        public static int ExecuteNonQueryEx(this OracleCommand com, bool logPerformance = false)
        {
            Stopwatch st = null;

            if (logPerformance)
            {
                st = new Stopwatch();
                st.Start();
            }

            try
            {
                int recordsAffected = com.ExecuteNonQuery();

                if (logPerformance)
                {
                    st.Stop();
                }

                return recordsAffected;
            }
            catch (Exception ex)
            {
                // do not log custom exception raised from oracle
                if (ex is OracleException && (((OracleException)ex).Number >= 20000 && ((OracleException)ex).Number <= 20999))
                {
                    // do nothing
                }
                else
                {
                    _logger.Error("ExecuteNonQuery error. Command {0}. Parameters {1}. Error {2}",
                        com.CommandText,
                        com.GetCommandParametersString(),
                        ex);
                }

                throw;
            }
        }
        public static string GetCommandParametersString(this OracleCommand com)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(com.CommandText);

            if (com.CommandType == CommandType.StoredProcedure)
            {
                sb.Append("(");

                string comma = null;
                foreach (OracleParameter item in com.Parameters)
                {
                    if (item.Direction == ParameterDirection.Input || item.Direction == ParameterDirection.InputOutput)
                    {
                        sb.AppendFormat("{0}{1} => {2}", comma, item.ParameterName, item.Value);
                    }
                    else if (item.Direction == ParameterDirection.Output)
                    {
                        sb.AppendFormat("{0}{1} => <out parameter>", comma, item.ParameterName);
                    }

                    comma = ", ";
                }

                sb.Append(")");
            }

            return sb.ToString();
        }
        public static T GetValue<T>(this OracleDataReader dr, string fieldName)
        {
            try
            {
                return Helper.GetValue<T>(dr[fieldName]);
            }
            catch (IndexOutOfRangeException ex)
            {
                throw new IndexOutOfRangeException(string.Format("Unable to find column '{0}' in the result set", fieldName), ex);
            }
        }
        public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> source, string property)
        {
            if (property.IndexOf(',') != -1)
            {
                return OrderBy<T>(source, property.Split(','));
            }
            else if (property.StartsWith("-"))
            {
                return ApplyOrder<T>(source, property.Substring(1).Trim(), "OrderByDescending");
            }
            else
            {
                return ApplyOrder<T>(source, property, "OrderBy");
            }
        }
        public static IOrderedQueryable<T> OrderByDescending<T>(this IQueryable<T> source, string property)
        {
            return ApplyOrder<T>(source, property, "OrderByDescending");
        }
        public static IOrderedQueryable<T> ThenBy<T>(this IOrderedQueryable<T> source, string property)
        {
            return ApplyOrder<T>(source, property, "ThenBy");
        }
        public static IOrderedQueryable<T> ThenByDescending<T>(this IOrderedQueryable<T> source, string property)
        {
            return ApplyOrder<T>(source, property, "ThenByDescending");
        }
        static IOrderedQueryable<T> ApplyOrder<T>(IQueryable<T> source, string property, string methodName)
        {
            string[] props = property.Split('.');
            Type type = typeof(T);
            ParameterExpression arg = Expression.Parameter(type, "x");
            Expression expr = arg;
            foreach (string prop in props)
            {
                // use reflection (not ComponentModel) to mirror LINQ
                PropertyInfo pi = type.GetProperty(prop, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

                // raise error if property is not found
                if (pi == null)
                {
                    throw new Exception(string.Format("Error occurred while sorting by property names. Property '{0}' not found on type {1}", prop, type.FullName));
                }

                expr = Expression.Property(expr, pi);
                type = pi.PropertyType;
            }
            Type delegateType = typeof(Func<,>).MakeGenericType(typeof(T), type);
            LambdaExpression lambda = Expression.Lambda(delegateType, expr, arg);

            object result = typeof(Queryable).GetMethods().Single(
                    method => method.Name == methodName
                            && method.IsGenericMethodDefinition
                            && method.GetGenericArguments().Length == 2
                            && method.GetParameters().Length == 2)
                    .MakeGenericMethod(typeof(T), type)
                    .Invoke(null, new object[] { source, lambda });
            return (IOrderedQueryable<T>)result;
        }
        public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> source, IEnumerable<string> properties)
        {
            // iterate the property names to sort and apply the sort
            IOrderedQueryable<T> sortedData = null;

            foreach (string prop in properties)
            {
                string propName = prop.Trim();
                bool ascending = true;

                if (string.IsNullOrEmpty(propName))
                {
                    continue;
                }

                if (propName.StartsWith("-"))
                {
                    ascending = false;
                    propName = prop.Substring(1).Trim();
                }

                // first iteration
                if (sortedData == null)
                {
                    if (ascending)
                    {
                        sortedData = source.AsQueryable().OrderBy(propName);
                    }
                    else
                    {
                        sortedData = source.AsQueryable().OrderByDescending(propName);
                    }
                }
                else
                {
                    // subsequent iterations...
                    if (ascending)
                    {
                        sortedData = sortedData.ThenBy(propName);
                    }
                    else
                    {
                        sortedData = sortedData.ThenByDescending(propName);
                    }
                }
            }

            return sortedData;
        }
        //public static string ToEffDateFormat(this DateTime date)
        //{
        //    return date.ToString(Helper.EFF_DATE_FORMAT);
        //}
        //public static string ToEffDateTimeFormat(this DateTime date)
        //{
        //    return date.ToString(Helper.EFF_DATE_TIME_FORMAT);
        //}
        //public static string ToEffTimeFormat(this DateTime date)
        //{
        //    return date.ToString(Helper.EFF_TIME_FORMAT);
        //}
    }
}
