using System.ServiceModel;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

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

        public EmployPersonCommandHandler(ICurrentUnitOfWorkFactory unitOfWorkFactory, IPersonRepository personRepository, IPersonAssembler personAssembler, IPartTimePercentageRepository partTimePercentageRepository, IContractScheduleRepository contractScheduleRepository, IContractRepository contractRepository, ITeamRepository teamRepository)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
            _personRepository = personRepository;
            _personAssembler = personAssembler;
            _partTimePercentageRepository = partTimePercentageRepository;
            _contractScheduleRepository = contractScheduleRepository;
            _contractRepository = contractRepository;
            _teamRepository = teamRepository;
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void Handle(EmployPersonCommandDto command)
        {
			checkIfAuthorized();

            PersonPeriod personPeriod;
            using (var uow = _unitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
            {
                var start = command.Period.StartDate.DateTime;

                _personAssembler.EnableSaveOrUpdate = true;
                var person = _personAssembler.DtoToDomainEntity(command.Person);

                var partTimePercentage =
                   _partTimePercentageRepository.Load(command.PersonContract.PartTimePercentageId.GetValueOrDefault());
                var contractSchedule =
                    _contractScheduleRepository.Load(command.PersonContract.ContractScheduleId.GetValueOrDefault());
                var team = _teamRepository.Load(command.Team.Id.GetValueOrDefault());
                var contract = _contractRepository.Load(command.PersonContract.ContractId.GetValueOrDefault());

                var personContract = new PersonContract(contract, partTimePercentage, contractSchedule);
                personPeriod = new PersonPeriod(new DateOnly(start), personContract, team);
                person.AddPersonPeriod(personPeriod);
                _personRepository.Add(person);
                uow.PersistAll();
            }
			command.Result = new CommandResultDto { AffectedId = personPeriod.Id, AffectedItems = 2 };
        }

		private static void checkIfAuthorized()
		{
			if (!PrincipalAuthorization.Instance().IsPermitted(DefinedRaptorApplicationFunctionPaths.OpenPersonAdminPage))
			{
				throw new FaultException("You're not allowed to modify person details.");
			}
		}
    }
}
