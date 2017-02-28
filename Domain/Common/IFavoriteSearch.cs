using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
	public interface IFavoriteSearch : IAggregateRoot, IChangeInfo, IBelongsToBusinessUnit, ICloneableEntity<IFavoriteSearch>
	{
		string Name { get; set; }
		string TeamIds { get; set; }
		string SearchTerm { get; set; }
		FavoriteSearchStatus Status { get; set; }
		WfmArea WfmArea { get; set; }
		IPerson Creator { get; set; }
	}
}