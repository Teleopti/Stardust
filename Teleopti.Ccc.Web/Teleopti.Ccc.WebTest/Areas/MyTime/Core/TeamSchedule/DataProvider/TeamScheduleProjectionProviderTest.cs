﻿using System;
using System.Drawing;
using System.Linq;
using System.Security.Principal;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.TeamSchedule.DataProvider
{
	[TestFixture]
	public class TeamScheduleProjectionProviderTest
	{
		private readonly TimeZoneInfo timeZone = TimeZoneInfoFactory.StockholmTimeZoneInfo();
		private IPrincipal principalBefore;
		private IPerson person;

		[SetUp]
		public void Setup()
		{
			setPrincipal();
		}

		private void setPrincipal()
		{
			principalBefore = System.Threading.Thread.CurrentPrincipal;
			person = PersonFactory.CreatePerson();
			person.PermissionInformation.SetDefaultTimeZone(timeZone);
			System.Threading.Thread.CurrentPrincipal = new TeleoptiPrincipal(
					 new TeleoptiIdentity("test", null, null, null), person);
		}

		[Test]
		public void ShouldReturnProjectionLayerColor()
		{
			var stubs = new StubFactory();
			var scheduleDay = stubs.ScheduleDayStub(DateTime.Today);
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
			var scheduleDay = stubs.ScheduleDayStub(DateTime.Today);
			var startTime = new DateTime(2011, 1, 2, 8, 0, 0, DateTimeKind.Utc);
			var endTime = new DateTime(2011, 1, 2, 17, 0, 0, DateTimeKind.Utc);
			var period = new DateTimePeriod(startTime, endTime);
			var layer = stubs.VisualLayerStub(period);
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
			const string activityName = "Phone";
			
			var stubs = new StubFactory();
			var scheduleDay = stubs.ScheduleDayStub(DateTime.Today);
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
			var dayOff = PersonAssignmentFactory.CreateAssignmentWithDayOff();
			var scheduleDay = new StubFactory().ScheduleDayStub(DateTime.Today, SchedulePartView.DayOff, dayOff);

			var target = new TeamScheduleProjectionProvider(MockRepository.GenerateMock<IProjectionProvider>());

			var result = target.Projection(scheduleDay);

			result.DayOff.Should().Be(dayOff.DayOff());
			result.Layers.Should().Be.Empty();
		}

		[Test]
		public void ShouldProjectProjectionInsteadOfDayOff()
		{
			var stubs = new StubFactory();
			var startTime = new DateTime(2011, 1, 2, 8, 0, 0, DateTimeKind.Utc);
			var endTime = new DateTime(2011, 1, 2, 17, 0, 0, DateTimeKind.Utc);
			var period = new DateTimePeriod(startTime, endTime);
			var layer = stubs.VisualLayerStub(period);
			var projection = stubs.ProjectionStub(new[] { layer });
			var dayOff = PersonAssignmentFactory.CreateAssignmentWithDayOff();
			var scheduleDay = new StubFactory().ScheduleDayStub(DateTime.Today, SchedulePartView.DayOff, dayOff);

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
			scheduleDay.Add(PersonAssignmentFactory.CreateAssignmentWithDayOff(scheduleDay.Scenario, scheduleDay.Person, new DateOnly(2001,1,1), new DayOffTemplate()));

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

		[TearDown]
		public void Teardown()
		{
			System.Threading.Thread.CurrentPrincipal = principalBefore;
		}
	}
}
