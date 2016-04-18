using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
	public interface IBusinessUnitScope
	{
		void OnThisThreadUse(IBusinessUnit businessUnit);
	}
}