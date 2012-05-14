using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Interfaces.Domain;
using Environment = NHibernate.Cfg.Environment;

namespace Teleopti.Ccc.InfrastructureTest.NHibernateConfiguration
{
	[TestFixture]
	public class FasterProxyFactoryFactoryTest : DatabaseTest
	{
		[Test]
		public void ShouldHaveFasterProxyEnabled()
		{
			Environment.BytecodeProvider.ProxyFactoryFactory
				.Should().Be.InstanceOf<FasterProxyFactoryFactory>();
		}

		[Test]
		public void ShouldThrowOriginalExceptionOnProxyInstance()
		{
			using (var newSession = Session.SessionFactory.OpenSession())
			{
				var proxy = newSession.Load<Person>(SetupFixtureForAssembly.loggedOnPerson.Id);
				Assert.Throws<ArgumentNullException>(() =>
									proxy.AddPersonPeriod(null));
			}
		}
	}
}