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

		public void FillToggles()
		{
			var toggleQuerier = _toggleManager as ToggleQuerier;
			toggleQuerier?.RefetchToggles();
		}

		public virtual bool Toggle(Toggles toggle)
		{
			return _toggleManager != null && _toggleManager.IsEnabled(toggle);
		}

		public IocArgs Args()
		{
			return _args;
		}

		public void AddToggleManagerToBuilder(ContainerBuilder builder)
		{
			builder.RegisterInstance(_toggleManager).As<IToggleManager>().As<IToggleFiller>().SingleInstance();
		}
	}
}