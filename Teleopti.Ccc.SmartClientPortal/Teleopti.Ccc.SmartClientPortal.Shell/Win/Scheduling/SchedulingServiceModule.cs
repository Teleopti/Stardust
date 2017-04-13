using Autofac;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Scheduling
{
    public class SchedulingServiceModule : Module
    {
	    protected override void Load(ContainerBuilder builder)
	    {
			builder.RegisterType<VirtualSkillHelper>().As<IVirtualSkillHelper>().InstancePerLifetimeScope();
			builder.RegisterType<BestShiftChooser>().InstancePerLifetimeScope();
			builder.RegisterType<ScheduleOptimizerHelper>().InstancePerLifetimeScope();
			builder.RegisterType<OptimizerHelperHelper>().InstancePerLifetimeScope();
			builder.RegisterType<CurrentTeleoptiPrincipal>().As<ICurrentTeleoptiPrincipal>().SingleInstance();
		}
	}
}
