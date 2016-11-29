using System;
using AutoMapper;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Toggle;
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
		private readonly IQueuedAbsenceRequestRepository _queuedAbsenceRequestRepository;
		private readonly IToggleManager _toggleManager;

		public AbsenceRequestPersister(IPersonRequestRepository personRequestRepository,
									   IMappingEngine mapper, 
									   IAbsenceRequestSynchronousValidator absenceRequestSynchronousValidator, 
									   IPersonRequestCheckAuthorization personRequestCheckAuthorization, 
									   IAbsenceRequestIntradayFilter absenceRequestIntradayFilter, IQueuedAbsenceRequestRepository queuedAbsenceRequestRepository, IToggleManager toggleManager)
		{
			_personRequestRepository = personRequestRepository;
			_mapper = mapper;
			_absenceRequestSynchronousValidator = absenceRequestSynchronousValidator;
			_personRequestCheckAuthorization = personRequestCheckAuthorization;
			_absenceRequestIntradayFilter = absenceRequestIntradayFilter;
			_queuedAbsenceRequestRepository = queuedAbsenceRequestRepository;
			_toggleManager = toggleManager;
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
				var existingPeriod = personRequest.Request.Period;
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

				if (_toggleManager.IsEnabled(Toggles.Wfm_Requests_ApprovingModifyRequests_41930))
				{
					if (personRequest.Request.Period != existingPeriod)
					{
						var updatedRows = _queuedAbsenceRequestRepository.UpdateRequestPeriod(personRequest.Id.GetValueOrDefault(), personRequest.Request.Period);
						if(updatedRows == 0)
							throw new InvalidOperationException();
					}
						
				}
			}
			else
			{
				personRequest = _mapper.Map<AbsenceRequestForm, IPersonRequest>(form);
				var result = _absenceRequestSynchronousValidator.Validate(personRequest);
				if (!result.IsValid)
				{
					personRequest.Deny(result.ValidationErrors, _personRequestCheckAuthorization, null,
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