using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakeGroupPageCreator : IGroupPageCreator
	{
		public IGroupPagePerDate CreateGroupPagePerDate(ISelectedPeriod currentView, IGroupPageDataProvider groupPageDataProvider,
			GroupPageLight selectedGrouping)
		{
			throw new NotImplementedException();
		}

		public IGroupPagePerDate CreateGroupPagePerDate(IList<DateOnly> dates, IGroupPageDataProvider groupPageDataProvider,
			GroupPageLight selectedGrouping)
		{
			throw new NotImplementedException();
		}

		public IGroupPagePerDate CreateGroupPagePerDate(IList<DateOnly> dates, IGroupPageDataProvider groupPageDataProvider,
			GroupPageLight selectedGrouping, bool useAllLoadedPersons)
		{
			throw new NotImplementedException();
		}
	}
}