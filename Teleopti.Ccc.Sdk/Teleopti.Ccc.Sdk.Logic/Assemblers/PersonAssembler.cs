using System;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.QueryHandler;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.Logic.Assemblers
{
    public interface IPersonAssembler : IAssembler<IPerson, PersonDto>
    {
        bool IgnorePersonPeriods { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether person information should be updated or new persons created when running Dto => Do assembling.
        /// </summary>
        /// <value><c>true</c> if [enable save or update]; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 8/24/2010
        /// </remarks>
        bool EnableSaveOrUpdate { get; set; }
    }

    public class PersonAssembler : Assembler<IPerson,PersonDto>, IPersonAssembler
    {
        private readonly IAssembler<IWorkflowControlSet,WorkflowControlSetDto> _workflowControlSetAssembler;
	    private readonly IPersonAccountUpdater _personAccountUpdater;
	    public IPersonRepository PersonRepository { get; private set; }
        public bool IgnorePersonPeriods { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether person information should be updated or new persons created when running Dto => Do assembling.
        /// </summary>
        /// <value><c>true</c> if [enable save or update]; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 8/24/2010
        /// </remarks>
        public bool EnableSaveOrUpdate { get; set; }

		public PersonAssembler(IPersonRepository personRepository, IAssembler<IWorkflowControlSet, WorkflowControlSetDto> workflowControlSetAssembler, IPersonAccountUpdater personAccountUpdater)
        {
            _workflowControlSetAssembler = workflowControlSetAssembler;
			_personAccountUpdater = personAccountUpdater;
			PersonRepository = personRepository;
            IgnorePersonPeriods = false;
        }

        public override PersonDto DomainEntityToDto(IPerson entity)
        {
            PersonDto personDto = new PersonDto();
            personDto.Id = entity.Id;
            personDto.Name = entity.Name.ToString();
            personDto.FirstName = entity.Name.FirstName;
            personDto.LastName = entity.Name.LastName;
            personDto.Email = entity.Email;
            personDto.EmploymentNumber = entity.EmploymentNumber;
            personDto.CultureLanguageId = entity.PermissionInformation.CultureLCID();
            personDto.UICultureLanguageId = entity.PermissionInformation.UICultureLCID();
            if (entity.ApplicationAuthenticationInfo != null)
            {
                personDto.ApplicationLogOnName = entity.ApplicationAuthenticationInfo.ApplicationLogOnName;
                personDto.ApplicationLogOnPassword = entity.ApplicationAuthenticationInfo.Password;
            }
            else
            {
                personDto.ApplicationLogOnName = "";
                personDto.ApplicationLogOnPassword = "";
            }
	        if (entity.AuthenticationInfo != null)
	        {
		        var identities = IdentityHelper.Split(entity.AuthenticationInfo.Identity);
		        personDto.WindowsDomain = identities.Item1;
		        personDto.WindowsLogOnName = identities.Item2;
	        }
	        else
	        {
		        personDto.WindowsDomain = "";
		        personDto.WindowsLogOnName = "";
	        }
	        personDto.Note = entity.Note;
            personDto.IsDeleted = ((IDeleteTag)entity).IsDeleted;
            
            if (entity.WorkflowControlSet != null)
                personDto.WorkflowControlSet = _workflowControlSetAssembler.DomainEntityToDto(entity.WorkflowControlSet);

            TimeZoneInfo timeZone = entity.PermissionInformation.DefaultTimeZone();
            personDto.TimeZoneId = timeZone.Id;
            if (entity.TerminalDate.HasValue)
				personDto.TerminationDate = new DateOnlyDto { DateTime = entity.TerminalDate.Value };

            if (!IgnorePersonPeriods)
            {
                foreach (IPersonPeriod personPeriod in entity.PersonPeriodCollection)
                {
                    personDto.PersonPeriodCollection.Add(PersonPeriodDoToDto(personPeriod));
                }
            }

            return personDto;
        }

        public override IPerson DtoToDomainEntity(PersonDto dto)
        {
            if (!dto.Id.HasValue)
            {
                return CreateNewPerson(dto);
            }

            IPerson person = PersonRepository.Get(dto.Id.Value);
            if (EnableSaveOrUpdate)
            {
                UpdatePerson(dto, person);
            }

            return person;
        }

        private IPerson CreateNewPerson(PersonDto dto)
        {
            IPerson person = new Person();
	        if (dto.TerminationDate != null)
	        {
				  // don´t use a real updater when having a new person, it will crash
		        person.TerminatePerson(dto.TerminationDate.ToDateOnly(), new PersonAccountUpdaterDummy());
	        }
            UpdatePerson(dto, person);

            return person;
        }

        private void UpdatePerson(PersonDto dto, IPerson person)
        {
            if (dto.CultureLanguageId.HasValue)
                person.PermissionInformation.SetCulture(new CultureInfo(dto.CultureLanguageId.Value));
            if (!string.IsNullOrEmpty(dto.FirstName)||!string.IsNullOrEmpty(dto.LastName))
                person.Name = new Name(dto.FirstName, dto.LastName);
            else
                throw new ArgumentException("Both first and last name cannot be empty");
            if (!string.IsNullOrEmpty(dto.TimeZoneId))
                person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.FindSystemTimeZoneById(dto.TimeZoneId));
            else
                throw new ArgumentException("Timezone cannot be empty");
            if (dto.UICultureLanguageId.HasValue)
                person.PermissionInformation.SetUICulture(new CultureInfo(dto.UICultureLanguageId.Value));
            if (!string.IsNullOrEmpty(dto.ApplicationLogOnName) && !string.IsNullOrEmpty(dto.ApplicationLogOnPassword))
                person.ApplicationAuthenticationInfo = new ApplicationAuthenticationInfo
                                                           {
                                                               ApplicationLogOnName = dto.ApplicationLogOnName,
                                                               Password = dto.ApplicationLogOnPassword
                                                           };
            if (!string.IsNullOrEmpty(dto.Email))
                person.Email = dto.Email;
            if (!string.IsNullOrEmpty(dto.EmploymentNumber))
                person.EmploymentNumber = dto.EmploymentNumber;
	        if (!string.IsNullOrEmpty(dto.WindowsDomain) && !string.IsNullOrEmpty(dto.WindowsLogOnName))
		        person.AuthenticationInfo = new AuthenticationInfo
		        {
			        Identity = IdentityHelper.Merge(dto.WindowsDomain, dto.WindowsLogOnName)
		        };
			if (string.IsNullOrEmpty(dto.WindowsDomain) && !string.IsNullOrEmpty(dto.WindowsLogOnName))
				person.AuthenticationInfo = new AuthenticationInfo
				{
					Identity = dto.WindowsLogOnName
				};

            if(personTerminated(dto, person))
                person.TerminatePerson(dto.TerminationDate.ToDateOnly(), _personAccountUpdater) ;
            if(personActivated(dto, person))
                person.ActivatePerson(_personAccountUpdater);
            if (!string.IsNullOrEmpty(dto.Note))
                person.Note = dto.Note;
            if(dto.IsDeleted)
                ((IDeleteTag)person).SetDeleted();
        }

	    private static bool personActivated(PersonDto dto, IPerson person)
	    {
			if (dto.TerminationDate == null && person.TerminalDate.HasValue)
				return true;
			return false;
	    }

	    private static bool personTerminated(PersonDto dto, IPerson person)
	    {
		    if (dto.TerminationDate != null)
		    {
			    if (!person.TerminalDate.HasValue)
				    return true;
			    if (person.TerminalDate.Value != new DateOnly(dto.TerminationDate.DateTime))
				    return true;
		    }
		    return false;
	    }


	    private static PersonPeriodDto PersonPeriodDoToDto(IPersonPeriod entity)
        {
            PersonPeriodDto personPeriodDto = new PersonPeriodDto();
			personPeriodDto.Period = new DateOnlyPeriodDto
			{
				StartDate = new DateOnlyDto { DateTime = entity.Period.StartDate },
				EndDate = new DateOnlyDto { DateTime = entity.Period.EndDate }
			};
            
            personPeriodDto.PersonContract = PersonContractDoToDto(entity.PersonContract);

            return personPeriodDto;
        }

        private static PersonContractDto PersonContractDoToDto(IPersonContract entity)
        {
            PersonContractDto personContractDto = new PersonContractDto();
            personContractDto.Id = personContractDto.Id;
#pragma warning disable 0612
            personContractDto.AverageWorkTimePerDay = entity.AverageWorkTimePerDay;
#pragma warning restore 0612
            personContractDto.AverageWorkTime = DateTime.SpecifyKind(DateTime.MinValue.AddTicks(entity.AverageWorkTimePerDay.Ticks), DateTimeKind.Utc);
            if (entity.Contract!=null)
                personContractDto.ContractId = entity.Contract.Id.GetValueOrDefault(Guid.Empty);
            if (entity.PartTimePercentage != null)
                personContractDto.PartTimePercentageId = entity.PartTimePercentage.Id.GetValueOrDefault(Guid.Empty);

            return personContractDto;
        }
    }
}
