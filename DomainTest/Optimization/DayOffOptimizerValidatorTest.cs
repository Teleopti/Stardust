using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling.Rules;


namespace Teleopti.Ccc.DomainTest.Optimization
{
    [TestFixture]
    public class DayOffOptimizerValidatorTest
    {
        private DayOffOptimizerValidator _target;
        private MockRepository _mock;
        private IScheduleMatrixPro _scheduleMatrixPro;
        private INewDayOffRule _dayOffRule;
        private DateOnly _dateOnly;
        private IDictionary<IPerson, IScheduleRange> _dictionary;
        private IScheduleDayPro _scheduleDayPro;
        private IScheduleDay _scheduleDay;
        private IBusinessRuleResponse _businessRuleResponse;
        private IScheduleRange _scheduleRange;
        private IPerson _person;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _scheduleMatrixPro = _mock.StrictMock<IScheduleMatrixPro>();
            _dayOffRule = _mock.StrictMock<INewDayOffRule>();
            _scheduleRange = _mock.StrictMock<IScheduleRange>();
            _person = _mock.StrictMock<IPerson>();
            _dictionary = new Dictionary<IPerson, IScheduleRange>{ { _person, _scheduleRange } };
            
            _target = new DayOffOptimizerValidator(_dayOffRule);
            _dateOnly = new DateOnly(2011, 1, 10);
            _scheduleDayPro = _mock.StrictMock<IScheduleDayPro>();
            _scheduleDay = _mock.StrictMock<IScheduleDay>();
            _businessRuleResponse = _mock.StrictMock<IBusinessRuleResponse>();
        }

        [Test]
        public void ShouldValidate()
        {
            using(_mock.Record())
            {
                Expect.Call(_scheduleMatrixPro.ActiveScheduleRange).Return(_scheduleRange);
                Expect.Call(_scheduleMatrixPro.Person).Return(_person);
                Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(_dateOnly)).Return(_scheduleDayPro);
                Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay);
                Expect.Call(_dayOffRule.Validate(_dictionary, new List<IScheduleDay> { _scheduleDay })).Return(new List<IBusinessRuleResponse>());
            }

            using(_mock.Playback())
            {
                var result = _target.Validate(_dateOnly, _scheduleMatrixPro);
                Assert.IsTrue(result);
            }
        }

        [Test]
        public void ShouldNotValidate()
        {
            using (_mock.Record())
            {
                Expect.Call(_scheduleMatrixPro.ActiveScheduleRange).Return(_scheduleRange);
                Expect.Call(_scheduleMatrixPro.Person).Return(_person);
                Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(_dateOnly)).Return(_scheduleDayPro);
                Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay);
                Expect.Call(_dayOffRule.Validate(_dictionary, new List<IScheduleDay> { _scheduleDay })).Return(new List<IBusinessRuleResponse> { _businessRuleResponse });
            }

            using (_mock.Playback())
            {
                var result = _target.Validate(_dateOnly, _scheduleMatrixPro);
                Assert.IsFalse(result);
            }
        }
    }
}
