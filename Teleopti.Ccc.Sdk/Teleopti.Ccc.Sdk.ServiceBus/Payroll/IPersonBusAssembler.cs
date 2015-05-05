using System.Collections.Generic;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.MultiTenancy;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBus.Payroll
{
    public interface IPersonBusAssembler
    {
		 IEnumerable<PersonDto> CreatePersonDto(IEnumerable<IPerson> persons, ITenantPeopleLoader tenantPeopleLoader);
    }
}