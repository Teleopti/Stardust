﻿using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.RequestContext
{
	[TestFixture]
	public class CurrentDataSourceTest
	{
		[Test]
		public void ShouldReturnCurrentDataSource()
		{
			var identityProvider = MockRepository.GenerateMock<ICurrentIdentity>();
			var target = new CurrentDataSource(identityProvider, new DataSourceState());
			var dataSource = MockRepository.GenerateMock<IDataSource>();
			var identity = new TeleoptiIdentity("hej", dataSource, null, null);

			identityProvider.Stub(x => x.Current()).Return(identity);

			target.Current().Should().Be.SameInstanceAs(dataSource);
		}

		[Test]
		public void ShouldReturnNullWhenCurrentPrincipalNotDefined()
		{
			var identityProvider = MockRepository.GenerateMock<ICurrentIdentity>();
			var target = new CurrentDataSource(identityProvider, new DataSourceState());

			identityProvider.Stub(x => x.Current()).Return(null);

			target.Current().Should().Be.Null();
		}

		[Test]
		public void ShouldReturnCurrentDataSourceName()
		{
			var identityProvider = MockRepository.GenerateMock<ICurrentIdentity>();
			var target = new CurrentDataSource(identityProvider, new DataSourceState());
			var dataSource = MockRepository.GenerateMock<IDataSource>();
			dataSource.Stub(x => x.DataSourceName).Return("datasource");
			var identity = new TeleoptiIdentity("hej", dataSource, null, null);

			identityProvider.Stub(x => x.Current()).Return(identity);

			target.CurrentName().Should().Be("datasource");
		}

	}
}