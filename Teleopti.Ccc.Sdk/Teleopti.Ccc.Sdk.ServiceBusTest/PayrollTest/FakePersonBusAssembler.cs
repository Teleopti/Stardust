using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.MultiTenancy;
using Teleopti.Ccc.Sdk.ServiceBus.Payroll;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.PayrollTest
{
	public class FakePersonBusAssembler : IPersonBusAssembler
	{
		public IEnumerable<PersonDto> CreatePersonDto(IEnumerable<IPerson> persons, ITenantPeopleLoader tenantPeopleLoader)
		{
			return new List<PersonDto>();
		}
	}
}