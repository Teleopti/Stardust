using System;
using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.Domain.ToggleAdmin
{
	public class DeleteToggleOverride
	{
		private readonly IPersistToggleOverride _persistToggleOverride;

		public DeleteToggleOverride(IPersistToggleOverride persistToggleOverride)
		{
			_persistToggleOverride = persistToggleOverride;
		}
		public void Execute(string toggle)
		{
			_persistToggleOverride.Delete((Toggles)Enum.Parse(typeof(Toggles),toggle));
		}
	}
}