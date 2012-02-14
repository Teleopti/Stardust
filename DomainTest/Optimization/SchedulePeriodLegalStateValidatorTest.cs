using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
    [TestFixture]
    public class SchedulePeriodLegalStateValidatorTest
    {

        #region Variables 

        private MockRepository _mockRepository;
        private SchedulePeriodDayOffLegalStateValidator _target;
        private IDayOffLegalStateValidator _dayOffLegalStateValidator1;
        private IDayOffLegalStateValidator _dayOffLegalStateValidator2;
        private MinMax<int> _periodRange;

        #endregion

        [SetUp]
        public void Setup()
        {
            _mockRepository = new MockRepository();
            _dayOffLegalStateValidator1 = _mockRepository.StrictMock<IDayOffLegalStateValidator>();
            _dayOffLegalStateValidator2 = _mockRepository.StrictMock<IDayOffLegalStateValidator>();
            IList<IDayOffLegalStateValidator> validatorList = new List<IDayOffLegalStateValidator> { _dayOffLegalStateValidator1, _dayOffLegalStateValidator2 };
            _periodRange = new MinMax<int>(10, 23);
            _target = new SchedulePeriodDayOffLegalStateValidator(validatorList, _periodRange);
        }

        [Test]
        public void VerifyIsValidWithAllValidatorValid()
        {
            BitArray array = createBitArrayForTest();
            using (_mockRepository.Record())
            {
                Expect.Call(_dayOffLegalStateValidator1.IsValid(array, 11)).Return(true).Repeat.Once();
                Expect.Call(_dayOffLegalStateValidator2.IsValid(array, 11)).Return(true).Repeat.Once();
                Expect.Call(_dayOffLegalStateValidator1.IsValid(array, 12)).Return(true).Repeat.Once();
                Expect.Call(_dayOffLegalStateValidator2.IsValid(array, 12)).Return(true).Repeat.Once();
                Expect.Call(_dayOffLegalStateValidator1.IsValid(array, 17)).Return(true).Repeat.Once();
                Expect.Call(_dayOffLegalStateValidator2.IsValid(array, 17)).Return(true).Repeat.Once();
            }
            using (_mockRepository.Playback())
            {
                Assert.IsTrue(_target.Validate(array));
            }
        }

        [Test]
        public void VerifyIsValidWithOneValidatorInvalid()
        {
            BitArray array = createBitArrayForTest();
            using (_mockRepository.Record())
            {
                Expect.Call(_dayOffLegalStateValidator1.IsValid(array, 11)).Return(true).Repeat.Once();
                Expect.Call(_dayOffLegalStateValidator2.IsValid(array, 11)).Return(true).Repeat.Once();
                Expect.Call(_dayOffLegalStateValidator1.IsValid(array, 12)).Return(true).Repeat.Once();
                Expect.Call(_dayOffLegalStateValidator2.IsValid(array, 12)).Return(true).Repeat.Once();
                Expect.Call(_dayOffLegalStateValidator1.IsValid(array, 17)).Return(false).Repeat.Once();
            }
            using (_mockRepository.Playback())
            {
                Assert.IsFalse(_target.Validate(array));
            }
        }

        [Test]
        public void VerifyIsValidWithNoActiveValidator()
        {
            BitArray array = createBitArrayForTest();
            _target = new SchedulePeriodDayOffLegalStateValidator(new List<IDayOffLegalStateValidator>(), _periodRange);
            Assert.IsTrue(_target.Validate(array));
        }

        private static BitArray createBitArrayForTest()
        {
            bool[] values = new bool[]
                                {
                                    true,
                                    true,
                                    false,
                                    false,
                                    false,
                                    false,
                                    false,

                                    false,
                                    true,
                                    false,
                                    false,
                                    true,
                                    true,
                                    false,

                                    false,
                                    false,
                                    false,
                                    true,
                                    false,
                                    false,
                                    false,

                                    false,
                                    false,
                                    false,
                                    false,
                                    true,
                                    true,
                                    true, 

                                    false,
                                    false,
                                    false,
                                    false,
                                    false,
                                    false,
                                    true


                                };
            BitArray result = new BitArray(values);
            return result;
        }
    }
}
