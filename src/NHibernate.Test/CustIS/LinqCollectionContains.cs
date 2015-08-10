using System.Collections;
using System.Linq;
using NHibernate.Tool.hbm2ddl;
using NUnit.Framework;

namespace NHibernate.Test.CustIS
{
    [TestFixture]
    public class LinqCollectionContains : TestCase
    {
        protected override bool CheckDatabaseWasCleaned()
        {
            return true;
        }

        protected override void CreateSchema()
        {
            new SchemaExport(cfg).Create(false, true);
        }

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
						"CustIS.Person.hbm.xml"
					};
            }
        }

        protected override void BuildSessionFactory()
        {
            cfg.Properties["connection.driver_class"] = "NHibernate.Test.CustIS.DataAccessUtils.CustomOracleDataClientDriver, NHibernate.Test";

            base.BuildSessionFactory();

            var script = new[]
            {
                "CREATE OR REPLACE TYPE NH_INT_ARRAY AS TABLE OF NUMBER;",
                "CREATE OR REPLACE TYPE NH_LONG_ARRAY AS TABLE OF NUMBER;",
                "CREATE OR REPLACE TYPE NH_STRING_ARRAY AS TABLE OF VARCHAR2(4000);"
            };

            using (var session = OpenSession())
            using (var cmd = session.Connection.CreateCommand())
            {
                foreach (var str in script)
                {
                    cmd.CommandText = str;
                    cmd.ExecuteNonQuery();
                }
            }
        }

        [Category("IN")]
        [Test(Description = "Tests IN bulk binding for Oracle")]
        public void TestLinqInCondition()
        {
            using (var session = OpenSession())
            {
                var query = from entity in session.Query<Person>()
                            where new long[] { 0, 1, 2 }.Contains(entity.Id)
                            select entity;
                query.ToList();
            }
        }
    }
}
