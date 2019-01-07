using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.ApplicationLayer.PeopleSearch;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Core;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Core.DataProvider;
using Teleopti.Ccc.Web.Areas.TeamSchedule.IoC;
using Teleopti.Ccc.Web.Core;
using Teleopti.Ccc.Web.Core.IoC;
using Teleopti.Ccc.WebTest.Areas.Global;


namespace Teleopti.Ccc.WebTest.Areas.TeamSchedule
{
	public class TeamScheduleTestAttribute : IoCTestAttribute
	{
		protected override void Extend(IExtend extend, IocConfiguration configuration)
		{
			extend.AddModule(new WebModule(configuration, null));
			extend.AddModule(new TeamScheduleAreaModule());

			extend.AddService<FakeStorage>();
		}

		protected override void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FakeSchedulePersonProvider>().For<ISchedulePersonProvider>();
			isolate.UseTestDouble<FakeUserCulture>().For<IUserCulture>();
			isolate.UseTestDouble<Global.FakePermissionProvider>().For<IPermissionProvider>();
			isolate.UseTestDouble<FakeCurrentUnitOfWorkFactory>().For<ICurrentUnitOfWorkFactory>();
			isolate.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
			isolate.UseTestDouble<FakeUserTimeZone>().For<IUserTimeZone>();
			isolate.UseTestDouble<FakeCommonAgentNameProvider>().For<ICommonAgentNameProvider>();
			isolate.UseTestDouble<FakePeopleSearchProvider>().For<IPeopleSearchProvider>();
			isolate.UseTestDouble<FakePersonRepository>().For<IPersonRepository>();
			isolate.UseTestDouble<SyncCommandDispatcher>().For<ICommandDispatcher>();
			isolate.UseTestDouble<FakeTimeZoneGuard>().For<ITimeZoneGuard>();
			isolate.UseTestDouble<ProjectionProvider>().For<IProjectionProvider>();
			isolate.UseTestDouble<FakeGlobalSettingDataRepository>().For<IGlobalSettingDataRepository>();
			isolate.UseTestDouble<ProjectionSplitter>().For<IProjectionSplitter>();
			isolate.UseTestDouble<ScheduleProjectionHelper>().For<IScheduleProjectionHelper>();
			isolate.UseTestDouble<IanaTimeZoneProvider>().For<IIanaTimeZoneProvider>();
			isolate.UseTestDouble<PersonNameProvider>().For<IPersonNameProvider>();
			isolate.UseTestDouble<TeamScheduleShiftViewModelProvider>().For<TeamScheduleShiftViewModelProvider>();
			isolate.UseTestDouble<FakePersonalSettingDataRepository>().For<IPersonalSettingDataRepository>();
			isolate.UseTestDouble<ChangeActivityTypeFormValidator>().For<IChangeActivityTypeFormValidator>();

			var fakeCommandHandler = new FakeCommandHandler();
			isolate.UseTestDouble(fakeCommandHandler).For<IHandleCommand<BackoutScheduleChangeCommand>>();
			isolate.UseTestDouble(fakeCommandHandler).For<IHandleCommand<ChangeShiftCategoryCommand>>();
			isolate.UseTestDouble(fakeCommandHandler).For<IHandleCommand<FixNotOverwriteLayerCommand>>();
			isolate.UseTestDouble(fakeCommandHandler).For<IHandleCommand<EditScheduleNoteCommand>>();
			isolate.UseTestDouble(fakeCommandHandler).For<IHandleCommand<MoveShiftLayerCommand>>();
			isolate.UseTestDouble(fakeCommandHandler).For<IHandleCommand<MoveShiftCommand>>();
			isolate.UseTestDouble(fakeCommandHandler).For<IHandleCommand<AddDayOffCommand>>();
			isolate.UseTestDouble(fakeCommandHandler).For<IHandleCommand<AddActivityCommand>>();
			isolate.UseTestDouble(fakeCommandHandler).For<IHandleCommand<AddPersonalActivityCommand>>();
			isolate.UseTestDouble(fakeCommandHandler).For<IHandleCommand<AddOvertimeActivityCommand>>();
			isolate.UseTestDouble(fakeCommandHandler).For<IHandleCommand<RemoveActivityCommand>>();
			isolate.UseTestDouble(fakeCommandHandler).For<IHandleCommand<RemoveDayOffCommand>>();
			isolate.UseTestDouble(fakeCommandHandler).For<IHandleCommand<RemoveShiftCommand>>();
			isolate.UseTestDouble(fakeCommandHandler).For<IHandleCommand<MultipleChangeScheduleCommand>>();
			isolate.UseTestDouble(fakeCommandHandler).For<IHandleCommand<RemoveSelectedPersonAbsenceCommand>>();

			isolate.UseTestDouble<FakeActivityRepository>().For<IActivityRepository>();
			isolate.UseTestDouble<FullPermission>().For<IAuthorization>();
			isolate.UseTestDouble<FakePersonAssignmentWriteSideRepository>().For<IWriteSideRepositoryTypedId<IPersonAssignment, PersonAssignmentKey>>();
			isolate.UseTestDouble<FakeRepositoryFactory>().For<IRepositoryFactory>();
			isolate.UseTestDouble<FakeScenarioRepository>().For<IScenarioRepository>();
			isolate.UseTestDouble<FakePersonAssignmentRepository>().For<IPersonAssignmentRepository>();
			isolate.UseTestDouble<FakePersonAbsenceRepository>().For<IPersonAbsenceRepository>();
			isolate.UseTestDouble<FakeMeetingRepository>().For<IMeetingRepository>();
			isolate.UseTestDouble<FakeNoteRepository>().For<INoteRepository>();
			isolate.UseTestDouble<FakePublicNoteRepository>().For<IPublicNoteRepository>();
			isolate.UseTestDouble<FakeAgentDayScheduleTagRepository>().For<IAgentDayScheduleTagRepository>();
			isolate.UseTestDouble<FakeShiftCategoryRepository>().For<IShiftCategoryRepository>();

			isolate.UseTestDouble<TeamScheduleViewModelFactory>().For<ITeamScheduleViewModelFactory>();
			isolate.UseTestDouble<FakeGroupingReadOnlyRepository>().For<IGroupingReadOnlyRepository>();
		}
	}
}
