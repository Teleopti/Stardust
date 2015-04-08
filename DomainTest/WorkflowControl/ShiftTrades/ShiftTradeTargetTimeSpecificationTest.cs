using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.WorkflowControl.ShiftTrades
{
    [TestFixture]
    public class ShiftTradeTargetTimeSpecificationTest
    {
        private ShiftTradeTargetTimeSpecification _target;
        private MockRepository _mocks;
        private ISchedulingResultStateHolder _stateHolder;
        private ISchedulePeriodTargetTimeCalculator _targetTimeTimeCalculator;
        private IScheduleDay _part1;
        private IScheduleDay _part2;
        private IScheduleDictionary _dic;
        private IScheduleRange _range;
        private IProjectionService _projectionService;
        private IVisualLayerCollection _visualLayerCollection;


        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _stateHolder = _mocks.StrictMock<ISchedulingResultStateHolder>();
            _targetTimeTimeCalculator = _mocks.StrictMock<ISchedulePeriodTargetTimeCalculator>();
            _target = new ShiftTradeTargetTimeSpecification(new ScheduleMatrixListCreator(()=> _stateHolder, new UniqueSchedulePartExtractor()), _targetTimeTimeCalculator);
            _part1 = _mocks.StrictMock<IScheduleDay>();
            _part2 = _mocks.StrictMock<IScheduleDay>();
            _dic = _mocks.StrictMock<IScheduleDictionary>();
            _range = _mocks.StrictMock<IScheduleRange>();
            _projectionService = _mocks.StrictMock<IProjectionService>();
            _visualLayerCollection = _mocks.StrictMock<IVisualLayerCollection>();

        }

        [Test]
        public void VerifyReturnsTrueIfAllInternalIsTrue()
        {
            IPerson person1 = PersonFactory.CreatePerson("P1");
            person1.AddSchedulePeriod(SchedulePeriodFactory.CreateSchedulePeriod(new DateOnly(2010, 1, 1), SchedulePeriodType.Day, 1));
            person1.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2010, 1, 1)));
            person1.WorkflowControlSet = new WorkflowControlSet("Hej");
            IPerson person2 = PersonFactory.CreatePerson("P2");
            person2.AddSchedulePeriod(SchedulePeriodFactory.CreateSchedulePeriod(new DateOnly(2010, 1, 1), SchedulePeriodType.Day, 1));
            person2.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2010, 1, 1)));
            person2.WorkflowControlSet = new WorkflowControlSet("Hej igen");
            DateTimePeriod dt = new DateTimePeriod(2010, 1, 1, 2020, 1, 2);
            using(_mocks.Record())
            {
                Expect.Call(_part1.Person).Return(person1).Repeat.Any();
                Expect.Call(_part2.Person).Return(person2).Repeat.Any();
                Expect.Call(_part1.Period).Return(dt).Repeat.Any();
                Expect.Call(_part2.Period).Return(dt).Repeat.Any();
                Expect.Call(_part1.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.Any();
                Expect.Call(_part2.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.Any();
                Expect.Call(_part1.Clone()).Return(_part1).Repeat.Any();
                Expect.Call(_part2.Clone()).Return(_part2).Repeat.Any();
                _part1.Merge(_part2, false);
                _part2.Merge(_part1, false);
                _part1.Clear<IPersonAssignment>(); //These should not be here, but something is wrong in the merge
                _part2.Clear<IPersonAssignment>(); //These should not be here, but something is wrong in the merge
                Expect.Call(_stateHolder.Schedules).Return(_dic).Repeat.Any();
                Expect.Call(_dic[person1]).Return(_range).Repeat.Once();
                Expect.Call(_range.ScheduledDay(new DateOnly(2010, 1, 1))).Return(_part1).Repeat.Once();
                Expect.Call(_part1.ProjectionService()).Return(_projectionService).Repeat.Once();
                Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection).Repeat.Once();
                Expect.Call(_visualLayerCollection.ContractTime()).Return(TimeSpan.FromHours(8)).Repeat.Once();
                Expect.Call(_targetTimeTimeCalculator.TargetWithTolerance(null)).IgnoreArguments().Return(new TimePeriod(TimeSpan.FromHours(8),
                                                                              TimeSpan.FromHours(8))).Repeat.Once();

                Expect.Call(_dic[person2]).Return(_range).Repeat.Once();
                Expect.Call(_range.ScheduledDay(new DateOnly(2010, 1, 1))).Return(_part2).Repeat.Once();
                Expect.Call(_part2.ProjectionService()).Return(_projectionService).Repeat.Once();
                Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection).Repeat.Once();
                Expect.Call(_visualLayerCollection.ContractTime()).Return(TimeSpan.FromHours(8)).Repeat.Once();
                Expect.Call(_targetTimeTimeCalculator.TargetWithTolerance(null)).IgnoreArguments().Return(new TimePeriod(TimeSpan.FromHours(8),
                                                                              TimeSpan.FromHours(8))).Repeat.Once();
            }
            using(_mocks.Playback())
            {
                Assert.IsTrue(_target.IsSatisfiedBy(tradeDetails()));
            }
        }

        [Test]
        public void VerifyReturnsFalseIfAnyInternalIsFalse()
        {
            IPerson person1 = PersonFactory.CreatePerson("P1");
            person1.AddSchedulePeriod(SchedulePeriodFactory.CreateSchedulePeriod(new DateOnly(2010, 1, 1), SchedulePeriodType.Day, 1));
            person1.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2010, 1, 1)));
            person1.WorkflowControlSet = new WorkflowControlSet("Hej");
            IPerson person2 = PersonFactory.CreatePerson("P2");
            person2.AddSchedulePeriod(SchedulePeriodFactory.CreateSchedulePeriod(new DateOnly(2010, 1, 1), SchedulePeriodType.Day, 1));
            person2.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2010, 1, 1)));
            person2.WorkflowControlSet = new WorkflowControlSet("Hej igen");
            DateTimePeriod dt = new DateTimePeriod(2010, 1, 1, 2020, 1, 2);
            using (_mocks.Record())
            {
                Expect.Call(_part1.Person).Return(person1).Repeat.Any();
                Expect.Call(_part2.Person).Return(person2).Repeat.Any();
                Expect.Call(_part1.Period).Return(dt).Repeat.Any();
                Expect.Call(_part2.Period).Return(dt).Repeat.Any();
                Expect.Call(_part1.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.Any();
                Expect.Call(_part2.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.Any();
                Expect.Call(_part1.Clone()).Return(_part1).Repeat.Any();
                Expect.Call(_part2.Clone()).Return(_part2).Repeat.Any();
                _part1.Clear<IPersonAssignment>(); //These should not be here, but something is wrong in the merge
                _part2.Clear<IPersonAssignment>(); //These should not be here, but something is wrong in the merge
                _part1.Merge(_part2, false);
                _part2.Merge(_part1, false);
                Expect.Call(_stateHolder.Schedules).Return(_dic).Repeat.Any();
                Expect.Call(_dic[person1]).Return(_range).Repeat.Once();
                Expect.Call(_range.ScheduledDay(new DateOnly(2010, 1, 1))).Return(_part1).Repeat.Once();
                Expect.Call(_part1.ProjectionService()).Return(_projectionService).Repeat.Once();
                Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection).Repeat.Once();
                Expect.Call(_visualLayerCollection.ContractTime()).Return(TimeSpan.FromHours(8)).Repeat.Once();
                Expect.Call(_targetTimeTimeCalculator.TargetWithTolerance(null)).IgnoreArguments().Return(new TimePeriod(TimeSpan.FromHours(8),
                                                                              TimeSpan.FromHours(8))).Repeat.Once();

                Expect.Call(_dic[person2]).Return(_range).Repeat.Once();
                Expect.Call(_range.ScheduledDay(new DateOnly(2010, 1, 1))).Return(_part2).Repeat.Once();
                Expect.Call(_part2.ProjectionService()).Return(_projectionService).Repeat.Once();
                Expect.Call(_projectionService.CreateProjection()).Return(_visualLayerCollection).Repeat.Once();
                Expect.Call(_visualLayerCollection.ContractTime()).Return(TimeSpan.FromHours(8)).Repeat.Once();
                Expect.Call(_targetTimeTimeCalculator.TargetWithTolerance(null)).IgnoreArguments().Return(new TimePeriod(TimeSpan.FromHours(7),
                                                                              TimeSpan.FromHours(7))).Repeat.Once();
            }
            using (_mocks.Playback())
            {
                Assert.IsFalse(_target.IsSatisfiedBy(tradeDetails()));
            }
        }

        [Test]
        public  void VerifyDenyReason()
        {
            Assert.AreEqual(_target.DenyReason, "ShiftTradeTargetTimeDenyReason");
            Assert.IsNotNull(UserTexts.Resources.ShiftTradeTargetTimeDenyReason);
        }

        private IList<IShiftTradeSwapDetail> tradeDetails()
        {
            // I listan ligger bara ett ChangeRequest, motbytet skapas av validatorn
            IList<IShiftTradeSwapDetail> ret = new List<IShiftTradeSwapDetail>();
            IShiftTradeSwapDetail detail1 = new ShiftTradeSwapDetail(null, null, new DateOnly(), new DateOnly());
            detail1.SchedulePartFrom = _part1;
            detail1.SchedulePartTo = _part2;
            ret.Add(detail1);
            //IShiftTradeSwapDetail detail2 = new ShiftTradeSwapDetail(null, null, new DateOnly(), new DateOnly());
            //detail2.SchedulePartFrom = _part2;
            //detail2.SchedulePartTo = _part1;
            //ret.Add(detail2);

            return ret;
        }
    }
}