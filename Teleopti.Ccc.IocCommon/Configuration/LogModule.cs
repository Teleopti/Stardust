using System.Diagnostics;
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

		[DebuggerStepThrough]
		private static void onComponentPreparing(object sender, PreparingEventArgs e)
		{
			var t = e.Component.Activator.LimitType;
			ResolvedParameter resolvedParameter = new ResolvedParameter((p, i) => p.ParameterType == typeof(ILog), (p, i) => LogManager.GetLogger(t));
			e.Parameters = e.Parameters.Union(new[]
				{
					resolvedParameter
				});
		}
	}
}