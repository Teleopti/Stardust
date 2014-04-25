﻿using System;
using Autofac;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.Foundation;
using Toggle.Net;
using Toggle.Net.Internal;
using Toggle.Net.Providers.TextFile;

namespace Teleopti.Ccc.IocCommon.Configuration
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
					var toggleChecker = new ToggleChecker(new FileProvider(new FileReader(_pathToToggleFile)));
					builder.Register(_ => new toggleCheckerWrapper(toggleChecker))
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
				return true;
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