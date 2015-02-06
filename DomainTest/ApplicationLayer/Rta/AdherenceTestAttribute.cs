using Autofac;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta
{
	public class AdherenceTestAttribute : IoCTestAttribute
	{
		protected override void RegisterInContainer(ContainerBuilder builder, IIocConfiguration configuration)
		{
			base.RegisterInContainer(builder, configuration);

			builder.RegisterType<FakeAdherencePercentageReadModelPersister>().As<IAdherencePercentageReadModelPersister>().AsSelf().SingleInstance();	
		}
	}
}