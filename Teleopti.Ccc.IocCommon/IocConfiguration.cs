using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.Toggle;

namespace Teleopti.Ccc.IocCommon
{
	public class IocConfiguration : IIocConfiguration
	{
		private readonly IocArgs _args;
		private readonly IocCache _cache = new IocCache();
		private readonly IToggleManager _toggleManager;

		public IocConfiguration(IocArgs args, IToggleManager toggleManager)
		{
			_args = args;
			_toggleManager = toggleManager;
		}

		public bool Toggle(Toggles toggle)
		{
			return _toggleManager != null && _toggleManager.IsEnabled(toggle);
		}

		public IocArgs Args()
		{
			return _args;
		}

		public IocCache Cache()
		{
			return _cache;
		}
	}
}