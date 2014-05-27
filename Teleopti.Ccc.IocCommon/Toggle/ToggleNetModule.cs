using System;
using Autofac;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.Foundation;
using Toggle.Net;
using Toggle.Net.Configuration;
using Toggle.Net.Providers.TextFile;

namespace Teleopti.Ccc.IocCommon.Toggle
{
	public class ToggleNetModule : Module
	{
		private readonly string _pathToToggle;

		public ToggleNetModule(string pathToToggle)
		{
			_pathToToggle = pathToToggle.Trim();
		}

		protected override void Load(ContainerBuilder builder)
		{
			builder.Register<IToggleManager>(c =>
			{
				if (_pathToToggle.StartsWith("http://") || _pathToToggle.StartsWith("https://"))
				{
					return new ToggleQuerier(_pathToToggle);
				}
				if (_pathToToggle.EndsWith("ALL", StringComparison.OrdinalIgnoreCase))
				{
					return new toggleManagerFullAccess();
				}
				var toggleConfiguration =
					new ToggleConfiguration(new FileProviderFactory(new FileReader(_pathToToggle), new DefaultSpecificationMappings()));
				var toggleChecker = toggleConfiguration.Create();
				return new toggleCheckerWrapper(toggleChecker);
			})
				.SingleInstance()
				.As<IToggleManager>();

			builder.RegisterType<TogglesActive>()
				.SingleInstance().As<ITogglesActive>();
			builder.RegisterType<AllToggles>()
				.SingleInstance().As<IAllToggles>();
		}

		private class toggleManagerFullAccess : IToggleManager
		{
			public bool IsEnabled(Toggles toggle)
			{
				return toggle != Toggles.DisabledFeature;
			}
		}

		private class toggleCheckerWrapper : IToggleManager
		{
			private readonly IToggleChecker _toggleChecker;

			public toggleCheckerWrapper(IToggleChecker toggleChecker)
			{
				_toggleChecker = toggleChecker;
			}

			public bool IsEnabled(Toggles toggle)
			{
				return _toggleChecker.IsEnabled(toggle.ToString());
			}
		}
	}
}