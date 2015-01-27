using Teleopti.Ccc.Domain.Security.AuthorizationEntities;

namespace Teleopti.Ccc.Infrastructure.Toggle
{
	public interface IApplicationFunctionsToggleFilter
	{
		AllFunctions FilteredFunctions();
	}
}