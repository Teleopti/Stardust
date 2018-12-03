using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.WorkflowControl.ShiftTrades
{
    [TestFixture]
    public class TargetTimeFlexibilitySpecificationTest
    {
        private ITargetTimeFlexibilityValidator _target;
        private IScheduleMatrixPro _matrix;
        private MockRepository _mocks;
        private ISchedulePeriodTargetTimeCalculator _targetTimeCalculator;
        private IPerson _person;
        private TimeSpan _flexibility = TimeSpan.FromHours(1);

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _matrix = _mocks.StrictMock<IScheduleMatrixPro>();
            _targetTimeCalculator = _mocks.StrictMock<ISchedulePeriodTargetTimeCalculator>();
            _target = new TargetTimeFlexibilityValidator(_matrix, _targetTimeCalculator);
            _person = PersonFactory.CreatePerson();
        }

        [Test]
        public void VerifyOneHourMoreTimeReturnsTrue()
        {
            IScheduleDay schedulePart = _mocks.StrictMock<IScheduleDay>();
            IProjectionService projection = _mocks.StrictMock<IProjectionService>();
            IVisualLayerCollection visualLayerCollection = _mocks.StrictMock<IVisualLayerCollection>();
            using (_mocks.Record())
            {
                mockExpectations();
				Expect.Call(schedulePart.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(new DateOnly(2010, 1, 1), TimeZoneInfo.Utc)).Repeat.AtLeastOnce();
                Expect.Call(schedulePart.ProjectionService()).Return(projection).Repeat.AtLeastOnce();
                Expect.Call(projection.CreateProjection()).Return(visualLayerCollection).Repeat.AtLeastOnce();
                Expect.Call(visualLayerCollection.ContractTime()).Return(TimeSpan.FromHours(9)).Repeat.AtLeastOnce();
                Expect.Call(schedulePart.Person).Return(_person).Repeat.Any();
            }
            using (_mocks.Playback())
            {
                Assert.IsTrue(_target.Validate(new List<IScheduleDay> { schedulePart }, _flexibility));
            }
            
        }

        [Test]
        public void VerifyTwoHourMoreTimeReturnsFalse()
        {
            IScheduleDay schedulePart = _mocks.StrictMock<IScheduleDay>();
            IProjectionService projection = _mocks.StrictMock<IProjectionService>();
            IVisualLayerCollection visualLayerCollection = _mocks.StrictMock<IVisualLayerCollection>();
            using (_mocks.Record())
            {
                mockExpectations();
				Expect.Call(schedulePart.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(new DateOnly(2010, 1, 1), TimeZoneInfo.Utc)).Repeat.AtLeastOnce();
                Expect.Call(schedulePart.ProjectionService()).Return(projection).Repeat.AtLeastOnce();
                Expect.Call(projection.CreateProjection()).Return(visualLayerCollection).Repeat.AtLeastOnce();
                Expect.Call(visualLayerCollection.ContractTime()).Return(TimeSpan.FromHours(10)).Repeat.AtLeastOnce();
                Expect.Call(schedulePart.Person).Return(_person).Repeat.Any();
            }
            using (_mocks.Playback())
            {
                 Assert.IsFalse(_target.Validate(new List<IScheduleDay> { schedulePart }, _flexibility));
           }
        }

        [Test]
        public void VerifyOneAndAHalfHourMoreTimeReturnsTrue()
        {
            IScheduleDay schedulePart = _mocks.StrictMock<IScheduleDay>();
            IProjectionService projection = _mocks.StrictMock<IProjectionService>();
            IVisualLayerCollection visualLayerCollection = _mocks.StrictMock<IVisualLayerCollection>();
            using (_mocks.Record())
            {
                mockExpectations();
                Expect.Call(schedulePart.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(new DateOnly(2010, 1, 1),TimeZoneInfo.Utc)).Repeat.AtLeastOnce();
                Expect.Call(schedulePart.ProjectionService()).Return(projection).Repeat.AtLeastOnce();
                Expect.Call(projection.CreateProjection()).Return(visualLayerCollection).Repeat.AtLeastOnce();
                Expect.Call(visualLayerCollection.ContractTime()).Return(TimeSpan.FromHours(9.5)).Repeat.AtLeastOnce();
                Expect.Call(schedulePart.Person).Return(_person).Repeat.Any();
            }
            using (_mocks.Playback())
            {
                Assert.IsTrue(_target.Validate(new List<IScheduleDay> { schedulePart }, _flexibility));
            }
        }

        [Test]
        public void VerifyOneHourLessTimeReturnsTrue()
        {
            IScheduleDay schedulePart = _mocks.StrictMock<IScheduleDay>();
            IProjectionService projection = _mocks.StrictMock<IProjectionService>();
            IVisualLayerCollection visualLayerCollection = _mocks.StrictMock<IVisualLayerCollection>();
            using (_mocks.Record())
            {
                mockExpectations();
				Expect.Call(schedulePart.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(new DateOnly(2010, 1, 1), TimeZoneInfo.Utc)).Repeat.AtLeastOnce();
                Expect.Call(schedulePart.ProjectionService()).Return(projection).Repeat.AtLeastOnce();
                Expect.Call(projection.CreateProjection()).Return(visualLayerCollection).Repeat.AtLeastOnce();
                Expect.Call(visualLayerCollection.ContractTime()).Return(TimeSpan.FromHours(7)).Repeat.AtLeastOnce();
                Expect.Call(schedulePart.Person).Return(_person).Repeat.Any();
            }
            using (_mocks.Playback())
            {
                  Assert.IsTrue(_target.Validate(new List<IScheduleDay> { schedulePart }, _flexibility));
          }
        }

	    [Test]
	    public void VerifyTwoHourLessTimeReturnsFalse()
	    {
		    IScheduleDay schedulePart = _mocks.StrictMock<IScheduleDay>();
		    IProjectionService projection = _mocks.StrictMock<IProjectionService>();
		    IVisualLayerCollection visualLayerCollection = _mocks.StrictMock<IVisualLayerCollection>();
		    using (_mocks.Record())
		    {
			    mockExpectations();
			    Expect.Call(schedulePart.DateOnlyAsPeriod)
				    .Return(new DateOnlyAsDateTimePeriod(new DateOnly(2010, 1, 1), TimeZoneInfo.Utc))
				    .Repeat.AtLeastOnce();
			    Expect.Call(schedulePart.ProjectionService()).Return(projection).Repeat.AtLeastOnce();
			    Expect.Call(projection.CreateProjection()).Return(visualLayerCollection).Repeat.AtLeastOnce();
			    Expect.Call(visualLayerCollection.ContractTime()).Return(TimeSpan.FromHours(6)).Repeat.AtLeastOnce();
			    Expect.Call(schedulePart.Person).Return(_person).Repeat.Any();
		    }
		    using (_mocks.Playback())
		    {
			    Assert.IsFalse(_target.Validate(new List<IScheduleDay> {schedulePart}, _flexibility));
		    }
	    }

	    private void mockExpectations()
        {
            IScheduleDayPro dayDO = _mocks.StrictMock<IScheduleDayPro>();
            IScheduleDayPro dayMain = _mocks.StrictMock<IScheduleDayPro>();
            IScheduleDay partDO = _mocks.StrictMock<IScheduleDay>();
            IScheduleDay partMain = _mocks.StrictMock<IScheduleDay>();
            var periodDays = new [] { dayMain, dayDO };
            IProjectionService projection1 = _mocks.StrictMock<IProjectionService>();
            IProjectionService projection2 = _mocks.StrictMock<IProjectionService>();
            IVisualLayerCollection visualLayerCollection1 = _mocks.StrictMock<IVisualLayerCollection>();
            IVisualLayerCollection visualLayerCollection2 = _mocks.StrictMock<IVisualLayerCollection>();

            Expect.Call(_matrix.EffectivePeriodDays).Return(periodDays);
            Expect.Call(dayMain.DaySchedulePart()).Return(partMain).Repeat.Any();
            Expect.Call(dayDO.DaySchedulePart()).Return(partDO).Repeat.Any();
            Expect.Call(partMain.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(new DateOnly(2010, 1, 1), TimeZoneInfo.Utc)).Repeat.Any();
			Expect.Call(partDO.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(new DateOnly(2010, 1, 2), TimeZoneInfo.Utc)).Repeat.Any();
            Expect.Call(partMain.ProjectionService()).Return(projection1).Repeat.Any();
            Expect.Call(partDO.ProjectionService()).Return(projection2).Repeat.Any();
            Expect.Call(projection1.CreateProjection()).Return(visualLayerCollection1).Repeat.Any();
            Expect.Call(projection2.CreateProjection()).Return(visualLayerCollection2).Repeat.Any();
            Expect.Call(visualLayerCollection1.ContractTime()).Return(TimeSpan.FromHours(8)).Repeat.Any();
            Expect.Call(visualLayerCollection2.ContractTime()).Return(TimeSpan.FromHours(0)).Repeat.Any();
            Expect.Call(_targetTimeCalculator.TargetWithTolerance(_matrix)).Return(new TimePeriod(TimeSpan.FromHours(7.5),
                                                                                     TimeSpan.FromHours(8.5))).Repeat.Any();
            Expect.Call(partDO.Person).Return(_person).Repeat.Any();
            Expect.Call(partMain.Person).Return(_person).Repeat.Any();
            Expect.Call(_matrix.Person).Return(_person).Repeat.Any();
        }
    }
}
