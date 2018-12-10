using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Assemblers;


namespace Teleopti.Ccc.Sdk.WcfHost.Service.Factory
{
    public class WriteProtectionFactory
    {
        private readonly IAssembler<IPerson, PersonDto> _personAssembler;
        private readonly IPersonRepository _personRepository;

        public WriteProtectionFactory(IAssembler<IPerson, PersonDto> personAssembler, IPersonRepository personRepository)
        {
            _personAssembler = personAssembler;
            _personRepository = personRepository;
        }

        public void SetWriteProtectionDate(PersonWriteProtectionDto personWriteProtectionDto)
        {
            using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                IPerson person = _personRepository.Get(personWriteProtectionDto.PersonId);
                person.PersonWriteProtection.PersonWriteProtectedDate = new DateOnly(personWriteProtectionDto.WriteProtectedToDate.DateTime);
                uow.PersistAll();
            }
        }

        public PersonWriteProtectionDto GetWriteProtectionDate(PersonDto personDto)
        {
            using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                var person = _personAssembler.DtoToDomainEntity(personDto);
                var dto = new PersonWriteProtectionDto();
                if (!person.Id.HasValue)
                    return dto;

                dto.PersonId = person.Id.Value;
                if (person.PersonWriteProtection.PersonWriteProtectedDate.HasValue)
					dto.WriteProtectedToDate = new DateOnlyDto { DateTime = person.PersonWriteProtection.PersonWriteProtectedDate.Value.Date };

                return dto;
            }
        }
    }
}