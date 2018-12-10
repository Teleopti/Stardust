using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.Sdk.Logic.QueryHandler;

namespace Teleopti.Ccc.Sdk.Logic.CommandHandler
{
    public class ChangePersonEmploymentCommandHandler : IHandleCommand<ChangePersonEmploymentCommandDto>
    {
        private readonly IAssembler<IPersonPeriod, PersonSkillPeriodDto> _personSkillPeriodAssembler;
        private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;
        private readonly ISkillRepository _skillRepository;
        private readonly IExternalLogOnRepository _externalLogOnRepository;
        private readonly IPersonRepository _personRepository;
        private readonly ITeamRepository _teamRepository;
        private readonly IPartTimePercentageRepository _partTimePercentageRepository;
        private readonly IContractScheduleRepository _contractScheduleRepository;
        private readonly IContractRepository _contractRepository;

        public ChangePersonEmploymentCommandHandler(IAssembler<IPersonPeriod, PersonSkillPeriodDto> personSkillPeriodAssembler, ICurrentUnitOfWorkFactory unitOfWorkFactory, ISkillRepository skillRepository, IExternalLogOnRepository externalLogOnRepository, IPersonRepository personRepository, ITeamRepository teamRepository, IPartTimePercentageRepository partTimePercentageRepository, IContractScheduleRepository contractScheduleRepository, IContractRepository contractRepository)
        {
            _personSkillPeriodAssembler = personSkillPeriodAssembler;
            _unitOfWorkFactory = unitOfWorkFactory;
            _skillRepository = skillRepository;
            _externalLogOnRepository = externalLogOnRepository;
            _personRepository = personRepository;
            _teamRepository = teamRepository;
            _partTimePercentageRepository = partTimePercentageRepository;
            _contractScheduleRepository = contractScheduleRepository;
            _contractRepository = contractRepository;
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void Handle(ChangePersonEmploymentCommandDto command)
        {
            Guid? affectedId;
            using (var uow = _unitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
            {
            	var startDate = new DateOnly(command.Period.StartDate.DateTime);
                var person = _personRepository.Get(command.Person.Id.GetValueOrDefault());
				person.VerifyCanBeModifiedByCurrentUser();

	            if (startDate > person.TerminalDate)
					throw new FaultException("You cannot change person employment after his leaving date.");

                var existPersonPeriod =
					person.PersonPeriodCollection.FirstOrDefault(pp => pp.StartDate == startDate);

                if (existPersonPeriod != null)
                {
                    affectedId = updateExistingPersonPeriod(command, person, existPersonPeriod, uow);
                }
                else
                {
                    affectedId = createNewPersonPeriod(command, person, uow);
                }
            }			
            command.Result = new CommandResultDto { AffectedId = affectedId, AffectedItems = 1 };
        }

        private Guid? createNewPersonPeriod(ChangePersonEmploymentCommandDto command, IPerson person, IUnitOfWork uow)
        {
            var lastPersonPeriod = person.PersonPeriodCollection.OrderBy(pp => pp.Period.StartDate).LastOrDefault();
            IPersonPeriod newPersonPeriod;
            if (lastPersonPeriod != null)
            {
                var team = command.Team == null
                               ? lastPersonPeriod.Team
                               : _teamRepository.Load(command.Team.Id.GetValueOrDefault());
				team.Site.ValidateBusinessUnitConsistency();
                newPersonPeriod = new PersonPeriod(command.Period.StartDate.ToDateOnly(), 
                                                   command.PersonContract == null
                                                       ? lastPersonPeriod.PersonContract
                                                       : createPersonContract(command.PersonContract),team);
                person.AddPersonPeriod(newPersonPeriod);
                var externalLogOnDtos = new List<ExternalLogOnDto>();
                lastPersonPeriod.ExternalLogOnCollection.ForEach(
                    e => externalLogOnDtos.Add(new ExternalLogOnDto
                        {
                            AcdLogOnOriginalId = e.AcdLogOnOriginalId,
                            AcdLogOnName = e.AcdLogOnName
                        }));

                var personSkillPeriodDtos = _personSkillPeriodAssembler.DomainEntityToDto(lastPersonPeriod);
                addDefaultPersonSkillsWhenNoDefined(command, personSkillPeriodDtos);

                resetExternalLogOns(command.ExternalLogOn ?? externalLogOnDtos, newPersonPeriod, person);
                resetPersonSkills(command, person, newPersonPeriod);

                newPersonPeriod.Note = string.IsNullOrEmpty(command.Note) ? lastPersonPeriod.Note : command.Note;
	            newPersonPeriod.BudgetGroup = lastPersonPeriod.BudgetGroup;
	            newPersonPeriod.RuleSetBag = lastPersonPeriod.RuleSetBag;
            }
            else
            {
                if (command.PersonContract == null || command.Team == null)
                    throw new FaultException(
                        "There is no person period existed before, you have to specify both person contract and team.");
                newPersonPeriod = createPersonPeriod(command);
                person.AddPersonPeriod(newPersonPeriod);
                if (command.ExternalLogOn != null) resetExternalLogOns(command.ExternalLogOn, newPersonPeriod, person);
#pragma warning disable 618
                if (command.PersonSkillPeriodCollection != null || command.PersonSkillCollection != null)
#pragma warning restore 618
                    resetPersonSkills(command, person, newPersonPeriod);
                if (!string.IsNullOrEmpty(command.Note)) newPersonPeriod.Note = command.Note;
            }
            
            uow.PersistAll();
            return newPersonPeriod.Id;
        }

        private void addDefaultPersonSkillsWhenNoDefined(ChangePersonEmploymentCommandDto command, PersonSkillPeriodDto personSkillPeriodDto)
        {
            if (command.PersonSkillCollection.IsNullOrEmpty() &&
#pragma warning disable 618
                (command.PersonSkillPeriodCollection.IsNullOrEmpty() ||
                 command.PersonSkillPeriodCollection.All(s => s.SkillCollection.IsNullOrEmpty())))
#pragma warning restore 618
            {
                command.PersonSkillCollection = personSkillPeriodDto.PersonSkillCollection;
            }
        }

        private Guid? updateExistingPersonPeriod(ChangePersonEmploymentCommandDto command, IPerson person, IPersonPeriod existPersonPeriod,
                                                 IUnitOfWork uow)
        {
            if (command.Team != null)
            {
				person.ChangeTeam(_teamRepository.Get(command.Team.Id.GetValueOrDefault()), existPersonPeriod);
				existPersonPeriod.Team.Site.ValidateBusinessUnitConsistency();
            }
            if (command.PersonContract != null)
            {
                existPersonPeriod.PersonContract = createPersonContract(command.PersonContract);
            }
            if (command.ExternalLogOn != null)
            {
                resetExternalLogOns(command.ExternalLogOn, existPersonPeriod, person);
            }
#pragma warning disable 618
            if (command.PersonSkillPeriodCollection != null || command.PersonSkillCollection != null)
#pragma warning restore 618
            {
                resetPersonSkills(command, person, existPersonPeriod);
            }
            if (!string.IsNullOrEmpty(command.Note))
                existPersonPeriod.Note = command.Note;

            uow.PersistAll();
            return existPersonPeriod.Id;
        }

        
        private IPersonPeriod createPersonPeriod(ChangePersonEmploymentCommandDto command)
        {
            var team = _teamRepository.Load(command.Team.Id.GetValueOrDefault());
			team.Site.ValidateBusinessUnitConsistency();
            var personContract = createPersonContract(command.PersonContract);
            return new PersonPeriod(command.Period.StartDate.ToDateOnly(), personContract, team);
        }

        private PersonContract createPersonContract(PersonContractDto personContractDto)
        {
            var partTimePercentage = _partTimePercentageRepository.Load(personContractDto.PartTimePercentageId.GetValueOrDefault());
            var contractSchedule = _contractScheduleRepository.Load(personContractDto.ContractScheduleId.GetValueOrDefault());
            var contract = _contractRepository.Load(personContractDto.ContractId.GetValueOrDefault());

			partTimePercentage.ValidateBusinessUnitConsistency();
			contractSchedule.ValidateBusinessUnitConsistency();
			contract.ValidateBusinessUnitConsistency();

            return new PersonContract(contract, partTimePercentage, contractSchedule);
        }

        private void resetPersonSkills(ChangePersonEmploymentCommandDto commandDto, IPerson person, IPersonPeriod personPeriod)
        {
			person.ResetPersonSkills(personPeriod);
            foreach (var personSkills in PersonSkills(commandDto))
            {
                var skill = _skillRepository.Load(personSkills.SkillId);
				skill.ValidateBusinessUnitConsistency();
                person.AddSkill(new PersonSkill(skill, new Percent(personSkills.Proficiency)) {Active = personSkills.Active}, personPeriod);
            }
        }

        private static IEnumerable<PersonSkillDto> PersonSkills(ChangePersonEmploymentCommandDto commandDto)
        {
#pragma warning disable 618
            if (commandDto.PersonSkillPeriodCollection!=null && commandDto.PersonSkillPeriodCollection.Any(s => s.PersonSkillCollection.Any()))
#pragma warning restore 618
            {
                throw new FaultException("This collection is not allowed. Use the PersonSkillCollection directly on the command instead.");
            }
            if (commandDto.PersonSkillCollection!=null && commandDto.PersonSkillCollection.Any())
            {
                return commandDto.PersonSkillCollection;
            }
#pragma warning disable 618
				if (commandDto.PersonSkillPeriodCollection == null)
					return new List<PersonSkillDto>();

            return from personSkillPeriodDto in commandDto.PersonSkillPeriodCollection
#pragma warning restore 618
                from s in personSkillPeriodDto.SkillCollection
                select new PersonSkillDto {Active = true, Proficiency = 1, SkillId = s};
        }

        private void resetExternalLogOns(IEnumerable<ExternalLogOnDto> externalLogOnDtos, IPersonPeriod personPeriod, IPerson person)
        {
			  person.ResetExternalLogOn(personPeriod);
            var externalLogOns = _externalLogOnRepository.LoadAll();
            var filteredExternalLogOns = new List<IExternalLogOn>();
	        filteredExternalLogOns.AddRange(externalLogOns.Where(e =>
	        {
				return externalLogOnDtos.Any(edto =>
					edto.AcdLogOnName.Equals(e.AcdLogOnName) &&
					edto.AcdLogOnOriginalId.Equals(e.AcdLogOnOriginalId));
	        }));
	        foreach (var filteredExternalLogOn in filteredExternalLogOns)
	        {
		        person.AddExternalLogOn(filteredExternalLogOn,personPeriod);
	        }
        }
    }
}
