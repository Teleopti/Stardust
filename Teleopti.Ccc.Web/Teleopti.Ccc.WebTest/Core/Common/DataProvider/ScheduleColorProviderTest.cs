using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Common.DataProvider
{
	[TestFixture]
	public class ScheduleColorProviderTest
	{

		[Test]
		public void ShouldReturnEmptyEnumerableOnNullSource()
		{
			var target = new ScheduleColorProvider();

			var result = target.GetColors(null);

			result.Should().Be.Empty();
		}

		[Test]
		public void ShouldOnlyGetUniqueColors()
		{
			var stubs = new StubFactory();
			var projection1 = stubs.ProjectionStub(new[] { stubs.VisualLayerStub(Color.Pink) });
			var projection2 = stubs.ProjectionStub(new[] { stubs.VisualLayerStub(Color.BlueViolet) });
			var projection3 = stubs.ProjectionStub(new[] { stubs.VisualLayerStub(Color.BlueViolet) });
			var scheduleDay1 = stubs.ScheduleDayStub(DateTime.Now, SchedulePartView.MainShift, 
				stubs.PersonAssignmentStub(new DateTimePeriod(), stubs.MainShiftStub(stubs.ShiftCategoryStub(Color.RoyalBlue)))
				);
			var scheduleDay2 = stubs.ScheduleDayStub(DateTime.Now, SchedulePartView.MainShift, 
				stubs.PersonAssignmentStub(new DateTimePeriod(), stubs.MainShiftStub(stubs.ShiftCategoryStub(Color.Pink)))
				);
			var source = new FakeScheduleColorSource
			             	{
			             		ScheduleDays = new[] {scheduleDay1, scheduleDay2},
			             		Projections = new[] {projection1, projection2, projection3}
			             	};

			var target = new ScheduleColorProvider();

			var result = target.GetColors(source);

			result.Should().Have.Count.EqualTo(3);
			result.Should().Have.SameValuesAs(new[] { Color.Pink, Color.BlueViolet, Color.RoyalBlue});
		}

		[Test]
		public void ShouldGetDisplayColorFromProjectionsVisualLayer()
		{
			var stubs = new StubFactory();
			var projection = stubs.ProjectionStub(new[] {stubs.VisualLayerStub(Color.Red)});
			var source = new FakeScheduleColorSource {Projections = new[] {projection}};

			var target = new ScheduleColorProvider();

			var result = target.GetColors(source);

			result.Single().Should().Be(Color.Red);
		}

		[Test]
		public void ShouldGetDisplayColorFromScheduleDayPersonAssignment()
		{
			var stubs = new StubFactory();
			var personAssignment = stubs.PersonAssignmentStub(new DateTimePeriod(), stubs.MainShiftStub(stubs.ShiftCategoryStub(Color.Blue)));
			var scheduleDay = stubs.ScheduleDayStub(DateTime.Now, SchedulePartView.MainShift, personAssignment);
			var source = new FakeScheduleColorSource { ScheduleDays = new[] { scheduleDay} };

			var target = new ScheduleColorProvider();

			var result = target.GetColors(source);

			result.Single().Should().Be(Color.Blue);
		}

		[Test]
		public void ShouldGetDisplayColorFromPersonAbsence()
		{
			var stubs = new StubFactory();
			var personAbsence = stubs.PersonAbsenceStub(new DateTimePeriod(), stubs.AbsenceLayerStub(stubs.AbsenceStub(Color.Olive)));
			var scheduleDay = stubs.ScheduleDayStub(DateTime.Now, SchedulePartView.FullDayAbsence, personAbsence);
			var source = new FakeScheduleColorSource { ScheduleDays = new[] { scheduleDay } };

			var target = new ScheduleColorProvider();

			var result = target.GetColors(source);

			result.Single().Should().Be(Color.Olive);
		}

		[Test]
		public void ShouldGetDisplayColorFromPreferenceShiftCategory()
		{
			var preferenceDay = new PreferenceDay(new Person(), DateOnly.Today,
			                                      new PreferenceRestriction
			                                      	{
			                                      		ShiftCategory = new ShiftCategory(" ")
			                                      		                	{
			                                      		                		DisplayColor = Color.Plum
			                                      		                	}
			                                      	});
			var source = new FakeScheduleColorSource {PreferenceDays = new[] {preferenceDay}};

			var target = new ScheduleColorProvider();

			var result = target.GetColors(source);

			result.Single().Should().Be(Color.Plum);
		}

		[Test]
		public void ShouldGetDisplayColorFromPreferenceAbsence()
		{
			var preferenceDay = new PreferenceDay(new Person(), DateOnly.Today,
												  new PreferenceRestriction
												  {
													  Absence = new Absence
													  {
														  DisplayColor = Color.DarkOliveGreen
													  }
												  });
			var source = new FakeScheduleColorSource { PreferenceDays = new[] { preferenceDay } };

			var target = new ScheduleColorProvider();

			var result = target.GetColors(source);

			result.Single().Should().Be(Color.DarkOliveGreen);
		}

		[Test]
		public void ShouldGetDisplayColorFromPreferenceDayOff()
		{
			var preferenceDay = new PreferenceDay(new Person(), DateOnly.Today,
			                                      new PreferenceRestriction
			                                      	{
			                                      		DayOffTemplate = new DayOffTemplate(new Description())
			                                      		                 	{
			                                      		                 		DisplayColor = Color.BlanchedAlmond
			                                      		                 	}
			                                      	});
			var source = new FakeScheduleColorSource { PreferenceDays = new[] { preferenceDay } };

			var target = new ScheduleColorProvider();

			var result = target.GetColors(source);

			result.Single().Should().Be(Color.BlanchedAlmond);
		}

		private class FakeScheduleColorSource : IScheduleColorSource
		{
			public IEnumerable<IScheduleDay> ScheduleDays { get; set; }
			public IEnumerable<IVisualLayerCollection> Projections { get; set; }
			public IEnumerable<IPreferenceDay> PreferenceDays { get; set; }
		}

	}
}