﻿using System;
using System.Collections.Generic;
using System.Linq;
using Iesi.Collections.Generic;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.Domain.Scheduling.Meetings
{
	public class Meeting : VersionedAggregateRootWithBusinessUnit, IMeeting, IProvideCustomChangeInfo
	{
		private IPerson _organizer;
		private Iesi.Collections.Generic.ISet<IMeetingPerson> _meetingPersons;
		private String _subject;
		private String _location;
		private String _description;
		private IScenario _scenario;
		private TimeSpan _startTime;
		private TimeSpan _endTime = TimeSpan.FromHours(1);
		private DateOnly _startDate;
		private DateOnly _endDate;
		private IActivity _activity;
		private IList<IRecurrentMeetingOption> meetingRecurrenceOptions = new List<IRecurrentMeetingOption>(1);
		private TimeZoneInfo _timeZoneCache;
		private string _timeZone;
		private Guid _originalMeetingId;
		private readonly IChangeTracker<IMeeting> _meetingChangeTracker = new MeetingChangeTracker();

		public Meeting(IPerson organizer, IEnumerable<IMeetingPerson> meetingPersons, String subject,
					   String location, String description, IActivity activity, IScenario scenario)
		{
			_meetingChangeTracker.TakeSnapshot(this);
			_organizer = organizer;
			_meetingPersons = new HashedSet<IMeetingPerson>();
			_subject = subject;
			_location = location;
			_description = description;
			_activity = activity;
			_scenario = scenario;
			_timeZoneCache = organizer.PermissionInformation.DefaultTimeZone();
			_timeZone = _timeZoneCache.Id;

			var defaultRecurrenceOption = new RecurrentDailyMeeting();
			defaultRecurrenceOption.SetParent(this);
			meetingRecurrenceOptions.Add(defaultRecurrenceOption);

			foreach (IMeetingPerson meetingPerson in meetingPersons)
			{
				addMeetingPerson(meetingPerson);
			}
		}

		protected Meeting()
		{
			_meetingPersons = new HashedSet<IMeetingPerson>();
		}

		public virtual IPerson Organizer
		{
			get { return _organizer; }
			set
			{
				verifyLastKnownStateIsSet();
				_organizer = value;
			}
		}

		private void verifyLastKnownStateIsSet()
		{
			_meetingChangeTracker.TakeSnapshot(this);
		}

		public virtual IEnumerable<IMeetingPerson> MeetingPersons
		{
			get { return _meetingPersons; }
		}

		public virtual bool ContainsPerson(IPerson person)
		{
			foreach (IMeetingPerson meetingPerson in _meetingPersons)
			{
				if (meetingPerson.Person.Equals(person))
					return true;
			}

			return false;
		}

		public virtual string Subject
		{
			set
			{
				verifyLastKnownStateIsSet();
				_subject = value;
			}
		}

		public virtual string GetSubject(ITextFormatter formatter)
		{
			if (formatter == null)
				throw new ArgumentNullException("formatter");
			
			return formatter.Format(_subject);
		}

		public virtual string Location
		{
			set
			{
				verifyLastKnownStateIsSet();
				_location = value;
			}
		}

		public virtual string GetLocation(ITextFormatter formatter)
		{
			if (formatter == null)
				throw new ArgumentNullException("formatter");
			
			return formatter.Format(_location);
		}

		public virtual string Description
		{
			set
			{
				verifyLastKnownStateIsSet();
				_description = value;
			}
		}

		public virtual string GetDescription(ITextFormatter formatter)
		{
			if (formatter == null)
				throw new ArgumentNullException("formatter");
			
			return formatter.Format(_description);
		}

		public virtual TimeZoneInfo TimeZone
		{
			get
			{
				if (_timeZoneCache == null)
				{
					_timeZoneCache = TimeZoneInfo.FindSystemTimeZoneById(_timeZone);
				}
				return _timeZoneCache;
			}
			set
			{
				verifyLastKnownStateIsSet();
				_timeZoneCache = value;
				_timeZone = _timeZoneCache.Id;
			}
		}

		public virtual IActivity Activity
		{
			get { return _activity; }
			set
			{
				verifyLastKnownStateIsSet();
				_activity = value;
			}
		}

		//Too move a meeting to another scenario
		public virtual void SetScenario(IScenario scenario)
		{
			verifyLastKnownStateIsSet();
			_scenario = scenario;
		}

		public virtual Guid OriginalMeetingId
		{
			get
			{
				if (_originalMeetingId == Guid.Empty && Id.HasValue)
					return Id.Value;
				return _originalMeetingId;
			}
			set { _originalMeetingId = value; }
		}

		public virtual void Snapshot()
		{
			verifyLastKnownStateIsSet();
		}

		public virtual IScenario Scenario
		{
			get { return _scenario; }
		}


		public virtual TimeSpan StartTime
		{
			get { return _startTime; }
			set
			{
				verifyLastKnownStateIsSet();
				var tmpEnd = value.Add(MeetingDuration());
				_startTime = value;
				// not over midningt
				if (tmpEnd > new TimeSpan(23, 59, 0))
				{
					_endTime = new TimeSpan(23, 59, 0);
				}
				else
				{
					_endTime = tmpEnd;
				}
				
			}
		}

		public virtual TimeSpan EndTime
		{
			get { return _endTime; }
			set
			{
				verifyLastKnownStateIsSet();
				if (value <= _startTime) value = _startTime.Add(MeetingDuration());
				_endTime = value;
			}
		}

		public virtual DateOnly EndDate
		{
			get { return _endDate; }
			set
			{
				verifyLastKnownStateIsSet();
				_endDate = value;
			}
		}

		public virtual DateOnly StartDate
		{
			get { return _startDate; }
			set
			{
				verifyLastKnownStateIsSet();
				_startDate = value;
			}
		}

		public virtual TimeSpan MeetingDuration()
		{
			return _endTime.Subtract(_startTime);
		}

		public virtual IRecurrentMeetingOption MeetingRecurrenceOption
		{
			get
			{
				return meetingRecurrenceOptions.FirstOrDefault();
			}
		}

		public virtual void ClearMeetingPersons()
		{
			verifyLastKnownStateIsSet();
			_meetingPersons.Clear();
		}

		public virtual void AddMeetingPerson(IMeetingPerson meetingPerson)
		{
			addMeetingPerson(meetingPerson);
		}

		private void addMeetingPerson(IMeetingPerson meetingPerson)
		{
			verifyLastKnownStateIsSet();
			if (!_meetingPersons.Contains(meetingPerson))
			{
				meetingPerson.SetParent(this);
				_meetingPersons.Add(meetingPerson);
			}
		}

		public virtual void RemovePerson(IPerson person)
		{
			verifyLastKnownStateIsSet();
			IMeetingPerson meetingPerson = _meetingPersons.FirstOrDefault(mp => mp.Person.Equals(person));
			if (meetingPerson != null)
				_meetingPersons.Remove(meetingPerson);
		}

		public virtual IList<IPersonMeeting> GetPersonMeetings(IPerson person)
		{
			IList<IPersonMeeting> personMeetings = new List<IPersonMeeting>();
			IMeetingPerson meetingPerson = _meetingPersons.FirstOrDefault(mp => mp.Person.Equals(person));
			if (meetingPerson != null)
			{
				foreach (DateOnly recurringDate in GetRecurringDates())
				{
					personMeetings.Add(new PersonMeeting(this, meetingPerson, getMeetingPeriod(recurringDate)));
				}
			}

			return personMeetings;
		}

		public virtual DateTimePeriod MeetingPeriod(DateOnly meetingDay)
		{
			return getMeetingPeriod(meetingDay);
		}

		private DateTimePeriod getMeetingPeriod(DateOnly dateOnly)
		{
			DateTime localStartTime = dateOnly.Date.Add(_startTime);
			if (TimeZone.IsInvalidTime(localStartTime))
				localStartTime = localStartTime.AddHours(1);

            DateTime startTime = TimeZone.SafeConvertTimeToUtc(localStartTime);

			DateTime localEndTime = dateOnly.Date.Add(_endTime);
			if (TimeZone.IsInvalidTime(localEndTime) || localEndTime < localStartTime)
				localEndTime = localEndTime.AddHours(1);

            DateTime endTime = TimeZone.SafeConvertTimeToUtc(localEndTime);

			return new DateTimePeriod(startTime, endTime);
		}

		public virtual void SetRecurrentOption(IRecurrentMeetingOption recurrentMeetingOption)
		{
			verifyLastKnownStateIsSet();
			meetingRecurrenceOptions.Clear();
			if (recurrentMeetingOption != null)
			{
				meetingRecurrenceOptions.Add(recurrentMeetingOption);
				recurrentMeetingOption.SetParent(this);
			}
		}

		public virtual void RemoveRecurrentOption()
		{
			verifyLastKnownStateIsSet();
			SetRecurrentOption(new RecurrentDailyMeeting());
			_endDate = _startDate;
		}

		public virtual IList<DateOnly> GetRecurringDates()
		{
			if (MeetingRecurrenceOption == null)
				return new List<DateOnly>();

			return MeetingRecurrenceOption.GetMeetingDays(_startDate, _endDate);
		}

		#region ICloneableEntity<Meeting> Members

		public virtual object Clone()
		{
			return EntityClone();
		}

		public virtual IMeeting NoneEntityClone()
		{
			var retobj = cloneEntity(r => r.NoneEntityClone(), m => m.NoneEntityClone());
			retobj.SetId(null);

			return retobj;
		}

		public virtual IMeeting EntityClone()
		{
			return cloneEntity(r => r.EntityClone(), m => m.EntityClone());
		}

		private IMeeting cloneEntity(Func<IRecurrentMeetingOption,IRecurrentMeetingOption> cloneRecurrenceFunction,Func<IMeetingPerson,IMeetingPerson> cloneMeetingPersonFunction)
		{
			var retobj = (Meeting)MemberwiseClone();
			retobj.meetingRecurrenceOptions = new List<IRecurrentMeetingOption>(1);
			if (MeetingRecurrenceOption != null)
			{
				var recurrenceOption = cloneRecurrenceFunction(MeetingRecurrenceOption);
				recurrenceOption.SetParent(retobj);
				retobj.meetingRecurrenceOptions.Add(recurrenceOption);
			}
			retobj._meetingPersons = new HashedSet<IMeetingPerson>();
			if (_meetingPersons != null)
			{
				foreach (IMeetingPerson person in _meetingPersons)
				{
					var personMeeting = cloneMeetingPersonFunction(person);
					personMeeting.SetParent(retobj);
					retobj._meetingPersons.Add(personMeeting);
				}
			}

			return retobj;
		}

		#endregion

		public virtual IEnumerable<IRootChangeInfo> CustomChanges(DomainUpdateType status)
		{
			return _meetingChangeTracker.CustomChanges(this, status);
		}

		public virtual IAggregateRoot BeforeChanges()
		{
			return _meetingChangeTracker.BeforeChanges() ?? this;
		}
	}
}