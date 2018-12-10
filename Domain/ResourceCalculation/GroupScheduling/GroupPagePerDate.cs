using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling
{
	public class GroupPagePerDate : IGroupPagePerDate
	{
		private readonly IDictionary<DateOnly, IGroupPage> _groupPages;

		public GroupPagePerDate(IDictionary<DateOnly, IGroupPage> groupPages)
		{
			_groupPages = groupPages;
		}

		public IGroupPage GetGroupPageByDate(DateOnly dateOnly)
		{
			return _groupPages[dateOnly];
		}
	}
}