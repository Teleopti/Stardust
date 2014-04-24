using System;
using System.IO;
using Autofac;
using Teleopti.Ccc.Domain.FeatureFlags;
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
			_pathToToggleFile = pathToToggleFile;
		}

		protected override void Load(ContainerBuilder builder)
		{
			if (_pathToToggleFile.Equals("ALL", StringComparison.OrdinalIgnoreCase))
			{
				builder.Register(_ => new toggleManagerFullAccess())
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