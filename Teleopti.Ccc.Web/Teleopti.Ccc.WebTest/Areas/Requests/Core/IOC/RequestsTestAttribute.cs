using System;
using System.Globalization;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.People.Core.Providers;
using Teleopti.Ccc.Web.Core;
using Teleopti.Ccc.Web.Core.IoC;
using Teleopti.Ccc.WebTest.Areas.Global;
using Teleopti.Ccc.WebTest.Core.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Requests.Core.IOC
{
	class RequestsTestAttribute : IoCTestAttribute
	{
		protected override void Setup(ISystem system, IIocConfiguration configuration)
		{
			var scenario = new FakeCurrentScenario();
			var principalAuthorization = new FullPermission();

			CurrentAuthorization.DefaultTo(principalAuthorization);

			system.AddModule(new WebModule(configuration, null));

			system.AddService<FakeStorage>();
			system.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
			system.UseTestDouble<Global.FakePermissionProvider>().For<IPermissionProvider>();
			system.UseTestDouble(scenario).For<ICurrentScenario>();
			system.UseTestDouble(principalAuthorization).For<IAuthorization>();
			system.UseTestDouble<FakePersonNameProvider>().For<IPersonNameProvider>();
			system.UseTestDouble(new FakePersonRequestRepository()).For<IPersonRequestRepository>();
			system.UseTestDouble(new FakeGroupingReadOnlyRepository()).For<IGroupingReadOnlyRepository>();
			system.UseTestDouble(new FakePersonAbsenceRepositoryLegacy()).For<IPersonAbsenceRepository>();
			system.UseTestDouble(new FakePersonAbsenceAccountRepository()).For<IPersonAbsenceAccountRepository>();
			system.UseTestDouble<FakePeopleSearchProvider>().For<IPeopleSearchProvider>();

			system.UseTestDouble<SyncCommandDispatcher>().For<ICommandDispatcher>();
			system.UseTestDouble<FakeRequestApprovalServiceFactory>().For<IRequestApprovalServiceFactory>();
			system.UseTestDouble<FakeScheduleDataReadScheduleStorage>().For<IScheduleStorage>();

			system.UseTestDouble(new FakeUserTimeZone(TimeZoneInfo.Utc)).For<IUserTimeZone>();
			var personRequestCheckAuthorization = new PersonRequestAuthorizationCheckerConfigurable();
			system.UseTestDouble(personRequestCheckAuthorization).For<IPersonRequestCheckAuthorization>();
			system.UseTestDouble(new ConfigurablePermissions()).For<IAuthorization>();
			system.UseTestDouble(new FakeCommonAgentNameProvider()).For<ICommonAgentNameProvider>();

			system.UseTestDouble(new FakeLoggedOnUser()).For<ILoggedOnUser>();
			system.UseTestDouble(new FakeUserCulture(CultureInfo.GetCultureInfo("en-US"))).For<IUserCulture>();
			system.UseTestDouble<FakeEventPublisher>().For<IEventPublisher>();
			system.UseTestDouble(new FakeStardustJobFeedback()).For<IStardustJobFeedback>();

			system.UseTestDouble (new FakeToggleManager()).For<IToggleManager>();
		}
	}

    public class FakeDisableDeletedFilter : IDisableDeletedFilter
    {
        public IDisposable Disable()
        {
            return null;
        }
    }
}
