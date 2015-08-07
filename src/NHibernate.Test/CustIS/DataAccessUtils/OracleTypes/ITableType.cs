namespace CustIS.TradeNets.NHibernate.ApplicationBootstrap.DataAccessUtils.OracleTypes
{
    /// <summary> Табличный тип для Oracle. </summary>
    public interface ITableType
    {
        /// <summary> Имя типа в Oracle. </summary>
        string OraTypeName { get; }
    }
}