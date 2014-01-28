using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.DayOffPlanning;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Interfaces.Domain;


namespace Teleopti.Ccc.DomainTest.Optimization
{
	[TestFixture]
	public class DayOffRulesValidatorTest
	{
		private MockRepository _mocks;
		private IDayOffRulesValidator _target;
		private IDaysOffLegalStateValidatorsFactory _daysOffLegalStateValidatorsFactory;
		private IScheduleMatrixLockableBitArrayConverterEx _scheduleMatrixLockableBitArrayConverterEx;
		private IScheduleMatrixPro _matrix;
		private IOptimizationPreferences _optimizationPreferences;
		private IDayOffLegalStateValidator _dayOffLegalStateValidator;
		private IList<IDayOffLegalStateValidator> _dayOffLegalStateValidators;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_daysOffLegalStateValidatorsFactory = _mocks.StrictMock<IDaysOffLegalStateValidatorsFactory>();
			_scheduleMatrixLockableBitArrayConverterEx = _mocks.StrictMock<IScheduleMatrixLockableBitArrayConverterEx>();
			_target = new DayOffRulesValidator(_daysOffLegalStateValidatorsFactory, _scheduleMatrixLockableBitArrayConverterEx);
			_matrix = _mocks.StrictMock<IScheduleMatrixPro>();
			_optimizationPreferences = new OptimizationPreferences();
			_dayOffLegalStateValidator = _mocks.StrictMock<IDayOffLegalStateValidator>();
			_dayOffLegalStateValidators = new List<IDayOffLegalStateValidator>{_dayOffLegalStateValidator};
		}

		[Test]
		public void ShouldCallValidators()
		{
			ILockableBitArray array = new LockableBitArray(21, false, false, null);
			array.Set(7, true);
			BitArray longBitArray = array.ToLongBitArray();
			using (_mocks.Record())
			{
				Expect.Call(_scheduleMatrixLockableBitArrayConverterEx.Convert(_matrix,
				                                                               _optimizationPreferences.DaysOff.ConsiderWeekBefore,
				                                                               _optimizationPreferences.DaysOff.ConsiderWeekAfter))
				      .Return(array);
				Expect.Call(_daysOffLegalStateValidatorsFactory.CreateLegalStateValidators(array, _optimizationPreferences))
				      .Return(_dayOffLegalStateValidators);
				Expect.Call(_dayOffLegalStateValidator.IsValid(longBitArray, 14)).Return(true);
			}

			using (_mocks.Playback())
			{
				var result = _target.Validate(_matrix, _optimizationPreferences);
				Assert.IsTrue(result);
			}
		}

		[Test]
		public void ShouldReturnFalseIfAnyValidatorFails()
		{
			ILockableBitArray array = new LockableBitArray(21, false, false, null);
			array.Set(7, true);
			BitArray longBitArray = array.ToLongBitArray();
			using (_mocks.Record())
			{
				Expect.Call(_scheduleMatrixLockableBitArrayConverterEx.Convert(_matrix,
																			   _optimizationPreferences.DaysOff.ConsiderWeekBefore,
																			   _optimizationPreferences.DaysOff.ConsiderWeekAfter))
					  .Return(array);
				Expect.Call(_daysOffLegalStateValidatorsFactory.CreateLegalStateValidators(array, _optimizationPreferences))
					  .Return(_dayOffLegalStateValidators);
				Expect.Call(_dayOffLegalStateValidator.IsValid(longBitArray, 14)).Return(false);
			}

			using (_mocks.Playback())
			{
				var result = _target.Validate(_matrix, _optimizationPreferences);
				Assert.IsFalse(result);
			}
		}
	}
}