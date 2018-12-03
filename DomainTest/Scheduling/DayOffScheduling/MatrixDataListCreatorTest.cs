using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.DayOffScheduling;


namespace Teleopti.Ccc.DomainTest.Scheduling.DayOffScheduling
{
	[TestFixture]
	public class MatrixDataListCreatorTest
	{
		private MockRepository _mocks;
		private SchedulingOptions _schedulingOptions;
		private MatrixDataListCreator _target;
		private IScheduleMatrixPro _matrix;
		private IScheduleDayPro _scheduleDayPro1;
		private IScheduleDayDataMapper _scheduleDayDataMapper;
		private IVirtualSchedulePeriod _schedulePeriod;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_schedulingOptions = new SchedulingOptions();
			_matrix = _mocks.StrictMock<IScheduleMatrixPro>();
			_scheduleDayDataMapper = _mocks.StrictMock<IScheduleDayDataMapper>();
			_target = new MatrixDataListCreator(_scheduleDayDataMapper);
			_scheduleDayPro1 = _mocks.StrictMock<IScheduleDayPro>();
			_schedulePeriod = _mocks.StrictMock<IVirtualSchedulePeriod>();
		}

		[Test]
		public void ShouldReturnList()
		{
			using (_mocks.Record())
			{
				var matrixDays = new [] { _scheduleDayPro1 };

				Expect.Call(_matrix.FullWeeksPeriodDays).Return(matrixDays);
				Expect.Call(_scheduleDayDataMapper.Map(_scheduleDayPro1, _schedulingOptions)).Return(
					new ScheduleDayData(DateOnly.MinValue));
				Expect.Call(_matrix.SchedulePeriod).Return(_schedulePeriod);
				Expect.Call(_schedulePeriod.DaysOff()).Return(8);
			}

			using (_mocks.Playback())
			{
				IList<IMatrixData> result = _target.Create(new List<IScheduleMatrixPro> {_matrix}, _schedulingOptions);
				Assert.AreSame(_matrix, result[0].Matrix);
			}
		}
	}
}