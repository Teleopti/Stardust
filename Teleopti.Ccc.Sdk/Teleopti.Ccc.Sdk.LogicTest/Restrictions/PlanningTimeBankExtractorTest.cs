using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Sdk.Logic.Restrictions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.LogicTest.Restrictions
{
    [TestFixture]
    public class PlanningTimeBankExtractorTest
    {
        private MockRepository _mocks;
        private IPerson _person;
        private PlanningTimeBankExtractor _target;
        private DateOnly _dateOnly;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _person = _mocks.StrictMock<IPerson>();
            _target = new PlanningTimeBankExtractor(_person);
            _dateOnly = new DateOnly(2011, 2, 16);
        }

        [Test]
        public void ShouldSetFalseOnEditableWhenVirtualPeriodIsNotValid()
        {
            var virtualPeriod = _mocks.StrictMock<IVirtualSchedulePeriod>();
            Expect.Call(_person.VirtualSchedulePeriod(_dateOnly)).Return(virtualPeriod);
            Expect.Call(virtualPeriod.IsValid).Return(false);
            _mocks.ReplayAll();
            var result = _target.GetPlanningTimeBank(_dateOnly);
            Assert.That(result.IsEditable,Is.False);
            _mocks.VerifyAll();
        }
        [Test]
        public void ShouldSetEditableWhenFirstSchedulePeriod()
        {
            var contract = _mocks.StrictMock<IContract>();
            var virtualPeriod = _mocks.StrictMock<IVirtualSchedulePeriod>();
            var partTimePercentage = _mocks.StrictMock<IPartTimePercentage>(); 
            Expect.Call(_person.VirtualSchedulePeriod(_dateOnly)).Return(virtualPeriod);
            Expect.Call(virtualPeriod.IsValid).Return(true);
            Expect.Call(virtualPeriod.Contract).Return(contract);
            Expect.Call(contract.PlanningTimeBankMax).Return(TimeSpan.FromMinutes(10*60));
            Expect.Call(contract.PlanningTimeBankMin).Return(TimeSpan.FromMinutes(-5*60));
            Expect.Call(virtualPeriod.BalanceIn).Return(TimeSpan.FromHours(-2));
            Expect.Call(virtualPeriod.BalanceOut).Return(TimeSpan.FromHours(0));
            Expect.Call(virtualPeriod.IsOriginalPeriod()).Return(true);
            Expect.Call(virtualPeriod.Seasonality).Return(new Percent(0));
            Expect.Call(virtualPeriod.PeriodTarget()).Return(TimeSpan.FromMinutes(100));
            Expect.Call(virtualPeriod.PartTimePercentage).Return(partTimePercentage);
            Expect.Call(partTimePercentage.Percentage).Return(new Percent(1));
            Expect.Call(contract.AdjustTimeBankWithPartTimePercentage).Return(true);
            Expect.Call(contract.AdjustTimeBankWithSeasonality).Return(true);
            _mocks.ReplayAll();
            var result =_target.GetPlanningTimeBank(_dateOnly);
            Assert.That(result.IsEditable,Is.True);
            _mocks.VerifyAll();
        }
    }

    
}