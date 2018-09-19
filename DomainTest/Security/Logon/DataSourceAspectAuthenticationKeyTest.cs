using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Logon.Aspects;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.Security.Logon
{
	[DomainTest]
	[LoggedOff]
	[TestFixture]
	public class DataSourceAspectAuthenticationKeyTest : IExtendSystem
	{
		public Service TheService;
		public ICurrentDataSource DataSource;
		public FakeDatabase Database;
		
		public void Extend(IExtend extend, IocConfiguration configuration)
		{
			extend.AddService<Service>();
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

	}
}