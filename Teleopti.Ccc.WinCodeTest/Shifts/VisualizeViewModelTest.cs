using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.DomainTest.FakeData;
using Teleopti.Ccc.WinCode.Shifts;
using Teleopti.Ccc.WinCode.Shifts.Interfaces;
using Teleopti.Ccc.WinCode.Shifts.Models;
using Teleopti.Interfaces.Domain;
using Rhino.Mocks;

namespace Teleopti.Ccc.WinCodeTest.Shifts
{
    [TestFixture]
    public class VisualizeViewModelTest
    {
        private MockRepository _mock;
        private IVisualizeViewModel _target;
        private IWorkShiftRuleSet _ruleSet;
        private IVisualLayerCollection _visualLayerCollection;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            var projSvc = _mock.CreateMock<IRuleSetProjectionService>();
            _ruleSet = _mock.CreateMock<IWorkShiftRuleSet>();

            _visualLayerCollection = VisualLayerCollectionFactory.CreateForWorkShift(new Person(), TimeSpan.FromHours(8), TimeSpan.FromHours(17));

            using(_mock.Record())
            {
                Expect.Call(projSvc.ProjectionCollection(_ruleSet))
                    .Return(new List<IVisualLayerCollection> { _visualLayerCollection });
            }
            _mock.ReplayAll();

            _target = new VisualizeViewModel(projSvc, _ruleSet);
        }

        [Test]
        public void VerifyConstructor()
        {
            Assert.AreEqual(_visualLayerCollection.Count(), _target.PayloadInfo.Count);
        }
    }
}
