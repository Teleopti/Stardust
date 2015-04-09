using Autofac;
using Teleopti.Ccc.Domain.Scheduling.BackToLegalShift;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	public class BackToLegalShiftModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<BackToLegalShiftWorker>().As<IBackToLegalShiftWorker>();
			builder.RegisterType<LegalShiftDecider>().As<ILegalShiftDecider>();
			builder.RegisterType<BackToLegalShiftService>().As<IBackToLegalShiftService>();

			//BackToLegalShiftCommand
		}
	}
}