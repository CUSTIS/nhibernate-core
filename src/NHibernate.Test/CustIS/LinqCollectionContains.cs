using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NHibernate.DomainModel.Northwind.Entities;
using NHibernate.Test.Linq;
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
