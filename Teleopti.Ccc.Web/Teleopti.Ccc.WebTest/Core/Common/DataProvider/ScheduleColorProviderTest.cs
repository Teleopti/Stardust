using System;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;


namespace Teleopti.Ccc.WebTest.Core.Common.DataProvider
{
	[TestFixture]
	public class ScheduleColorProviderTest
	{

		[Test]
		public void ShouldReturnEmptyEnumerableOnNullSource()
		{
			var target = new ScheduleColorProvider(new FakeLoggedOnUser());

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
				stubs.PersonAssignmentStub(new DateTimePeriod(), stubs.ShiftCategoryStub(Color.RoyalBlue))
				);
			var scheduleDay2 = stubs.ScheduleDayStub(DateTime.Now, SchedulePartView.MainShift, 
				stubs.PersonAssignmentStub(new DateTimePeriod(), stubs.ShiftCategoryStub(Color.Pink))
				);
			var source = new ScheduleColorSource
			             	{
			             		ScheduleDays = new[] {scheduleDay1, scheduleDay2},
			             		Projections = new[] {projection1, projection2, projection3}
			             	};

			var target = new ScheduleColorProvider(new FakeLoggedOnUser());

			var result = target.GetColors(source);

			result.Should().Have.Count.EqualTo(3);
			result.Should().Have.SameValuesAs(new[] { Color.Pink, Color.BlueViolet, Color.RoyalBlue});
		}

		[Test]
		public void ShouldGetDisplayColorFromProjectionsVisualLayer()
		{
			var stubs = new StubFactory();
			var projection = stubs.ProjectionStub(new[] {stubs.VisualLayerStub(Color.Red)});
			var source = new ScheduleColorSource { Projections = new[] { projection } };

			var target = new ScheduleColorProvider(new FakeLoggedOnUser());

			var result = target.GetColors(source);

			result.Single().Should().Be(Color.Red);
		}

		[Test]
		public void ShouldGetDisplayColorFromScheduleDayPersonAssignment()
		{
			var stubs = new StubFactory();
			var personAssignment = stubs.PersonAssignmentStub(new DateTimePeriod(), stubs.ShiftCategoryStub(Color.Blue));
			var scheduleDay = stubs.ScheduleDayStub(DateTime.Now, SchedulePartView.MainShift, personAssignment);
			var source = new ScheduleColorSource { ScheduleDays = new[] { scheduleDay } };

			var target = new ScheduleColorProvider(new FakeLoggedOnUser());

			var result = target.GetColors(source);

			result.Single().Should().Be(Color.Blue);
		}

		[Test]
		public void ShouldGetDisplayColorFromPersonAbsence()
		{
			var stubs = new StubFactory();
			var personAbsence = stubs.PersonAbsenceStub(new DateTimePeriod(), stubs.AbsenceLayerStub(stubs.AbsenceStub(Color.Olive)));
			var scheduleDay = stubs.ScheduleDayStub(DateTime.Now, SchedulePartView.FullDayAbsence, personAbsence);
			var source = new ScheduleColorSource { ScheduleDays = new[] { scheduleDay } };

			var target = new ScheduleColorProvider(new FakeLoggedOnUser());

			var result = target.GetColors(source);

			result.Single().Should().Be(Color.Olive);
		}

		[Test]
		public void ShouldGetDisplayColorFromPreferenceShiftCategory()
		{
			var preferenceDay = new PreferenceDay(new Person(), DateOnly.Today,
			                                      new PreferenceRestriction
			                                      	{
			                                      		ShiftCategory = new ShiftCategory("sc")
			                                      		                	{
			                                      		                		DisplayColor = Color.Plum
			                                      		                	}
			                                      	});
			var source = new ScheduleColorSource { PreferenceDays = new[] { preferenceDay } };

			var target = new ScheduleColorProvider(new FakeLoggedOnUser());

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
			var source = new ScheduleColorSource { PreferenceDays = new[] { preferenceDay } };

			var target = new ScheduleColorProvider(new FakeLoggedOnUser());

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
			var source = new ScheduleColorSource { PreferenceDays = new[] { preferenceDay } };

			var target = new ScheduleColorProvider(new FakeLoggedOnUser());

			var result = target.GetColors(source);

			result.Single().Should().Be(Color.BlanchedAlmond);
		}

		[Test]
		public void ShouldGetDisplayColorFromWorkflowControlSetShiftCategory()
		{
			var shiftCategory = new ShiftCategory("sc") {DisplayColor = Color.Violet};
			var workflowControlSet = new WorkflowControlSet
			                         	{
			                         		AllowedPreferenceShiftCategories = new[] { shiftCategory }
			                         	};
			var source = new ScheduleColorSource {WorkflowControlSet = workflowControlSet};

			var target = new ScheduleColorProvider(new FakeLoggedOnUser());

			var result = target.GetColors(source);

			result.Single().Should().Be(Color.Violet);
		}

		[Test]
		public void ShouldGetDisplayColorFromWorkflowControlSetAbsence()
		{
			var absence = new Absence { DisplayColor = Color.Magenta };
			var workflowControlSet = new WorkflowControlSet
			                         	{
			                         		AllowedPreferenceAbsences = new[] {absence}
			                         	};
			var source = new ScheduleColorSource { WorkflowControlSet = workflowControlSet };

			var target = new ScheduleColorProvider(new FakeLoggedOnUser());

			var result = target.GetColors(source);

			result.Single().Should().Be(Color.Magenta);
		}

		[Test]
		public void ShouldGetDisplayColorFromWorkflowControlSetDayOffTemplates()
		{
			var dayOffTemplate = new DayOffTemplate(new Description()) { DisplayColor = Color.MediumSeaGreen};
			var workflowControlSet = new WorkflowControlSet
			                         	{
			                         		AllowedPreferenceDayOffs = new[] {dayOffTemplate}
			                         	};
			var source = new ScheduleColorSource { WorkflowControlSet = workflowControlSet };

			var target = new ScheduleColorProvider(new FakeLoggedOnUser());

			var result = target.GetColors(source);

			result.Single().Should().Be(Color.MediumSeaGreen);
		}

		[Test]
		public void ShouldGetDisplayColorFromPersonalShift()
		{
			var stubs = new StubFactory();
			var projection = stubs.ProjectionStub(new[] { stubs.VisualLayerStub(Color.LawnGreen) });
			var scheduleDay = stubs.ScheduleDayStub(DateTime.Now, SchedulePartView.PersonalShift, stubs.PersonAssignmentPersonalShiftStub());
			var source = new ScheduleColorSource
							{
								ScheduleDays = new[] { scheduleDay },
								Projections = new[] { projection }
							};

			var target = new ScheduleColorProvider(new FakeLoggedOnUser());

			var result = target.GetColors(source);

			result.Single().Should().Be(Color.LawnGreen);
		}

		[Test]
		public void ShouldGetDisplayColorFromOvertimeShift()
		{
			var stubs = new StubFactory();
			var projection = stubs.ProjectionStub(new[] { stubs.VisualLayerStub(Color.LawnGreen) });
			var scheduleDay = stubs.ScheduleDayStub(DateTime.Now, SchedulePartView.Overtime, stubs.PersonAssignmentPersonalShiftStub());
			var source = new ScheduleColorSource
			{
				ScheduleDays = new[] { scheduleDay },
				Projections = new[] { projection }
			};

			var target = new ScheduleColorProvider(new FakeLoggedOnUser());

			var result = target.GetColors(source);

			result.Single().Should().Be(Color.LawnGreen);
		}
	}
}