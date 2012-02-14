using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{

    [TestFixture]
    public class TimePeriodCanHaveShortBreakTest
    {
        private TimePeriodCanHaveShortBreak _target;
        private MockRepository _mock;
        private ISkillExtractor _skillExtractor;
        private ISkill _skill;
        private IList<ISkill> _skills;
        private TimePeriodWithSegment _timePeriodWithSegment;
        private int _skillResolution;
        private int _periodSegment;
        private TimeSpan _periodSegmentSpan;
        private IList<TimeSpan> _periodSegmentSpans;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _target = new TimePeriodCanHaveShortBreak();
            _skillExtractor = _mock.StrictMock<ISkillExtractor>();
            _skill = _mock.StrictMock<ISkill>();
            _skills = new List<ISkill> { _skill };
        }


        [Test]
        public void CannotHaveShortBreakWith15MinSkillAnd5MinSegmentStartAt0()
        {
            _skillResolution = 15;
            _periodSegment = 30;

            _periodSegmentSpan = TimeSpan.FromMinutes(_periodSegment);
            _periodSegmentSpans = new List<TimeSpan>{ _periodSegmentSpan };

            _timePeriodWithSegment = new TimePeriodWithSegment(8, 0, 16, 0, _periodSegment);

            using (_mock.Record())
            {
                CommonMocks();
            }
            using (_mock.Playback())
            {
                Assert.IsFalse(_target.CanHaveShortBreak(_skillExtractor, _timePeriodWithSegment));
                Assert.IsFalse(_target.CanHaveShortBreak(_skillExtractor, _periodSegmentSpans));
            }
        }

        [Test]
        public void CannotHaveShortBreakWith15MinSkillAnd30MinSegmentStartAt15()
        {
            _skillResolution = 15;
            _periodSegment = 30;

            _periodSegmentSpan = TimeSpan.FromMinutes(_periodSegment);
            _periodSegmentSpans = new List<TimeSpan> { _periodSegmentSpan };

            _timePeriodWithSegment = new TimePeriodWithSegment(8, 15, 16, 0, _periodSegment);

            using (_mock.Record())
            {
                CommonMocks();
            }
            using (_mock.Playback())
            {
                Assert.IsFalse(_target.CanHaveShortBreak(_skillExtractor, _timePeriodWithSegment));
                Assert.IsFalse(_target.CanHaveShortBreak(_skillExtractor, _periodSegmentSpans));
            }
        }

        [Test]
        public void CannotHaveShortBreakWith15MinSkillAnd30MinSegmentStartAt45()
        {
            _skillResolution = 15;
            _periodSegment = 30;

            _periodSegmentSpan = TimeSpan.FromMinutes(_periodSegment);
            _periodSegmentSpans = new List<TimeSpan> { _periodSegmentSpan };

            _timePeriodWithSegment = new TimePeriodWithSegment(8, 45, 16, 0, _periodSegment);

            using (_mock.Record())
            {
                CommonMocks();
            }
            using (_mock.Playback())
            {
                Assert.IsFalse(_target.CanHaveShortBreak(_skillExtractor, _timePeriodWithSegment));
                Assert.IsFalse(_target.CanHaveShortBreak(_skillExtractor, _periodSegmentSpans));
            }
        }

        [Test]
        public void CanHaveShortBreakWith15MinSkillAnd5MinSegmentStartAt10()
        {
            _skillResolution = 15;
            _periodSegment = 5;

            _periodSegmentSpan = TimeSpan.FromMinutes(_periodSegment);
            _periodSegmentSpans = new List<TimeSpan> { _periodSegmentSpan };

            _timePeriodWithSegment = new TimePeriodWithSegment(8, 10, 16, 0, _periodSegment);

            using (_mock.Record())
            {
                CommonMocks();
            }
            using (_mock.Playback())
            {
                Assert.IsTrue(_target.CanHaveShortBreak(_skillExtractor, _timePeriodWithSegment));
                Assert.IsTrue(_target.CanHaveShortBreak(_skillExtractor, _periodSegmentSpans));
            }
        }

        [Test]
        public void CanHaveShortBreakWhenSegmentDoesNotFitToSkillResolution()
        {
            _skillResolution = 20;
            _periodSegment = 15;

            _periodSegmentSpan = TimeSpan.FromMinutes(_periodSegment);
            _periodSegmentSpans = new List<TimeSpan> { _periodSegmentSpan };

            _timePeriodWithSegment = new TimePeriodWithSegment(8, 0, 16, 0, _periodSegment);

            using (_mock.Record())
            {
                CommonMocks();
            }
            using (_mock.Playback())
            {
                Assert.IsTrue(_target.CanHaveShortBreak(_skillExtractor, _timePeriodWithSegment));
                Assert.IsTrue(_target.CanHaveShortBreak(_skillExtractor, _periodSegmentSpans));
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "BreakEven"), Test]
        public void CannotHaveShortBreakEvenWhenTimePeriodResolutionDoesNotFitToStartTime()
        {
            _skillResolution = 15;
            _periodSegment = 30;

            _timePeriodWithSegment = new TimePeriodWithSegment(8, 15, 17, 0, _periodSegment);

            using (_mock.Record())
            {
                CommonMocks();
            }
            using (_mock.Playback())
            {
                Assert.IsFalse(_target.CanHaveShortBreak(_skillExtractor, _timePeriodWithSegment));
            }
        }

        [Test]
        public void CanHaveShortBreakSegmentIsEqualWithSkillResolution()
        {
            _skillResolution = 15;
            _periodSegment = 15;

            _timePeriodWithSegment = new TimePeriodWithSegment(8, 1, 16, 0, _periodSegment);

            using (_mock.Record())
            {
                CommonMocks();
            }
            using (_mock.Playback())
            {
                Assert.IsTrue(_target.CanHaveShortBreak(_skillExtractor, _timePeriodWithSegment));
            }
        }

        private void CommonMocks()
        {
            Expect.Call(_skillExtractor.ExtractSkills())
               .Return(_skills).Repeat.AtLeastOnce();
            Expect.Call(_skill.DefaultResolution)
                .Return(_skillResolution).Repeat.AtLeastOnce();

        }
    }
}
