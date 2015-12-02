using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling.DayOffScheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.DayOffScheduling
{
	[TestFixture]
	public class MatrixDataListInSteadyStateTest
	{
		private IMatrixDataListInSteadyState _target;
		
		[SetUp]
		public void Setup()
		{
			_target = new MatrixDataListInSteadyState();
		}

		[Test]
		public void ShouldReturnTrueIfAllDaysOffIsSame()
		{
			IDictionary<DateOnly, IScheduleDayData> dataList = createList();
			dataList[new DateOnly(2013, 1, 3)].IsDayOff = true;
			IMatrixData matrixData = new MatrixDataForTest(dataList.Values);
			IList<IMatrixData> matrixDataList = new List<IMatrixData>{ matrixData };
			bool result = _target.IsListInSteadyState(matrixDataList);
			Assert.IsTrue(result);
		}

		[Test]
		public void ShouldReturnFalseIfItem1HaveMoreDaysOff()
		{
			IDictionary<DateOnly, IScheduleDayData> dataList;

			dataList = createList();
			dataList[new DateOnly(2013, 1, 3)].IsDayOff = true;
			dataList[new DateOnly(2013, 1, 4)].IsDayOff = true;
			IMatrixData matrixData1 = new MatrixDataForTest(dataList.Values);

			dataList = createList();
			dataList[new DateOnly(2013, 1, 3)].IsDayOff = true;
			IMatrixData matrixData2 = new MatrixDataForTest(dataList.Values);

			IList<IMatrixData> matrixDataList = new List<IMatrixData> { matrixData1, matrixData2 };
			bool result = _target.IsListInSteadyState(matrixDataList);
			Assert.IsFalse(result);
		}

		[Test]
		public void ShouldReturnFalseIfItem2HaveMoreDaysOff()
		{
			IDictionary<DateOnly, IScheduleDayData> dataList;

			dataList = createList();
			dataList[new DateOnly(2013, 1, 3)].IsDayOff = true;
			
			IMatrixData matrixData1 = new MatrixDataForTest(dataList.Values);

			dataList = createList();
			dataList[new DateOnly(2013, 1, 3)].IsDayOff = true;
			dataList[new DateOnly(2013, 1, 4)].IsDayOff = true;
			IMatrixData matrixData2 = new MatrixDataForTest(dataList.Values);

			IList<IMatrixData> matrixDataList = new List<IMatrixData> { matrixData1, matrixData2 };
			bool result = _target.IsListInSteadyState(matrixDataList);
			Assert.IsFalse(result);
		}

		[Test]
		public void ShouldReturnFalseSameNumberOfDaysOffButAtDifferentPlaces()
		{
			IDictionary<DateOnly, IScheduleDayData> dataList;

			dataList = createList();
			dataList[new DateOnly(2013, 1, 3)].IsDayOff = true;

			IMatrixData matrixData1 = new MatrixDataForTest(dataList.Values);

			dataList = createList();
			dataList[new DateOnly(2013, 1, 4)].IsDayOff = true;
			IMatrixData matrixData2 = new MatrixDataForTest(dataList.Values);

			IList<IMatrixData> matrixDataList = new List<IMatrixData> { matrixData1, matrixData2 };
			bool result = _target.IsListInSteadyState(matrixDataList);
			Assert.IsFalse(result);
		}

		[Test]
		public void ShouldReturnFalseIfItemsHaveDifferentPeriods()
		{
			IDictionary<DateOnly, IScheduleDayData> dataList;

			dataList = createList();
			dataList[new DateOnly(2013, 1, 3)].IsDayOff = true;

			IMatrixData matrixData1 = new MatrixDataForTest(dataList.Values);

			dataList = createList();
			dataList[new DateOnly(2013, 1, 3)].IsDayOff = true;
			dataList.Remove(new DateOnly(2013, 1, 10));
			IMatrixData matrixData2 = new MatrixDataForTest(dataList.Values);

			IList<IMatrixData> matrixDataList = new List<IMatrixData> { matrixData1, matrixData2 };
			bool result = _target.IsListInSteadyState(matrixDataList);
			Assert.IsFalse(result);
		}

		[Test]
		public void ShoulReturnFalseIfNotSameTargetDaysOff()
		{
			IDictionary<DateOnly, IScheduleDayData> dataList = createList();
			dataList[new DateOnly(2013, 1, 3)].IsDayOff = true;
			MatrixDataForTest matrixData = new MatrixDataForTest(dataList.Values);
			matrixData.SetTargetDaysOff(8);
			MatrixDataForTest matrixData1 = new MatrixDataForTest(dataList.Values);
			matrixData.SetTargetDaysOff(7);
			IList<IMatrixData> matrixDataList = new List<IMatrixData> { matrixData, matrixData1 };
			bool result = _target.IsListInSteadyState(matrixDataList);
			Assert.IsFalse(result);
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

			return dataList;
		}
	}

	public class MatrixDataForTest : MatrixData
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public MatrixDataForTest(IEnumerable<IScheduleDayData> scheduleDayDataCollectionForTest)
			: base(null)
		{
			foreach (var scheduleDayData in scheduleDayDataCollectionForTest)
			{
				ScheduleDayDataDictionary.Add(scheduleDayData.DateOnly, scheduleDayData);
			}

		}

		public void SetTargetDaysOff(int value)
		{
			TargetDaysOff = value;
		}

		public void SetDayOff(DateOnly date)
		{
			ScheduleDayDataDictionary[date].IsDayOff = true;
		}
	}
}