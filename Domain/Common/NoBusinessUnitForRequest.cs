using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
	public class NoBusinessUnitForRequest : IBusinessUnitForRequest
	{
		public IBusinessUnit TryGetBusinessUnit()
		{
			return null;
		}
	}
}