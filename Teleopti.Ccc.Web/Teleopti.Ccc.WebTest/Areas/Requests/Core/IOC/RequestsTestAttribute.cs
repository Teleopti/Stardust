using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Core;
using Teleopti.Ccc.Web.Core.IoC;
using Teleopti.Ccc.WebTest.Core.Common;
using Teleopti.Ccc.WebTest.Core.WeekSchedule.Mapping;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Requests.Core.IOC
{
	class RequestsTestAttribute : IoCTestAttribute
	{
		protected override void Setup(ISystem system, IIocConfiguration configuration)
		{
			var scenario = new FakeCurrentScenario();
			var principalAuthorization = new PrincipalAuthorizationWithFullPermission();

			PrincipalAuthorization.SetInstance(principalAuthorization);

			system.AddModule(new WebModule(configuration, null));

			system.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
			system.UseTestDouble(new FakePermissionProvider(false)).For<IPermissionProvider>();
			system.UseTestDouble(scenario).For<ICurrentScenario>();
			system.UseTestDouble(principalAuthorization).For<IPrincipalAuthorization>();
			system.UseTestDouble<FakePersonNameProvider>().For<IPersonNameProvider>();
			system.UseTestDouble(new FakePersonRequestRepository()).For<IPersonRequestRepository>();		
		}
		
		

	}
}
