using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;


namespace Teleopti.Ccc.DomainTest.Optimization
{
    [TestFixture]
    public class WorkShiftRuleSetHasShortBreakTest
    {
        private WorkShiftRuleSetCanHaveShortBreak _target;
        private MockRepository _mock;
        private ITimePeriodCanHaveShortBreak _timePeriodCanHaveShortBreak;
        private ISkillExtractor _skillExtractor;
        private IWorkShiftRuleSet _workShiftRuleSet;
        private IWorkShiftTemplateGenerator _workShiftTemplateGenerator;
        private TimePeriodWithSegment _timePeriodWithSegment;
        private IList<IWorkShiftExtender> _workShiftExtenders;
        private IList<IWorkShiftLimiter> _workShiftLimiters;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _timePeriodCanHaveShortBreak = _mock.StrictMock<ITimePeriodCanHaveShortBreak>();
            _skillExtractor = _mock.StrictMock<ISkillExtractor>();
            _workShiftRuleSet = _mock.StrictMock<IWorkShiftRuleSet>();
            _workShiftTemplateGenerator = _mock.StrictMock<IWorkShiftTemplateGenerator>();
            _timePeriodWithSegment = new TimePeriodWithSegment(new TimePeriod(07, 01, 19, 01), new TimeSpan(1));
            _workShiftExtenders = new List<IWorkShiftExtender>();
            _workShiftLimiters = new List<IWorkShiftLimiter>();
            _target = new WorkShiftRuleSetCanHaveShortBreak(_timePeriodCanHaveShortBreak, _skillExtractor);
        }

        [Test]
        public void WorkShiftTemplateShouldBeChecked()
        {
            const int expectedWorkShiftTemplateCheckTimes = 2;

            using(_mock.Record())
            {
                CommonMocks();
                Expect.Call(_timePeriodCanHaveShortBreak.CanHaveShortBreak(_skillExtractor, _timePeriodWithSegment))
                    .Return(false).Repeat.Times(expectedWorkShiftTemplateCheckTimes);
            }
            using(_mock.Playback())
            {
                Assert.IsFalse(_target.CanHaveShortBreak(_workShiftRuleSet));
            }
        }

        [Test]
        public void WorkShiftTemplateShouldBeReturnAfterTheFirstTrueResult()
        {
            const int expectedWorkShiftTemplateCheckWithTrueResult = 1;

            using (_mock.Record())
            {
                CommonMocks();
                Expect.Call(_timePeriodCanHaveShortBreak.CanHaveShortBreak(_skillExtractor, _timePeriodWithSegment))
                    .Return(true).Repeat.Times(expectedWorkShiftTemplateCheckWithTrueResult);
            }
            using (_mock.Playback())
            {
                Assert.IsTrue(_target.CanHaveShortBreak(_workShiftRuleSet));
            }
        }

        [Test]
        public void OnlyIfStartTimeOfWorkShiftFalseShouldTheEndTimeBeChecked()
        {

            using (_mock.Record())
            {
                CommonMocks();
                Expect.Call(_timePeriodCanHaveShortBreak.CanHaveShortBreak(_skillExtractor, _timePeriodWithSegment))
                    .Return(false).Repeat.Times(1);
                Expect.Call(_timePeriodCanHaveShortBreak.CanHaveShortBreak(_skillExtractor, _timePeriodWithSegment))
                    .Return(true).Repeat.Times(1);
            }
            using (_mock.Playback())
            {
                Assert.IsTrue(_target.CanHaveShortBreak(_workShiftRuleSet));
            }
        }

        [Test]
        public void AutoPositionedActivityExtenderTypeShouldCheckAutoPositionIntervalSegmentAndStartSegment()
        {
            const int expectedWorkShiftTemplateCheckTimes = 2;
            TimeSpan extenderSegment = new TimeSpan(1);

            IAutoPositionedActivityExtender workShiftExtender1 = _mock.StrictMock<IAutoPositionedActivityExtender>();
            _workShiftExtenders.Add(workShiftExtender1);

            using (_mock.Record())
            {
                CommonMocks();
                Expect.Call(workShiftExtender1.ActivityLengthWithSegment)
                    .Return(_timePeriodWithSegment);
                Expect.Call(workShiftExtender1.StartSegment)
                    .Return(extenderSegment);
                Expect.Call(workShiftExtender1.AutoPositionIntervalSegment)
                    .Return(extenderSegment);

                Expect.Call(_timePeriodCanHaveShortBreak.CanHaveShortBreak(_skillExtractor, _timePeriodWithSegment))
                    .Return(false).Repeat.Times(expectedWorkShiftTemplateCheckTimes + 1);
                Expect.Call(_timePeriodCanHaveShortBreak.CanHaveShortBreak(_skillExtractor, new List<TimeSpan> { extenderSegment, extenderSegment }))
                    .Return(false).Repeat.Times(1);
            }
            using (_mock.Playback())
            {
                Assert.IsFalse(_target.CanHaveShortBreak(_workShiftRuleSet));
            }
        }

