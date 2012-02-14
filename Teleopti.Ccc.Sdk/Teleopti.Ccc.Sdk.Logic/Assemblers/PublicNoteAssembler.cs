using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.Logic.Assemblers
{
    public class PublicNoteAssembler : Assembler<IPublicNote, PublicNoteDto>
    {
        private readonly IPublicNoteRepository _repository;
        private readonly IAssembler<IPerson, PersonDto> _personAssembler;

        public PublicNoteAssembler(IPublicNoteRepository repository, IAssembler<IPerson, PersonDto> personAssembler)
        {
            _repository = repository;
            _personAssembler = personAssembler;
        }

        public override PublicNoteDto DomainEntityToDto(IPublicNote entity)
        {
            var dto = new PublicNoteDto
                          {
                              Id = entity.Id,
                              Person = _personAssembler.DomainEntityToDto(entity.Person),
                              ScheduleNote = entity.ScheduleNote,
                              Date = new DateOnlyDto(entity.NoteDate)
                          };

            return dto;
        }

        public override IPublicNote DtoToDomainEntity(PublicNoteDto dto)
        {
            IPublicNote publicNote = null;
            if (dto.Id.HasValue)
                publicNote = _repository.Get(dto.Id.Value);

            return publicNote;
        }
    }
}