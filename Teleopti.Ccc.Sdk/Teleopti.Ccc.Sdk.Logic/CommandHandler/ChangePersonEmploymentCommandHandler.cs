﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

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
            using (var uow = _unitOfWorkFactory.LoggedOnUnitOfWorkFactory().CreateAndOpenUnitOfWork())
            {
            	var startDate = new DateOnly(command.Period.StartDate.DateTime);
                var person = _personRepository.Get(command.Person.Id.GetValueOrDefault());
				checkIfAuthorized(person);

                var existPersonPeriod =
					person.PersonPeriodCollection.FirstOrDefault(pp => pp.StartDate == startDate);

                if (existPersonPeriod != null)
                {
                    affectedId = updateExistingPersonPeriod(command, existPersonPeriod, uow);
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
                checkBusinessUnitConsistency(team.Site);
                newPersonPeriod = new PersonPeriod(new DateOnly(command.Period.StartDate.DateTime), 
                                                   command.PersonContract == null
                                                       ? lastPersonPeriod.PersonContract
                                                       : createPersonContract(command.PersonContract),team);
                var externalLogOnDtos = new List<ExternalLogOnDto>();
                lastPersonPeriod.ExternalLogOnCollection.ForEach(
                    e => externalLogOnDtos.Add(new ExternalLogOnDto
                        {
                            AcdLogOnOriginalId = e.AcdLogOnOriginalId,
                            AcdLogOnName = e.AcdLogOnName
                        }));

                var personSkillPeriodDtos = _personSkillPeriodAssembler.DomainEntityToDto(lastPersonPeriod);
                resetExternalLogOns(command.ExternalLogOn ?? externalLogOnDtos, newPersonPeriod);
                resetPersonSkills(command.PersonSkillPeriodCollection ??
                                  new List<PersonSkillPeriodDto> {personSkillPeriodDtos}, newPersonPeriod);

                newPersonPeriod.Note = string.IsNullOrEmpty(command.Note) ? lastPersonPeriod.Note : command.Note;
            }
            else
            {
                if (command.PersonContract == null || command.Team == null)
                    throw new FaultException(
                        "There is no person period existed before, you have to specify both person contract and team.");
                newPersonPeriod = createPersonPeriod(command);
                if (command.ExternalLogOn != null) resetExternalLogOns(command.ExternalLogOn, newPersonPeriod);
                if (command.PersonSkillPeriodCollection != null)
                    resetPersonSkills(command.PersonSkillPeriodCollection, newPersonPeriod);
                if (!string.IsNullOrEmpty(command.Note)) newPersonPeriod.Note = command.Note;
            }
            if (newPersonPeriod == null) throw new FaultException("Create person period error.");

            person.AddPersonPeriod(newPersonPeriod);
            uow.PersistAll();
            return newPersonPeriod.Id;
        }

        private Guid? updateExistingPersonPeriod(ChangePersonEmploymentCommandDto command, IPersonPeriod existPersonPeriod,
                                                 IUnitOfWork uow)
        {
            if (command.Team != null)
            {
                existPersonPeriod.Team = _teamRepository.Get(command.Team.Id.GetValueOrDefault());
                checkBusinessUnitConsistency(existPersonPeriod.Team.Site);
            }
            if (command.PersonContract != null)
            {
                existPersonPeriod.PersonContract = createPersonContract(command.PersonContract);
            }
            if (command.ExternalLogOn != null)
            {
                resetExternalLogOns(command.ExternalLogOn, existPersonPeriod);
            }
            if (command.PersonSkillPeriodCollection != null)
            {
                resetPersonSkills(command.PersonSkillPeriodCollection, existPersonPeriod);
            }
            if (!string.IsNullOrEmpty(command.Note))
                existPersonPeriod.Note = command.Note;

            uow.PersistAll();
            return existPersonPeriod.Id;
        }

        private void checkBusinessUnitConsistency(IBelongsToBusinessUnit belongsToBusinessUnit)
        {
            var currentBusinessUnit = ((TeleoptiIdentity) TeleoptiPrincipal.Current.Identity).BusinessUnit.Id.GetValueOrDefault();
            if (belongsToBusinessUnit.BusinessUnit != null && currentBusinessUnit != belongsToBusinessUnit.BusinessUnit.Id.GetValueOrDefault())
            {
                throw new FaultException(
                    string.Format(
                        "Adding references to items from a different business unit than the currently specified in the header is not allowed. (Type: {0}, Current business unit id: {1}, Attempted business unit id: {2})",
                        belongsToBusinessUnit.GetType(), currentBusinessUnit,
                        belongsToBusinessUnit.BusinessUnit.Id.GetValueOrDefault()));
            }
        }

        private IPersonPeriod createPersonPeriod(ChangePersonEmploymentCommandDto command)
        {
            var team = _teamRepository.Load(command.Team.Id.GetValueOrDefault());
            checkBusinessUnitConsistency(team.Site);
            var personContract = createPersonContract(command.PersonContract);
            return new PersonPeriod(new DateOnly(command.Period.StartDate.DateTime), personContract, team);
        }

        private PersonContract createPersonContract(PersonContractDto personContractDto)
        {
            var partTimePercentage = _partTimePercentageRepository.Load(personContractDto.PartTimePercentageId.GetValueOrDefault());
            var contractSchedule = _contractScheduleRepository.Load(personContractDto.ContractScheduleId.GetValueOrDefault());
            var contract = _contractRepository.Load(personContractDto.ContractId.GetValueOrDefault());

            checkBusinessUnitConsistency((IBelongsToBusinessUnit) partTimePercentage);
            checkBusinessUnitConsistency((IBelongsToBusinessUnit) contractSchedule);
            checkBusinessUnitConsistency((IBelongsToBusinessUnit) contract);

            return new PersonContract(contract, partTimePercentage, contractSchedule);
        }

        private void resetPersonSkills(IEnumerable<PersonSkillPeriodDto> personSkillPeriodDtos, IPersonPeriod personPeriod)
        {
            personPeriod.ResetPersonSkill();
            foreach (var personSkills in from personSkillPeriodDto in personSkillPeriodDtos
                                         select personSkillPeriodDto.SkillCollection.Distinct())
            {
                personSkills.ForEach(s =>
                    {
                        var skill = _skillRepository.Load(s);
                        checkBusinessUnitConsistency(skill);
                        personPeriod.AddPersonSkill(new PersonSkill(skill, new Percent(1)));
                    });
            }
        }

        private void resetExternalLogOns(IEnumerable<ExternalLogOnDto> externalLogOnDtos, IPersonPeriod personPeriod)
        {
            personPeriod.ResetExternalLogOn();
            var externalLogOns = _externalLogOnRepository.LoadAllExternalLogOns();
            var filteredExternalLogOns = new List<IExternalLogOn>();
            externalLogOns.ForEach(e =>
                                       {
                                           if (externalLogOnDtos.Any(
                                               edto =>
                                               edto.AcdLogOnName.Equals(e.AcdLogOnName) &&
                                               edto.AcdLogOnOriginalId.Equals(e.AcdLogOnOriginalId)))
                                               filteredExternalLogOns.Add(e);
                                       });

            filteredExternalLogOns.ForEach(personPeriod.AddExternalLogOn);
        }

		private static void checkIfAuthorized(IPerson person)
		{
			if (!PrincipalAuthorization.Instance().IsPermitted(DefinedRaptorApplicationFunctionPaths.OpenPersonAdminPage,DateOnly.Today,person))
			{
				throw new FaultException("You're not allowed to modify person details.");
			}
		}
    }
}
