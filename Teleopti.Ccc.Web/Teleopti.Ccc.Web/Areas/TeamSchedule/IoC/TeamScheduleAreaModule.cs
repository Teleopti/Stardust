using Autofac;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.ApplicationLayer.ExportSchedule;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Settings.DataProvider;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Core;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Core.AbsenceHandler;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Core.DataProvider;

namespace Teleopti.Ccc.Web.Areas.TeamSchedule.IoC
{
	public class TeamScheduleAreaModule:Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<TeamScheduleShiftViewModelProvider>().AsSelf().SingleInstance();
			builder.RegisterType<TeamScheduleViewModelFactory>().As<ITeamScheduleViewModelFactory>().SingleInstance();
			builder.RegisterType<AbsencePersister>().As<IAbsencePersister>().SingleInstance();
			builder.RegisterType<PermissionChecker>().As<IPermissionChecker>().SingleInstance();
			builder.RegisterType<AgentsPerPageSettingPersisterAndProvider>().As<ISettingsPersisterAndProvider<AgentsPerPageSetting>>().SingleInstance();

			builder.RegisterType<CommonNameDescriptionSetting>().As<ICommonNameDescriptionSetting>().SingleInstance();
			builder.RegisterType<SwapAndModifyServiceNew>().As<ISwapAndModifyServiceNew>().SingleInstance();

			builder.RegisterType<SwapMainShiftForTwoPersonsCommandHandler>().As<ISwapMainShiftForTwoPersonsCommandHandler>().SingleInstance();
			builder.RegisterType<MoveShiftLayerCommandHelper>().As<IMoveShiftLayerCommandHelper>().SingleInstance();
			builder.RegisterType<TeamScheduleCommandHandlingProvider>().As<ITeamScheduleCommandHandlingProvider>().SingleInstance();
			builder.RegisterType<TeamScheduleActivityCommandService>().SingleInstance();
			builder.RegisterType<AbsenceCommandConverter>().As<IAbsenceCommandConverter>();
			builder.RegisterType<ScheduleValidationProvider>().As<IScheduleValidationProvider>().SingleInstance();
			builder.RegisterType<ProjectionSplitter>().As<IProjectionSplitter>().SingleInstance();

			builder.RegisterType<FavoriteSearchProvider>().As<IFavoriteSearchProvider>().SingleInstance();
			builder.RegisterType<ExportScheduleService>().SingleInstance();
			
			builder.RegisterType<ChangeActivityTypeFormValidator>().As<IChangeActivityTypeFormValidator>().SingleInstance();
		}
	}
}