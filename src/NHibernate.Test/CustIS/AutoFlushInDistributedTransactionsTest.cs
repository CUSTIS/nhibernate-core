using System.Collections;
using System.Data;
using System.Data.OracleClient;
using System.Linq.Dynamic;
using System.Linq;
using System.Transactions;
using NHibernate.Linq;
using NUnit.Framework;

namespace NHibernate.Test.CustIS
{
    [TestFixture]
    public class AutoFlushInDistributedTransactionsTest : TestCase
    {
        protected override string MappingsAssembly
        {
            get { return "NHibernate.Test"; }
        }

        protected override IList Mappings
        {
            get
            {
                return new string[]
					{
						"CustIS.Person.hbm.xml",
					};
            }
        }

        [Test]
        public void InsideDtcAutoFlushIsWorking()
        {
            IDbConnection con;
            using (var tx = new TransactionScope())
            {
                using (var s = sessions.OpenSession())
                {
                    var person = new Person("Вася");
                    s.Save(person);

                    var persons = s.Query<Person>().ToList();

                    CollectionAssert.AreEqual(new[] { person }, persons);

                    con = s.Connection;
                }
            }
        }
    }
}