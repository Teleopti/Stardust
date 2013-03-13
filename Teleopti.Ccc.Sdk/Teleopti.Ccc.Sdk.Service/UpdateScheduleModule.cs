using Autofac;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Sdk.Logic.CommandHandler;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.WcfService
{
	public class UpdateScheduleModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<BusinessRulesForPersonalAccountUpdate>().As<IBusinessRulesForPersonalAccountUpdate>().
			        InstancePerDependency();
			builder.RegisterType<SchedulingResultStateHolder>().As<ISchedulingResultStateHolder>().InstancePerDependency();
		}
	}
}