using System;
using System.Linq;
using Autofac;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Hangfire;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Web.WebInteractions;

namespace Teleopti.Ccc.Rta.PerformanceTest.Code
{
	public class RtaPerformanceTestAttribute : InfrastructureTestAttribute
	{
		public ImpersonateSystem Impersonate;
		public IDataSourceScope DataSource;
		public WithUnitOfWork WithUnitOfWork;
		public IBusinessUnitRepository BusinessUnits;

		protected override void Setup(ISystem system, IIocConfiguration configuration)
		{
			base.Setup(system, configuration);
			system.AddService<TestConfiguration>();
			system.AddService<Http>();
			system.AddService<DataCreator>();
			system.AddService<StatesSender>();
			system.AddService<StatesArePersisted>();
		}

		protected override void BeforeTest()
		{
			base.BeforeTest();

			Guid businessUnitId;
			using (DataSource.OnThisThreadUse(DataSourceHelper.TestTenantName))
				businessUnitId = WithUnitOfWork.Get(() => BusinessUnits.LoadAll().First()).Id.Value;
			Impersonate.Impersonate(DataSourceHelper.TestTenantName, businessUnitId);
		}

		protected override void AfterTest()
		{
			base.AfterTest();

			Impersonate.EndImpersonation();
		}

		protected override void Startup(IComponentContext container)
		{
			base.Startup(container);
			container.Resolve<HangfireClientStarter>().Start();
		}
	}
}