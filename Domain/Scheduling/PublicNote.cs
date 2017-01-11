﻿using System;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
    public class PublicNote : VersionedAggregateRootWithBusinessUnit, IPublicNote
    {
        private IPerson _person;
        private IScenario _scenario;
        private string _scheduleNote;
        private DateOnly _noteDate;

        public PublicNote(IPerson person, DateOnly noteDate, IScenario scenario, string scheduleNote) : this()
        {
            InParameter.StringTooLong("scheduleNote", scheduleNote, 255);

            _person = person;
            _noteDate = noteDate;
            _scenario = scenario;
            _scheduleNote = scheduleNote;
        }

        /// <summary>
        /// For Nhib only
        /// </summary>
        protected PublicNote()
        {
            
        }

        public virtual DateTimePeriod Period
        {
            get
            {
                DateTime agentLocalStart = DateTime.SpecifyKind(_noteDate.Date, DateTimeKind.Unspecified);
                TimeSpan defaultEndTime = TimeSpan.FromHours(24);
                DateTime agentLocalEnd = DateTime.SpecifyKind(_noteDate.Date.Add(defaultEndTime), DateTimeKind.Unspecified);

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

        public virtual string GetScheduleNote(ITextFormatter formatter)
        {
			if (formatter == null)
				throw new ArgumentNullException("formatter");
			
			return formatter.Format(_scheduleNote);
        }

		// For nHibernate
    	private string ScheduleNote
    	{
    		get { return _scheduleNote; }
    	}

        public virtual DateOnly NoteDate
        {
            get { return _noteDate; }
        }

        public virtual bool BelongsToPeriod(IDateOnlyAsDateTimePeriod dateAndPeriod)
        {
            InParameter.NotNull(nameof(dateAndPeriod), dateAndPeriod);
            return dateAndPeriod.DateOnly == _noteDate;
        }

        public virtual bool BelongsToPeriod(DateOnlyPeriod dateOnlyPeriod)
        {
            return dateOnlyPeriod.Contains(_noteDate);
        }

        public virtual bool BelongsToScenario(IScenario scenario)
        {
            InParameter.NotNull(nameof(scenario), scenario);
            return scenario.Equals(_scenario);
        }

        public virtual void AppendScheduleNote(string text)
        {
            string combined = _scheduleNote.Length == 0 ? text : string.Concat(_scheduleNote, " ", text);
            InParameter.StringTooLong(nameof(text), combined, 255);
            _scheduleNote = combined;
        }

        public virtual void ClearScheduleNote()
        {
            _scheduleNote = string.Empty;
        }

        public virtual object Clone()
        {
            PublicNote clone = (PublicNote)MemberwiseClone();

            return clone;
        }

        public virtual IPublicNote NoneEntityClone()
        {
            IPublicNote retObj = (PublicNote)MemberwiseClone();
            retObj.SetId(null);

            return retObj;
        }

        public virtual IAggregateRoot MainRoot => _person;

	    public virtual string FunctionPath => DefinedRaptorApplicationFunctionPaths.ModifyPersonAssignment;

	    public virtual IPersistableScheduleData CreateTransient()
        {
            return NoneEntityClone();
        }

        public virtual IPersistableScheduleData CloneAndChangeParameters(IScheduleParameters parameters)
        {
            InParameter.NotNull(nameof(parameters), parameters);
            var retObj = (PublicNote)NoneEntityClone();
            retObj._scenario = parameters.Scenario;
            return retObj;
        }
    }
}
