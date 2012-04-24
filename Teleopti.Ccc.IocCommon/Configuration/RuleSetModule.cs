using Autofac;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	public class RuleSetModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<CreateWorkShiftsFromTemplate>()
				.As<ICreateWorkShiftsFromTemplate>()
				.SingleInstance();
			builder.RegisterType<ShiftCreatorService>()
				.As<IShiftCreatorService>()
				.SingleInstance();
			builder.RegisterType<RuleSetProjectionService>()
				.As<IRuleSetProjectionService>()
				.InstancePerLifetimeScope();
		}
	}
}