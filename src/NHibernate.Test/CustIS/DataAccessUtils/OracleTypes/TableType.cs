using System;
using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;


namespace CustIS.TradeNets.NHibernate.ApplicationBootstrap.DataAccessUtils.OracleTypes
{
    /// <summary> Базовый абстрактный класс для всех табличных типов в Oracle. </summary>
    /// <typeparam name="T">Тип элемента.</typeparam>
    internal abstract class TableType<T> : IOracleCustomType, ITableType
    {
        protected const string TYPE_PREFIX = "NH_";
        
        /// <summary> Значение </summary>        
        [OracleArrayMapping]
        public virtual T[] Value { get; set; }

        /// <summary> Преобразование в формат Oracle. </summary>        
        public void FromCustomObject(OracleConnection con, IntPtr pUdt)
        {
            if (Value != null)
            {
                OracleUdt.SetValue(con, pUdt, 0, Value);
            }
        }

        /// <summary> Преобразование из формата Oracle. </summary>        
        public void ToCustomObject(OracleConnection con, IntPtr pUdt)
        {
            Value = ((T[])(OracleUdt.GetValue(con, pUdt, 0)));
        }

        /// <summary> Имя типа в Oracle. </summary>
        public abstract string OraTypeName { get; }
    }
}