using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories.Tenant;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Wfm.Administration.Core.Modules;

namespace Teleopti.Wfm.AdministrationTest
{
	public class AdministrationTestAttribute : DomainTestAttribute
	{
		protected override void Setup(ISystem system, IIocConfiguration configuration)
		{
			base.Setup(system, configuration);
			system.AddModule(new EtlToolModule());
		}
	}
}
