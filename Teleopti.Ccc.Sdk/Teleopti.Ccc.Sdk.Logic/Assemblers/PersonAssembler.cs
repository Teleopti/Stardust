using System;
using System.Globalization;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.QueryHandler;

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

	public class PersonAssembler : Assembler<IPerson, PersonDto>, IPersonAssembler
	{
		private readonly IAssembler<IWorkflowControlSet, WorkflowControlSetDto> _workflowControlSetAssembler;
		private readonly IPersonAccountUpdater _personAccountUpdater;
		private readonly IPersonRepository personRepository;
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

		public PersonAssembler(IPersonRepository personRepository,
			IAssembler<IWorkflowControlSet, WorkflowControlSetDto> workflowControlSetAssembler,
			IPersonAccountUpdater personAccountUpdater)
		{
			_workflowControlSetAssembler = workflowControlSetAssembler;
			_personAccountUpdater = personAccountUpdater;
			this.personRepository = personRepository;
			IgnorePersonPeriods = false;
		}

		public override PersonDto DomainEntityToDto(IPerson entity)
		{
			PersonDto personDto = new PersonDto
			{
				Id = entity.Id,
				Name = entity.Name.ToString(),
				FirstName = entity.Name.FirstName,
				LastName = entity.Name.LastName,
				Email = entity.Email,
				EmploymentNumber = entity.EmploymentNumber,
				CultureLanguageId = entity.PermissionInformation.CultureLCID(),
				UICultureLanguageId = entity.PermissionInformation.UICultureLCID(),
				FirstDayOfWeek = entity.FirstDayOfWeek
			};
			
			personDto.ApplicationLogOnName = "";
			personDto.ApplicationLogOnPassword = "";
			
#pragma warning disable 618
			personDto.WindowsDomain = "";
			personDto.WindowsLogOnName = "";
#pragma warning restore 618
			personDto.Identity = "";
			personDto.Note = entity.Note;
			personDto.IsDeleted = ((IDeleteTag)entity).IsDeleted;

			if (entity.WorkflowControlSet != null)
				personDto.WorkflowControlSet = _workflowControlSetAssembler.DomainEntityToDto(entity.WorkflowControlSet);

			TimeZoneInfo timeZone = entity.PermissionInformation.DefaultTimeZone();
			personDto.TimeZoneId = timeZone.Id;
			if (entity.TerminalDate.HasValue)
				personDto.TerminationDate = new DateOnlyDto { DateTime = entity.TerminalDate.Value.Date };

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

			IPerson person = personRepository.Get(dto.Id.Value);
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
			if (string.IsNullOrEmpty(dto.FirstName) && string.IsNullOrEmpty(dto.LastName))
				throw new ArgumentException("Both first and last name cannot be empty");
			else
				person.SetName(new Name(dto.FirstName, dto.LastName));
			if (!string.IsNullOrEmpty(dto.TimeZoneId))
				person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.FindSystemTimeZoneById(dto.TimeZoneId));
			else
				throw new ArgumentException("Timezone cannot be empty");
			if (dto.UICultureLanguageId.HasValue)
				person.PermissionInformation.SetUICulture(new CultureInfo(dto.UICultureLanguageId.Value));
			if (!string.IsNullOrEmpty(dto.Email))
				person.Email = dto.Email;
			if (!string.IsNullOrEmpty(dto.EmploymentNumber))
				person.SetEmploymentNumber(dto.EmploymentNumber);
			if (personTerminated(dto, person))
				person.TerminatePerson(dto.TerminationDate.ToDateOnly(), _personAccountUpdater);
			if (personActivated(dto, person))
				person.ActivatePerson(_personAccountUpdater);
			if (!string.IsNullOrEmpty(dto.Note))
				person.Note = dto.Note;
			if (dto.IsDeleted)
				((IDeleteTag)person).SetDeleted();
			if (dto.FirstDayOfWeek.HasValue)
				person.FirstDayOfWeek = dto.FirstDayOfWeek.Value;
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
			PersonPeriodDto personPeriodDto = new PersonPeriodDto
			{
				Period = new DateOnlyPeriodDto
				{
					StartDate = new DateOnlyDto {DateTime = entity.Period.StartDate.Date},
					EndDate = new DateOnlyDto {DateTime = entity.Period.EndDate.Date}
				},
				PersonContract = PersonContractDoToDto(entity.PersonContract),
				Team = new TeamDto
				{
					Id = entity.Team.Id
				}
			};
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
			if (entity.Contract != null)
				personContractDto.ContractId = entity.Contract.Id.GetValueOrDefault(Guid.Empty);
			if (entity.PartTimePercentage != null)
				personContractDto.PartTimePercentageId = entity.PartTimePercentage.Id.GetValueOrDefault(Guid.Empty);

			personContractDto.ContractScheduleId = entity.ContractSchedule.Id.GetValueOrDefault();
			return personContractDto;
		}
	}
}
