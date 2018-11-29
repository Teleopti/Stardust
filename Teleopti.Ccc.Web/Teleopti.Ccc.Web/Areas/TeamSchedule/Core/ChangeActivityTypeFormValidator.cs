using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Models;


namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Core
{

	public interface IChangeActivityTypeFormValidator
	{
		ChangeActivityTypeFormValidateResult Validate(ChangeActivityTypeFormData input);
	}

	public class ChangeActivityTypeFormValidator : IChangeActivityTypeFormValidator
	{
		private readonly IPersonRepository _personRepository;
		private readonly IActivityRepository _activityRepository;
		private readonly IScheduleStorage _scheduleStorage;
		private readonly ICurrentScenario _currentScenario;
		private readonly IPermissionProvider _permissionProvider;

		public ChangeActivityTypeFormValidator(IPersonRepository personRepository,
			IActivityRepository activityRepository,
			IScheduleStorage scheduleStorage,
			ICurrentScenario currentScenario,
			IPermissionProvider permissionProvider)
		{
			_personRepository = personRepository;
			_activityRepository = activityRepository;
			_scheduleStorage = scheduleStorage;
			_currentScenario = currentScenario;
			_permissionProvider = permissionProvider;
		}
		public ChangeActivityTypeFormValidateResult Validate(ChangeActivityTypeFormData input)
		{

			var result = new ChangeActivityTypeFormValidateResult();
			result.IsValid = true;

			IPerson person = null;
			if (input.Date == default(DateTime)
				|| input.PersonId == default(Guid)
				|| input.Layers.IsNullOrEmpty()
				|| input.Layers.Any(l => l.IsNew && (!l.StartTime.HasValue || !l.EndTime.HasValue))
				|| (person = _personRepository.Get(input.PersonId)) == null)
			{
				return invalidInputResult(result, null);
			}
			result.Person = person;

			var date = new DateOnly(input.Date);

			result = permissionErrorResult(result, person, date);
			if (!result.IsValid) return result;

			var activities = new Dictionary<Guid, IActivity>();

			foreach (var activityId in input.Layers.Select(l => l.ActivityId))
			{
				if (activities.ContainsKey(activityId)) continue;
				var activity = _activityRepository.Get(activityId);
				if (activity == null)
				{
					return invalidInputResult(result, Resources.ActivityTypeChangedByOthers);
				}
				activities.Add(activityId, activity);
			}
			result.Activities = activities;

			var scheduleDic = _scheduleStorage.FindSchedulesForPersons(_currentScenario.Current(),
				new[] { person },
				new ScheduleDictionaryLoadOptions(false, false),
				date.ToDateTimePeriod(TimeZoneInfo.Utc), new[] { person }, false);

			var scheduleRange = scheduleDic[person];
			var scheduleDay = scheduleRange.ScheduledDay(date);
			var shiftLayers = scheduleDay.PersonAssignment().ShiftLayers.ToDictionary(sl => sl.Id);

			var inputLayerIds = input.Layers.SelectMany(l => l.ShiftLayerIds);
			if (shiftLayers.IsNullOrEmpty() || inputLayerIds.Any(id => !shiftLayers.ContainsKey(id)))
			{
				return invalidInputResult(result, Resources.ShiftChangedByOthers);
			}

			result.ScheduleDictionary = scheduleDic;

			return result;

		}

		private ChangeActivityTypeFormValidateResult permissionErrorResult(
			ChangeActivityTypeFormValidateResult result,
			IPerson person,
			DateOnly date)
		{
			if (!_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ModifyWriteProtectedSchedule)
				&& person.PersonWriteProtection.IsWriteProtected(date))
			{
				result.IsValid = false;
				result.ErrorMessages.Add(Resources.SaveFailedForModifyWriteProtectedSchedule);
			}

			if (!_permissionProvider.IsPersonSchedulePublished(date, person) &&
				!_permissionProvider.HasPersonPermission(DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules, date,
					person))
			{
				result.IsValid = false;
				result.ErrorMessages.Add(Resources.SaveFailedForNoPermissionToEditUnpublishedSchedule);
			}

			return result;
		}

		private ChangeActivityTypeFormValidateResult invalidInputResult(ChangeActivityTypeFormValidateResult result, string errorMessage)
		{
			var message = errorMessage.IsNullOrEmpty() ? Resources.SaveFailedForInvalidInput : errorMessage;
			result.IsValid = false;
			result.ErrorMessages.Add(message);
			return result;
		}
	}

	public class ChangeActivityTypeFormValidateResult
	{
		public ChangeActivityTypeFormValidateResult()
		{
			ErrorMessages = new List<string>();
		}
		public bool IsValid { get; set; }
		public IPerson Person { get; set; }
		public IDictionary<Guid, IActivity> Activities { get; set; }
		public IScheduleDictionary ScheduleDictionary { get; set; }
		public IList<string> ErrorMessages { get; set; }
	}
}