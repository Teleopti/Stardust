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
using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Web;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.Web;

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

		private IDisposable _transactionHookScope;
		private List<Type> _scopeExtenders = new List<Type>();

		protected override FakeConfigReader Config()
		{
			var config = base.Config();
			config.FakeConnectionString("MessageBroker", InfraTestConfigReader.AnalyticsConnectionString);
			config.FakeConnectionString("Tenancy", InfraTestConfigReader.ConnectionString);
			config.FakeConnectionString("Hangfire", InfraTestConfigReader.AnalyticsConnectionString);
			config.FakeConnectionString("RtaTracer", InfraTestConfigReader.AnalyticsConnectionString);
			return config;
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
		protected override void Setup(ISystem system, IIocConfiguration configuration)
		{
			base.Setup(system, configuration);

			// Tenant stuff
			system.UseTestDouble<TenantAuthenticationFake>().For<ITenantAuthentication>();
			system.AddService(TenantUnitOfWorkManager.Create(InfraTestConfigReader.ConnectionString));

			// Hangfire bus maybe? ;)
			if (QueryAllAttributes<RealHangfireAttribute>().IsEmpty())
				system.UseTestDouble<FakeHangfireEventClient>().For<IHangfireEventClient>();

			// message broker
			system.UseTestDouble(new FakeSignalR()).For<ISignalR>();
			system.UseTestDouble<FakeMessageSender>().For<IMessageSender>();

			// stardust
			system.UseTestDouble<FakeStardustJobFeedback>().For<IStardustJobFeedback>();
			
			// extend scope by including handlers
			var scopeExtenders = QueryAllAttributes<ExtendScopeAttribute>();
			_scopeExtenders.AddRange(scopeExtenders.Select(x => x.Handler));
			if (scopeExtenders.Any())
				system.UseTestDouble<FakeEventPublisher>().For<IEventPublisher>();

			system.AddService<Database>();
			system.AddService<DatabaseLegacy>();
			system.AddService<AnalyticsDatabase>();
			system.UseTestDouble<FakeTransactionHook>().For<ITransactionHook>(); // just adds one hook to the list
			system.UseTestDouble<MutableFakeCurrentHttpContext>().For<ICurrentHttpContext>();

			// fake for now. if real repo needs to be included in the scope....
			system.UseTestDouble<FakeLicenseRepository>().For<ILicenseRepository, ILicenseRepositoryForLicenseVerifier>();
		}

		protected override void BeforeTest()
		{
			base.BeforeTest();

			// extend scope by including handlers
			_scopeExtenders.ForEach(x => (Publisher as FakeEventPublisher).AddHandler(x));

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