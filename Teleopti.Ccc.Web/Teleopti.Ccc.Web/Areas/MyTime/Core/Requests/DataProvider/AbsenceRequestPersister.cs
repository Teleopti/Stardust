using System;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider
{
	public class AbsenceRequestPersister : IAbsenceRequestPersister
	{
		private readonly IPersonRequestRepository _personRequestRepository;
		private readonly IAbsenceRequestSynchronousValidator _absenceRequestSynchronousValidator;
		private readonly IPersonRequestCheckAuthorization _personRequestCheckAuthorization;
		private readonly IAbsenceRequestIntradayFilter _absenceRequestIntradayFilter;
		private readonly IQueuedAbsenceRequestRepository _queuedAbsenceRequestRepository;
		private readonly IActivityRepository _activityRepository;
		private readonly ISkillTypeRepository _skillTypeRepository;
		private readonly IDisableDeletedFilter _disableDeletedFilter;
		private readonly IToggleManager _toggleManager;
		private readonly AbsenceRequestFormMapper _mapper;
		private readonly RequestsViewModelMapper _requestsMapper;

		public AbsenceRequestPersister(IPersonRequestRepository personRequestRepository,
									   IAbsenceRequestSynchronousValidator absenceRequestSynchronousValidator, 
									   IPersonRequestCheckAuthorization personRequestCheckAuthorization, 
									   IAbsenceRequestIntradayFilter absenceRequestIntradayFilter, 
									   IQueuedAbsenceRequestRepository queuedAbsenceRequestRepository, 
									   IToggleManager toggleManager, AbsenceRequestFormMapper mapper, 
									   RequestsViewModelMapper requestsMapper, IActivityRepository activityRepository, 
									   ISkillTypeRepository skillTypeRepository, IDisableDeletedFilter disableDeletedFilter)
		{
			_personRequestRepository = personRequestRepository;
			_absenceRequestSynchronousValidator = absenceRequestSynchronousValidator;
			_personRequestCheckAuthorization = personRequestCheckAuthorization;
			_absenceRequestIntradayFilter = absenceRequestIntradayFilter;
			_queuedAbsenceRequestRepository = queuedAbsenceRequestRepository;
			_toggleManager = toggleManager;
			_mapper = mapper;
			_requestsMapper = requestsMapper;
			_activityRepository = activityRepository;
			_skillTypeRepository = skillTypeRepository;
			_disableDeletedFilter = disableDeletedFilter;
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
				_mapper.Map(form, personRequest);

				checkAndProcessDeny(personRequest);
				
				if (_toggleManager.IsEnabled(Toggles.Wfm_Requests_ApprovingModifyRequests_41930))
				{
					if (personRequest.Request.Period != existingPeriod && !personRequest.IsDenied)
					{
						var updatedRows = _queuedAbsenceRequestRepository.UpdateRequestPeriod(personRequest.Id.GetValueOrDefault(), personRequest.Request.Period);
						if (updatedRows == 0)
							throw new InvalidOperationException();
					}
				}
			}
			else
			{
				personRequest = _mapper.Map(form);
				using (_disableDeletedFilter.Disable())
				{
					_skillTypeRepository.LoadAll();
					_activityRepository.LoadAll();
				}
				
				checkAndProcessDeny(personRequest);
				_personRequestRepository.Add(personRequest);

				if (!personRequest.IsDenied)
				{
					_absenceRequestIntradayFilter.Process(personRequest);
				}

			}

			return _requestsMapper.Map(personRequest);
		}

		private void checkAndProcessDeny(IPersonRequest personRequest)
		{
			var result = _absenceRequestSynchronousValidator.Validate(personRequest);
			if (!result.IsValid)
			{
				personRequest.Deny(result.ValidationErrors, _personRequestCheckAuthorization, null,
									PersonRequestDenyOption.AutoDeny | result.DenyOption.GetValueOrDefault(PersonRequestDenyOption.None));
			}
		}
	}
}