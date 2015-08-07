namespace CustIS.TradeNets.NHibernate.ApplicationBootstrap.DataAccessUtils.OracleTypes
{
    /// <summary> Табличный тип, содержащий элементы типа <see cref="long"/> </summary>
    internal class LongTableType : TableType<long?>
    {
        /// <summary> Имя типа в Oracle. </summary>
        public const string TYPE_NAME = TYPE_PREFIX + "LONG_ARRAY";

        /// <summary> Имя типа в Oracle. </summary>
        public override string OraTypeName
        {
            get { return TYPE_NAME; }
        }

        /// <summary> Табличный тип, содержащий элементы типа <see cref="long"/> </summary>
        public LongTableType()
        {
            
        }

        /// <summary> Табличный тип, содержащий элементы типа <see cref="long"/> </summary>
        public LongTableType(long?[] value)
        {
            Value = value;
        }
    }
}