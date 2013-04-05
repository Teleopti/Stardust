﻿using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.UnitOfWork
{
	[TestFixture]
	public class UnitOfWorkFactoryProviderTest
	{
		private IUnitOfWorkFactoryProvider target;
		private ICurrentTeleoptiPrincipal currentTeleoptiPrincipal;

		[SetUp]
		public void Setup()
		{
			currentTeleoptiPrincipal = MockRepository.GenerateMock<ICurrentTeleoptiPrincipal>();
			target = new UnitOfWorkFactoryProvider(currentTeleoptiPrincipal);
		}

		[Test]
		public void ShouldReturnIdentityApplicationDatasouce()
		{
			var expectedUnitOfWorkFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			var dataSource = new FakeDataSource {Application = expectedUnitOfWorkFactory};
			var identity = MockRepository.GenerateMock<ITeleoptiIdentity>();
			var teleoptiPrincipal = new TeleoptiPrincipal(identity, new Person());
			currentTeleoptiPrincipal.Expect(x => x.Current()).Return(teleoptiPrincipal);
			identity.Expect(x => x.IsAuthenticated).Return(true);
			identity.Expect(x => x.DataSource).Return(dataSource);

			target.LoggedOnUnitOfWorkFactory().Should().Be.SameInstanceAs(expectedUnitOfWorkFactory);
		}

		[Test]
		public void ShouldThrowIfIdentityDoesNotExist()
		{
			//or should null be returned?
			currentTeleoptiPrincipal.Expect(x => x.Current()).Return(null);
			Assert.Throws<PermissionException>(() => target.LoggedOnUnitOfWorkFactory());
		}

		[Test]
		public void ShouldThrowIfNotAuthenticated()
		{
			//or should null be returned?
			var identity = MockRepository.GenerateMock<ITeleoptiIdentity>();
			var teleoptiPrincipal = new TeleoptiPrincipal(identity, new Person());
			currentTeleoptiPrincipal.Expect(x => x.Current()).Return(teleoptiPrincipal);
			identity.Expect(x => x.IsAuthenticated).Return(false);

			Assert.Throws<PermissionException>(() => target.LoggedOnUnitOfWorkFactory());
		}
	}
}