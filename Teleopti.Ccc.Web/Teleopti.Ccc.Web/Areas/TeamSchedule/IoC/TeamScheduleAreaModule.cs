using Autofac;
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
			builder.RegisterType<TeamScheduleProjectionProvider>().As<ITeamScheduleProjectionProvider>().SingleInstance();
			builder.RegisterType<TeamScheduleViewModelFactory>().As<ITeamScheduleViewModelFactory>().SingleInstance();
			builder.RegisterType<AbsencePersister>().As<IAbsencePersister>().SingleInstance();
			builder.RegisterType<TeamScheduleGroupPageViewModelFactory>().As<ITeamScheduleGroupPageViewModelFactory>().SingleInstance();
			builder.RegisterType<AgentsPerPageSettingPersisterAndProvider>().As<ISettingsPersisterAndProvider<AgentsPerPageSetting>>().SingleInstance();		
		}
	}
}