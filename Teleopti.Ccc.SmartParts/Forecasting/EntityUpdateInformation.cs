﻿using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.SmartParts.Forecasting
{
	public class EntityUpdateInformation
	{
		public Name? Name { get; set; }
		public DateTime? LastUpdate { get; set; }
		public string Tag { get; set; }
		public override bool Equals(object obj)
		{
			var rhs = obj as EntityUpdateInformation;
			if (Name.Equals(rhs.Name) && LastUpdate.Equals(rhs.LastUpdate))
				return true;
			return false;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}
}
