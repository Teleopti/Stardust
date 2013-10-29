using System.Linq;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public interface IPersonToSingleAgentTeamRootPersonGroupConverter
	{
		IRootPersonGroup Convert(IPerson person, DateOnly dateOnly);
	}
	public class PersonToSingleAgentTeamRootPersonGroupConverter : IPersonToSingleAgentTeamRootPersonGroupConverter
	{
		private readonly IGroupPageCreator<IPerson> _groupPageCreator;

		public PersonToSingleAgentTeamRootPersonGroupConverter(IGroupPageCreator<IPerson> groupPageCreator)
		{
			_groupPageCreator = groupPageCreator;
		}

		public IRootPersonGroup Convert(IPerson person, DateOnly dateOnly)
		{
			var personList = new[] { person };
			IGroupPageOptions options = new GroupPageOptions(personList)
				{
					SelectedPeriod = new DateOnlyPeriod(dateOnly, dateOnly),
					CurrentGroupPageName = "SingleAgentTeam",
					CurrentGroupPageNameKey = "SingleAgentTeam"
				};
			var groupPage = _groupPageCreator.CreateGroupPage(personList, options);
			return groupPage.RootGroupCollection.First();
		}
	}
}