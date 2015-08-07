namespace CustIS.TradeNets.NHibernate.ApplicationBootstrap.DataAccessUtils.OracleTypes
{
    /// <summary> Табличный тип, содержащий элементы типа <see cref="int"/> </summary>
    internal class IntTableType : TableType<int?>
    {
        /// <summary> Имя типа в Oracle. </summary>
        public const string TYPE_NAME = TYPE_PREFIX + "INT_ARRAY";

        /// <summary> Имя типа в Oracle. </summary>
        public override string OraTypeName
        {
            get { return TYPE_NAME; }
        }

        /// <summary> Табличный тип, содержащий элементы типа <see cref="int"/> </summary>
        public IntTableType()
        {
            
        }

        /// <summary> Табличный тип, содержащий элементы типа <see cref="int"/> </summary>
        public IntTableType(int?[] value)
        {
            Value = value;
        }
    }
}