using System;
using Autofac;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.ApplicationLayer.Intraday;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Hangfire;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Messaging.Client;

namespace Teleopti.Ccc.Requests.PerformanceTest.AbsenceRequests
{
	public class IntradayRequestPerformanceTestAttribute : IoCTestAttribute
	{
		public ResourceCalculateReadModelUpdater ResourceCalculateReadModelUpdater;
		public WithUnitOfWork WithUnitOfWork;
		public AsSystem AsSystem;
		public IDataSourceScope DataSource;
		public FakeConfigReader ConfigReader;


		protected override void Setup(ISystem system, IIocConfiguration configuration)
		{
			base.Setup(system, configuration);
			
			system.UseTestDouble<ApproveRequestCommandHandler>().For<IHandleCommand<ApproveRequestCommand>>();
			system.UseTestDouble<DenyRequestCommandHandler>().For<IHandleCommand<DenyRequestCommand>>();
			system.UseTestDouble<RequestApprovalServiceFactory>().For<IRequestApprovalServiceFactory>();
			system.UseTestDouble<StardustJobFeedback>().For<IStardustJobFeedback>();
			system.UseTestDouble<NoMessageSender>().For<IMessageSender>();
			system.UseTestDouble<QueuedAbsenceRequestFastIntradayHandler>().For<QueuedAbsenceRequestFastIntradayHandler>();
			system.UseTestDouble<ResourceCalculateReadModelUpdater>().For<IHandleEvent<UpdateResourceCalculateReadModelEvent>>();
			system.AddService<Database>();
			system.AddModule(new TenantServerModule(configuration));

		}

		protected override void BeforeTest()
		{
			using (DataSource.OnThisThreadUse("Teleopti WFM"))
				AsSystem.Logon("Teleopti WFM", new Guid("1fa1f97c-ebff-4379-b5f9-a11c00f0f02b"));
		}

		protected override FakeConfigReader Config()
		{
			var config = base.Config();
			config.FakeConnectionString("Tenancy", InfraTestConfigReader.ConnectionString);
			config.FakeConnectionString("Hangfire", InfraTestConfigReader.AnalyticsConnectionString);
			return config;
		}

		protected override void Startup(IComponentContext container)
		{
			base.Startup(container);
			container.Resolve<HangfireClientStarter>().Start();
		}
	}

}
