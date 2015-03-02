﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.ServiceModel;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.Requests;

namespace Teleopti.Ccc.Sdk.WcfService.Factory
{
	public class PersonRequestFactory : IPersonRequestFactory
	{
		private readonly IPersistPersonRequest _persistPersonRequest;
		private readonly IMessagePopulatingServiceBusSender _serviceBusSender;
		private readonly IPersonRequestRepository _personRequestRepository;
		private readonly ICurrentScenario _currentScenario;
		private readonly IScheduleRepository _scheduleRepository;
		private readonly IPersonRepository _personRepository;
		private readonly IAssembler<IPersonRequest, PersonRequestDto> _personRequestAssembler;

		public PersonRequestFactory(IPersistPersonRequest persistPersonRequest, IMessagePopulatingServiceBusSender serviceBusSender, IPersonRequestRepository personRequestRepository, ICurrentScenario currentScenario, IScheduleRepository scheduleRepository, IPersonRepository personRepository, IAssembler<IPersonRequest, PersonRequestDto> personRequestAssembler)
		{
			_persistPersonRequest = persistPersonRequest;
			_serviceBusSender = serviceBusSender;
			_personRequestRepository = personRequestRepository;
			_currentScenario = currentScenario;
			_scheduleRepository = scheduleRepository;
			_personRepository = personRepository;
			_personRequestAssembler = personRequestAssembler;
		}

		public ICollection<PersonRequestDto> GetAllRequestsForPerson(PersonDto person)
		{
			IPerson logonPerson = _personRepository.Load(person.Id.GetValueOrDefault(Guid.Empty));
			var personRequestList = _personRequestRepository.FindAllRequestsForAgent(logonPerson);

			ICollection<PersonRequestDto> personRequestDtoList = new List<PersonRequestDto>(_personRequestAssembler.DomainEntitiesToDtos(personRequestList));

			return personRequestDtoList;
		}

		public ICollection<PersonRequestDto> GetAllRequestModifiedWithinPeriodOrPending(PersonDto person, DateTime utcStartDate, DateTime utcEndDate)
		{
			IPerson logonPerson = _personRepository.Load(person.Id.GetValueOrDefault(Guid.Empty));
			IList<IPersonRequest> personRequestList =
				_personRequestRepository.FindAllRequestModifiedWithinPeriodOrPending(logonPerson,
																					 new DateTimePeriod(utcStartDate,
																										utcEndDate));

			ICollection<PersonRequestDto> personRequestDtoList =
				new List<PersonRequestDto>(_personRequestAssembler.DomainEntitiesToDtos(personRequestList));

			return personRequestDtoList;
		}

		public ICollection<PersonRequestDto> GetPersonRequests(PersonDto person, DateTime localStartDate, DateTime localEndDate)
		{
			DateTime utcStartDate = new DateTime(localStartDate.Ticks, DateTimeKind.Utc);
			DateTime utcEndDate = new DateTime(localEndDate.Ticks, DateTimeKind.Utc);
			DateTimePeriod period = new DateTimePeriod(utcStartDate.AddDays(-1), utcEndDate.AddDays(1));

			IPerson logonPerson = _personRepository.Load(person.Id.GetValueOrDefault(Guid.Empty));
			IList<IPersonRequest> personRequestList = _personRequestRepository.Find(logonPerson, period);

			ICollection<PersonRequestDto> personRequestDtoList = new List<PersonRequestDto>(_personRequestAssembler.DomainEntitiesToDtos(personRequestList));

			return personRequestDtoList;
		}

		public void DeletePersonRequest(PersonRequestDto personRequestDto, IUnitOfWork unitOfWork)
		{
			using (new MessageBrokerSendEnabler())
			{
				IPersonRequest personRequest =
					_personRequestRepository.Load(personRequestDto.Id.GetValueOrDefault(Guid.Empty));
				_personRequestRepository.Remove(personRequest);
			}
		}

