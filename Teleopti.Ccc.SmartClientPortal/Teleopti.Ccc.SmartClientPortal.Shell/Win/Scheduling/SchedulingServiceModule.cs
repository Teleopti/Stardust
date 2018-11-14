using Autofac;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling
{
    public class SchedulingServiceModule : Module
    {
	    protected override void Load(ContainerBuilder builder)
	    {
			builder.RegisterType<VirtualSkillHelper>().As<IVirtualSkillHelper>().InstancePerLifetimeScope();
			builder.RegisterType<BestShiftChooser>().InstancePerLifetimeScope();
			builder.RegisterType<CurrentTeleoptiPrincipal>().As<ICurrentTeleoptiPrincipal>().SingleInstance();
		}
	}
}
