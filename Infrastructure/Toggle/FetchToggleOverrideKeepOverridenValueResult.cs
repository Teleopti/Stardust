using System.Collections.Generic;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.Infrastructure.Toggle
{
	//not thread safe!
	public class FetchToggleOverrideKeepOverridenValueResult : FetchToggleOverride
	{
		private readonly IDictionary<Toggles, bool?> _state;
		
		public FetchToggleOverrideKeepOverridenValueResult(IConfigReader configReader) : base(configReader)
		{
			_state = new Dictionary<Toggles, bool?>();
		}

		
		public override bool? OverridenValue(Toggles toggle)
		{
			if (_state.TryGetValue(toggle, out var result))
			{
				return result;
			}
			var newResult = base.OverridenValue(toggle); 
			_state[toggle] = newResult;
			return newResult;
		}
	}
}