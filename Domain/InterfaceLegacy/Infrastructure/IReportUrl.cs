using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure
{
	public interface IReportUrl
	{
		string Build(IApplicationFunction applicationFunction);
	}
}