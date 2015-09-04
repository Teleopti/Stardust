using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Aspects;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Aspects
{
	[RtaTest]
	[TestFixture]
	[Toggle(Toggles.RTA_MultiTenancy_32539)]
	public class DataSourceFromAuthenticationKeyAspectTest : ISetup
	{
		public AspectedService TheService;
		public FakeApplicationData ApplicationData;
		public FakeTenants Tenants;
		public ICurrentDataSource DataSource;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddService<AspectedService>();
		}

		public class AspectedService
		{
			private readonly ICurrentDataSource _dataSource;

			public AspectedService(ICurrentDataSource dataSource)
			{
				_dataSource = dataSource;
			}

			public IDataSource RanWithDataSource;

			[DataSourceFromAuthenticationKey]
			public virtual void Does(Input input)
			{
				RanWithDataSource = _dataSource.Current();
			}

			[DataSourceFromAuthenticationKey]
			public virtual void Does(IEnumerable<Input> inputs)
			{
				RanWithDataSource = _dataSource.Current();
			}

			[DataSourceFromAuthenticationKey]
			public virtual void Does(GenericInput<object> input)
			{
				RanWithDataSource = _dataSource.Current();
			}

		}

		public class Input
		{
			public string AuthenticationKey { get; set; }
		}

		public class GenericInput<T>
		{
			public string AuthenticationKey { get; set; }
		}

		[Test]
		public void ShouldSetCurrentDatasource()
		{
			IDataSource expected = new FakeDataSource("tenant");
			ApplicationData.RegisteredDataSources = new[] {expected};
			Tenants.Has(new Tenant("tenant") {RtaKey = "key"});

			TheService.Does(new Input {AuthenticationKey = "key"});

			TheService.RanWithDataSource.Should().Be(expected);
		}

		[Test]
		public void ShouldResetCurrentDatasource()
		{
			IDataSource expected = new FakeDataSource("tenant");
			ApplicationData.RegisteredDataSources = new[] { expected };
			Tenants.Has(new Tenant("tenant") { RtaKey = "key" });

			TheService.Does(new Input { AuthenticationKey = "key" });

			DataSource.Current().Should().Be.Null();
		}

		[Test]
		public void ShouldNotSetCurrentDatasourceForInvalidKey()
		{
			IDataSource expected = new FakeDataSource("tenant");
			ApplicationData.RegisteredDataSources = new[] { expected };
			Tenants.Has(new Tenant("tenant") { RtaKey = "key" });

			TheService.Does(new Input { AuthenticationKey = "wrong" });

			TheService.RanWithDataSource.Should().Be.Null();
		}

		[Test]
		public void ShouldSetCurrentDatasourceFromEnumerableInput()
		{
			IDataSource expected = new FakeDataSource("tenant");
			ApplicationData.RegisteredDataSources = new[] { expected };
			Tenants.Has(new Tenant("tenant") { RtaKey = "key" });

			TheService.Does(new[] {new Input {AuthenticationKey = "key"}}.Union(new Input[] {}));

			TheService.RanWithDataSource.Should().Be(expected);
		}

		[Test]
		public void ShouldSetCurrentDatasourceFromGenericType()
		{
			IDataSource expected = new FakeDataSource("tenant");
			ApplicationData.RegisteredDataSources = new[] { expected };
			Tenants.Has(new Tenant("tenant") { RtaKey = "key" });

			TheService.Does(new GenericInput<object> {AuthenticationKey = "key"});

			TheService.RanWithDataSource.Should().Be(expected);
		}

		[Test]
		public void ShouldFindTenantWhenAuthenticationKeyIsSentInWrongEncoding()
		{
			IDataSource expected = new FakeDataSource("tenant");
			ApplicationData.RegisteredDataSources = new[] {expected};
			Tenants.Has(new Tenant("tenant") {RtaKey = Domain.ApplicationLayer.Rta.Service.Rta.LegacyAuthenticationKey});

			TheService.Does(new Input
			{
				AuthenticationKey = Domain.ApplicationLayer.Rta.Service.Rta.LegacyAuthenticationKey.Remove(2, 2).Insert(2, "_")
			});

			TheService.RanWithDataSource.Should().Be(expected);
		}
	}
}
