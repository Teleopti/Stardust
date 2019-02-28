using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Assemblers;


namespace Teleopti.Ccc.Sdk.WcfHost.Service.Factory
{
    public class PublicNoteTypeFactory
    {
        private readonly IAssembler<IPublicNote, PublicNoteDto> _publicNoteAssembler;
        private readonly IAssembler<IPerson, PersonDto> _personAssembler;
        private readonly IPublicNoteRepository _publicNoteRepository;
        private readonly ICurrentScenario _scenarioRepository;
        private readonly PersonsFromLoadOptionFactory _personsFromLoadOptionFactory;

		  public PublicNoteTypeFactory(IAssembler<IPublicNote, PublicNoteDto> publicNoteAssembler, IAssembler<IPerson, PersonDto> personAssembler, IPublicNoteRepository publicNoteRepository, ICurrentScenario scenarioRepository, PersonsFromLoadOptionFactory personsFromLoadOptionFactory)
        {
            _publicNoteAssembler = publicNoteAssembler;
            _personAssembler = personAssembler;
            _publicNoteRepository = publicNoteRepository;
            _scenarioRepository = scenarioRepository;
            _personsFromLoadOptionFactory = personsFromLoadOptionFactory;
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "3")]
		public ICollection<PublicNoteDto> GetPublicNotes(PublicNoteLoadOptionDto publicNoteLoadOptionDto, ICollection<TeamDto> teamDtoCollection, DateOnlyDto startDate, DateOnlyDto endDate)
        {
            ICollection<PublicNoteDto> publicNoteDtosToReturn;
            ICollection<PersonDto> personDtoCollection = _personsFromLoadOptionFactory.GetPersonFromLoadOption(publicNoteLoadOptionDto, teamDtoCollection,
                                                                     startDate, endDate);

            using (IUnitOfWork unitOfWork = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                var period = new DateOnlyPeriod(new DateOnly(startDate.DateTime), new DateOnly(endDate.DateTime));
                var publicNoteRepository = PublicNoteRepository.DONT_USE_CTOR(unitOfWork);
                ICollection<IPerson> personDomainCollection = _personAssembler.DtosToDomainEntities(personDtoCollection).ToList();
                
                ICollection<IPublicNote> publicNotes = publicNoteRepository.Find(period, personDomainCollection, _scenarioRepository.Current());
                publicNoteDtosToReturn = _publicNoteAssembler.DomainEntitiesToDtos(publicNotes).ToList();
            }

			if (publicNoteDtosToReturn.Count > 0)
				return publicNoteDtosToReturn.GroupBy(x => new {x.Date.DateTime, x.Person.Id}).Select(x => x.Last()).ToList();
			
            return publicNoteDtosToReturn;
        }

        public void SavePublicNote(PublicNoteDto publicNoteDto)
        {
            if (publicNoteDto == null)
                throw new FaultException("Parameter publicNoteDto cannot be null.");

            DeletePublicNote(publicNoteDto);

            if (publicNoteDto.ScheduleNote == null)
                publicNoteDto.ScheduleNote = string.Empty;


                using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
                {
                    IPublicNote publicNote = new PublicNote(_personAssembler.DtoToDomainEntity(publicNoteDto.Person),
                                                            new DateOnly(publicNoteDto.Date.DateTime),
                                                            _scenarioRepository.Current(),
                                                            publicNoteDto.ScheduleNote);
                    _publicNoteRepository.Add(publicNote);
                    uow.PersistAll();
                }
        }

        public void DeletePublicNote(PublicNoteDto publicNoteDto)
        {

                using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
                {
                    IPublicNote publicNote = _publicNoteRepository.Find(new DateOnly(publicNoteDto.Date.DateTime),
                                                             _personAssembler.DtoToDomainEntity(publicNoteDto.Person),
                                                             _scenarioRepository.Current());
                    if (publicNote == null)
                        return;

                    _publicNoteRepository.Remove(publicNote);
                    uow.PersistAll();
                }
        }
    }
}