using System;
using Teleopti.Ccc.Web.Areas.People.Core.ViewModels;
using Teleopti.Interfaces.Domain;
namespace Teleopti.Ccc.Web.Areas.People.Core.Providers
{
	public interface IPeopleSearchProvider
	{
		PeopleSummaryModel SearchPeople(string keyword, int pageSize, int currentPageIndex, DateOnly currentDate);
	}
}
