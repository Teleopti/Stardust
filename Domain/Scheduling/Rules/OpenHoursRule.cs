using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Rules
{
    public class OpenHoursRule : INewBusinessRule
    {
        private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;
        private bool _haltModify = true;

        public OpenHoursRule(ISchedulingResultStateHolder schedulingResultStateHolder)
        {
            _schedulingResultStateHolder = schedulingResultStateHolder;
        }


        public string ErrorMessage
        {
            get { return ""; }
        }

        public bool IsMandatory
        {
            get { return false; }
        }

        public bool HaltModify
        {
            get { return _haltModify; }
            set { _haltModify = value; }
        }

        public bool ForDelete { get; set; }

        public IEnumerable<IBusinessRuleResponse> Validate(IDictionary<IPerson, IScheduleRange> rangeClones, IEnumerable<IScheduleDay> scheduleDays)
        {
            var responseList = new List<IBusinessRuleResponse>();
            foreach (var scheduleDay in scheduleDays)
            {
                responseList.AddRange(checkDay(rangeClones, scheduleDay));
            }

            return responseList;
        }

        private IEnumerable<IBusinessRuleResponse> checkDay(IDictionary<IPerson, IScheduleRange> rangeClones, IScheduleDay scheduleDay)
        {
            ICollection<IBusinessRuleResponse> responseList = new List<IBusinessRuleResponse>();
            IPerson person = scheduleDay.Person;
            DateOnly dateToCheck = scheduleDay.DateOnlyAsPeriod.DateOnly;
            IScheduleRange currentSchedules = rangeClones[person];
            var oldResponses = currentSchedules.BusinessRuleResponseInternalCollection;
            oldResponses.Remove(CreateResponse(person, dateToCheck, "remove"));
            //on delete this should be empty and never runned
            
            IBusinessRuleResponse response = checkScheduleDay(scheduleDay, person, dateToCheck);
            if(response != null)
            {
                responseList.Add(response);
                oldResponses.Add(response);
            }
            
            return responseList;
        }
        
        private IBusinessRuleResponse CreateResponse(IPerson person, DateOnly dateOnly, string message)
        {
            var dop = new DateOnlyPeriod(dateOnly, dateOnly);
            DateTimePeriod period = dop.ToDateTimePeriod(person.PermissionInformation.DefaultTimeZone());
            IBusinessRuleResponse response = new BusinessRuleResponse(typeof(OpenHoursRule), message, _haltModify, IsMandatory, period, person, dop) { Overridden = !_haltModify };
            return response;
        }

        private IBusinessRuleResponse checkScheduleDay(IScheduleDay scheduleDay, IPerson person, DateOnly dateOnly)
        {
            if (scheduleDay.HasProjection())
            {
                IVisualLayerCollection layerCollection = scheduleDay.ProjectionService().CreateProjection();
                if (layerCollection == null || !layerCollection.HasLayers)
                    return null;
                DateTimePeriod period = layerCollection.Period().Value;
	            var timeZone = person.PermissionInformation.DefaultTimeZone();
                
                foreach (IVisualLayer layer in layerCollection.FilterLayers<IActivity>())
                {
                    var activity = (IActivity) layer.Payload;
                    if ((activity).RequiresSkill)
                    {
                     IEnumerable<DateTimePeriod> openHours = createOpenHoursForAgent(period, dateOnly, person,activity );

                        bool found = openHours.Any(dateTimePeriod => dateTimePeriod.Contains(layer.Period));
                        if (!found)
                        {
                            var errorMessage = string.Format(TeleoptiPrincipal.CurrentPrincipal.Regional.Culture,
                                           UserTexts.Resources.BusinessRuleNoSkillsOpenErrorMessage,
                                           layer.DisplayDescription(),
                                           layer.Period.StartDateTimeLocal(timeZone),
                                           layer.Period.EndDateTimeLocal(timeZone));
                            return CreateResponse(person, dateOnly, errorMessage);
                        }
                    }
                }
            }
            return null;
        }

        private IEnumerable<DateTimePeriod> createOpenHoursForAgent(DateTimePeriod period, DateOnly date, IPerson person,IActivity activity )
        {
            var ret = new List<DateTimePeriod>();

            IList<ISkill> agentSkills = SkillsOnPerson(date, person);

            if(agentSkills.Count == 0)
                return ret;
            if(_schedulingResultStateHolder.SkillStaffPeriodHolder.SkillSkillStaffPeriodDictionary.Count == 0)
                return ret;

	        for (int index = 0; index < agentSkills.Count; index++)
            {
	            var agentSkill = agentSkills[index];
	            if (!agentSkill.Activity.Equals( activity))
                {
                    continue;
                }
	            ISkillStaffPeriodDictionary skillStaffPeriodDictionary;
	            if (_schedulingResultStateHolder.SkillStaffPeriodHolder.SkillSkillStaffPeriodDictionary.TryGetValue(agentSkill, out skillStaffPeriodDictionary))
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
