using System.Collections.Generic;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBus.Payroll
{
    public class PersonBusAssembler : IPersonBusAssembler
    {
        public IEnumerable<PersonDto> CreatePersonDto(IEnumerable<IPerson> persons)
        {
            var personAssembler = new PersonAssembler(null, new WorkflowControlSetAssembler(new ShiftCategoryAssembler(null), new DayOffAssembler(null),new ActivityAssembler(null), new AbsenceAssembler(null)), null);
            return personAssembler.DomainEntitiesToDtos(persons);
        }
    }
}