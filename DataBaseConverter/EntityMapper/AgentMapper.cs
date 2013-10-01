using System;
using System.Collections.Generic;
using System.Linq;
using Domain;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Interfaces.Domain;
using ShiftCategoryLimitation = Domain.ShiftCategoryLimitation;

namespace Teleopti.Ccc.DatabaseConverter.EntityMapper
{
    /// <summary>
    /// Tool for converting 6x agent
    /// </summary>
    public class AgentMapper : Mapper<IPerson, global::Domain.Agent>
    {
        //Todo : this line need to be removed when mapping of PersonPeriod and Personcontract finalized
        private readonly IPersonContract _dummyPersonContract;
        

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentMapper"/> class.
        /// </summary>
        /// <param name="mappedObjectPair">The mapped object pair.</param>
        /// <param name="timeZone">The time zone.</param>
        /// <param name="personAccountUpdater"></param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 10/26/2007
        /// </remarks>
        public AgentMapper(MappedObjectPair mappedObjectPair, TimeZoneInfo timeZone) : base(mappedObjectPair, timeZone)
        {
            
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="AgentMapper"/> class.
        /// </summary>
        /// <param name="mappedObjectPair">The mapped object pair.</param>
        /// <param name="timeZone">The time zone.</param>
        /// <param name="personContract">The person contract.</param>
        /// <param name="personAccountUpdater"></param>
        /// <remarks>
        /// Created by: sumeda herath
        /// Created date: 2008-02-05
        /// </remarks>
        public AgentMapper(MappedObjectPair mappedObjectPair, TimeZoneInfo timeZone, IPersonContract personContract)
            : base(mappedObjectPair, timeZone)
        {
            _dummyPersonContract = personContract;
        }


        /// <summary>
        /// Maps the specified old object.
        /// </summary>
        /// <param name="oldEntity">The old object.</param>
        /// <returns></returns>
        public override IPerson Map(global::Domain.Agent oldEntity)
        {
            if (oldEntity.PeriodCollection == null ||
                oldEntity.PeriodCollection.Count == 0)
                    return null;

            IPerson newPerson = new Domain.Common.Person();
            newPerson.Name = new Name(ConversionHelper.MapString(oldEntity.FirstName, 25), ConversionHelper.MapString(oldEntity.LastName, 25));
            newPerson.Email = ConversionHelper.MapString(oldEntity.Email, 50);
            newPerson.Note = ConversionHelper.MapString(oldEntity.Note, 1024);
            newPerson.EmploymentNumber = ConversionHelper.MapString(oldEntity.Sign, 50);
            ((PermissionInformation)newPerson.PermissionInformation).SetDefaultTimeZone(TimeZone);

            if (oldEntity.PeriodCollection != null)
            {

				if (TerminalDate(oldEntity.PeriodCollection).HasValue)
	                newPerson.TerminatePerson(TerminalDate(oldEntity.PeriodCollection).GetValueOrDefault(), new PersonAccountUpdaterDummy());
				else
					newPerson.ActivatePerson(new PersonAccountUpdaterDummy());


                foreach (var agentPeriod in oldEntity.PeriodCollection)
                {
                    MapAgentPeriod(newPerson, agentPeriod);
                }
            }

            if (oldEntity.WorkruleCollection != null)
            {
                foreach (var agentWorkRule in oldEntity.WorkruleCollection)
                {
                    SchedulePeriodType schedType; // = SchedulePeriodType.None;
                    int number = agentWorkRule.NumberOf;

                    switch (agentWorkRule.WorkloadTypeId)
                    {
                        case 1:
                            schedType = SchedulePeriodType.Day;
                            break;
                        case 2:
                            schedType = SchedulePeriodType.Week;
                            number = agentWorkRule.NumberOf / 7;
                            break;
                        default:
                            throw new ValidationException("Workload type was not week or day");
                    }

                    SchedulePeriod schedulePeriod = 
                        new SchedulePeriod(new DateOnly(agentWorkRule.Period.StartDate),
                                           schedType, number);
					schedulePeriod.AverageWorkTimePerDayOverride = agentWorkRule.AverageWorktimePerDay;
                    if (agentWorkRule.CategoryLimitationCollection.Any())
                        mapCategoryLimitations(schedulePeriod, agentWorkRule.CategoryLimitationCollection);

                    newPerson.AddSchedulePeriod(schedulePeriod);

                    //I'm not sure about this...
                    if (agentWorkRule.NumberOfFreedays>0) schedulePeriod.SetDaysOff(agentWorkRule.NumberOfFreedays);
                    
                    
                }
            }

            return newPerson;
        }

        private void MapAgentPeriod(IPerson newPerson, AgentPeriod agentPeriod)
        {
            //TODO! This should be moved to another map class
            DateTime startDate = agentPeriod.Period.StartDate;

            //Todo : Dummy contract need to be replace by the actual PersonContract when mapping of PersonPeriod and Personcontract finalized
            if (_dummyPersonContract == null || MappedObjectPair.Team.GetPaired(agentPeriod.UnitSub) == null)
                return;

            PersonPeriod personPeriod =
                new PersonPeriod(new DateOnly(startDate), _dummyPersonContract,
                                 MappedObjectPair.Team.GetPaired(agentPeriod.UnitSub));


            personPeriod.Note = agentPeriod.Note;
            
            mapRuleSetBag(personPeriod, agentPeriod);

            if (agentPeriod.LoginId.HasValue)
            {
                IExternalLogOn login = MappedObjectPair.ExternalLogOn.GetPaired(agentPeriod.LoginId.Value);
                if (login!=null) personPeriod.AddExternalLogOn(login);
            }

            newPerson.AddPersonPeriod(personPeriod);
            foreach (AgentSkill oldSkill in agentPeriod.SkillCollection)
            {
                if (oldSkill.PeriodSkill.Deleted) continue;
                var newSkill = MappedObjectPair.Skill.GetPaired(oldSkill.PeriodSkill);
                PersonSkill newPersonSkill =
                    new PersonSkill(newSkill, new Percent(oldSkill.Level.Value));
                personPeriod.AddPersonSkill(newPersonSkill);
            }
        }

        private void mapRuleSetBag(IPersonPeriod personPeriod, AgentPeriod agentPeriod)
        {
            IRuleSetBag ruleSetBag =
                                MappedObjectPair.RuleSetBag.GetPaired(
                                    new UnitEmploymentType(agentPeriod.Unit, agentPeriod.TypeOfEmployment));
            if (ruleSetBag != null)
            {
                personPeriod.RuleSetBag = ruleSetBag;
            }
        }

        /// <summary>
        /// Gets the terminal date
        /// </summary>
        /// <param name="agentPeriods"></param>
        /// <returns></returns>
        private static DateOnly? TerminalDate(IList<AgentPeriod> agentPeriods)
        {
            DateTime lastDate = agentPeriods.Max(s => s.Period.EndDate);
            if (lastDate.Year == 2059)
                return null;
            
            return new DateOnly(lastDate);
        }

        private void mapCategoryLimitations(ISchedulePeriod schedulePeriod, IList<ShiftCategoryLimitation> categoryLimitationCollection)
        {
            foreach (var shiftCategoryLimitation in categoryLimitationCollection)
            {
                IShiftCategoryLimitation newLimitation =
                    new Domain.Scheduling.Assignment.ShiftCategoryLimitation(
                        MappedObjectPair.ShiftCategory.GetPaired(shiftCategoryLimitation.Category));
                newLimitation.MaxNumberOf = shiftCategoryLimitation.Limit;
                newLimitation.Weekly = true;
                if (((int)shiftCategoryLimitation.LimitPeriod) != 0)
                    newLimitation.Weekly = false;
                schedulePeriod.AddShiftCategoryLimitation(newLimitation);
            }
        }
    }
}
