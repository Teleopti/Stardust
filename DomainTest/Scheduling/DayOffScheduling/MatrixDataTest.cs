using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.DayOffScheduling;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Interfaces.Domain;

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
		private IEffectiveRestrictionCreator _effectiveRestrictionCreator;
		private IScheduleDay _scheduleDay1;
		private IScheduleDay _scheduleDay2;
		private ISchedulingOptions _schedulingOptions;
		private IEffectiveRestriction _effectiveRestriction;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_matrix = _mocks.StrictMock<IScheduleMatrixPro>();
			_scheduleDayPro1 = _mocks.StrictMock<IScheduleDayPro>();
			_scheduleDayPro2 = _mocks.StrictMock<IScheduleDayPro>();
			_effectiveRestrictionCreator = _mocks.StrictMock<IEffectiveRestrictionCreator>();
			_scheduleDay1 = _mocks.StrictMock<IScheduleDay>();
			_scheduleDay2 = _mocks.StrictMock<IScheduleDay>();
			_schedulingOptions = new SchedulingOptions();
			_effectiveRestriction = _mocks.StrictMock<IEffectiveRestriction>();
		}

		[Test]
		public void ShouldInitializeInConstructor()
		{
			using (_mocks.Record())
			{
				initializionMocks();
			}

			using (_mocks.Playback())
			{
				_target = new MatrixData(_matrix, _effectiveRestrictionCreator, _schedulingOptions);
				Assert.AreEqual(2, _target.ScheduleDayDatas.Count);
				Assert.AreSame(_matrix, _target.Matrix);
			}
		}


		[Test]
		public void ShouldExposeDatas()
		{
			using (_mocks.Record())
			{
				initializionMocks();
			}

			using (_mocks.Playback())
			{
				_target = new MatrixData(_matrix, _effectiveRestrictionCreator, _schedulingOptions);
				IScheduleDayData data = _target.ScheduleDayDatas[0];
				Assert.AreEqual(DateOnly.MinValue, data.Date);
				Assert.IsTrue(data.HaveRestriction);
				Assert.IsFalse(data.IsContractDayOff);
				Assert.IsTrue(data.IsDayOff);
				Assert.IsTrue(data.IsScheduled);
			}
		}

		[Test]
		public void ShouldExposeIndexKey()
		{
			using (_mocks.Record())
			{
				initializionMocks();
			}

			using (_mocks.Playback())
			{
				_target = new MatrixData(_matrix, _effectiveRestrictionCreator, _schedulingOptions);
				IScheduleDayData data = _target[DateOnly.MinValue.AddDays(1)];
				Assert.AreEqual(DateOnly.MinValue.AddDays(1), data.Date);
				Assert.IsFalse(data.HaveRestriction);
				Assert.IsFalse(data.IsContractDayOff);
				Assert.IsFalse(data.IsDayOff);
				Assert.IsFalse(data.IsScheduled);
			}
		}

		private void initializionMocks()
		{
			IList<IScheduleDayPro> matrixDays = new List<IScheduleDayPro>{ _scheduleDayPro1, _scheduleDayPro2 };

			Expect.Call(_matrix.EffectivePeriodDays).Return(new ReadOnlyCollection<IScheduleDayPro>(matrixDays));

			Expect.Call(_scheduleDayPro1.Day).Return(DateOnly.MinValue);
			Expect.Call(_scheduleDayPro1.DaySchedulePart()).Return(_scheduleDay1);
			Expect.Call(_scheduleDay1.IsScheduled()).Return(true);
			Expect.Call(_scheduleDay1.SignificantPart()).Return(SchedulePartView.DayOff);
			Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(_scheduleDay1, _schedulingOptions)).Return(
				_effectiveRestriction);
			Expect.Call(_effectiveRestriction.IsRestriction).Return(true);

			Expect.Call(_scheduleDayPro2.Day).Return(DateOnly.MinValue.AddDays(1));
			Expect.Call(_scheduleDayPro2.DaySchedulePart()).Return(_scheduleDay2);
			Expect.Call(_scheduleDay2.IsScheduled()).Return(false);
			Expect.Call(_scheduleDay2.SignificantPart()).Return(SchedulePartView.None);
			Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(_scheduleDay2, _schedulingOptions)).Return(
				_effectiveRestriction);
			Expect.Call(_effectiveRestriction.IsRestriction).Return(false);
		}
	}
}