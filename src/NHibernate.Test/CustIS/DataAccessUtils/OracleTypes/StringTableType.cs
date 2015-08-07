namespace CustIS.TradeNets.NHibernate.ApplicationBootstrap.DataAccessUtils.OracleTypes
{
    /// <summary> Табличный тип, содержащий элементы типа <see cref="string"/> </summary>
    internal class StringTableType : TableType<string>
    {
        /// <summary> Имя типа в Oracle. </summary>
        public const string TYPE_NAME = TYPE_PREFIX + "STRING_ARRAY";

        /// <summary> Имя типа в Oracle. </summary>
        public override string OraTypeName
        {
            get { return TYPE_NAME; }
        }

        /// <summary> Табличный тип, содержащий элементы типа <see cref="string"/> </summary>
        public StringTableType()
        {
            
        }

        /// <summary> Табличный тип, содержащий элементы типа <see cref="string"/> </summary>
        public StringTableType(string[] value)
        {
            Value = value;
        }
    }
}