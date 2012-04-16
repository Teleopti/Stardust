using NHibernate;
using NUnit.Framework;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;

namespace Teleopti.Ccc.InfrastructureTest.NHibernateConfiguration
{
	[TestFixture]
	public class HybridWebSessionContextTest
	{
		// completely meaningless coverage test

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
		public void ShouldStuff()
		{
			var target = new HybridWebSessionContext(null);
			Assert.Throws<HibernateException>(() => target.CurrentSession());
		}
	}
}