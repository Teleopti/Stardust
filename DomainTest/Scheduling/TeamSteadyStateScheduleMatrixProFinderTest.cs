using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling
{
	[TestFixture]
	public class TeamSteadyStateScheduleMatrixProFinderTest
	{
		private TeamSteadyStateScheduleMatrixProFinder _target;
		private MockRepository _mocks;
		private IScheduleMatrixPro _scheduleMatrixPro;
		private IList<IScheduleMatrixPro> _matrixes;
		private IScheduleDay _scheduleDay;
		private IPerson _person1;
		private IPerson _person2;
		private IVirtualSchedulePeriod _virtualSchedulePeriod;
		private DateOnlyPeriod _dateOnlyPeriod;
		private IDateOnlyAsDateTimePeriod _dateOnlyAsDateTimePeriod;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_scheduleMatrixPro = _mocks.StrictMock<IScheduleMatrixPro>();
			_matrixes = new List<IScheduleMatrixPro>{_scheduleMatrixPro};
			_scheduleDay = _mocks.StrictMock<IScheduleDay>();
			_person1 = _mocks.StrictMock<IPerson>();
			_person2 = _mocks.StrictMock<IPerson>();
			_virtualSchedulePeriod = _mocks.StrictMock<IVirtualSchedulePeriod>();
			_dateOnlyPeriod = new DateOnlyPeriod(2012, 1, 1, 2012, 1, 2);
			_dateOnlyAsDateTimePeriod = _mocks.StrictMock<IDateOnlyAsDateTimePeriod>();
			_target = new TeamSteadyStateScheduleMatrixProFinder();
		}

		[Test]
		public void ShouldFindMatrix()
		{
			using(_mocks.Record())
			{
				Expect.Call(_scheduleMatrixPro.Person).Return(_person1);
				Expect.Call(_scheduleDay.Person).Return(_person1);
				Expect.Call(_scheduleMatrixPro.SchedulePeriod).Return(_virtualSchedulePeriod);
				Expect.Call(_virtualSchedulePeriod.DateOnlyPeriod).Return(_dateOnlyPeriod);
				Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod);
				Expect.Call(_dateOnlyAsDateTimePeriod.DateOnly).Return(new DateOnly(2012, 1, 1));
			}

			using(_mocks.Playback())
			{
				var matrix = _target.MatrixPro(_matrixes, _scheduleDay);
				Assert.IsNotNull(matrix);
			}		
		}

		[Test]
		public void ShouldNotFindMatrixWhenPersonDoNotMatch()
		{
			using (_mocks.Record())
			{
				Expect.Call(_scheduleMatrixPro.Person).Return(_person1);
				Expect.Call(_scheduleDay.Person).Return(_person2);
			}

			using (_mocks.Playback())
			{
				var matrix = _target.MatrixPro(_matrixes, _scheduleDay);
				Assert.IsNull(matrix);
			}	
		}

		[Test]
		public void ShouldNotFindMatrixWhenDateDoNotMatch()
		{
			using (_mocks.Record())
			{
				Expect.Call(_scheduleMatrixPro.Person).Return(_person1);
				Expect.Call(_scheduleDay.Person).Return(_person1);
				Expect.Call(_scheduleMatrixPro.SchedulePeriod).Return(_virtualSchedulePeriod);
				Expect.Call(_virtualSchedulePeriod.DateOnlyPeriod).Return(_dateOnlyPeriod);
				Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod);
				Expect.Call(_dateOnlyAsDateTimePeriod.DateOnly).Return(new DateOnly(2012, 1, 22));
			}

			using (_mocks.Playback())
			{
				var matrix = _target.MatrixPro(_matrixes, _scheduleDay);
				Assert.IsNull(matrix);
			}		
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void ShouldThrowExceptionIfMatrixesIsNull()
		{
			_target.MatrixPro(null, _scheduleDay);
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void ShouldThrowExceptionIfScheduleDayIsNull()
		{
			_target.MatrixPro(_matrixes, null);
		}
	}
}
