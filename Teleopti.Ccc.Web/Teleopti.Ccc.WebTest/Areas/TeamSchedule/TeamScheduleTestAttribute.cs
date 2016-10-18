﻿using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.People.Core.Providers;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Core.DataProvider;
using Teleopti.Ccc.Web.Areas.TeamSchedule.IoC;
using Teleopti.Ccc.Web.Core.IoC;
using Teleopti.Ccc.WebTest.Areas.Global;
using Teleopti.Ccc.WebTest.Areas.Search;
using Teleopti.Ccc.WebTest.Areas.TeamSchedule.Core;
using Teleopti.Ccc.WebTest.Core.Common.DataProvider;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebTest.Areas.TeamSchedule
{
	public class TeamScheduleTestAttribute : IoCTestAttribute
	{
		protected override void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddModule(new WebModule(configuration, null));
			system.AddModule(new TeamScheduleAreaModule());

			system.UseTestDouble<FakeSchedulePersonProvider>().For<ISchedulePersonProvider>();
			system.UseTestDouble<FakeScheduleStorage>().For<IScheduleStorage>();
			system.UseTestDouble<FakeUserCulture>().For<IUserCulture>();
			system.UseTestDouble<Global.FakePermissionProvider>().For<IPermissionProvider>();
			system.UseTestDouble<FakeCurrentUnitOfWorkFactory>().For<ICurrentUnitOfWorkFactory>();
			system.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
			system.UseTestDouble<FakeUserTimeZone>().For<IUserTimeZone>();
			system.UseTestDouble<FakeCommonAgentNameProvider>().For<ICommonAgentNameProvider>();
			system.UseTestDouble<FakePeopleSearchProvider>().For<IPeopleSearchProvider>();
			system.UseTestDouble<FakePersonRepository>().For<IPersonRepository>();
			system.UseTestDouble<PersonProvider>().For<IPersonProvider>();
			system.UseTestDouble<SyncCommandDispatcher>().For<ICommandDispatcher>();

			var fakeActivityCommandHandler = new FakeActivityCommandHandler();

			system.UseTestDouble<FakeActivityCommandHandler>(fakeActivityCommandHandler).For<IHandleCommand<AddActivityCommand>>();
			system.UseTestDouble<FakeActivityCommandHandler>(fakeActivityCommandHandler).For<IHandleCommand<AddPersonalActivityCommand>>();
			system.UseTestDouble<FakeActivityCommandHandler>(fakeActivityCommandHandler).For<IHandleCommand<RemoveActivityCommand>>();
			system.UseTestDouble<FakeActivityCommandHandler>(fakeActivityCommandHandler).For<IHandleCommand<BackoutScheduleChangeCommand>>();
			system.UseTestDouble<FakeActivityCommandHandler>(fakeActivityCommandHandler).For<IHandleCommand<ChangeShiftCategoryCommand>>();
			system.UseTestDouble<FakeActivityCommandHandler>(fakeActivityCommandHandler).For<IHandleCommand<FixNotOverwriteLayerCommand>>();
			system.UseTestDouble<FakeActivityCommandHandler>(fakeActivityCommandHandler).For<IHandleCommand<EditScheduleNoteCommand>>();
			system.UseTestDouble(fakeActivityCommandHandler).For<IHandleCommand<MoveShiftLayerCommand>>();
			system.UseTestDouble<FakeActivityRepository>().For<IActivityRepository>();
			system.UseTestDouble<FullPermission>().For<IAuthorization>();
			system.UseTestDouble<FakePersonAssignmentWriteSideRepository>().For<IWriteSideRepositoryTypedId<IPersonAssignment, PersonAssignmentKey>>();
			system.UseTestDouble<FakeCurrentScenario>().For<ICurrentScenario>();
			system.UseTestDouble<FakeShiftCategoryRepository>().For<IShiftCategoryRepository>();

			system.UseTestDouble<TeamScheduleViewModelFactory>().For<ITeamScheduleViewModelFactory>();

		}
	}
}
