using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.WcfService.Factory
{
    public class PublicNoteTypeFactory
    {
        private readonly IAssembler<IPublicNote, PublicNoteDto> _publicNoteAssembler;
        private readonly IAssembler<IPerson, PersonDto> _personAssembler;
        private readonly IPublicNoteRepository _publicNoteRepository;
        private readonly IScenarioProvider _scenarioProvider;
        private readonly PersonsFromLoadOptionFactory _personsFromLoadOptionFactory;

        public PublicNoteTypeFactory(IAssembler<IPublicNote,PublicNoteDto> publicNoteAssembler, IAssembler<IPerson,PersonDto> personAssembler, IPublicNoteRepository publicNoteRepository, IScenarioProvider scenarioProvider, PersonsFromLoadOptionFactory personsFromLoadOptionFactory)
        {
            _publicNoteAssembler = publicNoteAssembler;
            _personAssembler = personAssembler;
            _publicNoteRepository = publicNoteRepository;
            _scenarioProvider = scenarioProvider;
            _personsFromLoadOptionFactory = personsFromLoadOptionFactory;
        }

        public ICollection<PublicNoteDto> GetPublicNotes(PublicNoteLoadOptionDto publicNoteLoadOptionDto, ICollection<TeamDto> teamDtoCollection, DateOnlyDto startDate, DateOnlyDto endDate, string timeZoneId)
        {
            ICollection<PublicNoteDto> publicNoteDtosToReturn;
            ICollection<PersonDto> personDtoCollection = _personsFromLoadOptionFactory.GetPersonFromLoadOption(publicNoteLoadOptionDto, teamDtoCollection,
                                                                     startDate, endDate);

            using (IUnitOfWork unitOfWork = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                var period = new DateOnlyPeriod(new DateOnly(startDate.DateTime), new DateOnly(endDate.DateTime));
                var publicNoteRepository = new PublicNoteRepository(unitOfWork);
                ICollection<IPerson> personDomainCollection = _personAssembler.DtosToDomainEntities(personDtoCollection).ToList();
                
                ICollection<IPublicNote> publicNotes = publicNoteRepository.Find(period, personDomainCollection, _scenarioProvider.DefaultScenario());
                publicNoteDtosToReturn = _publicNoteAssembler.DomainEntitiesToDtos(publicNotes).ToList();
            }

            return publicNoteDtosToReturn;
        }

        public void SavePublicNote(PublicNoteDto publicNoteDto)
        {
            if (publicNoteDto == null)
                throw new FaultException("Parameter publicNoteDto cannot be null.");

            DeletePublicNote(publicNoteDto);

            if (publicNoteDto.ScheduleNote == null)
                publicNoteDto.ScheduleNote = string.Empty;

            using (new MessageBrokerSendEnabler())
            {
                using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
                {
                    IPublicNote publicNote = new PublicNote(_personAssembler.DtoToDomainEntity(publicNoteDto.Person),
                                                            new DateOnly(publicNoteDto.Date.DateTime),
                                                            _scenarioProvider.DefaultScenario(),
                                                            publicNoteDto.ScheduleNote);
                    _publicNoteRepository.Add(publicNote);
                    uow.PersistAll();
                }
            }
        }

        public void DeletePublicNote(PublicNoteDto publicNoteDto)
        {
            using (new MessageBrokerSendEnabler())
            {
                using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
                {
                    IPublicNote publicNote = _publicNoteRepository.Find(new DateOnly(publicNoteDto.Date.DateTime),
                                                             _personAssembler.DtoToDomainEntity(publicNoteDto.Person),
                                                             _scenarioProvider.DefaultScenario());
                    if (publicNote == null)
                        return;

                    _publicNoteRepository.Remove(publicNote);
                    uow.PersistAll();
                }
            }
        }
    }
}