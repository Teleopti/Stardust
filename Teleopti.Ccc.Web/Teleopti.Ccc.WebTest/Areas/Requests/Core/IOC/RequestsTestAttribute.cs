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
	public class RequestsTestAttribute : DomainTestAttribute
	{
		protected override void Extend(IExtend extend, IIocConfiguration configuration)
		{
			base.Extend(extend, configuration);
			extend.AddModule(new WebModule(configuration, null));

			extend.AddService<FakeStorage>();
		}

		protected override void Isolate(IIsolate isolate)
		{
			base.Isolate(isolate);

			var scenario = new FakeScenarioRepository();
			scenario.Has("Default");

			isolate.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
			isolate.UseTestDouble<Global.FakePermissionProvider>().For<IPermissionProvider>();
			isolate.UseTestDouble(scenario).For<IScenarioRepository>();
			isolate.UseTestDouble<FakeRepositoryFactory>().For<IRepositoryFactory>();
			isolate.UseTestDouble<FakePersonRequestRepository>().For<IPersonRequestRepository>();
			isolate.UseTestDouble<FakeGroupingReadOnlyRepository>().For<IGroupingReadOnlyRepository>();
			isolate.UseTestDouble<FakePersonAbsenceRepository>().For<IPersonAbsenceRepository>();
			isolate.UseTestDouble<FakeTeamRepository>().For<ITeamRepository>();
			isolate.UseTestDouble<FakePersonRepository>().For<IPersonRepository>();
			isolate.UseTestDouble<FakePersonAssignmentRepository>().For<IPersonAssignmentRepository>();
			isolate.UseTestDouble<FakePersonAbsenceRepository>().For<IPersonAbsenceRepository>();
			isolate.UseTestDouble<FakeMeetingRepository>().For<IMeetingRepository>();
			isolate.UseTestDouble<FakeNoteRepository>().For<INoteRepository>();
			isolate.UseTestDouble<FakePublicNoteRepository>().For<IPublicNoteRepository>();
			isolate.UseTestDouble<FakeAgentDayScheduleTagRepository>().For<IAgentDayScheduleTagRepository>();
			isolate.UseTestDouble<FakeOvertimeAvailabilityRepository>().For<IOvertimeAvailabilityRepository>();
			isolate.UseTestDouble<FakePersonAvailabilityRepository>().For<IPersonAvailabilityRepository>();
			isolate.UseTestDouble<FakePersonRotationRepository>().For<IPersonRotationRepository>();
			isolate.UseTestDouble<FakePersonAbsenceAccountRepository>().For<IPersonAbsenceAccountRepository>();
			isolate.UseTestDouble<FakePreferenceDayRepository>().For<IPreferenceDayRepository>();
			isolate.UseTestDouble<FakeStudentAvailabilityDayRepository>().For<IStudentAvailabilityDayRepository>();
			isolate.UseTestDouble<FakePersonAbsenceAccountRepository>().For<IPersonAbsenceAccountRepository>();
			isolate.UseTestDouble<FakeQueuedAbsenceRequestRepository>().For<IQueuedAbsenceRequestRepository>();
			isolate.UseTestDouble<FakePeopleSearchProvider>().For<IPeopleSearchProvider>();
			isolate.UseTestDouble<FakeCurrentUnitOfWorkFactory>().For<ICurrentUnitOfWorkFactory>();

			isolate.UseTestDouble<SyncCommandDispatcher>().For<ICommandDispatcher>();
			isolate.UseTestDouble<FakeRequestApprovalServiceFactory>().For<IRequestApprovalServiceFactory>();

			isolate.UseTestDouble(new FakeUserTimeZone(TimeZoneInfo.Utc)).For<IUserTimeZone>();
			var personRequestCheckAuthorization = new PersonRequestAuthorizationCheckerConfigurable();
			isolate.UseTestDouble(personRequestCheckAuthorization).For<IPersonRequestCheckAuthorization>();
			isolate.UseTestDouble(new FakeCommonAgentNameProvider()).For<ICommonAgentNameProvider>();

			isolate.UseTestDouble(new FakeLoggedOnUser()).For<ILoggedOnUser>();
			isolate.UseTestDouble(new FakeUserCulture(CultureInfo.GetCultureInfo("en-US"))).For<IUserCulture>();
			isolate.UseTestDouble<FakeEventPublisher>().For<IEventPublisher>();
			isolate.UseTestDouble(new FakeStardustJobFeedback()).For<IStardustJobFeedback>();

			isolate.UseTestDouble(new FakeToggleManager()).For<IToggleManager>();
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