using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.DayOffScheduling;


namespace Teleopti.Ccc.DomainTest.Scheduling.DayOffScheduling
{
	[TestFixture]
	public class BestSpotForAddingDayOffFinderTest
	{
		private IBestSpotForAddingDayOffFinder _target;

		[SetUp]
		public void Setup()
		{
			_target = new BestSpotForAddingDayOffFinder();
		}

		[Test]
		public void ShouldFindFirstFreeSpotBeforeBlockedContractDayOff()
		{
			IDictionary<DateOnly, IScheduleDayData> dataList = createList();
			dataList[new DateOnly(2013, 1, 3)].IsContractDayOff = true;
			dataList[new DateOnly(2013, 1, 3)].HaveRestriction = true;

			DateOnly? result = _target.Find(new List<IScheduleDayData>(dataList.Values));
			Assert.AreEqual(new DateOnly(2013, 1, 2), result.Value);
		}

		[Test]
		public void EmptyListShouldReturnNull()
		{
			DateOnly? result = _target.Find(new List<IScheduleDayData>());
			Assert.IsFalse(result.HasValue);
		}

		[Test]
		public void IfDayBeforeIsScheduledContinueSearch()
		{
			IDictionary<DateOnly, IScheduleDayData> dataList = createList();
			dataList[new DateOnly(2013, 1, 1)].IsScheduled = true;
			dataList[new DateOnly(2013, 1, 2)].IsScheduled = true;
			dataList[new DateOnly(2013, 1, 4)].IsScheduled = true;
			dataList[new DateOnly(2013, 1, 5)].IsContractDayOff = true;
			dataList[new DateOnly(2013, 1, 5)].IsScheduled = true;
			dataList[new DateOnly(2013, 1, 7)].IsContractDayOff = true;
			dataList[new DateOnly(2013, 1, 7)].IsScheduled = true;

			DateOnly? result = _target.Find(new List<IScheduleDayData>(dataList.Values));
			Assert.AreEqual(new DateOnly(2013, 1, 6), result.Value);
		}

		[Test]
		public void IfDayBeforeHasRestrictionContinueSearch()
		{
			IDictionary<DateOnly, IScheduleDayData> dataList = createList();
			dataList[new DateOnly(2013, 1, 1)].IsScheduled = true;
			dataList[new DateOnly(2013, 1, 2)].IsScheduled = true;
			dataList[new DateOnly(2013, 1, 4)].HaveRestriction = true;
			dataList[new DateOnly(2013, 1, 6)].HaveRestriction = true;
			dataList[new DateOnly(2013, 1, 7)].IsContractDayOff = true;
			dataList[new DateOnly(2013, 1, 7)].IsScheduled = true;

			DateOnly? result = _target.Find(new List<IScheduleDayData>(dataList.Values));
			Assert.AreEqual(new DateOnly(2013, 1, 5), result.Value);
		}

		[Test]
		public void IfAllDaysBeforeIsBlockedWeeShouldTryTwoDaysBefore()
		{
			IDictionary<DateOnly, IScheduleDayData> dataList = createList();
			dataList[new DateOnly(2013, 1, 1)].HaveRestriction = true;
			dataList[new DateOnly(2013, 1, 2)].HaveRestriction = true;
			//3:rd is free
			dataList[new DateOnly(2013, 1, 4)].HaveRestriction = true;
			dataList[new DateOnly(2013, 1, 5)].IsContractDayOff = true;
			dataList[new DateOnly(2013, 1, 5)].HaveRestriction = true;
			dataList[new DateOnly(2013, 1, 7)].HaveRestriction = true;
			dataList[new DateOnly(2013, 1, 8)].IsContractDayOff = true;
			dataList[new DateOnly(2013, 1, 8)].IsDayOff = true;

			DateOnly? result = _target.Find(new List<IScheduleDayData>(dataList.Values));
			Assert.AreEqual(new DateOnly(2013, 1, 3), result.Value);
		}

		[Test]
		public void ReturnDayIfIsContractDayOffAndNotBlocked()
		{
			IDictionary<DateOnly, IScheduleDayData> dataList = createList();
			dataList[new DateOnly(2013, 1, 6)].IsContractDayOff = true;
			dataList[new DateOnly(2013, 1, 7)].IsContractDayOff = true;
			dataList[new DateOnly(2013, 1, 7)].IsDayOff = true;

			DateOnly? result = _target.Find(new List<IScheduleDayData>(dataList.Values));
			Assert.AreEqual(new DateOnly(2013, 1, 6), result.Value);
		}

		private static IDictionary<DateOnly, IScheduleDayData> createList()
		{
			IDictionary<DateOnly, IScheduleDayData> dataList = new Dictionary<DateOnly, IScheduleDayData>();
			IScheduleDayData data;
			data = new ScheduleDayData(new DateOnly(2013, 1, 1));
			dataList.Add(data.DateOnly, data);
			data = new ScheduleDayData(new DateOnly(2013, 1, 2));
			dataList.Add(data.DateOnly, data);
			data = new ScheduleDayData(new DateOnly(2013, 1, 3));
			dataList.Add(data.DateOnly, data);
			data = new ScheduleDayData(new DateOnly(2013, 1, 4));
			dataList.Add(data.DateOnly, data);
			data = new ScheduleDayData(new DateOnly(2013, 1, 5));
			dataList.Add(data.DateOnly, data);
			data = new ScheduleDayData(new DateOnly(2013, 1, 6));
			dataList.Add(data.DateOnly, data);
			data = new ScheduleDayData(new DateOnly(2013, 1, 7));
			dataList.Add(data.DateOnly, data);
			data = new ScheduleDayData(new DateOnly(2013, 1, 8));
			dataList.Add(data.DateOnly, data);
			data = new ScheduleDayData(new DateOnly(2013, 1, 9));
			dataList.Add(data.DateOnly, data);
			data = new ScheduleDayData(new DateOnly(2013, 1, 10));
			dataList.Add(data.DateOnly, data);
			data = new ScheduleDayData(new DateOnly(2013, 1, 11));
			dataList.Add(data.DateOnly, data);
			data = new ScheduleDayData(new DateOnly(2013, 1, 12));
			dataList.Add(data.DateOnly, data);
			data = new ScheduleDayData(new DateOnly(2013, 1, 13));
			dataList.Add(data.DateOnly, data);
			data = new ScheduleDayData(new DateOnly(2013, 1, 14));
			dataList.Add(data.DateOnly, data);

			return dataList;
		}
	}
}