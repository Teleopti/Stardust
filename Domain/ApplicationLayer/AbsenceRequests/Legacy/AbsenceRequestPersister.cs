using System;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.WorkflowControl;

namespace Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests.Legacy
{
	public class AbsenceRequestPersister : IAbsenceRequestPersister
	{
		private readonly NewAbsenceRequestHandler _newAbsenceRequestHandler;
		private readonly ExistingAbsenceRequestHandler _existingAbsenceRequestHandler;
		private readonly IPersonRequestRepository _personRequestRepository;
		private readonly IAbsenceRequestSynchronousValidator _absenceRequestSynchronousValidator;
		private readonly IPersonRequestCheckAuthorization _personRequestCheckAuthorization;
		private readonly IQueuedAbsenceRequestRepository _queuedAbsenceRequestRepository;
		private readonly IActivityRepository _activityRepository;
		private readonly ISkillTypeRepository _skillTypeRepository;
		private readonly IDisableDeletedFilter _disableDeletedFilter;
		private readonly AbsenceRequestModelMapper _mapper;
		private readonly IAbsenceRequestValidatorProvider _absenceRequestValidatorProvider;
		private readonly INow _now;
		private readonly IAbsenceRequestSetting _absenceRequestSetting;
		private readonly IPermissionProvider _permissionProvider;
		private readonly IAbsenceRepository _absenceRepository;

		public AbsenceRequestPersister(
			NewAbsenceRequestHandler newAbsenceRequestHandler,
			ExistingAbsenceRequestHandler existingAbsenceRequestHandler,
			IPersonRequestRepository personRequestRepository,
			IAbsenceRequestSynchronousValidator absenceRequestSynchronousValidator,
			IPersonRequestCheckAuthorization personRequestCheckAuthorization,
			IQueuedAbsenceRequestRepository queuedAbsenceRequestRepository, AbsenceRequestModelMapper mapper,
			IActivityRepository activityRepository,
			ISkillTypeRepository skillTypeRepository, IDisableDeletedFilter disableDeletedFilter,
			IAbsenceRequestValidatorProvider absenceRequestValidatorProvider, INow now,
			IAbsenceRequestSetting absenceRequestSetting, IPermissionProvider permissionProvider,
			IAbsenceRepository absenceRepository)
		{
			_newAbsenceRequestHandler = newAbsenceRequestHandler;
			_existingAbsenceRequestHandler = existingAbsenceRequestHandler;
			_personRequestRepository = personRequestRepository;
			_absenceRequestSynchronousValidator = absenceRequestSynchronousValidator;
			_personRequestCheckAuthorization = personRequestCheckAuthorization;
			_queuedAbsenceRequestRepository = queuedAbsenceRequestRepository;
			_mapper = mapper;
			_activityRepository = activityRepository;
			_skillTypeRepository = skillTypeRepository;
			_disableDeletedFilter = disableDeletedFilter;
			_absenceRequestValidatorProvider = absenceRequestValidatorProvider;
			_now = now;
			_absenceRequestSetting = absenceRequestSetting;
			_permissionProvider = permissionProvider;
			_absenceRepository = absenceRepository;
		}

		public IPersonRequest Persist(AbsenceRequestModel model)
		{
			if (model.IsNew)
			{
				var personRequest = addRequest(model);
				_newAbsenceRequestHandler.Handle(personRequest);
				return personRequest;
			}
			else
			{
				var personRequest = _personRequestRepository.Find(model.PersonRequestId.Value);
				updateRequest(model, personRequest);
				_existingAbsenceRequestHandler.Handle(personRequest);
				return personRequest;
			}
		}

		private IPersonRequest addRequest(AbsenceRequestModel model)
		{
			var personRequest = _mapper.MapNewAbsenceRequest(model);
			if (!_permissionProvider.HasPersonPermission(DefinedRaptorApplicationFunctionPaths.AbsenceRequestsWeb,
				new DateOnly(_now.UtcDateTime()), personRequest.Person))
			{
				throw new UnauthorizedAccessException();
			}
			using (_disableDeletedFilter.Disable())
			{
				_skillTypeRepository.LoadAll();
				_activityRepository.LoadAll();
				_absenceRepository.LoadAll();
			}

			_personRequestRepository.Add(personRequest);
			executeSynchronousValidations(personRequest);
			return personRequest;
		}

		private void updateRequest(AbsenceRequestModel model, IPersonRequest personRequest)
		{
			if (!_permissionProvider.HasPersonPermission(DefinedRaptorApplicationFunctionPaths.AbsenceRequestsWeb,
				new DateOnly(_now.UtcDateTime()), personRequest.Person))
			{
				throw new UnauthorizedAccessException();
			}
			var existingPeriod = personRequest.Request.Period;
			_mapper.MapExistingAbsenceRequest(model, personRequest);

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