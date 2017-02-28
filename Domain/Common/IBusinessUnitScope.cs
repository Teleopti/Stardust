using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Common
{
	public interface IBusinessUnitScope
	{
		void OnThisThreadUse(IBusinessUnit businessUnit);
	}
}