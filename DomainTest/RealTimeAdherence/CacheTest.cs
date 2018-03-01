using System;
using Autofac;
using MbCache.Core;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.MultiTenancy;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.RealTimeAdherence
{
	[DomainTest]
	[TestFixture]
	public class CacheTest : ISetup
	{
		public OuterService Service;
		public CachedServiceImpl CachedService;
		public FakeDataSourceForTenant DataSources;
		public IDataSourceScope DataSource;
		public IMbCacheFactory Cache;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<NoCurrentIdentity>().For<ICurrentIdentity>();
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
				builder.RegisterType<OuterService>().SingleInstance();

				builder.CacheByClassProxy<CachedServiceImpl>().SingleInstance();
				_configuration.Cache().This<CachedServiceImpl>(
					(c, b) => b
						.CacheMethod(x => x.GetDataSourceName())
						.CacheKey(c.Resolve<CachePerDataSource>())
					);
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
		public void ShouldNotCreateCachingProxy()
		{
			Service.GetType().Should().Be.EqualTo<OuterService>();
		}

		[Test]
		public void ShouldCreateCachingProxy()
		{
			CachedService.GetType().Should().Not.Be.EqualTo<CachedServiceImpl>();
		}

		[Test]
		public void ShouldCache()
		{
			var datasource1 = new FakeDataSource("1");
			DataSources.Has(datasource1);

			using (DataSource.OnThisThreadUse(datasource1))
			{
				Service.GetDataSourceName();
				Service.GetDataSourceName();
			}

			CachedService.CalledCount.Should().Be(1);
		}

		[Test]
		public void ShouldCachePerDataSource()
		{
			IDataSource datasource1 = new FakeDataSource("1");
			IDataSource datasource2 = new FakeDataSource("2");
			DataSources.Has(datasource1);
			DataSources.Has(datasource2);

			using (DataSource.OnThisThreadUse(datasource1))
				Service.GetDataSourceName().Should().Be("1");

			using (DataSource.OnThisThreadUse(datasource2))
				Service.GetDataSourceName().Should().Be("2");

		}

		[Test]
		public void ShouldThrowIfNoCurrentDataSource()
		{
			Assert.Throws<NullReferenceException>(() => Service.GetDataSourceName());
		}

		[Test]
		public void ShouldInvalidateForAllDataSources()
		{
			IDataSource datasource1 = new FakeDataSource("1");
			DataSources.Has(datasource1);

			using (DataSource.OnThisThreadUse(datasource1))
				Service.GetDataSourceName().Should().Be("1");
			Cache.Invalidate<CachedServiceImpl>();
			using (DataSource.OnThisThreadUse(datasource1))
				Service.GetDataSourceName().Should().Be("1");

			CachedService.CalledCount.Should().Be(2);
		}
	}
}
