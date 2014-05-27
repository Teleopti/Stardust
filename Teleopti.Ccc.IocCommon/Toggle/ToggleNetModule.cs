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
		private readonly string _pathToToggleFile;

		public ToggleNetModule(string pathToToggleFile)
		{
			_pathToToggleFile = pathToToggleFile.Trim();
		}

		protected override void Load(ContainerBuilder builder)
		{
			if (_pathToToggleFile.EndsWith("ALL", StringComparison.OrdinalIgnoreCase))
			{
				builder.Register(_ => new toggleManagerFullAccess())
					.SingleInstance()
					.As<IToggleManager>();
			}
			else
			{
				if (_pathToToggleFile.StartsWith("http://") || _pathToToggleFile.StartsWith("https://"))
				{
					builder.Register(_ => new ToggleQuerier(_pathToToggleFile))
						.SingleInstance()
						.As<IToggleManager>();
				}
				else
				{
					builder.Register(c =>
					{
						var toggleConfiguration =
							new ToggleConfiguration(new FileProviderFactory(new FileReader(_pathToToggleFile),
								new DefaultSpecificationMappings()));
						var toggleChecker = toggleConfiguration.Create();
						return new toggleCheckerWrapper(toggleChecker);
					}
						)
						.SingleInstance()
						.As<IToggleManager>();
				}
			}

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