using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

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
                              ScheduleNote = entity.GetScheduleNote(new NormalizeText()),
							  Date = new DateOnlyDto { DateTime = entity.NoteDate.Date }
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