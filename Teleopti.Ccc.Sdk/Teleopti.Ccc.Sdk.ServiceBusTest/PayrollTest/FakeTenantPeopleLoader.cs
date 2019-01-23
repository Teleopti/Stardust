using System;
using System.Collections.Generic;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.MultiTenancy;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.PayrollTest
{
	public class FakeTenantPeopleLoader : ITenantPeopleLoader
	{
		public void FillDtosWithLogonInfo(IList<PersonDto> personDtos)
		{
			throw new NotImplementedException();
		}
	}
}