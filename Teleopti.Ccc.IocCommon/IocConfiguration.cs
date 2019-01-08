using Autofac;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.Toggle;

namespace Teleopti.Ccc.IocCommon
{
	public class IocConfiguration
	{
		private readonly IocArgs _args;
		private readonly IToggleManager _toggleManager;

		public IocConfiguration(IocArgs args, IToggleManager toggleManager)
		{
			_args = args;
			_toggleManager = toggleManager;
		}

		protected IocConfiguration()
		{
			//just to support some old mock tests...
		}

		public virtual bool IsToggleEnabled(Toggles toggle)
		{
			return _toggleManager?.IsEnabled(toggle) ?? false;
		}

		public IocArgs Args()
		{
			return _args;
		}

		public void AddToggleManagerToBuilder(ContainerBuilder builder)
		{
			if (_toggleManager is IToggleFiller)
			{
				builder.RegisterInstance(_toggleManager).As<IToggleManager>().As<IToggleFiller>().SingleInstance();
			}
			else
			{
				builder.RegisterInstance(_toggleManager).As<IToggleManager>().SingleInstance();
				builder.RegisterType<noToggleFiller>().As<IToggleFiller>().SingleInstance();
			}
		}
		
		private class noToggleFiller : IToggleFiller
		{
			public void RefetchToggles()
			{
			}
		}
	}
}