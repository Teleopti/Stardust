using Teleopti.Analytics.Etl.Common.Infrastructure;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Wfm.Administration.Core.Modules;
using Teleopti.Wfm.AdministrationTest.EtlTool;

namespace Teleopti.Wfm.AdministrationTest
{
	public class AdministrationTestAttribute : DomainTestAttribute
	{
		protected override void Setup(ISystem system, IIocConfiguration configuration)
		{
			base.Setup(system, configuration);
			system.AddModule(new EtlToolModule());

			system.UseTestDouble<FakeBaseConfigurationRepository>().For<IBaseConfigurationRepository>();
			system.UseTestDouble<FakeGeneralInfrastructure>().For<IGeneralInfrastructure>();
		}
	}
}
