using System;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.WorkflowControl;
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
		private readonly IAbsenceRequestProcessor _absenceRequestProcessor;
		private readonly IQueuedAbsenceRequestRepository _queuedAbsenceRequestRepository;
		private readonly IActivityRepository _activityRepository;
		private readonly ISkillTypeRepository _skillTypeRepository;
		private readonly IDisableDeletedFilter _disableDeletedFilter;
		private readonly AbsenceRequestFormMapper _mapper;
		private readonly IAbsenceRequestValidatorProvider _absenceRequestValidatorProvider;
		private readonly INow _now;
		private readonly IAbsenceRequestSetting _absenceRequestSetting;
		private readonly RequestsViewModelMapper _requestsMapper;

		public AbsenceRequestPersister(IPersonRequestRepository personRequestRepository,
									   IAbsenceRequestSynchronousValidator absenceRequestSynchronousValidator,
									   IPersonRequestCheckAuthorization personRequestCheckAuthorization,
									   IAbsenceRequestProcessor absenceRequestProcessor,
									   IQueuedAbsenceRequestRepository queuedAbsenceRequestRepository, AbsenceRequestFormMapper mapper,
									   RequestsViewModelMapper requestsMapper, IActivityRepository activityRepository,
									   ISkillTypeRepository skillTypeRepository, IDisableDeletedFilter disableDeletedFilter, IAbsenceRequestValidatorProvider absenceRequestValidatorProvider, INow now, IAbsenceRequestSetting absenceRequestSetting)
		{
			_personRequestRepository = personRequestRepository;
			_absenceRequestSynchronousValidator = absenceRequestSynchronousValidator;
			_personRequestCheckAuthorization = personRequestCheckAuthorization;
			_absenceRequestProcessor = absenceRequestProcessor;
			_queuedAbsenceRequestRepository = queuedAbsenceRequestRepository;
			_mapper = mapper;
			_requestsMapper = requestsMapper;
			_activityRepository = activityRepository;
			_skillTypeRepository = skillTypeRepository;
			_disableDeletedFilter = disableDeletedFilter;
			_absenceRequestValidatorProvider = absenceRequestValidatorProvider;
			_now = now;
			_absenceRequestSetting = absenceRequestSetting;
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
				updateRequest(form, personRequest);
			}
			else
			{
				personRequest = addRequest(form);
			}

			if (!personRequest.IsDenied)
			{
				_absenceRequestProcessor.Process(personRequest);
			}

			return _requestsMapper.Map(personRequest);
		}

		private IPersonRequest addRequest(AbsenceRequestForm form)
		{
			var personRequest = _mapper.MapNewAbsenceRequest(form);
			using (_disableDeletedFilter.Disable())
			{
				_skillTypeRepository.LoadAll();
				_activityRepository.LoadAll();
			}

			_personRequestRepository.Add(personRequest);
			executeSynchronousValidations(personRequest);
			return personRequest;
		}

		private void updateRequest(AbsenceRequestForm form, IPersonRequest personRequest)
		{
			var existingPeriod = personRequest.Request.Period;
			_mapper.MapExistingAbsenceRequest(form, personRequest);

			executeSynchronousValidations(personRequest);

			if (personRequest.IsDenied)
				return;

			if (personRequest.Request.Period == existingPeriod)
				return;

			var mergedPeriod = personRequest.Request.Person.WorkflowControlSet.GetMergedAbsenceRequestOpenPeriod((AbsenceRequest)personRequest.Request);

			if (_absenceRequestValidatorProvider.IsAnyStaffingValidatorEnabled(mergedPeriod) &&
				(!isIntradayRequest(personRequest) ||
				 !_absenceRequestValidatorProvider.IsValidatorEnabled<StaffingThresholdValidator>(mergedPeriod)))
			{
				var updatedRows =
					_queuedAbsenceRequestRepository.UpdateRequestPeriod(personRequest.Id.GetValueOrDefault(),
						personRequest.Request.Period);
				if (updatedRows == 0)
					throw new InvalidOperationException();
			}
		}

		private bool isIntradayRequest(IPersonRequest personRequest)
		{
			var startDateTime = _now.UtcDateTime();
			var intradayPeriod = new DateTimePeriod(startDateTime, startDateTime.AddHours(_absenceRequestSetting.ImmediatePeriodInHours));
			return personRequest.Request.Period.ElapsedTime() <= TimeSpan.FromDays(1) && intradayPeriod.Contains(personRequest.Request.Period.EndDateTime);
		}

		private void executeSynchronousValidations(IPersonRequest personRequest)
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