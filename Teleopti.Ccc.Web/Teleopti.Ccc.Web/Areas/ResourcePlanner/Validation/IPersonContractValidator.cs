﻿using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.ResourcePlanner.Validation
{
	public interface IPersonContractValidator
	{
		IEnumerable<PersonValidationError> GetPeopleMissingContract(ICollection<IPerson> people, DateOnlyPeriod range);
	}
}