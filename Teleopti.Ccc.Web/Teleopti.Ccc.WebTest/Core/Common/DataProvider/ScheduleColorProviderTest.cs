using System;
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
			var source = new[]
			             	{
			             		new FakeScheduleColorSource {Projection = projection1, ScheduleDay = scheduleDay1},
			             		new FakeScheduleColorSource {Projection = projection2, ScheduleDay = scheduleDay2},
			             		new FakeScheduleColorSource {Projection = projection3}
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
			var source = new[] {new FakeScheduleColorSource {Projection = projection}};

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
			var source = new[] { new FakeScheduleColorSource { ScheduleDay = scheduleDay } };

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
			var source = new[] { new FakeScheduleColorSource { ScheduleDay = scheduleDay } };

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
			var source = new[] {new FakeScheduleColorSource {PreferenceDay = preferenceDay}};

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
			var source = new[] { new FakeScheduleColorSource { PreferenceDay = preferenceDay } };

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
			var source = new[] { new FakeScheduleColorSource { PreferenceDay = preferenceDay } };

			var target = new ScheduleColorProvider();

			var result = target.GetColors(source);

			result.Single().Should().Be(Color.BlanchedAlmond);
		}

		private class FakeScheduleColorSource : IScheduleColorSource
		{
			public IScheduleDay ScheduleDay { get; set; }
			public IVisualLayerCollection Projection { get; set; }
			public IPreferenceDay PreferenceDay { get; set; }
		}

	}
}