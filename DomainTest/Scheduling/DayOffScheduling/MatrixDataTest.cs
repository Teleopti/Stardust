using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.DayOffScheduling;


namespace Teleopti.Ccc.DomainTest.Scheduling.DayOffScheduling
{
	[TestFixture]
	public class MatrixDataTest
	{
		private MockRepository _mocks;
		private IScheduleMatrixPro _matrix;
		private IMatrixData _target;
		private IScheduleDayPro _scheduleDayPro1;
		private IScheduleDayPro _scheduleDayPro2;
		private SchedulingOptions _schedulingOptions;
		private IScheduleDayDataMapper _scheduleDayDataMapper;
		private IVirtualSchedulePeriod _schedulePeriod;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_matrix = _mocks.StrictMock<IScheduleMatrixPro>();
			_scheduleDayPro1 = _mocks.StrictMock<IScheduleDayPro>();
			_scheduleDayPro2 = _mocks.StrictMock<IScheduleDayPro>();
			_schedulingOptions = new SchedulingOptions();
			_scheduleDayDataMapper = _mocks.StrictMock<IScheduleDayDataMapper>();
			_target = new MatrixData(_scheduleDayDataMapper);
			_schedulePeriod = _mocks.StrictMock<IVirtualSchedulePeriod>();
		}

		[Test]
		public void ShouldStoreAndExposeCollection()
		{
			using (_mocks.Record())
			{
				commonMocks();
			}

			using (_mocks.Playback())
			{
				_target.Store(_matrix, _schedulingOptions);
				Assert.AreEqual(2, _target.ScheduleDayDataCollection.Count);
				Assert.AreSame(_matrix, _target.Matrix);
			}
		}

		private void commonMocks()
		{
			var matrixDays = new [] {_scheduleDayPro1, _scheduleDayPro2};
			Expect.Call(_matrix.SchedulePeriod).Return(_schedulePeriod);
			Expect.Call(_schedulePeriod.DaysOff()).Return(8);
			Expect.Call(_matrix.FullWeeksPeriodDays).Return(matrixDays);
			Expect.Call(_scheduleDayDataMapper.Map(_scheduleDayPro1, _schedulingOptions)).Return(
				new ScheduleDayData(DateOnly.MinValue));
			Expect.Call(_scheduleDayDataMapper.Map(_scheduleDayPro2, _schedulingOptions)).Return(
				new ScheduleDayData(DateOnly.MinValue.AddDays(1)));
		}

		[Test]
		public void ShouldExposeTargetDaysOff()
		{
			using (_mocks.Record())
			{
				commonMocks();
			}

			using (_mocks.Playback())
			{
				_target.Store(_matrix, _schedulingOptions);
				Assert.AreEqual(8, _target.TargetDaysOff);
			}
		}
	}
}