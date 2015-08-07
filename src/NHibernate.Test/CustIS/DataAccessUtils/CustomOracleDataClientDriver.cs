using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using CustIS.TradeNets.NHibernate.ApplicationBootstrap.DataAccessUtils.OracleTypes;
using NHibernate.AdoNet;
using NHibernate.Driver;
using NHibernate.Engine;
using NHibernate.SqlCommand;
using NHibernate.SqlTypes;
using NHibernate.Type;
using NHibernate.Util;

namespace NHibernate.Test.CustIS.DataAccessUtils
{
    /// <summary> Драйвер соединения с БД. </summary>
    /// <remarks>
    ///     Данный драйвер является немного модифицированным драйвером Oracle,
    ///     который находится в NHibernate. Модификации включают в себя изменение
    ///     и оптимизацию некоторых свойств соединения и команд.
    /// </remarks>
    public class CustomOracleDataClientDriver : OracleDataClientDriver, IEmbeddedBatcherFactoryProvider
    {
        private const string driverAssemblyName = "Oracle.DataAccess";
        private readonly PropertyInfo _oracleCommandFetchSizeProp;
        private readonly PropertyInfo _oracleDbTypeProp;
        private readonly PropertyInfo _udtTypeNameProp;
        private readonly PropertyInfo _arrayBindStatusProp;
        private readonly System.Type _parameterType;
        private readonly System.Type _oracleDbTypeEnum;
        private readonly System.Type _oracleParameterStatusEnum;

        private readonly long? _fetchSize = null;

        /// <summary> Драйвер соединения с БД. </summary>
        public CustomOracleDataClientDriver()
        {
            System.Type oracleCommandType = ReflectHelper.TypeFromAssembly("Oracle.DataAccess.Client.OracleCommand", driverAssemblyName, false);
            _oracleCommandFetchSizeProp = oracleCommandType.GetProperty("FetchSize");

            _parameterType = ReflectHelper.TypeFromAssembly("Oracle.DataAccess.Client.OracleParameter", driverAssemblyName, false);
            _oracleDbTypeProp = _parameterType.GetProperty("OracleDbType");
            _arrayBindStatusProp = _parameterType.GetProperty("ArrayBindStatus");
            _udtTypeNameProp = _parameterType.GetProperty("UdtTypeName");

            _oracleDbTypeEnum = ReflectHelper.TypeFromAssembly("Oracle.DataAccess.Client.OracleDbType", driverAssemblyName, false);
            _oracleParameterStatusEnum = ReflectHelper.TypeFromAssembly("Oracle.DataAccess.Client.OracleParameterStatus", driverAssemblyName, false);
        }

        #region

        /// <summary> Создание пустой команды для NHibernate. </summary>        
        public override IDbCommand CreateCommand()
        {
            var command = base.CreateCommand();
            if (_fetchSize != null)
                _oracleCommandFetchSizeProp.SetValue(command, _fetchSize.Value, null);
            return command;
        }

        /// <summary> Generates an IDbCommand from the SqlString according to the requirements of the DataProvider. </summary>
        /// <param name="type">The <see cref="CommandType"/> of the command to generate.</param>
        /// <param name="sqlString">The SqlString that contains the SQL.</param>
        /// <param name="parameterTypes">The types of the parameters to generate for the command.</param>
        /// <returns>An IDbCommand with the CommandText and Parameters fully set.</returns>
        /// <remarks>
        /// В частности, в данном методе выполняется подстановка оптимизированных
        /// in-выражений, в которых вместо плоского списка элементов "in (1, 2, 3, 4, 5...)"
        /// используется более простой "in (select COLUMN_VALUE from TABLE(:var))". 
        /// Такая реализация приводит к хорошему кэшированию запросов в БД.
        /// </remarks>
        public override IDbCommand GenerateCommand(CommandType type, SqlString sqlString, SqlType[] parameterTypes)
        {
            IDbCommand cmd = CreateCommand();
            cmd.CommandType = type;

            SetCommandTimeout(cmd);
            SetCommandTextNParameters(cmd, sqlString, parameterTypes);

            return cmd;
        }

        /// <remarks>Переопределяем тип параметра для DateTime в базе данных на DATE</remarks>
        protected override void InitializeParameter(IDbDataParameter dbParam, string name, SqlType sqlType)
        {
            switch (sqlType.DbType)
            {
                //Timestamp columns not indexed by Oracle 11g date columns. - Use Date
                case DbType.DateTime:
                    base.InitializeParameter(dbParam, name, SqlTypeFactory.Date);
                    break;
                default:
                    base.InitializeParameter(dbParam, name, sqlType);
                    break;
            }

        }

