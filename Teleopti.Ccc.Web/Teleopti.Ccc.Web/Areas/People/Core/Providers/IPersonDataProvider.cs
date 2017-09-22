using System;
using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.People.Core.Models;

namespace Teleopti.Ccc.Web.Areas.People.Core.Providers
{
	public interface IPersonDataProvider
	{
		IEnumerable<PersonDataModel> RetrievePeople(DateTime date, IEnumerable<Guid> personIdList);
	}
}