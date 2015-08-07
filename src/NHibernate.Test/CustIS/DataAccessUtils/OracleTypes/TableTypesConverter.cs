using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;


namespace CustIS.TradeNets.NHibernate.ApplicationBootstrap.DataAccessUtils.OracleTypes
{
    /// <summary> Конвертор масивов из формата .NET в табличные типы Oracle. </summary>
    public static class TableTypesConverter
    {
        private static readonly IDictionary<Type, ICustomConverter> _converters;

        /// <summary> Конвертор масивов из формата .NET в табличные типы Oracle. </summary>
        static TableTypesConverter()
        {
            _converters = new Dictionary<Type, ICustomConverter>();

            var intc = new IntConverter();
            _converters[typeof (int)] = intc;
            _converters[typeof (int?)] = intc;

            var longc = new LongConverter();
            _converters[typeof(long)] = longc;
            _converters[typeof(long?)] = longc;

            var stringc = new StringConverter();
            _converters[typeof (string)] = stringc;

        }

        /// <summary> Конвертировать массив в табличный тип Oracle. </summary>
        /// <param name="values">Массив значений.</param>
        /// <returns>Экземпляр табличного типа Oracle.</returns>
        public static ITableType Convert(object[] values)
        {
            Contract.Assert(values != null);
            Contract.Assert(values.Length != 0);

            var itemType = values[0].GetType();
            return _converters[itemType].Convert(values);
        }

        /// <summary> Конвертор массивов типа <see cref="int"/> </summary>
        private class IntConverter: ICustomConverter
        {
            ITableType ICustomConverter.Convert(IEnumerable<object> values)
            {
                return new IntTableType(values.Where(v => v is int?).Cast<int?>().ToArray());
            }
        }

        /// <summary> Конвертор массивов типа <see cref="long"/> </summary>
        private class LongConverter: ICustomConverter
        {
            ITableType ICustomConverter.Convert(IEnumerable<object> values)
            {
                return new LongTableType(values.Where(v => v is long?).Cast<long?>().ToArray());
            }
        }

        /// <summary> Конвертор массивов типа <see cref="string"/> </summary>
        private class StringConverter: ICustomConverter
        {
            ITableType ICustomConverter.Convert(IEnumerable<object> values)
            {
                return new StringTableType(values.OfType<string>().ToArray());
            }
        }

        private interface ICustomConverter
        {
            ITableType Convert(IEnumerable<object> values);
        }
    }
}