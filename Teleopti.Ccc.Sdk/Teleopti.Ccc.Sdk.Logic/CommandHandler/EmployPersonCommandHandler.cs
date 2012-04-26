using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.Logic.CommandHandler
{
    public class EmployPersonCommandHandler : IHandleCommand<EmployPersonCommandDto>
    {
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IPersonRepository _personRepository;
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly IAssembler<IPerson, PersonDto> _personAssembler;

        public EmployPersonCommandHandler(IUnitOfWorkFactory unitOfWorkFactory, IPersonRepository personRepository, IRepositoryFactory repositoryFactory, IAssembler<IPerson, PersonDto> personAssembler)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
            _personRepository = personRepository;
            _repositoryFactory = repositoryFactory;
            _personAssembler = personAssembler;
        }

        public CommandResultDto Handle(EmployPersonCommandDto command)
        {
            PersonPeriod personPeriod;
            using (var uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
            {
                var start = command.Period.StartDate.DateTime;

                ((PersonAssembler)_personAssembler).EnableSaveOrUpdate = true;
                var person = _personAssembler.DtoToDomainEntity(command.Person);

                var partTimePercentage =
                   _repositoryFactory.CreatePartTimePercentageRepository(uow).Load(command.PersonContract.PartTimePercentageId.GetValueOrDefault());
                var contractSchedule =
                    _repositoryFactory.CreateContractScheduleRepository(uow).Load(command.PersonContract.ContractScheduleId.GetValueOrDefault());
                var team = _repositoryFactory.CreateTeamRepository(uow).Load(command.Team.Id.GetValueOrDefault());
                var contract = _repositoryFactory.CreateContractRepository(uow).Load(command.PersonContract.ContractId.GetValueOrDefault());

                var personContract = new PersonContract(contract, partTimePercentage, contractSchedule);
                personPeriod = new PersonPeriod(new DateOnly(start.Year, start.Month, start.Day), personContract, team);
                person.AddPersonPeriod(personPeriod);
                _personRepository.Add(person);
                uow.PersistAll();
            }
            return new CommandResultDto {AffectedId = personPeriod.Id, AffectedItems = 2};
        }
    }
}
