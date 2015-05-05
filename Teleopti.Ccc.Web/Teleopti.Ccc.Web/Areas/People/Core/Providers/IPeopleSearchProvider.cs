﻿using System;
using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.People.Core.ViewModels;
using Teleopti.Interfaces.Domain;
namespace Teleopti.Ccc.Web.Areas.People.Core.Providers
{
	public interface IPeopleSearchProvider
	{
		PeopleSummaryModel SearchPeople(IDictionary<PersonFinderField, string> criteriaDictionary, int pageSize, int currentPageIndex, DateOnly currentDate);
	}
}
