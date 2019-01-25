using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.DomainTest.DayOffPlanning
{
	[TestFixture]
	public class FlexibelDayOffDecisionMakerTest
	{
		private FlexibelDayOffDecisionMaker _target;
		private MockRepository _mock;
		private IDayOffLegalStateValidator _validator;
		private IList<IDayOffLegalStateValidator> _validators;
		private ILockableBitArray _bitArray;

		[SetUp]
		public void SetUp()
		{
			_mock = new MockRepository();
			_validator = _mock.StrictMock<IDayOffLegalStateValidator>();
			_validators = new List<IDayOffLegalStateValidator> { _validator };
			_bitArray = new LockableBitArray(7, false, false);
			_target = new FlexibelDayOffDecisionMaker(_validators);
		}

		[Test]
		public void ShouldFindBestDayToAddAndRemove()
		{	
			using (_mock.Record())
			{
				Expect.Call(_validator.IsValid(_bitArray.ToLongBitArray(), 6)).Return(true).IgnoreArguments().Repeat.AtLeastOnce();   
			}

			using (_mock.Playback())
			{
				_bitArray.Set(0, true);
				_bitArray.Set(6, true);
				var values = new List<double?>{ -100, -32, 32, -1, -10, 100, 0};
				var result = _target.Execute(_bitArray, values);

				Assert.IsTrue(result);
				Assert.IsFalse(_bitArray[0]);
				Assert.IsFalse(_bitArray[1]);
				Assert.IsFalse(_bitArray[2]);
				Assert.IsFalse(_bitArray[3]);
				Assert.IsFalse(_bitArray[4]);
				Assert.IsTrue(_bitArray[5]);
				Assert.IsTrue(_bitArray[6]);
			}
		}

		[Test]
		public void ShouldSkipLockedDays()
		{
			using (_mock.Record())
			{
				Expect.Call(_validator.IsValid(_bitArray.ToLongBitArray(), 6)).Return(true).IgnoreArguments().Repeat.AtLeastOnce();
			}

			using (_mock.Playback())
			{
				_bitArray.Set(0, true);
				_bitArray.Set(1, true);
				_bitArray.Set(6, true);
				_bitArray.Lock(0, true);
				_bitArray.Lock(5, true);

				var values = new List<double?> { -100, -32, 32, -1, -10, 100, 0 };
				var result = _target.Execute(_bitArray, values);

				Assert.IsTrue(result);
				Assert.IsTrue(_bitArray[0]);
				Assert.IsFalse(_bitArray[1]);
				Assert.IsTrue(_bitArray[2]);
				Assert.IsFalse(_bitArray[3]);
				Assert.IsFalse(_bitArray[4]);
				Assert.IsFalse(_bitArray[5]);
				Assert.IsTrue(_bitArray[6]);
			}
		}

		[Test]
		public void ShouldNotRemoveWhenNoUnderStaffing()
		{
			using (_mock.Record())
			{
				Expect.Call(_validator.IsValid(_bitArray.ToLongBitArray(), 6)).Return(true).IgnoreArguments().Repeat.AtLeastOnce();
			}

			using (_mock.Playback())
			{
				_bitArray.Set(0, true);
				_bitArray.Set(1, true);
				_bitArray.Set(2, true);
				_bitArray.Set(3, true);
				_bitArray.Set(4, true);
				_bitArray.Set(5, true);
				var values = new List<double?> { 1, 2, 3, 4, 5, 6, 7 };
				var result = _target.Execute(_bitArray, values);

				Assert.IsTrue(result);
				Assert.IsTrue(_bitArray[0]);
				Assert.IsTrue(_bitArray[1]);
				Assert.IsTrue(_bitArray[2]);
				Assert.IsTrue(_bitArray[3]);
				Assert.IsTrue(_bitArray[4]);
				Assert.IsTrue(_bitArray[5]);
				Assert.IsTrue(_bitArray[6]);
			}
		}

		[Test]
		public void ShouldNotAddWhenNoOverStaffing()
		{
			using (_mock.Record())
			{
				Expect.Call(_validator.IsValid(_bitArray.ToLongBitArray(), 6)).Return(true).IgnoreArguments().Repeat.AtLeastOnce();
			}

			using (_mock.Playback())
			{
				_bitArray.Set(6, true);
				var values = new List<double?> { -1, -2, -3, -4, -5, -6, -7 };
				var result = _target.Execute(_bitArray, values);

				Assert.IsTrue(result);
				Assert.IsFalse(_bitArray[0]);
				Assert.IsFalse(_bitArray[1]);
				Assert.IsFalse(_bitArray[2]);
				Assert.IsFalse(_bitArray[3]);
				Assert.IsFalse(_bitArray[4]);
				Assert.IsFalse(_bitArray[5]);
				Assert.IsFalse(_bitArray[6]);
			}
		}

		[Test]
		public void ShouldReturnFalseWhenNoValid()
		{
			using (_mock.Record())
			{
				Expect.Call(_validator.IsValid(_bitArray.ToLongBitArray(), 6)).Return(false).IgnoreArguments().Repeat.AtLeastOnce();
			}

			using (_mock.Playback())
			{
				_bitArray.Set(6, true);
				var values = new List<double?> { -100, -32, 32, -1, -10, 100, 0 };
				var result = _target.Execute(_bitArray, values);

				Assert.IsFalse(result);
				Assert.IsFalse(_bitArray[0]);
				Assert.IsFalse(_bitArray[1]);
				Assert.IsFalse(_bitArray[2]);
				Assert.IsFalse(_bitArray[3]);
				Assert.IsFalse(_bitArray[4]);
				Assert.IsFalse(_bitArray[5]);
				Assert.IsTrue(_bitArray[6]);
			}
		}

	}
}
