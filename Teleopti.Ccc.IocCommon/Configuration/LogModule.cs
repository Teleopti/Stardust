using System.Linq;
using Autofac;
using Autofac.Core;
using log4net;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	internal class LogModule : Module
	{
		protected override void AttachToComponentRegistration(IComponentRegistry componentRegistry, 
															IComponentRegistration registration)
		{
			registration.Preparing += onComponentPreparing;
		}

		static void onComponentPreparing(object sender, PreparingEventArgs e)
		{
			var t = e.Component.Activator.LimitType;
			e.Parameters = e.Parameters.Union(new[]
				{
					new ResolvedParameter((p, i) => p.ParameterType == typeof(ILog), (p, i) => LogManager.GetLogger(t))
				});
		}
	}
}