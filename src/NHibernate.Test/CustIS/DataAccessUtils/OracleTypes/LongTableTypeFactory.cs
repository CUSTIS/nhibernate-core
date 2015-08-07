using Oracle.DataAccess.Types;


namespace CustIS.TradeNets.NHibernate.ApplicationBootstrap.DataAccessUtils.OracleTypes
{
    /// <summary> Отображение типа в Oracle на реализацию типа в коде. </summary>
    [OracleCustomTypeMapping(LongTableType.TYPE_NAME)]
    internal class LongTableTypeFactory : TableTypeFactory<LongTableType, long?>
    {
    }
}