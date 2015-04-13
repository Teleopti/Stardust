using System;
using Teleopti.Ccc.Web.Areas.People.Core.ViewModels;
namespace Teleopti.Ccc.Web.Areas.People.Core.Providers
{
	public interface IPeopleSearchProvider
	{
		PeopleSummaryModel SearchPeople(string keyword, int pageSize, int currentPageIndex, Teleopti.Interfaces.Domain.DateOnly currentDate);
	}
}
