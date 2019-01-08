using Autofac;
using Autofac.Builder;
using LinFu.DynamicProxy;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Infrastructure.Aop;
using InvocationInfo = LinFu.DynamicProxy.InvocationInfo;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	public static class ContainerBuilderExtensions
	{
		private static readonly ProxyFactory proxyFactory = new ProxyFactory();
		
		public static IRegistrationBuilder<TInterface, SimpleActivatorData, SingleRegistrationStyle> RegisterToggledComponent<TToggleOn, TToggleOff, TInterface>(this ContainerBuilder builder, Toggles toggle)
			where TToggleOn : TInterface 
			where TToggleOff : TInterface
			where TInterface : class
		{
			builder.RegisterType<TToggleOn>().ApplyAspects();
			builder.RegisterType<TToggleOff>().ApplyAspects();
			return builder.Register(c => proxyFactory.CreateProxy<TInterface>(new toggledTypeInterceptor<TToggleOn, TToggleOff>(c, toggle)));
		}

		private class toggledTypeInterceptor<TToggleOn, TToggleOff> : IInterceptor
		{
			private readonly Toggles _toggle;
			private readonly IToggleManager _toggleManager;
			private readonly TToggleOn _onService;
			private readonly TToggleOff _offService;

			public toggledTypeInterceptor(IComponentContext componentContext, Toggles toggle)
			{
				_toggleManager = componentContext.Resolve<IToggleManager>();
				_onService = componentContext.Resolve<TToggleOn>();
				_offService = componentContext.Resolve<TToggleOff>();
				_toggle = toggle;
			}

			public object Intercept(InvocationInfo info)
			{
				var svc = _toggleManager.IsEnabled(_toggle) ? 
					(object)_onService : 
					_offService;
				return info.TargetMethod.Invoke(svc, info.Arguments);
			}
		}
	}
}