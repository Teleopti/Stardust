using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Template;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.NonBlendSkill
{
    public interface INonBlendSkillFromGroupingCreator
    {
        void ProcessDate(DateOnly dateOnly, IGroupPage groupPage);
    }

    public class NonBlendSkillFromGroupingCreator : INonBlendSkillFromGroupingCreator
    {
        private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;
        private readonly INonBlendPersonSkillFromGroupingCreator _nonBlendPersonSkillFromGroupingCreator;
        private readonly IActivity _activity;
        private ICollection<IPerson> _keys;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public NonBlendSkillFromGroupingCreator(ISchedulingResultStateHolder schedulingResultStateHolder, INonBlendPersonSkillFromGroupingCreator nonBlendPersonSkillFromGroupingCreator, IActivity activity)
        {
            _schedulingResultStateHolder = schedulingResultStateHolder;
            _nonBlendPersonSkillFromGroupingCreator = nonBlendPersonSkillFromGroupingCreator;
            _activity = activity;
            _keys = _schedulingResultStateHolder.Schedules.Keys;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
        public void ProcessDate(DateOnly dateOnly, IGroupPage groupPage)
        {
            var skills = _schedulingResultStateHolder.Skills;
            var skillsToRemove = skills.Where(skill => skill.SkillType.ForecastSource.Equals(ForecastSource.NonBlendSkill)).ToList();
            //remove from last if we have any
            foreach (var skill in skillsToRemove)
            {
                skills.Remove(skill);
            }

            var rootGroups = groupPage.RootGroupCollection;
            
            foreach (var rootPersonGroup in rootGroups)
            {
                var personsInDictionary = rootPersonGroup.PersonCollection.Where(_keys.Contains).ToList();
                if (personsInDictionary.Count > 0)
                {
                    var newSkill = createSkill(rootPersonGroup.Description.Name);
                    skills.Add(newSkill);
                    _nonBlendPersonSkillFromGroupingCreator.ProcessPersons(newSkill, personsInDictionary, dateOnly);
                }
                
                foreach (var childPersonGroup in rootPersonGroup.ChildGroupCollection)
                {
                    ProcessChildGroup(childPersonGroup, skills, dateOnly);
                }
            }
        }

        private void ProcessChildGroup(IChildPersonGroup childPersonGroup, IList<ISkill> skills, DateOnly dateOnly)
        {   
            var personsInDictionary = childPersonGroup.PersonCollection.Where(_keys.Contains).ToList();
            if (personsInDictionary.Count > 0)
            {
                var newSkill = createSkill(childPersonGroup.Description.Name);
                skills.Add(newSkill);
            
                _nonBlendPersonSkillFromGroupingCreator.ProcessPersons(newSkill, personsInDictionary, dateOnly);
            }
            
            foreach (var personGroup in childPersonGroup.ChildGroupCollection)
            {
                ProcessChildGroup(personGroup, skills, dateOnly);
            }
        }

        private ISkill createSkill(string name)
        {
            var newSkill = new Skill(name, "", Color.Firebrick, 15,
                        new SkillTypePhone(new Description(), ForecastSource.NonBlendSkill)) { Activity = _activity };

            //IWorkload workLoad = new Workload(newSkill);
            //newSkill.AddWorkload(workLoad);

            IList<ITemplateSkillDataPeriod> templateSkillDataPeriods = new List<ITemplateSkillDataPeriod>();
            var baseDate = SkillDayTemplate.BaseDate.Date.AddHours(8);
            var timeZone = new CccTimeZoneInfo(TimeZoneInfo.Utc);
            for (int i = 0; i < 40; i++)
            {
                DateTimePeriod period = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(baseDate.AddMinutes(15 * i),
                                                                     baseDate.AddMinutes(15 * (i + 1)), timeZone);

				ServiceAgreement serviceAgreement = new ServiceAgreement(new ServiceLevel(new Percent(1), 1 ), new Percent(0), new Percent(1));
            	ITemplateSkillDataPeriod templateSkillDataPeriod = new TemplateSkillDataPeriod(serviceAgreement,
            	                                                                               new SkillPersonData(0, 0),
            	                                                                               period);

                templateSkillDataPeriods.Add(templateSkillDataPeriod);
            }

            newSkill.SetTemplateAt(0, new SkillDayTemplate("fake", templateSkillDataPeriods));
            newSkill.SetTemplateAt(1, new SkillDayTemplate("fake", templateSkillDataPeriods));
            newSkill.SetTemplateAt(2, new SkillDayTemplate("fake", templateSkillDataPeriods));
            newSkill.SetTemplateAt(3, new SkillDayTemplate("fake", templateSkillDataPeriods));
            newSkill.SetTemplateAt(4, new SkillDayTemplate("fake", templateSkillDataPeriods));
            newSkill.SetTemplateAt(5, new SkillDayTemplate("fake", templateSkillDataPeriods));
            newSkill.SetTemplateAt(6, new SkillDayTemplate("fake", templateSkillDataPeriods));
            return newSkill;
        }
    }
}