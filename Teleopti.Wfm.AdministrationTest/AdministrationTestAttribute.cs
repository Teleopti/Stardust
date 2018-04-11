using Teleopti.Analytics.Etl.Common.Infrastructure;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;
using Teleopti.Analytics.Etl.Common.Service;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Wfm.Administration.Core.Modules;
using Teleopti.Wfm.AdministrationTest.FakeData;

namespace Teleopti.Wfm.AdministrationTest
{
	public class AdministrationTestAttribute : DomainTestAttribute
	{
		protected override void Setup(ISystem system, IIocConfiguration configuration)
		{
			base.Setup(system, configuration);
			system.AddModule(new EtlToolModule());

			system.UseTestDouble<FakeBaseConfigurationRepository>().For<IBaseConfigurationRepository>();
			system.UseTestDouble<FakePmInfoProvider>().For<IPmInfoProvider>();
			system.UseTestDouble<FakeToggleManager>().For<IToggleManager>();
			system.UseTestDouble<FakeGeneralInfrastructure>().For<IGeneralInfrastructure>();
			system.UseTestDouble<MutableNow>().For<INow>();
			system.UseTestDouble<FakeJobScheduleRepository>().For<IJobScheduleRepository>();
			system.UseTestDouble<FakeJobHistoryRepository>().For<IJobHistoryRepository>();
		}
	}
}
