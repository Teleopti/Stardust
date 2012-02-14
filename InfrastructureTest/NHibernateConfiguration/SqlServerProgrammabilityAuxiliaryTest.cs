using NUnit.Framework;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;

namespace Teleopti.Ccc.InfrastructureTest.NHibernateConfiguration
{
	[TestFixture]
	public class SqlServerProgrammabilityAuxiliaryTest
	{
		private SqlServerProgrammabilityAuxiliary target;

		/* completly meaningless coverage test
		 * will be tested "for real" when db is created by nh
		 * The problem is coverage on nightly build where
		 * schema is created by trunk script. The code won't run and coverage is zero.
		 */

		[SetUp]
		public void Setup()
		{
			target = new SqlServerProgrammabilityAuxiliary();
		}


		[Test]
		public void StupidTest()
		{
			Assert.IsNotNull(target.SqlCreateString(null,null,null,null));
			Assert.IsNotNull(target.SqlDropString(null,null,null));
			target.AddDialectScope(string.Empty);
			target.SetParameterValues(null);
			Assert.IsTrue(target.AppliesToDialect(null));
		}
	}
}