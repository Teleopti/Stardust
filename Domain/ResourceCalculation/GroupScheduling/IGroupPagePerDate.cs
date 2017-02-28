﻿using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling
{
	public interface IGroupPagePerDate
	{
		IGroupPage GetGroupPageByDate(DateOnly dateOnly);
	}
}