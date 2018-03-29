using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.ApplicationLayer.PeopleSearch;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Core.DataProvider;
using Teleopti.Ccc.Web.Areas.TeamSchedule.IoC;
using Teleopti.Ccc.Web.Core.IoC;
using Teleopti.Ccc.WebTest.Areas.Global;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.TeamSchedule
{
	public class TeamScheduleTestAttribute : IoCTestAttribute
	{
		protected override void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddModule(new WebModule(configuration, null));
			system.AddModule(new TeamScheduleAreaModule());

			system.AddService<FakeStorage>();
			system.UseTestDouble<FakeSchedulePersonProvider>().For<ISchedulePersonProvider>();
			system.UseTestDouble<FakeUserCulture>().For<IUserCulture>();
			system.UseTestDouble<Global.FakePermissionProvider>().For<IPermissionProvider>();
			system.UseTestDouble<FakeCurrentUnitOfWorkFactory>().For<ICurrentUnitOfWorkFactory>();
			system.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
			system.UseTestDouble<FakeUserTimeZone>().For<IUserTimeZone>();
			system.UseTestDouble<FakeCommonAgentNameProvider>().For<ICommonAgentNameProvider>();
			system.UseTestDouble<FakePeopleSearchProvider>().For<IPeopleSearchProvider>();
			system.UseTestDouble<FakePersonRepository>().For<IPersonRepository>();
			system.UseTestDouble<SyncCommandDispatcher>().For<ICommandDispatcher>();
			system.UseTestDouble<FakeTimeZoneGuard>().For<ITimeZoneGuard>();

			var fakeCommandHandler = new FakeCommandHandler();
			system.UseTestDouble(fakeCommandHandler).For<IHandleCommand<BackoutScheduleChangeCommand>>();
			system.UseTestDouble(fakeCommandHandler).For<IHandleCommand<ChangeShiftCategoryCommand>>();
			system.UseTestDouble(fakeCommandHandler).For<IHandleCommand<FixNotOverwriteLayerCommand>>();
			system.UseTestDouble(fakeCommandHandler).For<IHandleCommand<EditScheduleNoteCommand>>();
			system.UseTestDouble(fakeCommandHandler).For<IHandleCommand<MoveShiftLayerCommand>>();
			system.UseTestDouble(fakeCommandHandler).For<IHandleCommand<MoveShiftCommand>>();
			system.UseTestDouble(fakeCommandHandler).For<IHandleCommand<AddDayOffCommand>>();
			system.UseTestDouble(fakeCommandHandler).For<IHandleCommand<AddActivityCommand>>();
			system.UseTestDouble(fakeCommandHandler).For<IHandleCommand<AddPersonalActivityCommand>>();
			system.UseTestDouble(fakeCommandHandler).For<IHandleCommand<AddOvertimeActivityCommand>>();
			system.UseTestDouble(fakeCommandHandler).For<IHandleCommand<RemoveActivityCommand>>();
			system.UseTestDouble(fakeCommandHandler).For<IHandleCommand<RemoveDayOffCommand>>();
			system.UseTestDouble(fakeCommandHandler).For<IHandleCommand<RemoveShiftCommand>>();

			system.UseTestDouble<FakeActivityRepository>().For<IActivityRepository>();
			system.UseTestDouble<FullPermission>().For<IAuthorization>();
			system.UseTestDouble<FakePersonAssignmentWriteSideRepository>().For<IWriteSideRepositoryTypedId<IPersonAssignment, PersonAssignmentKey>>();
			system.UseTestDouble<FakeRepositoryFactory>().For<IRepositoryFactory>();
			system.UseTestDouble<FakeScenarioRepository>().For<IScenarioRepository>();
			system.UseTestDouble<FakePersonAssignmentRepository>().For<IPersonAssignmentRepository>();
			system.UseTestDouble<FakePersonAbsenceRepository>().For<IPersonAbsenceRepository>();
			system.UseTestDouble<FakeMeetingRepository>().For<IMeetingRepository>();
			system.UseTestDouble<FakeNoteRepository>().For<INoteRepository>();
			system.UseTestDouble<FakePublicNoteRepository>().For<IPublicNoteRepository>();
			system.UseTestDouble<FakeAgentDayScheduleTagRepository>().For<IAgentDayScheduleTagRepository>();
			system.UseTestDouble<FakeShiftCategoryRepository>().For<IShiftCategoryRepository>();

			system.UseTestDouble<TeamScheduleViewModelFactory>().For<ITeamScheduleViewModelFactory>();
			system.UseTestDouble<FakeGroupingReadOnlyRepository>().For<IGroupingReadOnlyRepository>();
		}
	}
}
