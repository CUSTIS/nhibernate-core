using System.Collections.Generic;

namespace NHibernate.Test.CustIS.DataAccessUtils
{
    /// <summary> Инфо IN-выражения в заданном SQL-запросе. </summary>
    internal class QueryCommandMatchingInfo
    {
        /// <summary> Оптимизатор IN-выражения в заданном SQL-запросе. </summary>
        public QueryCommandMatchingInfo(string sql, IList<ParamMatchingInfo> @params)
        {
            Sql = sql;
            Params = @params;
        }

        /// <summary> Новый SQL-запрос с оптимизированными IN-выражениями. </summary>
        public string Sql { get; private set; }

        /// <summary> Диапазоны параметров, которые должны быть заменены. </summary>
        public IList<ParamMatchingInfo> Params { get; private set; }
    }
}