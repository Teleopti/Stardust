using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Logon.Aspects;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Logon
{
	[DomainTest]
	[LoggedOff]
	[TestFixture]
	[Toggle(Toggles.RTA_MultiTenancy_32539)]
	public class DataSourceAspectAuthenticationKeyTest : ISetup
	{
		public Service TheService;
		public ICurrentDataSource DataSource;
		public FakeDatabase Database;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddService<Service>();
		}

		public class Service
		{
			private readonly ICurrentDataSource _dataSource;

			public Service(ICurrentDataSource dataSource)
			{
				_dataSource = dataSource;
			}

			public IDataSource RanWithDataSource;

			[DataSource]
			public virtual void Does(object input)
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
		public void ShouldSetDataSourceFromAuthenticationKey()
		{
			Database.WithTenant("tenant", "key");

			TheService.Does(new Input {AuthenticationKey = "key"});

			TheService.RanWithDataSource.DataSourceName.Should().Be("tenant");
		}

		[Test]
		public void ShouldNotSetCurrentDatasourceForInvalidAuthenticationKey()
		{
			Database.WithTenant("tenant", "key");

			TheService.Does(new Input { AuthenticationKey = "wrong" });

			TheService.RanWithDataSource.Should().Be.Null();
		}

		[Test]
		public void ShouldSetCurrentDatasourceFromEnumerableInput()
		{
			Database.WithTenant("tenant", "key");

			TheService.Does(new[] {new Input {AuthenticationKey = "key"}}.Union(new Input[] {}));

			TheService.RanWithDataSource.DataSourceName.Should().Be("tenant");
		}

		[Test]
		public void ShouldSetCurrentDatasourceFromGenericType()
		{
			Database.WithTenant("tenant", "key");

			TheService.Does(new GenericInput<object> {AuthenticationKey = "key"});

			TheService.RanWithDataSource.DataSourceName.Should().Be("tenant");
		}

		[Test]
		public void ShouldFindTenantWhenAuthenticationKeyIsSentInWrongEncoding()
		{
			Database.WithTenant("tenant", ConfiguredKeyAuthenticator.LegacyAuthenticationKey);

			TheService.Does(new Input
			{
				AuthenticationKey = ConfiguredKeyAuthenticator.LegacyAuthenticationKey.Remove(2, 2).Insert(2, "_")
			});

			TheService.RanWithDataSource.DataSourceName.Should().Be("tenant");
		}

	}
}