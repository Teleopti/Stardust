using System;
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

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

		public IDisposable OnThisThreadUse(IBusinessUnit businessUnit)
		{
		    var current = _businessUnit;
			_businessUnit = businessUnit;
			return new GenericDisposable(()=> { _businessUnit = current; });
		}
	}
}