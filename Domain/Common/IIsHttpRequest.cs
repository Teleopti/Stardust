using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
	public interface IIsHttpRequest
	{
		bool IsHttpRequest();
		IBusinessUnit BusinessUnitForRequest();
	}
}