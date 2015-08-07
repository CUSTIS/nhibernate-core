using System;
using Oracle.DataAccess.Types;


namespace CustIS.TradeNets.NHibernate.ApplicationBootstrap.DataAccessUtils.OracleTypes
{
    /// <summary>Базовая фабрика для табличных типов Oracle. </summary>
    public abstract class TableTypeFactory<T, TElement> : IOracleCustomTypeFactory, IOracleArrayTypeFactory
        where T : IOracleCustomType, new()
    {
        /// <summary>Returns a new custom object to represent an Oracle Object or Collection.</summary>
        public virtual IOracleCustomType CreateObject()
        {
            return new T();
        }

        /// <summary>Returns a new array of the specified numElems to store Oracle Collection elements.</summary>
        /// <param name="numElems">The number of collection elements to be returned.</param>
        public virtual Array CreateArray(int numElems)
        {
            return new TElement[numElems];
        }

        /// <summary>
        /// Returns a newly allocated OracleUdtStatus array of the specified numElems that will be used to store
        /// the null status of the collection elements.
        /// </summary>
        /// <param name="numElems">The number of collection elements to be returned.</param>
        public virtual Array CreateStatusArray(int numElems)
        {
            return new OracleUdtStatus[numElems];
        }
    }
}