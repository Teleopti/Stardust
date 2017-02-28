﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Permissions.Controllers
{
	public class AvailableDataForRoleInput
	{
		public AvailableDataForRoleInput()
		{
			BusinessUnits = new Collection<Guid>();
			Sites = new Collection<Guid>();
			Teams = new Collection<Guid>();
		}

		public ICollection<Guid> BusinessUnits { get; set; }
		public ICollection<Guid> Sites { get; set; }
		public ICollection<Guid> Teams { get; set; }
		public AvailableDataRangeOption? RangeOption { get; set; }
	}
}