		public PersonRequestDto SavePersonRequest(PersonRequestDto personRequestDto, IUnitOfWork unitOfWork)
		{
			if (personRequestDto.Request is AbsenceRequestDto)
			{
				// Do not allow absence request to be saved
				throw new FaultException("Invalid request type. Use another method to save Absence requests.");

			}

			Action<IPersonRequest> requestCallback = setNewPersonRequestToPending;

			var result = _persistPersonRequest.Persist(personRequestDto, unitOfWork, requestCallback);

			//Call RSB!
			var message = new NewShiftTradeRequestCreated
				{
					PersonRequestId = result.Id.GetValueOrDefault(Guid.Empty)
				};
			_serviceBusSender.Send(message, true);

			return new PersonRequestDto { Id = result.Id };
		}

		private void addNewRequest(IPersonRequest personRequest)
		{
			if (!personRequest.Id.HasValue)
			{
				_personRequestRepository.Add(personRequest);
			}
		}

		public PersonRequestDto CreateShiftTradeRequest(PersonDto requester, string subject, string message, ICollection<ShiftTradeSwapDetailDto> shiftTradeSwapDetailDtos)
		{
			if (shiftTradeSwapDetailDtos.Count == 0)
				throw new FaultException("You must supply at least one item in the list shiftTradeSwapDetailDtos.");

			var personRequestDto = new PersonRequestDto
													{
														Person = requester,
														Subject = subject,
														Message = message
													};

			var shiftTradeRequestDto = new ShiftTradeRequestDto();
			foreach (var shiftTradeSwapDetailDto in shiftTradeSwapDetailDtos)
				shiftTradeRequestDto.ShiftTradeSwapDetails.Add(shiftTradeSwapDetailDto);

			personRequestDto.Request = shiftTradeRequestDto;

			return CreatePersonRequest(personRequestDto);
		}

		public PersonRequestDto SetShiftTradeRequest(PersonRequestDto personRequestDto, string subject, string message, ICollection<ShiftTradeSwapDetailDto> shiftTradeSwapDetailDtos)
		{
			if (shiftTradeSwapDetailDtos.Count == 0) throw new FaultException("You must supply at least one item in the list shiftTradeSwapDetailDtos.");

			personRequestDto.Subject = subject;
			personRequestDto.Message = message;

			ShiftTradeRequestDto shiftTradeRequestDto = new ShiftTradeRequestDto();
			foreach (ShiftTradeSwapDetailDto shiftTradeSwapDetailDto in shiftTradeSwapDetailDtos)
			{
				shiftTradeRequestDto.ShiftTradeSwapDetails.Add(shiftTradeSwapDetailDto);
			}
			ShiftTradeRequestDto previousShiftTradeRequestDto = personRequestDto.Request as ShiftTradeRequestDto;
			if (previousShiftTradeRequestDto != null)
			{
				shiftTradeRequestDto.Id = previousShiftTradeRequestDto.Id;
			}

			personRequestDto.Request = shiftTradeRequestDto;

			return CreatePersonRequest(personRequestDto);
		}

		private void setNewPersonRequestToPending(IPersonRequest personRequest)
		{
			if (!personRequest.Id.HasValue && personRequest.IsNew)
			{
				personRequest.Pending();
			}
			addNewRequest(personRequest);
		}

		public PersonRequestDto GetPersonRequestDto(PersonRequestDto personRequestDto)
		{
			IPersonRequest personRequest = _personRequestRepository.Find(personRequestDto.Id.GetValueOrDefault(Guid.Empty));

			// Fix for 10152, when a PersonRequest is deleted by user personRequestRepository cannot find it.
			if (personRequest != null)
			{
				personRequestDto = _personRequestAssembler.DomainEntityToDto(personRequest);
			}
			return personRequestDto;
		}

