using Autofac;
using Teleopti.Ccc.Domain.DistributedLock;
using Teleopti.Ccc.Infrastructure.DistributedLock;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	internal class DistributedLockModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<DistributedLockAcquirer>().As<IDistributedLockAcquirer>().SingleInstance();
		}
	}
}