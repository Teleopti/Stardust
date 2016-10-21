using System.Collections.Generic;
using Teleopti.Ccc.Infrastructure.Repositories;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeSmartPersonPropertyQuerier : ISmartPersonPropertyQuerier
	{
		private readonly IList<SmartPersonPropertySuggestion> _suggestions = new List<SmartPersonPropertySuggestion>();

		public FakeSmartPersonPropertyQuerier WithSuggestion(SmartPersonPropertySuggestion suggestion)
		{
			_suggestions.Add(suggestion);
			return this;
		}

		public IEnumerable<SmartPersonPropertySuggestion> GetWorkflowControlSetSuggestions()
		{
			return _suggestions;
		}
	}
}