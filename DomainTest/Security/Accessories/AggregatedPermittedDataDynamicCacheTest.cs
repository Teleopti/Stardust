using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Security.Accessories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Security.Accessories
{
    [TestFixture]
    public class AggregatedPermittedDataDynamicCacheTest
    {
        private IAggregatedPermittedDataDynamicCache _target;
        private CalculateAggregatedPermittedDataMethod _calculateAggregatedPermittedDataMethod;
        private MockRepository _mocks;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _calculateAggregatedPermittedDataMethod = _mocks.StrictMock<CalculateAggregatedPermittedDataMethod>();
            _target = new AggregatedPermittedDataDynamicCache(_calculateAggregatedPermittedDataMethod);
            _target.Enabled = true;
        }

        [Test]
        public void VerifyEnabled()
        {
            bool setValue = false;
            _target.Enabled = setValue;
            Assert.AreEqual(setValue, _target.Enabled);

            setValue = true;
            _target.Enabled = setValue;
            Assert.AreEqual(setValue, _target.Enabled);
        }

        [Test]
        public void VerifyAggregatedPermittedDataWithOneCall()
        {
            IApplicationFunction function = ApplicationFunctionFactory.CreateApplicationFunction("F1");
            IAvailableData availableData = _mocks.StrictMock<IAvailableData>();

            _mocks.Record();

            Expect.Call(_calculateAggregatedPermittedDataMethod.Invoke(function)).Return(availableData).Repeat.Once();

            _mocks.ReplayAll();

            _target.AggregatedPermittedData(function);

            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyAggregatedPermittedDataWithTwoCallsSameFunction()
        {
            IApplicationFunction function = ApplicationFunctionFactory.CreateApplicationFunction("F1");
            IAvailableData availableData = _mocks.StrictMock<IAvailableData>();

            _mocks.Record();

            Expect.Call(_calculateAggregatedPermittedDataMethod.Invoke(function)).Return(availableData).Repeat.Once();

            _mocks.ReplayAll();

            _target.AggregatedPermittedData(function);
            _target.AggregatedPermittedData(function);

            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyAggregatedPermittedDataWithTwoCallsDifferentFunction()
        {
            IApplicationFunction function = ApplicationFunctionFactory.CreateApplicationFunction("F1");
            IApplicationFunction anotherFunction = ApplicationFunctionFactory.CreateApplicationFunction("F2"); ;
            IAvailableData availableData = _mocks.StrictMock<IAvailableData>();

            _mocks.Record();

            Expect.Call(_calculateAggregatedPermittedDataMethod.Invoke(function)).Return(availableData).Repeat.Once();
            Expect.Call(_calculateAggregatedPermittedDataMethod.Invoke(anotherFunction)).Return(availableData).Repeat.Once();

            _mocks.ReplayAll();

            _target.AggregatedPermittedData(function);
            _target.AggregatedPermittedData(anotherFunction);

            _mocks.VerifyAll();
        }

    }
}
