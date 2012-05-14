﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Common.Clipboard;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
    [TestFixture]
    public class WeekPresenterTest
    {
        private WeekPresenter target;
        private MockRepository mocks;
        private IScheduleViewBase viewBase;
        private IScenario scenario;
        private GridlockManager gridlockManager;
        private ClipHandler<IScheduleDay> clipHandlerSchedulePart;
        private SchedulerStateHolder schedulerState;
        private readonly DateTime _date = new DateTime(2008, 11, 04, 0, 0, 0, DateTimeKind.Utc);
        private ICccTimeZoneInfo timeZoneInfo;
        private IOverriddenBusinessRulesHolder _overriddenBusinessRulesHolder;
        private IScheduleDayChangeCallback _scheduleDayChangeCallback;

        [SetUp]
        public void Setup()
        {
            
            mocks = new MockRepository();
            viewBase = mocks.StrictMock<IScheduleViewBase>();
            scenario = mocks.StrictMock<IScenario>();
            gridlockManager = new GridlockManager();
            clipHandlerSchedulePart = new ClipHandler<IScheduleDay>();
            schedulerState = new SchedulerStateHolder(scenario,new DateOnlyPeriodAsDateTimePeriod(new DateOnlyPeriod(), TeleoptiPrincipal.Current.Regional.TimeZone), new List<IPerson>());
            _overriddenBusinessRulesHolder = new OverriddenBusinessRulesHolder();
            _scheduleDayChangeCallback = mocks.DynamicMock<IScheduleDayChangeCallback>(); 

            target = new WeekPresenter(viewBase, schedulerState, gridlockManager, clipHandlerSchedulePart,
                                      SchedulePartFilter.None, _overriddenBusinessRulesHolder, _scheduleDayChangeCallback, NullScheduleTag.Instance);
            timeZoneInfo = new CccTimeZoneInfo(TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time"));
        }

        [Test]
        public void VerifyInstanceCreated()
        {
            Assert.IsNotNull(target);
        }

        [Test]
        public void VerifyCreateSpanDictionaryFromScheduleDefault()
        {
            var schedulePart = CreateScheduleDayAndSetBasicExpectation(SchedulePartView.None);

            mocks.ReplayAll();
            var result = WeekPresenter.CreateSpanDictionaryFromSchedule(schedulePart);
            Assert.AreEqual(11, (int)result[TimeZoneHelper.ConvertFromUtc(_date, timeZoneInfo).Date].ElapsedTime().TotalHours);
            Assert.AreEqual(TimeSpan.FromHours(7), result[TimeZoneHelper.ConvertFromUtc(_date, timeZoneInfo).Date].StartDateTimeLocal(timeZoneInfo).TimeOfDay);
            mocks.VerifyAll();
        }

        [Test]
        public void VerifyCreateSpanDictionaryFromScheduleMainShift()
        {
            IPersonAssignment pa1 = mocks.StrictMock<IPersonAssignment>();
            IPersonAssignment pa2 = mocks.StrictMock<IPersonAssignment>();

            var schedulePart = CreateScheduleDayAndSetBasicExpectation(SchedulePartView.MainShift);
            Expect.Call(schedulePart.PersonAssignmentCollection()).Return(
                new ReadOnlyCollection<IPersonAssignment>(new List<IPersonAssignment> { pa1, pa2 })).Repeat.AtLeastOnce();
            Expect.Call(pa1.Period).Return(new DateTimePeriod(_date.AddHours(10), _date.AddHours(15))).Repeat.AtLeastOnce();
            Expect.Call(pa2.Period).Return(new DateTimePeriod(_date.AddHours(18), _date.AddHours(25))).Repeat.AtLeastOnce();
            
            mocks.ReplayAll();
            var result = WeekPresenter.CreateSpanDictionaryFromSchedule(schedulePart);
            Assert.AreEqual(17, (int)result[TimeZoneHelper.ConvertFromUtc(_date, timeZoneInfo).Date].ElapsedTime().TotalHours);
            Assert.AreEqual(TimeSpan.FromHours(18), result[TimeZoneHelper.ConvertFromUtc(_date, timeZoneInfo).Date].StartDateTimeLocal(timeZoneInfo).TimeOfDay);
            mocks.VerifyAll();
        }

        [Test]
        public void VerifyCreateSpanDictionaryFromScheduleFullDayAbsence()
        {
            IProjectionService projectionService = mocks.StrictMock<IProjectionService>();
            IVisualLayer vl1 = mocks.StrictMock<IVisualLayer>();
            IVisualLayer vl2 = mocks.StrictMock<IVisualLayer>();
            var pl1 = new Activity("1");
				var pl2 = new Activity("d");
            IVisualLayerCollection visualLayerCollection = new VisualLayerCollection(null, new List<IVisualLayer>{vl1,vl2}, new ProjectionPayloadMerger());

            var schedulePart = CreateScheduleDayAndSetBasicExpectation(SchedulePartView.FullDayAbsence);
            Expect.Call(schedulePart.ProjectionService()).Return(projectionService).Repeat.AtLeastOnce();
            Expect.Call(projectionService.CreateProjection()).Return(visualLayerCollection).Repeat.AtLeastOnce();
            Expect.Call(vl1.Period).Return(new DateTimePeriod(_date.AddHours(10), _date.AddHours(15))).Repeat.AtLeastOnce();
            Expect.Call(vl1.Payload).Return(pl1).Repeat.AtLeastOnce();
            Expect.Call(vl2.Payload).Return(pl2).Repeat.AtLeastOnce();
            Expect.Call(vl1.EntityClone()).Return(vl1);
            Expect.Call(vl2.EntityClone()).Return(vl2);
            
            mocks.ReplayAll();
            var result = WeekPresenter.CreateSpanDictionaryFromSchedule(schedulePart);
            Assert.AreEqual(7, (int)result[TimeZoneHelper.ConvertFromUtc(_date, timeZoneInfo).Date].ElapsedTime().TotalHours); //Only first is picked!
            Assert.AreEqual(TimeSpan.FromHours(18), result[TimeZoneHelper.ConvertFromUtc(_date, timeZoneInfo).Date].StartDateTimeLocal(timeZoneInfo).TimeOfDay);
            mocks.VerifyAll();
        }

        [Test]
        public void VerifyCreateSpanDictionaryFromSchedulePersonalShift()
        {
            IProjectionService projectionService = mocks.StrictMock<IProjectionService>();
            IVisualLayer vl1 = mocks.StrictMock<IVisualLayer>();
            IVisualLayerCollection visualLayerCollection = new VisualLayerCollection(null, new List<IVisualLayer> { vl1 }, new ProjectionPayloadMerger());
            IPersonAssignment pa1 = mocks.StrictMock<IPersonAssignment>();
            IPersonalShift ps1 = mocks.StrictMock<IPersonalShift>();

            var schedulePart = CreateScheduleDayAndSetBasicExpectation(SchedulePartView.PersonalShift);
            Expect.Call(ps1.ProjectionService()).Return(projectionService).Repeat.AtLeastOnce();
            Expect.Call(projectionService.CreateProjection()).Return(visualLayerCollection).Repeat.AtLeastOnce();
            Expect.Call(schedulePart.PersonAssignmentCollection()).Return(
                new ReadOnlyCollection<IPersonAssignment>(new List<IPersonAssignment> { pa1 })).Repeat.AtLeastOnce();
            Expect.Call(pa1.PersonalShiftCollection).Return(new ReadOnlyCollection<IPersonalShift>(new List<IPersonalShift> { ps1 })).Repeat.AtLeastOnce();
            Expect.Call(vl1.Period).Return(new DateTimePeriod(_date.AddHours(10), _date.AddHours(15))).Repeat.AtLeastOnce();
            Expect.Call(vl1.EntityClone()).Return(vl1);
            
            mocks.ReplayAll();
            var result = WeekPresenter.CreateSpanDictionaryFromSchedule(schedulePart);
            Assert.AreEqual(7, (int)result[TimeZoneHelper.ConvertFromUtc(_date, timeZoneInfo).Date].ElapsedTime().TotalHours);
            Assert.AreEqual(TimeSpan.FromHours(18), result[TimeZoneHelper.ConvertFromUtc(_date, timeZoneInfo).Date].StartDateTimeLocal(timeZoneInfo).TimeOfDay);
            mocks.VerifyAll();
        }

        private IScheduleDay CreateScheduleDayAndSetBasicExpectation(SchedulePartView schedulePartView)
        {
            IScheduleDay schedulePart = mocks.StrictMock<IScheduleDay>();
            Expect.Call(schedulePart.TimeZone).Return(timeZoneInfo).Repeat.AtLeastOnce();
            Expect.Call(schedulePart.Period).Return(new DateTimePeriod(_date, _date.AddDays(1))).Repeat.AtLeastOnce();
            Expect.Call(schedulePart.SignificantPart()).Return(schedulePartView).Repeat.Once();

            return schedulePart;
        }
    }
}
