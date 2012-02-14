﻿using System;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.DataProvider;
using Teleopti.Ccc.WebTest.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.TeamSchedule.DataProvider
{
	[TestFixture]
	public class TeamScheduleProjectionProviderTest
	{
		[Test]
		public void ShouldReturnProjectionLayerColor()
		{
			var stubs = new StubFactory();
			var scheduleDay = stubs.ScheduleDayStub(DateOnly.Today);
			var projection = stubs.ProjectionStub(new[] { stubs.VisualLayerStub(Color.Red) });

			var projectionProvider = MockRepository.GenerateMock<IProjectionProvider>();

			projectionProvider.Stub(x => x.Projection(scheduleDay)).Return(projection);

			var target = new TeamScheduleProjectionProvider(projectionProvider);

			var result = target.Projection(scheduleDay);

			result.Layers.Single().DisplayColor.Should().Be(Color.Red);
		}

		[Test]
		public void ShouldReturnProjectionLayerPeriod()
		{
			var stubs = new StubFactory();
			var scheduleDay = stubs.ScheduleDayStub(DateOnly.Today);
			var startTime = new DateTime(2011, 1, 2, 8, 0, 0, DateTimeKind.Utc);
			var endTime = new DateTime(2011, 1, 2, 17, 0, 0, DateTimeKind.Utc);
			var period = new DateTimePeriod(startTime, endTime);
			var layer = stubs.VisualLayerStub();
			layer.Period = period;
			var projection = stubs.ProjectionStub(new[] { layer });

			var projectionProvider = MockRepository.GenerateMock<IProjectionProvider>();

			projectionProvider.Stub(x => x.Projection(scheduleDay)).Return(projection);

			var target = new TeamScheduleProjectionProvider(projectionProvider);

			var result = target.Projection(scheduleDay);

			result.Layers.Single().Period.Value.Should().Be(period);
		}

		[Test]
		public void ShouldReturnProjectionLayerActivityName()
		{
			var activityName = "Phone";
			
			var stubs = new StubFactory();
			var scheduleDay = stubs.ScheduleDayStub(DateOnly.Today);
			var layer = stubs.VisualLayerStub(activityName);

			var projection = stubs.ProjectionStub(new[] { layer });

			var projectionProvider = MockRepository.GenerateMock<IProjectionProvider>();

			projectionProvider.Stub(x => x.Projection(scheduleDay)).Return(projection);

			var target = new TeamScheduleProjectionProvider(projectionProvider);

			var result = target.Projection(scheduleDay);

			result.Layers.Single().ActivityName.Should().Be(activityName);
		}

		[Test]
		public void ShouldProjectDayOff()
		{
			var stubs = new StubFactory();
			var period = new DateTimePeriod();
			var dayOff = stubs.PersonDayOffStub(period);
			var scheduleDay = new StubFactory().ScheduleDayStub(DateOnly.Today, SchedulePartView.DayOff, dayOff);

			var target = new TeamScheduleProjectionProvider(MockRepository.GenerateMock<IProjectionProvider>());

			var result = target.Projection(scheduleDay);

			result.DayOff.Should().Be(dayOff);
			result.Layers.Should().Be.Empty();
		}

		[Test]
		public void ShouldProjectProjectionInsteadOfDayOff()
		{
			var stubs = new StubFactory();
			var startTime = new DateTime(2011, 1, 2, 8, 0, 0, DateTimeKind.Utc);
			var endTime = new DateTime(2011, 1, 2, 17, 0, 0, DateTimeKind.Utc);
			var period = new DateTimePeriod(startTime, endTime);
			var layer = stubs.VisualLayerStub();
			layer.Period = period;
			var projection = stubs.ProjectionStub(new[] { layer });
			var dayOff = stubs.PersonDayOffStub(period);
			var scheduleDay = new StubFactory().ScheduleDayStub(DateOnly.Today, SchedulePartView.DayOff, dayOff);

			var projectionProvider = MockRepository.GenerateMock<IProjectionProvider>();

			projectionProvider.Stub(x => x.Projection(scheduleDay)).Return(projection);

			var target = new TeamScheduleProjectionProvider(projectionProvider);

			var result = target.Projection(scheduleDay);

			result.DayOff.Should().Be.Null();
			var resultLayer = result.Layers.Single();
			resultLayer.Period.Value.Should().Be(period);
		}

		[Test]
		public void ShouldSetSortDateToStartDateIfNormalProjection()
		{
			var scheduleDay = new SchedulePartFactoryForDomain().CreatePartWithMainShift();

			var target = new TeamScheduleProjectionProvider(new ProjectionProvider());
			var result = target.Projection(scheduleDay);

			result.SortDate.Should().Be.EqualTo(scheduleDay.ProjectionService().CreateProjection().Period().Value.StartDateTime);
		}

		[Test]
		public void ShouldSetCorrectSortDateIfFullDayAbsence()
		{
			var scheduleDay = new SchedulePartFactoryForDomain().CreatePartWithMainShift();
			scheduleDay.Add(new PersonAbsence(scheduleDay.Person, scheduleDay.Scenario, new AbsenceLayer(new Absence(), new DateTimePeriod(2001,1,1,2001,1,2))));

			var target = new TeamScheduleProjectionProvider(new ProjectionProvider());
			var result = target.Projection(scheduleDay);

			result.SortDate.Should().Be.EqualTo(
				scheduleDay.ProjectionService().CreateProjection().Period().Value.StartDateTime.AddDays(TeamScheduleProjectionProvider.AbsenceFullDayExtraDays));
		}

		[Test]
		public void ShouldSetCorrectSortDateIfDayOff()
		{
			var scheduleDay = new SchedulePartFactoryForDomain().CreatePartWithoutMainShift();
			scheduleDay.Add(new PersonDayOff(scheduleDay.Person, scheduleDay.Scenario,new DayOffTemplate(new Description("d")), new DateOnly(2001,1,1)));

			var target = new TeamScheduleProjectionProvider(new ProjectionProvider());
			var result = target.Projection(scheduleDay);

			result.SortDate.Should().Be.EqualTo(scheduleDay.Period.StartDateTime.AddDays(TeamScheduleProjectionProvider.DayOffExtraDays));
		}

		[Test]
		public void ShouldSetCorrectSortDateIfEmptyDay()
		{
			var scheduleDay = new SchedulePartFactoryForDomain().CreatePartWithoutMainShift();
			var target = new TeamScheduleProjectionProvider(new ProjectionProvider());
			var result = target.Projection(scheduleDay);
			result.SortDate.Should().Be.EqualTo(
				scheduleDay.Period.StartDateTime.AddDays(TeamScheduleProjectionProvider.EmptyExtraDays));
		}
	}
}
