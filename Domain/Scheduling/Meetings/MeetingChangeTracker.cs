using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Meetings
{
	public class MeetingChangeTracker : IChangeTracker<IMeeting>
	{
		private IMeeting _beforeChanges;
		private List<IRootChangeInfo> _listOfUpdates;
		private IEnumerable<IPerson> _currentParticipants;
		private IEnumerable<IPerson> _previouslyParticipating;
		private IEnumerable<IPerson> _removedPeople;
		private IEnumerable<IPerson> _newPeople;
		private IEnumerable<IPerson> _currentPeople;
		private IList<DateTimePeriod> _currentTimes;
		private IList<DateTimePeriod> _oldTimes;
		private IEnumerable<DateTimePeriod> _addedTimes;
		private IEnumerable<DateTimePeriod> _removedTimes;

		public IEnumerable<IRootChangeInfo> CustomChanges(IMeeting afterChanges, DomainUpdateType status)
		{
			if (_beforeChanges == null) return new List<IRootChangeInfo>();

			_listOfUpdates = new List<IRootChangeInfo>();
			if (status == DomainUpdateType.Delete)
			{
				handleDeletedMeeting();
			}
			else
			{
				prepareCollections(afterChanges);

				appendPeopleRemovedFromMeeting(afterChanges);
				appendPeopleAddedToMeeting(afterChanges);
				appendTimesAddedForMeeting(afterChanges);
				appendTimesRemovedForMeeting(afterChanges);

				handleOtherChangedProperties(afterChanges);
			}
			ResetSnapshot();
			TakeSnapshot(afterChanges);
			return _listOfUpdates;
		}

		public void ResetSnapshot()
		{
			_beforeChanges = null;
		}

		public IMeeting BeforeChanges()
		{
			return _beforeChanges;
		}

		private void handleOtherChangedProperties(IMeeting afterChanges)
		{
			var noFormatting = new NoFormatting();

			if (_beforeChanges.GetSubject(noFormatting) != afterChanges.GetSubject(noFormatting) ||
				_beforeChanges.GetLocation(noFormatting) != afterChanges.GetLocation(noFormatting) ||
				_beforeChanges.GetDescription(noFormatting) != afterChanges.GetDescription(noFormatting) ||
			    !afterChanges.Scenario.Equals(_beforeChanges.Scenario) ||
				!afterChanges.Activity.Equals(_beforeChanges.Activity))
			{
				foreach (var period in _currentTimes)
				{
					_listOfUpdates.AddRange(_currentPeople.Select(p => (IRootChangeInfo)
					                                                   new MeetingChangedDetail(
					                                                   	new MeetingChangedEntity { MainRoot = p, Period = period, Id = afterChanges.Id },
					                                                   	DomainUpdateType.Update)));
				}
			}
		}

		private void appendTimesRemovedForMeeting(IMeeting afterChanges)
		{
			foreach (var period in _removedTimes)
			{
				_listOfUpdates.AddRange(_currentPeople.Select(p => (IRootChangeInfo)
				                                                   new MeetingChangedDetail(
				                                                   	new MeetingChangedEntity { MainRoot = p, Period = period, Id = afterChanges.Id },
				                                                   	DomainUpdateType.Delete)));
			}
		}

		private void appendTimesAddedForMeeting(IMeeting afterChanges)
		{
			foreach (var period in _addedTimes)
			{
				_listOfUpdates.AddRange(_currentPeople.Select(p => (IRootChangeInfo)
				                                                   new MeetingChangedDetail(
				                                                   	new MeetingChangedEntity { MainRoot = p, Period = period, Id = afterChanges.Id },
				                                                   	DomainUpdateType.Insert)));
			}
		}

		private void appendPeopleAddedToMeeting(IMeeting afterChanges)
		{
			foreach (var person in _newPeople)
			{
				_listOfUpdates.AddRange(
					_currentTimes.Select(
						t =>
						(IRootChangeInfo)
						new MeetingChangedDetail(new MeetingChangedEntity { MainRoot = person, Period = t, Id = afterChanges.Id },
						                         DomainUpdateType.Insert)));
			}
		}

		private void appendPeopleRemovedFromMeeting(IMeeting afterChanges)
		{
			foreach (var person in _removedPeople)
			{
				_listOfUpdates.AddRange(
					_oldTimes.Select(
						t =>
						(IRootChangeInfo)
						new MeetingChangedDetail(new MeetingChangedEntity { MainRoot = person, Period = t, Id = afterChanges.Id },
						                         DomainUpdateType.Update)));
			}
		}

		private void prepareCollections(IMeeting afterChanges)
		{
			_currentParticipants = afterChanges.MeetingPersons.Select(m => m.Person);
			_previouslyParticipating = _beforeChanges.MeetingPersons.Select(m => m.Person);
			_removedPeople = _previouslyParticipating.Except(_currentParticipants);
			_newPeople = _currentParticipants.Except(_previouslyParticipating);
			_currentPeople = _currentParticipants.Except(_newPeople);

			_currentTimes = new List<DateTimePeriod>();
			foreach (DateOnly recurringDate in afterChanges.GetRecurringDates())
			{
				_currentTimes.Add(afterChanges.MeetingPeriod(recurringDate));
			}

			_oldTimes = new List<DateTimePeriod>();
			foreach (DateOnly recurringDate in _beforeChanges.GetRecurringDates())
			{
				_oldTimes.Add(_beforeChanges.MeetingPeriod(recurringDate));
			}

			_addedTimes = _currentTimes.Except(_oldTimes);
			_removedTimes = _oldTimes.Except(_currentTimes);
		}

		private void handleDeletedMeeting()
		{
			var previouslyParticipating = _beforeChanges.MeetingPersons.Select(m => m.Person);
			foreach (DateOnly recurringDate in _beforeChanges.GetRecurringDates())
			{
				_listOfUpdates.AddRange(
					previouslyParticipating.Select(
						p =>
						(IRootChangeInfo)
						new MeetingChangedDetail(
							new MeetingChangedEntity { MainRoot = p, Period = _beforeChanges.MeetingPeriod(recurringDate), Id = _beforeChanges.Id },
							DomainUpdateType.Delete)));
			}
		}

		public void TakeSnapshot(IMeeting beforeChanges)
		{
			if (_beforeChanges == null)
			{
				_beforeChanges = beforeChanges.EntityClone();
			}
		}
	}
}