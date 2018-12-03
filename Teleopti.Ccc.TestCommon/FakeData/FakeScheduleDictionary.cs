using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakeScheduleDictionary : IScheduleDictionary
	{
		public IEnumerator<KeyValuePair<IPerson, IScheduleRange>> GetEnumerator()
		{
			throw new NotImplementedException();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public void Add(KeyValuePair<IPerson, IScheduleRange> item)
		{
			throw new NotImplementedException();
		}

		public void Clear()
		{
			throw new NotImplementedException();
		}

		public bool Contains(KeyValuePair<IPerson, IScheduleRange> item)
		{
			throw new NotImplementedException();
		}

		public void CopyTo(KeyValuePair<IPerson, IScheduleRange>[] array, int arrayIndex)
		{
			throw new NotImplementedException();
		}

		public bool Remove(KeyValuePair<IPerson, IScheduleRange> item)
		{
			throw new NotImplementedException();
		}

		public int Count { get; private set; }
		public bool IsReadOnly { get; private set; }
		public bool ContainsKey(IPerson key)
		{
			throw new NotImplementedException();
		}

		public void Add(IPerson key, IScheduleRange value)
		{
			throw new NotImplementedException();
		}

		public bool Remove(IPerson key)
		{
			throw new NotImplementedException();
		}

		public bool TryGetValue(IPerson key, out IScheduleRange value)
		{
			throw new NotImplementedException();
		}

		public IScheduleRange this[IPerson key]
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		public ICollection<IPerson> Keys { get; private set; }
		public ICollection<IScheduleRange> Values { get; private set; }
		public object Clone()
		{
			throw new NotImplementedException();
		}

		public bool PermissionsEnabled { get; private set; }
		public IScheduleDateTimePeriod Period { get; private set; }
		public IScenario Scenario { get; private set; }
		public IDifferenceCollection<IPersistableScheduleData> DifferenceSinceSnapshot()
		{
			throw new NotImplementedException();
		}

		public void ExtractAllScheduleData(IScheduleExtractor extractor, DateTimePeriod period)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<IBusinessRuleResponse> Modify(IScheduleDay scheduleDay, IScheduleDayChangeCallback scheduleDayChangeCallback)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<IBusinessRuleResponse> Modify(ScheduleModifier modifier, IEnumerable<IScheduleDay> scheduleParts,
			INewBusinessRuleCollection newBusinessRuleCollection, IScheduleDayChangeCallback scheduleDayChangeCallback,
			IScheduleTagSetter scheduleTagSetter, bool forceModify = false, bool isSystemModifying = false)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<IBusinessRuleResponse> Modify(IScheduleDay scheduleDay, INewBusinessRuleCollection newBusinessRuleCollection,
			bool forceModify = false)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<IBusinessRuleResponse> Modify(ScheduleModifier modifier, IEnumerable<IScheduleDay> scheduleParts,
			INewBusinessRuleCollection newBusinessRuleCollection, IScheduleDayChangeCallback scheduleDayChangeCallback,
			IScheduleTagSetter scheduleTagSetter)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<IBusinessRuleResponse> Modify(IScheduleDay scheduleDay, INewBusinessRuleCollection newBusinessRuleCollection)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<IBusinessRuleResponse> Modify(ScheduleModifier modifier, IScheduleDay schedulePart,
			INewBusinessRuleCollection newBusinessRuleCollection, IScheduleDayChangeCallback scheduleDayChangeCallback,
			IScheduleTagSetter scheduleTagSetter)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<IBusinessRuleResponse> CheckBusinessRules(IEnumerable<IScheduleDay> scheduleParts, INewBusinessRuleCollection newBusinessRuleCollection)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<IScheduleDay> SchedulesForDay(DateOnly dateOnly)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<IScheduleDay> SchedulesForPeriod(DateOnlyPeriod period, params IPerson[] agents)
		{
			throw new NotImplementedException();
		}

		public void TakeSnapshot()
		{
			throw new NotImplementedException();
		}

		public IDifferenceCollectionService<IPersistableScheduleData> DifferenceCollectionService { get; private set; }
		public ICollection<IPersonAbsenceAccount> ModifiedPersonAccounts { get; private set; }
		public IPersistableScheduleData UpdateFromBroker<T>(ILoadAggregateFromBroker<T> personAssignmentRepository, Guid id) where T : IPersistableScheduleData
		{
			throw new NotImplementedException();
		}

		public void MeetingUpdateFromBroker<T>(ILoadAggregateFromBroker<T> meetingRepository, Guid id) where T : IMeeting
		{
			throw new NotImplementedException();
		}

		public IPersistableScheduleData DeleteFromBroker(Guid id)
		{
			throw new NotImplementedException();
		}

		public void ValidateBusinessRulesOnPersons(IEnumerable<IPerson> people, INewBusinessRuleCollection newBusinessRuleCollection)
		{
			throw new NotImplementedException();
		}

		public event EventHandler<ModifyEventArgs> PartModified
		{
			add { }
			remove { }
		}
		public void SetUndoRedoContainer(IUndoRedoContainer container)
		{
			throw new NotImplementedException();
		}

		public void DeleteMeetingFromBroker(Guid id)
		{
			throw new NotImplementedException();
		}

		public DateTime ScheduleLoadedTime { get; set; }
	}
}