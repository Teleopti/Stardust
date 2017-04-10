using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Specification;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.Sdk.WcfHost.Service.Factory
{
    public class PersonRequestedShiftTrade : Specification<IPerson>
    {
        private readonly PersonRequestDto _personRequestDto;

        public PersonRequestedShiftTrade(PersonRequestDto personRequestDto)
        {
            _personRequestDto = personRequestDto;
        }

        public override bool IsSatisfiedBy(IPerson obj)
        {
            return _personRequestDto.Person.Id.Value == obj.Id.Value;
        }
    }
}