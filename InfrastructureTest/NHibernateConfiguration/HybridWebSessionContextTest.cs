using NHibernate;
using NUnit.Framework;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;

namespace Teleopti.Ccc.InfrastructureTest.NHibernateConfiguration
{
	[TestFixture]
	public class HybridWebSessionContextTest
	{
		// completely meaningless coverage test

		[Test]
		public void ShouldStuff()
		{
			var target = new HybridWebSessionContext(null);
			Assert.Throws<HibernateException>(() => target.CurrentSession());
		}
	}
}