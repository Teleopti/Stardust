using System;
using Autofac;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.IocCommon.Configuration;
using Toggle.Net;
using Toggle.Net.Configuration;
using Toggle.Net.Providers.TextFile;
using Toggle.Net.Specifications;

namespace Teleopti.Ccc.IocCommon.Toggle
{
	public class ToggleNetModule : Module
	{
		private readonly string _pathToToggle;
		private readonly string _toggleMode;
		
		public ToggleNetModule(string pathToToggle, string toggleMode)
		{
			_toggleMode = toggleMode;
			_pathToToggle = pathToToggle.Trim();
		}

		protected override void Load(ContainerBuilder builder)
		{
			if (togglePathIsAnUrl())
			{
				builder.Register(c => new ToggleQuerier(c.Resolve<ICurrentDataSource>(), _pathToToggle))
					.SingleInstance()
					.As<IToggleManager>()
					.As<IToggleFiller>();
			}
			else
			{
				builder.Register<IToggleManager>(c =>
				{
					const string developerMode = "DEV";
					const string rcMode = "RC";

					var toggleMode = _toggleMode==null ? 
						string.Empty : 
						_toggleMode.Trim();

					if (togglePathIsNotDefined())
						return new FalseToggleManager();

					var defaultSpecification = toggleMode.Equals(developerMode, StringComparison.OrdinalIgnoreCase)
						? (IToggleSpecification) new TrueSpecification()
						: new FalseSpecification();
					var rcSpecification = toggleMode.Equals(developerMode, StringComparison.OrdinalIgnoreCase) ||
																toggleMode.Equals(rcMode, StringComparison.OrdinalIgnoreCase)
						? (IToggleSpecification) new TrueSpecification()
						: new FalseSpecification();

					var specMappings = new DefaultSpecificationMappings();
					specMappings.AddMapping("rc", rcSpecification);
					var toggleConfiguration = new ToggleConfiguration(new FileProviderFactory(new FileReader(_pathToToggle), specMappings));
					toggleConfiguration.SetDefaultSpecification(defaultSpecification);
					return new toggleCheckerWrapper(toggleConfiguration.Create());
				})
				.SingleInstance()
				.As<IToggleManager>();
			}

			builder.RegisterType<TogglesActive>()
				.SingleInstance().As<ITogglesActive>();
			builder.RegisterType<AllToggles>()
				.SingleInstance().As<IAllToggles>();
		}

		public static void RegisterDependingModules(ContainerBuilder builder)
		{
			//if using in scenarios where not all modules are registered
			builder.RegisterModule<UnitOfWorkModule>();
			builder.RegisterModule<AuthenticationModule>();
		}

		private bool togglePathIsAnUrl()
		{
			return _pathToToggle.StartsWith("http://") || _pathToToggle.StartsWith("https://");
		}

		private bool togglePathIsNotDefined()
		{
			return string.IsNullOrEmpty(_pathToToggle);
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