using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.Scheduling.Rules
{
	public class OpenHoursRule : INewBusinessRule
	{
		private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;

		public OpenHoursRule(ISchedulingResultStateHolder schedulingResultStateHolder)
		{
			_schedulingResultStateHolder = schedulingResultStateHolder;
		}

		public bool IsMandatory => false;

		public bool HaltModify { get; set; } = true;

		public bool Configurable => false;

		public bool ForDelete { get; set; }

		public IEnumerable<IBusinessRuleResponse> Validate(IDictionary<IPerson, IScheduleRange> rangeClones,
			IEnumerable<IScheduleDay> scheduleDays)
		{
			var responseList = new List<IBusinessRuleResponse>();
			foreach (var scheduleDay in scheduleDays)
			{
				responseList.AddRange(checkDay(rangeClones, scheduleDay));
			}

			return responseList;
		}

		public string Description => Resources.DescriptionOfOpenHoursRule;

		private IEnumerable<IBusinessRuleResponse> checkDay(IDictionary<IPerson, IScheduleRange> rangeClones,
			IScheduleDay scheduleDay)
		{
			ICollection<IBusinessRuleResponse> responseList = new List<IBusinessRuleResponse>();
			var person = scheduleDay.Person;
			var dateToCheck = scheduleDay.DateOnlyAsPeriod.DateOnly;
			var currentSchedules = rangeClones[person];
			var oldResponses = currentSchedules.BusinessRuleResponseInternalCollection;
			oldResponses.Remove(createResponse(person, dateToCheck, "remove"));
			//on delete this should be empty and never runned

			var response = checkScheduleDay(scheduleDay, person, dateToCheck);
			if (response == null) return responseList;

			responseList.Add(response);
			oldResponses.Add(response);

			return responseList;
		}

		private IBusinessRuleResponse createResponse(IPerson person, DateOnly dateOnly, string message)
		{
			var friendlyName = Resources.BusinessRuleNoSkillsOpenFriendlyName;
			var dop = dateOnly.ToDateOnlyPeriod();
			var period = dop.ToDateTimePeriod(person.PermissionInformation.DefaultTimeZone());
			IBusinessRuleResponse response = new BusinessRuleResponse(typeof(OpenHoursRule), message, HaltModify, IsMandatory,
				period, person, dop, friendlyName) {Overridden = !HaltModify};
			return response;
		}

		private IBusinessRuleResponse checkScheduleDay(IScheduleDay scheduleDay, IPerson person, DateOnly dateOnly)
		{
			if (!scheduleDay.HasProjection()) return null;

			var layerCollection = scheduleDay.ProjectionService().CreateProjection();
			if (layerCollection == null || !layerCollection.HasLayers)
				return null;

			var period = layerCollection.Period().Value;
			var timeZone = person.PermissionInformation.DefaultTimeZone();

			var errorMessageTemplate = Resources.BusinessRuleNoSkillsOpenErrorMessage;
			foreach (var layer in layerCollection.FilterLayers<IActivity>())
			{
				var activity = (IActivity) layer.Payload;
				if (!activity.RequiresSkill) continue;

				var openHours = createOpenHoursForAgent(period, dateOnly, person, activity);
				if (openHours.Any(dateTimePeriod => dateTimePeriod.Contains(layer.Period))) continue;

				var errorMessage = string.Format(errorMessageTemplate,
					layer.Payload.ConfidentialDescription_DONTUSE(person),
					layer.Period.StartDateTimeLocal(timeZone),
					layer.Period.EndDateTimeLocal(timeZone));
				return createResponse(person, dateOnly, errorMessage);
			}
			return null;
		}

		private IEnumerable<DateTimePeriod> createOpenHoursForAgent(DateTimePeriod period, DateOnly date, IPerson person,
			IActivity activity)
		{
			var ret = new List<DateTimePeriod>();

			var agentSkills = SkillsOnPerson(date, person);

			if (agentSkills.Count == 0)
				return ret;
			if (_schedulingResultStateHolder.SkillStaffPeriodHolder.SkillSkillStaffPeriodDictionary.Count == 0)
				return ret;

			for (var index = 0; index < agentSkills.Count; index++)
			{
				var agentSkill = agentSkills[index];
				if (!agentSkill.Activity.Equals(activity))
				{
					continue;
				}
				ISkillStaffPeriodDictionary skillStaffPeriodDictionary;
				if (_schedulingResultStateHolder.SkillStaffPeriodHolder.SkillSkillStaffPeriodDictionary.TryGetValue(agentSkill,
					out skillStaffPeriodDictionary))
				{
					ret.AddRange(skillStaffPeriodDictionary.SkillOpenHoursCollection.Where(s => s.Intersect(period)));
				}
			}
			return DateTimePeriod.MergePeriods(ret);
		}

		public static IList<ISkill> SkillsOnPerson(DateOnly dateToCheckOn, IPerson person)
		{
			var skills = new List<ISkill>();

			var period = person.Period(dateToCheckOn);
			if (period != null)
			{
				skills.AddRange(from personSkill in period.PersonSkillCollection
					where personSkill.Active
					select personSkill.Skill);
			}

			return skills;
		}
	}
}