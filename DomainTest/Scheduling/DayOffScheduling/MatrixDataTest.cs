using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.DayOffScheduling;
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
		private ISchedulingOptions _schedulingOptions;
		private IScheduleDayDataMapper _scheduleDayDataMapper;

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
		}

		[Test]
		public void ShouldStoreAndExposeCollection()
		{
			using (_mocks.Record())
			{
				IList<IScheduleDayPro> matrixDays = new List<IScheduleDayPro> { _scheduleDayPro1, _scheduleDayPro2 };
				Expect.Call(_matrix.EffectivePeriodDays).Return(new ReadOnlyCollection<IScheduleDayPro>(matrixDays));
				Expect.Call(_scheduleDayDataMapper.Map(_scheduleDayPro1, _schedulingOptions)).Return(
					new ScheduleDayData(DateOnly.MinValue));
				Expect.Call(_scheduleDayDataMapper.Map(_scheduleDayPro2, _schedulingOptions)).Return(
					new ScheduleDayData(DateOnly.MinValue.AddDays(1)));
			}

			using (_mocks.Playback())
			{
				_target.Store(_matrix, _schedulingOptions);
				Assert.AreEqual(2, _target.ScheduleDayDataCollection.Count);
				Assert.AreSame(_matrix, _target.Matrix);
			}
		}

		[Test]
		public void ShouldExposeIndexKey()
		{
			using (_mocks.Record())
			{
				IList<IScheduleDayPro> matrixDays = new List<IScheduleDayPro> { _scheduleDayPro1, _scheduleDayPro2 };
				Expect.Call(_matrix.EffectivePeriodDays).Return(new ReadOnlyCollection<IScheduleDayPro>(matrixDays));
				Expect.Call(_scheduleDayDataMapper.Map(_scheduleDayPro1, _schedulingOptions)).Return(
					new ScheduleDayData(DateOnly.MinValue));
				Expect.Call(_scheduleDayDataMapper.Map(_scheduleDayPro2, _schedulingOptions)).Return(
					new ScheduleDayData(DateOnly.MinValue.AddDays(1)));
			}

			using (_mocks.Playback())
			{
				_target.Store(_matrix, _schedulingOptions);
				IScheduleDayData data = _target[DateOnly.MinValue.AddDays(1)];
				Assert.AreEqual(DateOnly.MinValue.AddDays(1), data.DateOnly);
			}
		}

        [Test]
        public void ShouldFindTheKey()
        {
            using (_mocks.Record())
            {
                IList<IScheduleDayPro> matrixDays = new List<IScheduleDayPro> { _scheduleDayPro1, _scheduleDayPro2 };
                Expect.Call(_matrix.EffectivePeriodDays).Return(new ReadOnlyCollection<IScheduleDayPro>(matrixDays));
                Expect.Call(_scheduleDayDataMapper.Map(_scheduleDayPro1, _schedulingOptions)).Return(
                    new ScheduleDayData(DateOnly.MinValue));
                Expect.Call(_scheduleDayDataMapper.Map(_scheduleDayPro2, _schedulingOptions)).Return(
                    new ScheduleDayData(DateOnly.MinValue.AddDays(1)));
            }

            using (_mocks.Playback())
            {
                _target.Store(_matrix, _schedulingOptions);
                var keyExists = _target.ContainsKey( DateOnly.MinValue.AddDays(1));
                Assert.IsTrue(keyExists);
            }
        }
	}
}