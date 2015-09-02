using Autofac;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.Aop;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Aspects
{
	[IoCTest]
	[TestFixture]
	public class CacheTest : ISetup
	{
		public OuterService Service;
		public CachedServiceImpl CachedService;
		public FakeApplicationData ApplicationData;
		public IDataSourceScope DataSource;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FakeApplicationData>().For<IApplicationData>();
			system.AddModule(new TestModule(configuration));
		}

		public class TestModule : Module
		{
			private readonly IIocConfiguration _configuration;

			public TestModule(IIocConfiguration configuration)
			{
				_configuration = configuration;
			}

			protected override void Load(ContainerBuilder builder)
			{
				builder.RegisterType<OuterService>().SingleInstance().ApplyAspects();

				_configuration.Args().CacheBuilder
					.For<CachedServiceImpl>()
					.CacheMethod(x => x.GetDataSourceName())
					.As<CachedServiceImpl>();
				builder.RegisterConcreteMbCacheComponent<CachedServiceImpl>()
					.SingleInstance();
			}
		}

		public class OuterService
		{
			private readonly CachedServiceImpl _cached;

			public OuterService(CachedServiceImpl cached)
			{
				_cached = cached;
			}

			public virtual string GetDataSourceName()
			{
				return _cached.GetDataSourceName();
			}

		}

		public class CachedServiceImpl
		{
			private readonly ICurrentDataSource _dataSource;

			public CachedServiceImpl(ICurrentDataSource dataSource)
			{
				_dataSource = dataSource;
			}

			public virtual int CalledCount { get; set; }

			public virtual string GetDataSourceName()
			{
				CalledCount++;
				return _dataSource.CurrentName();
			}
		}

		[Test]
		public void ShouldCache()
		{
			IDataSource datasource1 = new FakeDataSource("1");
			ApplicationData.RegisteredDataSources = new[] {datasource1};

			using (DataSource.OnThisThreadUse(datasource1))
			{
				Service.GetDataSourceName();
				Service.GetDataSourceName();
			}

			CachedService.CalledCount.Should().Be(1);
		}

		[Test, Ignore]
		public void ShouldCachePerDataSource()
		{
			IDataSource datasource1 = new FakeDataSource("1");
			IDataSource datasource2 = new FakeDataSource("2");
			ApplicationData.RegisteredDataSources = new[] {datasource1, datasource2};

			using (DataSource.OnThisThreadUse(datasource1))
				Service.GetDataSourceName().Should().Be("1");

			using (DataSource.OnThisThreadUse(datasource2))
				Service.GetDataSourceName().Should().Be("2");

		}

	}
}
