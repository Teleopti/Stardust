using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization.MatrixLockers;



namespace Teleopti.Ccc.DomainTest.Optimization
{
	[TestFixture]
	public class MatrixShiftsNotAvailibleLockerTest
	{
		private MockRepository _mocks;
		private MatrixShiftsNotAvailibleLocker _target;
		private IList<IScheduleMatrixPro> _matrixList;
		private IScheduleMatrixPro _matrix;
		private IScheduleDayPro _scheduleDayPro;
		private IPerson _person;
		private IPersonPeriod _personPeriod;
		private IRuleSetBag _ruleSetBag;
		private IWorkShiftRuleSet _ruleSet;
			
		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_target = new MatrixShiftsNotAvailibleLocker();
			_matrix = _mocks.StrictMock<IScheduleMatrixPro>();
			_matrixList = new List<IScheduleMatrixPro> {_matrix};
			_scheduleDayPro = _mocks.StrictMock<IScheduleDayPro>();
			_person = _mocks.StrictMock<IPerson>();
			_personPeriod = _mocks.StrictMock<IPersonPeriod>();
			_ruleSetBag = _mocks.StrictMock<IRuleSetBag>();
			_ruleSet = _mocks.StrictMock<IWorkShiftRuleSet>();
	
		}

		[Test]
		public void ShouldLockDaysWithNoAvailibleWorkShifts()
		{
			using (_mocks.Record())
			{
				commonMocks(false);
				Expect.Call(_ruleSet.IsValidDate(DateOnly.MinValue)).Return(false);
				Expect.Call(() => _matrix.LockDay(DateOnly.MinValue));
			}

			using (_mocks.Playback())
			{
				_target.Execute(_matrixList);
			}
		}

		[Test]
		public void ShouldoLockDaysWithAvailibleWorkShifts()
		{
			using (_mocks.Record())
			{
				commonMocks(false);
				Expect.Call(_ruleSet.IsValidDate(DateOnly.MinValue)).Return(true);
			}

			using (_mocks.Playback())
			{
				_target.Execute(_matrixList);
			}
		}

		[Test]
		public void ShouldHandleNullBag()
		{
			using (_mocks.Record())
			{
				commonMocks(true);
			}

			using (_mocks.Playback())
			{
				_target.Execute(_matrixList);
			}
		}

		private void commonMocks(bool nullBag)
		{
			Expect.Call(_matrix.Person).Return(_person);
			Expect.Call(_matrix.EffectivePeriodDays)
			      .Return(new [] {_scheduleDayPro});
			Expect.Call(_scheduleDayPro.Day).Return(DateOnly.MinValue);
			Expect.Call(_person.Period(DateOnly.MinValue)).Return(_personPeriod);
			
			if (nullBag)
			{
				Expect.Call(_personPeriod.RuleSetBag).Return(null);
				return;
			}

			Expect.Call(_personPeriod.RuleSetBag).Return(_ruleSetBag);
			Expect.Call(_ruleSetBag.RuleSetCollection)
			      .Return(new ReadOnlyCollection<IWorkShiftRuleSet>(new List<IWorkShiftRuleSet> {_ruleSet}));
		}
	}
}