using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Logon.Aspects;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages;

namespace Teleopti.Ccc.DomainTest.Logon
{
	[DomainTest]
	[LoggedOff]
	[TestFixture]
	public class DataSourceAspectTest : ISetup
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

			[TenantScope]
			public virtual void Does(object input)
			{
				RanWithDataSource = _dataSource.Current();
			}

		}
		
		public class Input
		{
			public string Tenant { get; set; }
		}

		public class GenericInput<T>
		{
			public string Tenant { get; set; }
		}

		public class InputWithLogonContext : ILogOnContext
		{
			public string LogOnDatasource { get; set; }
			public Guid LogOnBusinessUnitId { get; set; }
		}

		[Test]
		public void ShouldSetCurrentDatasourceFromTenantStringArgument()
		{
			Database.WithTenant("tenant");

			TheService.Does("tenant");

			TheService.RanWithDataSource.DataSourceName.Should().Be("tenant");
		}

		[Test]
		public void ShouldSetCurrentDatasourceFromTenantProperty()
		{
			Database.WithTenant("tenant");

			TheService.Does(new Input { Tenant = "tenant" });

			TheService.RanWithDataSource.DataSourceName.Should().Be("tenant");
		}

		[Test]
		public void ShouldSetCurrentDatasourceFromEnumerableInput()
		{
			Database.WithTenant("tenant");

			TheService.Does(new[] { new Input { Tenant = "tenant" } }.Union(new Input[] { }));

			TheService.RanWithDataSource.DataSourceName.Should().Be("tenant");
		}

		[Test]
		public void ShouldSetCurrentDatasourceFromGenericType()
		{
			Database.WithTenant("tenant", "key");

			TheService.Does(new GenericInput<object> { Tenant = "tenant" });

			TheService.RanWithDataSource.DataSourceName.Should().Be("tenant");
		}

		[Test]
		public void ShouldSetCurrentDatasourceFromLogonContext()
		{
			Database.WithTenant("tenant", "key");

			TheService.Does(new InputWithLogonContext {LogOnDatasource = "tenant"});

			TheService.RanWithDataSource.DataSourceName.Should().Be("tenant");
		}

		[Test]
		public void ShouldResetCurrentDatasource()
		{
			Database.WithTenant("tenant");

			TheService.Does(new Input { Tenant = "tenant" });

			DataSource.Current().Should().Be.Null();
		}
		
	}
}
