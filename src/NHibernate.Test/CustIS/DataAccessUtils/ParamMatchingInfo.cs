using System.Data;
using NHibernate.SqlCommand;

namespace NHibernate.Test.CustIS.DataAccessUtils
{
    /// <summary>  Информация об объединении диапазона параметров запроса <see cref="ISqlCommand.QueryParameters"/> в один параметр-массив команды <see cref="IDbCommand.Parameters"/>. </summary>
    public class ParamMatchingInfo
    {
        /// <summary>  Информация об объединении диапазона параметров запроса <see cref="ISqlCommand.QueryParameters"/> в один параметр-массив команды <see cref="IDbCommand.Parameters"/>. </summary>
        public ParamMatchingInfo(string firstQueryParameter, string lastQueryParameter, string commandParameter)
        {
            FirstQueryParameter = firstQueryParameter;
            LastQueryParameter = lastQueryParameter;
            CommandParameter = commandParameter;
        }

        /// <summary> Имя первого заменяемого параметра в диапазоне. </summary>
        public string FirstQueryParameter { get; private set; }

        /// <summary> Имя последнего заменяемого параметра в диапазоне. </summary>
        public string LastQueryParameter { get; private set; }

        /// <summary> Имя нового параметра. </summary>
        public string CommandParameter { get; private set; }
    }
}