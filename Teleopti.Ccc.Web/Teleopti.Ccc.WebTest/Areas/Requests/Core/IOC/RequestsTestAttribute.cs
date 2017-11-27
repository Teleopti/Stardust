using System;
using System.Globalization;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.PeopleSearch;
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
using Teleopti.Ccc.Web.Core.IoC;
using Teleopti.Ccc.WebTest.Areas.Global;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Requests.Core.IOC
{
	class RequestsTestAttribute : IoCTestAttribute
	{
		protected override void Setup(ISystem system, IIocConfiguration configuration)
		{
			var principalAuthorization = new FullPermission();

			CurrentAuthorization.DefaultTo(principalAuthorization);
			var scenario = new FakeScenarioRepository();
			scenario.Has("Default");

			system.AddModule(new WebModule(configuration, null));

			system.AddService<FakeStorage>();
			system.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
			system.UseTestDouble<Global.FakePermissionProvider>().For<IPermissionProvider>();
			system.UseTestDouble(principalAuthorization).For<IAuthorization>();
			system.UseTestDouble(scenario).For<IScenarioRepository>();
			system.UseTestDouble<FakeRepositoryFactory>().For<IRepositoryFactory>();
			system.UseTestDouble<FakePersonRequestRepository>().For<IPersonRequestRepository>();
			system.UseTestDouble<FakeGroupingReadOnlyRepository>().For<IGroupingReadOnlyRepository>();
			system.UseTestDouble<FakePersonAbsenceRepository>().For<IPersonAbsenceRepository>();
			system.UseTestDouble<FakeTeamRepository>().For<ITeamRepository>();
			system.UseTestDouble<FakePersonRepository>().For<IPersonRepository>();
			system.UseTestDouble<FakePersonAssignmentRepository>().For<IPersonAssignmentRepository>();
			system.UseTestDouble<FakePersonAbsenceRepository>().For<IPersonAbsenceRepository>();
			system.UseTestDouble<FakeMeetingRepository>().For<IMeetingRepository>();
			system.UseTestDouble<FakeNoteRepository>().For<INoteRepository>();
			system.UseTestDouble<FakePublicNoteRepository>().For<IPublicNoteRepository>();
			system.UseTestDouble<FakeAgentDayScheduleTagRepository>().For<IAgentDayScheduleTagRepository>();
			system.UseTestDouble<FakeOvertimeAvailabilityRepository>().For<IOvertimeAvailabilityRepository>();
			system.UseTestDouble<FakePersonAvailabilityRepository>().For<IPersonAvailabilityRepository>();
			system.UseTestDouble<FakePersonRotationRepository>().For<IPersonRotationRepository>();
			system.UseTestDouble<FakePersonAbsenceAccountRepository>().For<IPersonAbsenceAccountRepository>();
			system.UseTestDouble<FakePreferenceDayRepository>().For<IPreferenceDayRepository>();
			system.UseTestDouble<FakeStudentAvailabilityDayRepository>().For<IStudentAvailabilityDayRepository>();
			system.UseTestDouble<FakePersonAbsenceAccountRepository>().For<IPersonAbsenceAccountRepository>();
			system.UseTestDouble<FakeQueuedAbsenceRequestRepository>().For<IQueuedAbsenceRequestRepository>();
			system.UseTestDouble<FakePeopleSearchProvider>().For<IPeopleSearchProvider>();
			system.UseTestDouble<FakeCurrentUnitOfWorkFactory>().For<ICurrentUnitOfWorkFactory>();

			system.UseTestDouble<SyncCommandDispatcher>().For<ICommandDispatcher>();
			system.UseTestDouble<FakeRequestApprovalServiceFactory>().For<IRequestApprovalServiceFactory>();
			
			system.UseTestDouble(new FakeUserTimeZone(TimeZoneInfo.Utc)).For<IUserTimeZone>();
			var personRequestCheckAuthorization = new PersonRequestAuthorizationCheckerConfigurable();
			system.UseTestDouble(personRequestCheckAuthorization).For<IPersonRequestCheckAuthorization>();
			system.UseTestDouble(new FakePermissions()).For<IAuthorization>();
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
