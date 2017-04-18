using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts.Interfaces;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts.Models;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Shifts
{
    [TestFixture]
    public class ActivityTimeLimiterViewModelTest
    {
        private IActivityTimeLimiterViewModel _target;
        private ActivityTimeLimiter _limiter;
        private IWorkShiftRuleSet _ruleSet;

        [SetUp]
        public void Setup()
        {
            _ruleSet = WorkShiftRuleSetFactory.Create();
            _limiter = new ActivityTimeLimiter(ActivityFactory.CreateActivity("Adagena Nama"),
                                               TimeSpan.FromHours(1),
                                               OperatorLimiter.Equals);
            _target = new ActivityTimeLimiterViewModel(_ruleSet, _limiter);
        }

        [Test]
        public void VerifyConstructor()
        {
            Assert.IsNotNull(_target);
        }

        [Test]
        public void VerifyCanReadWriteProperties()
        {
            IActivity newActivity = ActivityFactory.CreateActivity("Illagena Kama");
            _target.TargetActivity = newActivity;
            Assert.AreEqual(newActivity, _target.TargetActivity);

            _target.Time = TimeSpan.FromMinutes(15);
            Assert.AreEqual(TimeSpan.FromMinutes(15), _target.Time);

            _target.Operator = LanguageResourceHelper.TranslateEnumValue(OperatorLimiter.Equals);
            Assert.AreEqual(LanguageResourceHelper.TranslateEnumValue(OperatorLimiter.Equals), _target.Operator);


        }
    }
}
