using Autofac;
using Teleopti.Ccc.Infrastructure.Util;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	public class EmailModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<EmailSender>().As<IEmailSender>();
		}
	}
}