using System;
using Autofac;
using MbCache.Core;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories.Tenant;
using Teleopti.Ccc.TestCommon.IoC;
using Module = Autofac.Module;

namespace Teleopti.Ccc.DomainTest
{
	[DomainTest]
	[NoDefaultData]
	public class CacheTest : IIsolateSystem, IExtendSystem
	{
		public OuterService Service;
		public CachedServiceImpl CachedService;
		public IDataSourceScope DataSource;
		public IMbCacheFactory Cache;
		public FakeTenants Tenants;

		public void Extend(IExtend extend, IocConfiguration configuration)
		{
			extend.AddModule(new TestModule(configuration));
		}

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<NoCurrentIdentity>().For<ICurrentIdentity>();
		}

		public class TestModule : Module
		{
			private readonly IocConfiguration _configuration;

			public TestModule(IocConfiguration configuration)
			{
				_configuration = configuration;
			}

			protected override void Load(ContainerBuilder builder)
			{
				builder.RegisterType<OuterService>().SingleInstance();

				builder.CacheByClassProxy<CachedServiceImpl>().SingleInstance();
				_configuration.Args().Cache.This<CachedServiceImpl>(
					(c, b) => b
						.CacheMethod(x => x.GetDataSourceName())
						.OverrideCacheKey(c.Resolve<CachePerDataSource>())
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
			Tenants.Has("1");

			using (DataSource.OnThisThreadUse("1"))
			{
				Service.GetDataSourceName();
				Service.GetDataSourceName();
			}

			CachedService.CalledCount.Should().Be(1);
		}

		[Test]
		public void ShouldCachePerDataSource()
		{
			Tenants.Has("1");
			Tenants.Has("2");

			using (DataSource.OnThisThreadUse("1"))
				Service.GetDataSourceName().Should().Be("1");

			using (DataSource.OnThisThreadUse("2"))
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
			Tenants.Has("1");

			using (DataSource.OnThisThreadUse("1"))
				Service.GetDataSourceName().Should().Be("1");
			Cache.Invalidate<CachedServiceImpl>();
			using (DataSource.OnThisThreadUse("1"))
				Service.GetDataSourceName().Should().Be("1");

			CachedService.CalledCount.Should().Be(2);
		}
	}
}