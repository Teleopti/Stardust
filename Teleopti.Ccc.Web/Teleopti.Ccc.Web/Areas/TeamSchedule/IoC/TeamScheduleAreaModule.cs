using Autofac;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Core;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Core.AbsenceHandler;

namespace Teleopti.Ccc.Web.Areas.TeamSchedule.IoC
{
	public class TeamScheduleAreaModule:Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<TeamScheduleProjectionProvider>().As<ITeamScheduleProjectionProvider>().SingleInstance();
			builder.RegisterType<TeamScheduleViewModelFactory>().As<ITeamScheduleViewModelFactory>().SingleInstance();
			builder.RegisterType<AbsencePersister>().As<IAbsencePersister>().SingleInstance();
		}
	}
}