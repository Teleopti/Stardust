using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.DayOffScheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.DayOffScheduling
{
	[TestFixture]
	public class MatrixDataListCreatorTest
	{
		private MockRepository _mocks;
		private ISchedulingOptions _schedulingOptions;
		private IMatrixDataListCreator _target;
		private IScheduleMatrixPro _matrix;
		private IScheduleDayPro _scheduleDayPro1;
		private IEffectiveRestrictionCreator _effectiveRestrictionCreator;
		private IScheduleDay _scheduleDay1;
		private IEffectiveRestriction _effectiveRestriction;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_effectiveRestrictionCreator = _mocks.StrictMock<IEffectiveRestrictionCreator>();
			_schedulingOptions = new SchedulingOptions();
			_matrix = _mocks.StrictMock<IScheduleMatrixPro>();
			_target = new MatrixDataListCreator(_effectiveRestrictionCreator);
			_scheduleDayPro1 = _mocks.StrictMock<IScheduleDayPro>();
			_scheduleDay1 = _mocks.StrictMock<IScheduleDay>();
			_effectiveRestriction = _mocks.StrictMock<IEffectiveRestriction>();
		}

		[Test]
		public void ShouldReturnList()
		{
			using (_mocks.Record())
			{
				IList<IScheduleDayPro> matrixDays = new List<IScheduleDayPro> { _scheduleDayPro1 };

				Expect.Call(_matrix.EffectivePeriodDays).Return(new ReadOnlyCollection<IScheduleDayPro>(matrixDays));

				Expect.Call(_scheduleDayPro1.Day).Return(DateOnly.MinValue);
				Expect.Call(_scheduleDayPro1.DaySchedulePart()).Return(_scheduleDay1);
				Expect.Call(_scheduleDay1.IsScheduled()).Return(true);
				Expect.Call(_scheduleDay1.SignificantPart()).Return(SchedulePartView.DayOff);
				Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(_scheduleDay1, _schedulingOptions)).Return(
					_effectiveRestriction);
				Expect.Call(_effectiveRestriction.IsRestriction).Return(true);
			}

			using (_mocks.Playback())
			{
				IList<IMatrixData> result = _target.Create(new List<IScheduleMatrixPro> {_matrix}, _schedulingOptions);
				Assert.AreSame(_matrix, result[0].Matrix);
			}
		}
	}
}