using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Domain.MessageBroker.Server;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Hangfire;
using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;

namespace Teleopti.Ccc.TestCommon.IoC
{
	public class RealHangfireAttribute : Attribute
	{
	}

	[AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
	public class ExtendScopeAttribute : Attribute
	{
		public Type Handler { get; }

		public ExtendScopeAttribute(Type handler)
		{
			Handler = handler;
		}
	}

	public class InfrastructureTestAttribute : IoCTestAttribute
	{
		public ITransactionHooksScope TransactionHooksScope;
		public IEnumerable<ITransactionHook> TransactionHooks;
		public FakeMessageSender MessageSender;
		public FakeTransactionHook TransactionHook;
		public IDataSourceForTenant DataSourceForTenant;
		public IEventPublisher Publisher;
		public HangfireClientStarter HangfireClientStarter;
		private IDisposable _transactionHookScope;
		
		protected override FakeConfigReader Config()
		{
			var config = base.Config();
			config.FakeConnectionString("MessageBroker", InfraTestConfigReader.AnalyticsConnectionString);
			config.FakeConnectionString("Tenancy", InfraTestConfigReader.ConnectionString);
			config.FakeConnectionString("Hangfire", InfraTestConfigReader.AnalyticsConnectionString);
			config.FakeConnectionString("RtaTracer", InfraTestConfigReader.AnalyticsConnectionString);
			return config;
		}

		protected override void Extend(IExtend extend, IocConfiguration configuration)
		{
			base.Extend(extend, configuration);
			
			extend.AddService(TenantUnitOfWorkManager.Create(InfraTestConfigReader.ConnectionString));
			extend.AddService<Database>();
			extend.AddService<DatabaseLegacy>();
			extend.AddService<AnalyticsDatabase>();
		}

		//
		// Should fake:
		// Config
		// Http
		// Message broker
		//
		// Should NOT fake:
		// Database
		// Hangfire		<--(??)
		// Bus			<--(??)
		//
		// ... we guess ...
		protected override void Isolate(IIsolate isolate)
		{
			base.Isolate(isolate);

			// Tenant stuff
			isolate.UseTestDouble<TenantAuthenticationFake>().For<ITenantAuthentication>();

			// Hangfire bus maybe? ;)
			if (QueryAllAttributes<RealHangfireAttribute>().IsEmpty())
				isolate.UseTestDouble<FakeHangfireEventClient>().For<IHangfireEventClient>();

			// message broker
			isolate.UseTestDouble(new FakeSignalR()).For<ISignalR>();
			isolate.UseTestDouble<FakeMessageSender>().For<IMessageSender>();

			// stardust
			isolate.UseTestDouble<FakeStardustJobFeedback>().For<IStardustJobFeedback>();
			
			// extend scope by including handlers
			if (QueryAllAttributes<ExtendScopeAttribute>().Any())
				isolate.UseTestDouble<FakeEventPublisher>().For<IEventPublisher>();

			isolate.UseTestDouble<FakeTransactionHook>().For<ITransactionHook>(); // just adds one hook to the list

			// fake for now. if real repo needs to be included in the scope....
			isolate.UseTestDouble<FakeLicenseRepository>().For<ILicenseRepository, ILicenseRepositoryForLicenseVerifier>();
		}

		protected override void BeforeTest()
		{
			base.BeforeTest();

			if (QueryAllAttributes<RealHangfireAttribute>().Any())
				HangfireClientStarter.Start();
			
			// extend scope by including handlers
			var scopeExtenders = QueryAllAttributes<ExtendScopeAttribute>().Select(x => x.Handler);
			scopeExtenders.ForEach(x => (Publisher as FakeEventPublisher).AddHandler(x));

			DataSourceForTenant.MakeSureDataSourceCreated(
				DataSourceHelper.TestTenantName,
				InfraTestConfigReader.ConnectionString,
				InfraTestConfigReader.AnalyticsConnectionString,
				null);

			MessageSender.AllNotifications.Clear();
			TransactionHook.Clear();

			_transactionHookScope = TransactionHooksScope.GloballyUse(TransactionHooks);
		}

		protected override void AfterTest()
		{
			base.AfterTest();

			_transactionHookScope?.Dispose();
		}
	}
}