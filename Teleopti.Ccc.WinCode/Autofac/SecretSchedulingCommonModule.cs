using Autofac;
using Teleopti.Ccc.Secrets.DayOffPlanning;
using Teleopti.Ccc.Secrets.WorkShiftCalculator;
using Teleopti.Ccc.Secrets.WorkShiftPeriodValueCalculator;

namespace Teleopti.Ccc.WinCode.Autofac
{
	public class SecretSchedulingCommonModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<WorkShiftCalculator>().As<IWorkShiftCalculator>().InstancePerLifetimeScope();
			builder.RegisterType<DayOffBackToLegalStateFunctions>().As<IDayOffBackToLegalStateFunctions>();
			builder.RegisterType<WorkShiftPeriodValueCalculator>().As<IWorkShiftPeriodValueCalculator>();
		}
	}
}