        /// <inheritDoc />
        public override void BindParameter(IType expectedType, IDbCommand command, object value, int index, ISessionImplementor session)
        {
            IList<CommandOptimizedParamSpanInfo> optimizedParamSpans;
            bool withinSpan = false;
            int spanIndex = -1;
            var exists = _commandPropTable.TryGetValue(command, out optimizedParamSpans);
            if (exists)
            {
                var result = optimizedParamSpans
                    .Select((sp, idx) => new {Index = idx, SpanInfo = sp})
                    .FirstOrDefault(
                        pair => pair.SpanInfo.FirstParamIdxInSpan <= index && index <= pair.SpanInfo.LastParamIdxInSpan);
                if (result != null)
                {
                    spanIndex = result.Index;
                    withinSpan = true;
                }
            }


            if (exists && withinSpan)
            {
                optimizedParamSpans[spanIndex].Values[index - optimizedParamSpans[spanIndex].FirstParamIdxInSpan] = value;

                if (index == optimizedParamSpans[spanIndex].LastParamIdxInSpan)
                {
                    int length = optimizedParamSpans[spanIndex].LastParamIdxInSpan
                                 - optimizedParamSpans[spanIndex].FirstParamIdxInSpan
                                 + 1;

                    var oracleParameterStatuses = Array.CreateInstance(_oracleParameterStatusEnum, length);
                    oracleParameterStatuses.Fill(Enum.Parse(_oracleParameterStatusEnum, "Success"));

                    // Формируем специальный ODT из значений заменяемых параметров
                    var odtValue = TableTypesConverter.Convert(optimizedParamSpans[spanIndex].Values.ToArray());

                    var dataParameter = ((IDataParameter)command.Parameters[optimizedParamSpans[spanIndex].NewCommandParamIdx]);
                    _oracleDbTypeProp.SetValue(dataParameter, Enum.Parse(_oracleDbTypeEnum, "Array"), null);
                    _arrayBindStatusProp.SetValue(dataParameter, oracleParameterStatuses, null);
                    _udtTypeNameProp.SetValue(dataParameter, odtValue.OraTypeName, null);
                    dataParameter.Value = odtValue;
                }
            }
            else
            {
                base.BindParameter(expectedType, command, value, index, session);
            }
        }

        #endregion


        #region Внутренние методы

        private class CommandOptimizedParamSpanInfo
        {
            private readonly object[] _values;
            private readonly int _firstParamIdxInSpan;
            private readonly int _lastParamIdxInSpan;
            private readonly int _newCommandParamIdx;

            public CommandOptimizedParamSpanInfo(int firstParamIdxInSpan, int lastParamIdxInSpan, int newCommandParamIdx)
            {
                _firstParamIdxInSpan = firstParamIdxInSpan;
                _lastParamIdxInSpan = lastParamIdxInSpan;
                _newCommandParamIdx = newCommandParamIdx;
                _values = new object[lastParamIdxInSpan - firstParamIdxInSpan + 1];
            }

            public object[] Values {
                get { return _values; }
            }

            public int FirstParamIdxInSpan
            {
                get { return _firstParamIdxInSpan; }
            }

            public int LastParamIdxInSpan
            {
                get { return _lastParamIdxInSpan; }
            }

            public int NewCommandParamIdx
            {
                get { return _newCommandParamIdx; }
            }
        }

        /// <summary> Диапазоны индексов (первый, последний) оптимизированных параметров команды </summary>
        private readonly ConditionalWeakTable<IDbCommand, IList<CommandOptimizedParamSpanInfo>> _commandPropTable = new ConditionalWeakTable<IDbCommand, IList<CommandOptimizedParamSpanInfo>>();

        private readonly InListAnalyzer _analyzer = new InListAnalyzer();

        private void SetCommandTextNParameters(IDbCommand cmd, SqlString sqlString, SqlType[] sqlTypes)
        {
            SqlStringFormatter formatter = GetSqlStringFormatter();

            formatter.Format(sqlString);
            var analyzeResult = _analyzer.BuildOptimizationInfo(formatter.GetFormattedText());
            var resultSql = analyzeResult.Sql;

            var optimizedParamSpans = new List<CommandOptimizedParamSpanInfo>();
            _commandPropTable.Add(cmd, optimizedParamSpans);

            var currentParamSpanIdx = 0;
            var insideInListBulkBinding = false;
            // Индекс первого параметра текущего диапазона в массиве <paramref cref="sqlTypes" />
            var firstParamIdxInSpan = -1;
            for (int i = 0; i < sqlTypes.Length; i++)
            {
                string queryParamName = ToParameterName(i);
                
                var arrayBindEnabled = currentParamSpanIdx < analyzeResult.Params.Count;
                if (!insideInListBulkBinding && arrayBindEnabled && analyzeResult.Params[currentParamSpanIdx].FirstQueryParameter == FormatNameForParameter(queryParamName))
                {
                    insideInListBulkBinding = true;
                    firstParamIdxInSpan = i;
                }
                
                if (insideInListBulkBinding)
                {
                    if (arrayBindEnabled
                        && analyzeResult.Params[currentParamSpanIdx].LastQueryParameter == FormatNameForParameter(queryParamName))
                    {
                        var newParameterName = ToParameterName(firstParamIdxInSpan);
                        IDbDataParameter dbParam =
                            GenerateParameter(cmd,
                                              newParameterName,
                                              sqlTypes[i]);
                        var newParamIdx = cmd.Parameters.Add(dbParam);
                        resultSql = resultSql.Replace(analyzeResult.Params[currentParamSpanIdx].CommandParameter,
                                                      newParameterName);

                        var lastParamIdxInSpan = i;
                        optimizedParamSpans.Add(new CommandOptimizedParamSpanInfo(firstParamIdxInSpan, lastParamIdxInSpan, newParamIdx));
                        firstParamIdxInSpan = -1;
                        currentParamSpanIdx++;
                        insideInListBulkBinding = false;
                    }
                }
                else
                {
                    IDbDataParameter dbParam = GenerateParameter(cmd, queryParamName, sqlTypes[i]);
                    cmd.Parameters.Add(dbParam);
                }
            }

            cmd.CommandText = resultSql;
        }

        private static string ToParameterName(int index)
        {
            return "p" + index;
        }

        #endregion
    }
}