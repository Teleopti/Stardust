﻿using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.ResourcePlanner.Validation
{
	public interface IPersonShiftBagValidator
	{
		IEnumerable<PersonValidationError> GetPeopleMissingShiftBag(ICollection<IPerson> people, DateOnlyPeriod range);
	}
}