using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Requests.Core.Provider
{
	public interface IRequestsProvider
	{
		IEnumerable<IPersonRequest> RetrieveRequests(DateOnlyPeriod period);
		IEnumerable<IPersonRequest> RetrieveRequests(DateOnlyPeriod period, IDictionary<PersonFinderField, string> agentSearchTerms);
	}
}