        [Test]
        public void ActivityNormalExtenderTypeShouldCheckActivityPositionWithSegment()
        {
            const int expectedWorkShiftTemplateCheckTimes = 2;

            IActivityNormalExtender workShiftExtender1 = _mock.StrictMock<IActivityNormalExtender>();
            _workShiftExtenders.Add(workShiftExtender1);

            using (_mock.Record())
            {
                CommonMocks();
                Expect.Call(workShiftExtender1.ActivityLengthWithSegment)
                    .Return(_timePeriodWithSegment);
                Expect.Call(workShiftExtender1.ActivityPositionWithSegment)
                    .Return(_timePeriodWithSegment);

                Expect.Call(_timePeriodCanHaveShortBreak.CanHaveShortBreak(_skillExtractor, _timePeriodWithSegment))
                    .Return(false).Repeat.Times(expectedWorkShiftTemplateCheckTimes + _workShiftExtenders.Count * 2);

            }
            using (_mock.Playback())
            {
                Assert.IsFalse(_target.CanHaveShortBreak(_workShiftRuleSet));
            }
        }

        [Test]
        public void AllWorkShiftExtendersShouldBeCheckedForFalse()
        {
            const int expectedWorkShiftTemplateCheckTimes = 2;

            IActivityNormalExtender workShiftExtender1 = _mock.StrictMock<IActivityNormalExtender>();
            IActivityNormalExtender workShiftExtender2 = _mock.StrictMock<IActivityNormalExtender>();
            _workShiftExtenders.Add(workShiftExtender1);
            _workShiftExtenders.Add(workShiftExtender2);

            using (_mock.Record())
            {
                CommonMocks();
                Expect.Call(workShiftExtender1.ActivityLengthWithSegment)
                    .Return(_timePeriodWithSegment);
                Expect.Call(workShiftExtender1.ActivityPositionWithSegment)
                    .Return(_timePeriodWithSegment);
                Expect.Call(workShiftExtender2.ActivityLengthWithSegment)
                    .Return(_timePeriodWithSegment);
                Expect.Call(workShiftExtender2.ActivityPositionWithSegment)
                    .Return(_timePeriodWithSegment);
                
                Expect.Call(_timePeriodCanHaveShortBreak.CanHaveShortBreak(_skillExtractor, _timePeriodWithSegment))
                    .Return(false).Repeat.Times(expectedWorkShiftTemplateCheckTimes + _workShiftExtenders.Count * 2);

            }
            using (_mock.Playback())
            {
                Assert.IsFalse(_target.CanHaveShortBreak(_workShiftRuleSet));
            }
        }

        [Test]
        public void IfWorkShiftExtendersCanHaveShortBreakSoJumpOut()
        {
            const int expectedWorkShiftTemplateCheckTimes = 2;
            const bool workShiftExtender2ActivityPositionWithSegmentResult = true;

            IActivityNormalExtender workShiftExtender1 = _mock.StrictMock<IActivityNormalExtender>();
            IActivityNormalExtender workShiftExtender2 = _mock.StrictMock<IActivityNormalExtender>();
            IActivityNormalExtender workShiftExtender3 = _mock.StrictMock<IActivityNormalExtender>();
            _workShiftExtenders.Add(workShiftExtender1);
            _workShiftExtenders.Add(workShiftExtender2);
            _workShiftExtenders.Add(workShiftExtender3);

            using (_mock.Record())
            {
                CommonMocks();
                Expect.Call(workShiftExtender1.ActivityLengthWithSegment)
                    .Return(_timePeriodWithSegment);
                Expect.Call(workShiftExtender1.ActivityPositionWithSegment)
                    .Return(_timePeriodWithSegment);
                Expect.Call(workShiftExtender2.ActivityLengthWithSegment)
                    .Return(_timePeriodWithSegment);
                Expect.Call(workShiftExtender2.ActivityPositionWithSegment)
                    .Return(_timePeriodWithSegment);


                Expect.Call(_timePeriodCanHaveShortBreak.CanHaveShortBreak(_skillExtractor, _timePeriodWithSegment))
                    .Return(false).Repeat.Times(3 + expectedWorkShiftTemplateCheckTimes);

                Expect.Call(_timePeriodCanHaveShortBreak.CanHaveShortBreak(_skillExtractor, _timePeriodWithSegment))
                     .Return(workShiftExtender2ActivityPositionWithSegmentResult); // the workShiftExtender2.ActivityPositionWithSegment will return true

                // note that there is no call for workShiftExtender3 as code jumps out before;

            }
            using (_mock.Playback())
            {
                Assert.IsTrue(_target.CanHaveShortBreak(_workShiftRuleSet));
            }
        }

        private void CommonMocks()
        {
            Expect.Call(_workShiftRuleSet.TemplateGenerator)
                .Return(_workShiftTemplateGenerator).Repeat.Any();
            Expect.Call(_workShiftTemplateGenerator.StartPeriod)
                .Return(_timePeriodWithSegment).Repeat.Any();
            Expect.Call(_workShiftTemplateGenerator.EndPeriod)
                .Return(_timePeriodWithSegment).Repeat.Any();
            Expect.Call(_workShiftRuleSet.ExtenderCollection).Return(
                new ReadOnlyCollection<IWorkShiftExtender>(_workShiftExtenders)).Repeat.Any();
            Expect.Call(_workShiftRuleSet.LimiterCollection).Return(
                new ReadOnlyCollection<IWorkShiftLimiter>(_workShiftLimiters)).Repeat.Any();
        }
    }
}
