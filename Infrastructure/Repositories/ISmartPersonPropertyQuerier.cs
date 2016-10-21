using System.Collections.Generic;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public interface ISmartPersonPropertyQuerier
	{
		IEnumerable<SmartPersonPropertySuggestion> GetWorkflowControlSetSuggestions();
	}
}