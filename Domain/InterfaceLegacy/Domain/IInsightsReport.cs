using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface IInsightsReport : IAggregateRoot, IDeleteTag, IVersioned, ICreateInfo, IChangeInfo
	{
		string Name { get; set; }
		bool IsBuildIn { get; set; }
	}
}