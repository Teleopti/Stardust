using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Core.RequestContext
{
	public interface ICurrentBusinessUnitProvider
	{
		IBusinessUnit CurrentBusinessUnit();
	}
}