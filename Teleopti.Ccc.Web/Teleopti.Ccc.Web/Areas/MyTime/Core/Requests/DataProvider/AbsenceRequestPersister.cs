using System;
using AutoMapper;
using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider
{
	public class AbsenceRequestPersister : IAbsenceRequestPersister
	{
		private readonly IPersonRequestRepository _personRequestRepository;
		private readonly IMappingEngine _mapper;

		private readonly IAbsenceRequestSynchronousValidator _absenceRequestSynchronousValidator;
		private readonly IPersonRequestCheckAuthorization _personRequestCheckAuthorization;
		private readonly IAbsenceRequestIntradayFilter _absenceRequestIntradayFilter;

		public AbsenceRequestPersister(IPersonRequestRepository personRequestRepository,
									   IMappingEngine mapper, 
									   IAbsenceRequestSynchronousValidator absenceRequestSynchronousValidator, 
									   IPersonRequestCheckAuthorization personRequestCheckAuthorization, 
									   IAbsenceRequestIntradayFilter absenceRequestIntradayFilter)
		{
			_personRequestRepository = personRequestRepository;
			_mapper = mapper;
			_absenceRequestSynchronousValidator = absenceRequestSynchronousValidator;
			_personRequestCheckAuthorization = personRequestCheckAuthorization;
			_absenceRequestIntradayFilter = absenceRequestIntradayFilter;
		}

		public RequestViewModel Persist(AbsenceRequestForm form)
		{
			IPersonRequest personRequest = null;
			if (form.EntityId.HasValue)
			{
				personRequest = _personRequestRepository.Find(form.EntityId.Value);
			}
			if (personRequest != null)
			{
				try
				{
					_mapper.Map(form, personRequest);
				}
				catch (AutoMapperMappingException e)
				{
					if (e.InnerException is InvalidOperationException)
					{
						// this catch is intent to catch InvalidOperationException throw from PersonRequest#CheckIfEditable
						throw e.InnerException;
					}
					throw;
				}
			}
			else
			{
				personRequest = _mapper.Map<AbsenceRequestForm, IPersonRequest>(form);
				var result = _absenceRequestSynchronousValidator.Validate(personRequest);
				if (!result.IsValid)
				{
					personRequest.Deny(null, result.ValidationErrors, _personRequestCheckAuthorization,
									   PersonRequestDenyOption.AutoDeny | result.DenyOption.GetValueOrDefault(PersonRequestDenyOption.None));
				}
				_personRequestRepository.Add(personRequest);

				if (!personRequest.IsDenied)
				{
					_absenceRequestIntradayFilter.Process(personRequest);
				}

			}

			return _mapper.Map<IPersonRequest, RequestViewModel>(personRequest);
		}
	}
}