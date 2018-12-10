using System;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;

namespace Teleopti.Ccc.Domain.Scheduling
{
    public class Note : VersionedAggregateRootWithBusinessUnit, INote
    {
        private readonly IPerson _person;
        private IScenario _scenario;
        private string _scheduleNote;
        private DateOnly _noteDate;

        public Note(IPerson person, DateOnly noteDate, IScenario scenario, string scheduleNote) : this()
        {
            InParameter.StringTooLong(nameof(scheduleNote), scheduleNote, 255);

            _person = person;
            _noteDate = noteDate;
            _scenario = scenario;
            _scheduleNote = scheduleNote;
        }
		
        protected Note()
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

        public virtual IPerson Person => _person;

	    public virtual IScenario Scenario => _scenario;

	    public virtual string GetScheduleNote(ITextFormatter formatter)
        {
			if (formatter == null)
				throw new ArgumentNullException(nameof(formatter));
			
			return formatter.Format(_scheduleNote);
        }

		private string ScheduleNote => _scheduleNote;

	    public virtual DateOnly NoteDate => _noteDate;

	    public virtual bool BelongsToPeriod(IDateOnlyAsDateTimePeriod dateAndPeriod)
        {
            return dateAndPeriod.DateOnly == _noteDate;
        }

        public virtual bool BelongsToPeriod(DateOnlyPeriod dateOnlyPeriod)
        {
            return dateOnlyPeriod.Contains(_noteDate);
        }

        public virtual bool BelongsToScenario(IScenario scenario)
        {
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
            Note clone = (Note)MemberwiseClone();

            return clone;
        }

        public virtual INote NoneEntityClone()
        {
            INote retObj = (Note)MemberwiseClone();
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
            var retObj = (Note)NoneEntityClone();
            retObj._scenario = parameters.Scenario;
            return retObj;
        }
    }
}
