using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
            var dateOnlyPeriod = new DateOnlyPeriod(dateOnly, dateOnly);
            IBusinessRuleResponse response = new BusinessRuleResponse(typeof(OpenHoursRule), message, _haltModify, IsMandatory, period, person, dateOnlyPeriod) { Overridden = !_haltModify };
            return response;
        }

        private IBusinessRuleResponse checkScheduleDay(IScheduleDay scheduleDay, IPerson person, DateOnly dateOnly)
        {
            if (scheduleDay.HasProjection)
            {
                IVisualLayerCollection layerCollection = scheduleDay.ProjectionService().CreateProjection();
                if (layerCollection == null || !layerCollection.HasLayers)
                    return null;
                DateTimePeriod period = layerCollection.Period().Value;
                IEnumerable<DateTimePeriod> openHours = createOpenHoursForAgent(period.StartDateTime, person);

                if (openHours.Any(dateTimePeriod => dateTimePeriod.Contains(period)))
                {
                    return null;
                }

                foreach (IVisualLayer layer in layerCollection.FilterLayers<IActivity>())
                {
                    if (((IActivity)layer.Payload).RequiresSkill)
                    {
                        bool found = openHours.Any(dateTimePeriod => dateTimePeriod.Contains(layer.Period));
                        if (!found)
                        {
                            string errorMessage = string.Format(TeleoptiPrincipal.Current.Regional.Culture,
                                            UserTexts.Resources.BusinessRuleNoSkillsOpenErrorMessage,
                                            layer.DisplayDescription(),
                                            layer.Period.LocalStartDateTime,
                                            layer.Period.LocalEndDateTime);
                            return CreateResponse(person, dateOnly, errorMessage);
                        }
                    }
                }
            }
            return null;
        }

        private IEnumerable<DateTimePeriod> createOpenHoursForAgent(DateTime startDateTime, IPerson person)
        {
            IList<DateTimePeriod> ret = new List<DateTimePeriod>();

            IList<ISkill> agentSkills = SkillsOnPerson(startDateTime, person);

            if(agentSkills.Count == 0)
                return ret;
            if(_schedulingResultStateHolder.SkillStaffPeriodHolder.SkillSkillStaffPeriodDictionary.Count == 0)
                return ret;

            ISkillStaffPeriodDictionary skillStaffPeriodDictionary;

            for (int index = 0; index < agentSkills.Count; index++)
            {
                if (_schedulingResultStateHolder.SkillStaffPeriodHolder.SkillSkillStaffPeriodDictionary.TryGetValue(agentSkills[index], out skillStaffPeriodDictionary))
                {
                    ret = DateTimePeriod.MergeLists(ret, new ReadOnlyCollection<DateTimePeriod>(skillStaffPeriodDictionary.SkillOpenHoursCollection));
                }   
            }
            return ret;
        }
 
        public static IList<ISkill> SkillsOnPerson(DateTime dateToCheckOn, IPerson person)
        {
            var skills = new List<ISkill>();
            ICccTimeZoneInfo timeZoneInfo = person.PermissionInformation.DefaultTimeZone();
            var scheduleDateOnlyPerson = new DateOnly(TimeZoneHelper.ConvertFromUtc(dateToCheckOn, timeZoneInfo).Date);
            
			var period = person.Period(scheduleDateOnlyPerson);
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
