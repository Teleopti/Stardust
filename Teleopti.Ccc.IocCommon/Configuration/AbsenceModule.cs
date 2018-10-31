using Autofac;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.SaveSchedulePart;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	public class AbsenceModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<PersonAbsenceCreator>().As<IPersonAbsenceCreator>();
			builder.RegisterType<PersonAbsenceRemover>().As<IPersonAbsenceRemover>();
			builder.RegisterType<AbsenceCommandConverter>().As<IAbsenceCommandConverter>();
			builder.RegisterType<SaveSchedulePartService>().As<ISaveSchedulePartService>().InstancePerDependency();
		}
	}
}
