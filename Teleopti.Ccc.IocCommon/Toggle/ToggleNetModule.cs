﻿using System;
using Autofac;
using log4net;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Toggle;
using Toggle.Net;
using Toggle.Net.Configuration;
using Toggle.Net.Providers.TextFile;
using Toggle.Net.Specifications;

namespace Teleopti.Ccc.IocCommon.Toggle
{
	internal class ToggleNetModule : Module
	{
		private readonly IIocConfiguration _configuration;
		private const string missingPathToToggle = "Path to toggle file is missing. Please use a valid path (or use a http address to point to the toggle.net service)!";
		private static readonly ILog logger = LogManager.GetLogger(typeof(ToggleNetModule));

		public ToggleNetModule(IIocConfiguration configuration)
		{
			_configuration = configuration;
		}

		protected override void Load(ContainerBuilder builder)
		{
			var pathToToggle = _configuration.Args().FeatureToggle;
			var toggleModeArg = _configuration.Args().ToggleMode;

			if (string.IsNullOrEmpty(pathToToggle))
			{
				logger.Warn(missingPathToToggle);
				builder.RegisterType<FalseToggleManager>()
					.SingleInstance()
					.As<IToggleManager>();
			}
			else if (togglePathIsAnUrl())
			{
				builder.Register(c => new ToggleQuerier(pathToToggle))
					.SingleInstance()
					.As<IToggleManager>()
					.As<IToggleFiller>();
			}
			else
			{
				builder.Register<IToggleManager>(c =>
				{
					const string developerMode = "ALL";
					const string rcMode = "RC";

					var toggleMode = toggleModeArg==null ? 
						string.Empty : 
						toggleModeArg.Trim();

					var defaultSpecification = toggleMode.Equals(developerMode, StringComparison.OrdinalIgnoreCase)
						? (IToggleSpecification) new TrueSpecification()
						: new FalseSpecification();
					var rcSpecification = toggleMode.Equals(developerMode, StringComparison.OrdinalIgnoreCase) ||
																toggleMode.Equals(rcMode, StringComparison.OrdinalIgnoreCase)
						? (IToggleSpecification) new TrueSpecification()
						: new FalseSpecification();

					var specMappings = new DefaultSpecificationMappings();
					specMappings.AddMapping("rc", rcSpecification);
					specMappings.AddMapping("dev", defaultSpecification);
					var toggleConfiguration = new ToggleConfiguration(new FileParser(new FileReader(pathToToggle), specMappings));
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

		private bool togglePathIsAnUrl()
		{
			var pathToToggle = _configuration.Args().FeatureToggle;
			return pathToToggle.StartsWith("http://") || pathToToggle.StartsWith("https://");
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