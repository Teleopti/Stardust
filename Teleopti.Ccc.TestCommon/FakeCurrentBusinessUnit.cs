﻿using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeCurrentBusinessUnit : ICurrentBusinessUnit, IBusinessUnitScope
	{
		private IBusinessUnit _businessUnit;

		public void FakeBusinessUnit(IBusinessUnit businessUnit)
		{
			_businessUnit = businessUnit;
		}

		public IBusinessUnit Current()
		{
			return _businessUnit;
		}

		public void OnThisThreadUse(IBusinessUnit businessUnit)
		{
			_businessUnit = businessUnit;
		}
	}

}