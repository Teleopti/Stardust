using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Sdk.Logic.Assemblers;

namespace Teleopti.Ccc.Sdk.Logic.CommandHandler
{
    public class EmployPersonCommandHandler : IHandleCommand<EmployPersonCommandDto>
    {
        private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IPersonRepository _personRepository;
        private readonly IPersonAssembler _personAssembler;
        private readonly IPartTimePercentageRepository _partTimePercentageRepository;
        private readonly IContractScheduleRepository _contractScheduleRepository;
        private readonly IContractRepository _contractRepository;
        private readonly ITeamRepository _teamRepository;
		private readonly ICurrentAuthorization _currentAuthorization;
		private readonly ICurrentBusinessUnit _currentBusinessUnit;

		public EmployPersonCommandHandler(ICurrentUnitOfWorkFactory unitOfWorkFactory, IPersonRepository personRepository,
            IPersonAssembler personAssembler, IPartTimePercentageRepository partTimePercentageRepository,
            IContractScheduleRepository contractScheduleRepository, IContractRepository contractRepository,
            ITeamRepository teamRepository, ICurrentAuthorization currentAuthorization, ICurrentBusinessUnit currentBusinessUnit)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
            _personRepository = personRepository;
            _personAssembler = personAssembler;
            _partTimePercentageRepository = partTimePercentageRepository;
            _contractScheduleRepository = contractScheduleRepository;
            _contractRepository = contractRepository;
            _teamRepository = teamRepository;
			_currentAuthorization = currentAuthorization;
			_currentBusinessUnit = currentBusinessUnit;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void Handle(EmployPersonCommandDto command)
        {
			ValidationExtensions.VerifyCanModifyPeople(_currentAuthorization);

            PersonPeriod personPeriod;
            using (var uow = _unitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
            {
                var team = _teamRepository.Load(command.Team.Id.GetValueOrDefault());
                team.Site.ValidateBusinessUnitConsistency(_currentBusinessUnit);

                var start = command.Period.StartDate.DateTime;

                _personAssembler.EnableSaveOrUpdate = true;
                var person = _personAssembler.DtoToDomainEntity(command.Person);

                var partTimePercentage =
                   _partTimePercentageRepository.Load(command.PersonContract.PartTimePercentageId.GetValueOrDefault());
                var contractSchedule =
                    _contractScheduleRepository.Load(command.PersonContract.ContractScheduleId.GetValueOrDefault());
                var contract = _contractRepository.Load(command.PersonContract.ContractId.GetValueOrDefault());

                var personContract = new PersonContract(contract, partTimePercentage, contractSchedule);
                personPeriod = new PersonPeriod(new DateOnly(start), personContract, team);
                person.AddPersonPeriod(personPeriod);
                _personRepository.Add(person);
                uow.PersistAll();
            }
			command.Result = new CommandResultDto { AffectedId = personPeriod.Id, AffectedItems = 2 };
        }
    }
}
