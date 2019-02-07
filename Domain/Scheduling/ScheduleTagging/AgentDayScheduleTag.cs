using System;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;

namespace Teleopti.Ccc.Domain.Scheduling.ScheduleTagging
{
    public class AgentDayScheduleTag : AggregateRoot_Events_ChangeInfo_Versioned_BusinessUnit, IAgentDayScheduleTag

    {
        private readonly IPerson _person;
        private IScenario _scenario;
        private DateOnly _tagDate;
        private IScheduleTag _scheduleTag;

        public AgentDayScheduleTag(IPerson person, DateOnly tagDate, IScenario scenario, IScheduleTag scheduleTag)
            : this()
        {
            _person = person;
            _tagDate = tagDate;
            _scenario = scenario;
            _scheduleTag = scheduleTag;
        }

        /// <summary>
        /// For Nhib only
        /// </summary>
        protected AgentDayScheduleTag()
        {

        }

        public virtual DateTimePeriod Period
        {
            get
            {
                DateTime agentLocalStart = DateTime.SpecifyKind(_tagDate.Date, DateTimeKind.Unspecified);
                TimeSpan defaultEndTime = TimeSpan.FromHours(24);
                DateTime agentLocalEnd = DateTime.SpecifyKind(_tagDate.Date.Add(defaultEndTime), DateTimeKind.Unspecified);

                return TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(agentLocalStart, agentLocalEnd, _person.PermissionInformation.DefaultTimeZone());
            }
        }

        public virtual IPerson Person
        {
            get { return _person; }
        }

        public virtual IScenario Scenario
        {
            get { return _scenario; }
        }

        public virtual object Clone()
        {
            var clone = (AgentDayScheduleTag)MemberwiseClone();
            return clone;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public virtual bool BelongsToPeriod(IDateOnlyAsDateTimePeriod dateAndPeriod)
        {
            return dateAndPeriod.DateOnly == _tagDate;
        }

        public virtual bool BelongsToPeriod(DateOnlyPeriod dateOnlyPeriod)
        {
            return dateOnlyPeriod.Contains(_tagDate);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public virtual bool BelongsToScenario(IScenario scenario)
        {
            return scenario.Equals(_scenario);
        }

        public virtual IAggregateRoot MainRoot
        {
            get { return _person; }
        }

        public virtual string FunctionPath
        {
            get { return DefinedRaptorApplicationFunctionPaths.ModifyPersonAssignment; }
        }

        public virtual IScheduleTag ScheduleTag
        {
            get { return _scheduleTag; }
            set { _scheduleTag = value; }
        }

        public virtual DateOnly TagDate
        {
            get { return _tagDate; }
        }

        public virtual IPersistableScheduleData CreateTransient()
        {
            return NoneEntityClone();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public virtual IPersistableScheduleData CloneAndChangeParameters(IScheduleParameters parameters)
        {
            var retObj = (AgentDayScheduleTag)NoneEntityClone();
            retObj._scenario = parameters.Scenario;
            return retObj;
        }

        public virtual IAgentDayScheduleTag NoneEntityClone()
        {
            IAgentDayScheduleTag retObj = (AgentDayScheduleTag)MemberwiseClone();
            retObj.SetId(null);

            return retObj;
        }
    }
}