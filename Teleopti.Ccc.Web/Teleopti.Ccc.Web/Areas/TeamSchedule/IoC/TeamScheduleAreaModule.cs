using Autofac;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Infrastructure.Persisters.Schedules;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Settings.DataProvider;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Core;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Core.AbsenceHandler;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Core.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.TeamSchedule.IoC
{
	public class TeamScheduleAreaModule:Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<TeamScheduleProjectionProvider>().As<ITeamScheduleProjectionProvider>().SingleInstance();
			builder.RegisterType<TeamScheduleViewModelFactory>().As<ITeamScheduleViewModelFactory>().SingleInstance();
			builder.RegisterType<AbsencePersister>().As<IAbsencePersister>().SingleInstance();
			builder.RegisterType<PermissionChecker>().As<IPermissionChecker>().SingleInstance();
			builder.RegisterType<AgentsPerPageSettingPersisterAndProvider>().As<ISettingsPersisterAndProvider<AgentsPerPageSetting>>().SingleInstance();

			builder.RegisterType<CommonNameDescriptionSetting>().As<ICommonNameDescriptionSetting>().SingleInstance();
			builder.RegisterType<PersonRepository>().As<IPersonRepository>().SingleInstance();
			builder.RegisterType<ScheduleStorage>().As<IScheduleStorage>().SingleInstance();
			builder.RegisterType<ScenarioRepository>().As<IScenarioRepository>().SingleInstance();
			builder.RegisterType<SwapAndModifyServiceNew>().As<ISwapAndModifyServiceNew>().SingleInstance();
			builder.RegisterType<ScheduleDictionaryPersister>().As<IScheduleDictionaryPersister>().SingleInstance();

			builder.RegisterType<SwapMainShiftForTwoPersonsCommandHandler>().As<ISwapMainShiftForTwoPersonsCommandHandler>().SingleInstance();
		}
	}
}