		public PersonRequestDto CreatePersonRequest(PersonRequestDto personRequestDto)
		{
			var personRequest = _personRequestAssembler.DtoToDomainEntity(personRequestDto);
			var shiftTradeRequestSetChecksum =
				new ShiftTradeRequestSetChecksum(_currentScenario,
												 _scheduleRepository);
			shiftTradeRequestSetChecksum.SetChecksum(personRequest.Request);
			return _personRequestAssembler.DomainEntityToDto(personRequest);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		public PersonRequestDto AcceptShiftTradeRequest(PersonRequestDto personRequestDto, IUnitOfWork unitOfWork, IPerson person)
		{
			if (DomainPersonRequestHasBeenDeleted(personRequestDto.Id.GetValueOrDefault(Guid.Empty)))
			{
				personRequestDto.IsDeleted = true;
				return personRequestDto;
			}

			PersonRequestedShiftTrade personRequestedShiftTrade = new PersonRequestedShiftTrade(personRequestDto);
			if (personRequestedShiftTrade.IsSatisfiedBy(person))
			{
				var command = new AcceptPreviouslyReferredShiftTradeCommand(_scheduleRepository, _personRequestRepository,
																			_currentScenario, _serviceBusSender,
																			personRequestDto);
				command.Execute();
			}
			else
			{
				var message = new AcceptShiftTrade
				{
					PersonRequestId = personRequestDto.Id.GetValueOrDefault(Guid.Empty),
					AcceptingPersonId = person.Id.GetValueOrDefault(Guid.Empty),
					Message = personRequestDto.Message
				};
				_serviceBusSender.Send(message, true);
			}

			return personRequestDto;
		}

		/// <summary>
		/// Check with Domain if PersonRequest has been deleted.
		/// </summary>
		private bool DomainPersonRequestHasBeenDeleted(Guid personRequestGuid)
		{
			IPersonRequest domainPersonRequest = _personRequestRepository.Load(personRequestGuid);
			IDeleteTag deleteTag = (IDeleteTag)domainPersonRequest;
			return deleteTag.IsDeleted;
		}

		public PersonRequestDto DenyShiftTradeRequest(PersonRequestDto personRequestDto, IUnitOfWork unitOfWork)
		{
			if (DomainPersonRequestHasBeenDeleted(personRequestDto.Id.GetValueOrDefault(Guid.Empty)))
			{
				personRequestDto.IsDeleted = true;
				return personRequestDto;
			}
			IPersonRequest domainPersonRequest;
			using (new MessageBrokerSendEnabler())
			{
				domainPersonRequest = _personRequestRepository.Load(personRequestDto.Id.GetValueOrDefault(Guid.Empty));
				domainPersonRequest.TrySetMessage(personRequestDto.Message);
				domainPersonRequest.Deny(TeleoptiPrincipal.CurrentPrincipal.GetPerson(_personRepository), "RequestDenyReasonOtherPart", new SdkPersonRequestAuthorizationCheck());
				unitOfWork.PersistAll();
			}
			return personRequestDto;
		}

		public PersonRequestDto UpdatePersonRequestMessage(PersonRequestDto personRequestDto, IUnitOfWork unitOfWork)
		{
			IPersonRequest domainPersonRequest;
			if (DomainPersonRequestHasBeenDeleted(personRequestDto.Id.GetValueOrDefault(Guid.Empty)))
			{
				personRequestDto.IsDeleted = true;
				return personRequestDto;
			}
			using (new MessageBrokerSendEnabler())
			{
				domainPersonRequest = _personRequestRepository.Load(personRequestDto.Id.GetValueOrDefault(Guid.Empty));
				domainPersonRequest.TrySetMessage(personRequestDto.Message);
				unitOfWork.PersistAll();
			}
			return personRequestDto;
		}
	}

	/// <summary>
	/// Todo! Add checks for request app function!
	/// </summary>
	public class SdkPersonRequestAuthorizationCheck : IPersonRequestCheckAuthorization
	{
		public void VerifyEditRequestPermission(IPersonRequest personRequest)
		{
		}

		public bool HasEditRequestPermission(IPersonRequest personRequest)
		{
			return true;
		}

		public bool HasViewRequestPermission(IPersonRequest personRequest)
		{
			return true;
		}
	}
}