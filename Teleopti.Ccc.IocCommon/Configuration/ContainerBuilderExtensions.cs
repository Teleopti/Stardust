using Autofac;
using LinFu.DynamicProxy;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.Toggle;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	public static class ContainerBuilderExtensions
	{
		private static readonly ProxyFactory proxyFactory = new ProxyFactory();
		
		public static void RegisterToggledTypeTest<TToggleOn, TToggleOff, IT>(
			this ContainerBuilder builder, IToggleManager toggleManager, Toggles toggle)
			where TToggleOn : IT 
			where TToggleOff : IT
			where IT : class
		{
			builder.RegisterType<TToggleOn>().SingleInstance();
			builder.RegisterType<TToggleOff>().SingleInstance();
			var proxy = proxyFactory.CreateProxy<IT>(new toggledTypeInterceptor<TToggleOn, TToggleOff>(builder, toggle));
			builder.RegisterInstance(proxy);
		}
		
		
		private class toggledTypeInterceptor<TToggleOn, TToggleOff> : IInterceptor
		{
			private readonly Toggles _toggle;
			private object _onSvc;
			private object _offSvc;
			private IToggleManager _toggleManager;

			public toggledTypeInterceptor(ContainerBuilder builder, Toggles toggle)
			{
				_toggle = toggle;
				builder.RegisterBuildCallback(container =>
				{
					_toggleManager = container.Resolve<IToggleManager>();
					_onSvc = container.Resolve<TToggleOn>(); 
					_offSvc = container.Resolve<TToggleOff>(); 
				});
			}

			public object Intercept(InvocationInfo info)
			{
				var svc = _toggleManager.IsEnabled(_toggle) ? _onSvc : _offSvc;
				return info.TargetMethod.Invoke(svc, info.Arguments);
			}
		}
	}
}