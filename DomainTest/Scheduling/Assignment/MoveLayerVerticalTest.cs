using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Assignment
{
	[TestFixture]
	public class MoveLayerVerticalTest
	{
		[Test]
		public void ShouldMoveUpMainshiftLayer()
		{
			var target = new MoveLayerVertical();
			var ass = PersonAssignmentFactory.CreateAssignmentWithThreeMainshiftLayers();
			var orgLayers = new List<IMainShiftLayer>(ass.MainLayers());
			var firstLayer = orgLayers[0];
			var middleLayer = orgLayers[1];
			var lastLayer = orgLayers[2];

			target.MoveUp(ass, lastLayer);
			//new instances - cant check for equality
			var res = ass.MainLayers().ToArray();
			res[0].Period.Should().Be.EqualTo(firstLayer.Period);
			res[1].Period.Should().Be.EqualTo(lastLayer.Period);
			res[2].Period.Should().Be.EqualTo(middleLayer.Period);
		}

		[Test]
		public void ShouldMoveUpOvertimeLayer()
		{
			var target = new MoveLayerVertical();
			var ass = PersonAssignmentFactory.CreateAssignmentWithThreeOvertimeLayers();
			var orgLayers = ass.OvertimeLayers().ToList();
			var firstLayer = orgLayers[0];
			var middleLayer = orgLayers[1];
			var lastLayer = orgLayers[2];

			target.MoveUp(ass, lastLayer);
			//new instances - cant check for equality
			var res = ass.OvertimeLayers().ToArray();
			res[0].Period.Should().Be.EqualTo(firstLayer.Period);
			res[1].Period.Should().Be.EqualTo(lastLayer.Period);
			res[2].Period.Should().Be.EqualTo(middleLayer.Period);
		}

		[Test]
		public void ShouldMoveUpPersonalLayer()
		{
			var target = new MoveLayerVertical();
			var ass = PersonAssignmentFactory.CreateAssignmentWithThreePersonalLayers();
			var orgLayers = ass.PersonalLayers().ToArray();
			var firstLayer = orgLayers[0];
			var middleLayer = orgLayers[1];
			var lastLayer = orgLayers[2];
			target.MoveUp(ass, lastLayer);
			//new instances - cant check for equality
			var res = ass.PersonalLayers().ToArray();
			res[0].Period.Should().Be.EqualTo(firstLayer.Period);
			res[1].Period.Should().Be.EqualTo(lastLayer.Period);
			res[2].Period.Should().Be.EqualTo(middleLayer.Period);
		}

		[Test]
		public void ShouldMoveDownMainshiftLayer()
		{
			var target = new MoveLayerVertical();
			var ass = PersonAssignmentFactory.CreateAssignmentWithThreeMainshiftLayers();
			var orgLayers = new List<IMainShiftLayer>(ass.MainLayers());
			var firstLayer = orgLayers[0];
			var middleLayer = orgLayers[1];
			var lastLayer = orgLayers[2];

			target.MoveDown(ass, firstLayer);
			//new instances - cant check for equality
			var res = ass.MainLayers().ToArray();
			res[0].Period.Should().Be.EqualTo(middleLayer.Period);
			res[1].Period.Should().Be.EqualTo(firstLayer.Period);
			res[2].Period.Should().Be.EqualTo(lastLayer.Period);
		}

		[Test]
		public void ShouldMoveDownOvertimeLayer()
		{
			var target = new MoveLayerVertical();
			var ass = PersonAssignmentFactory.CreateAssignmentWithThreeOvertimeLayers();
			var orgLayers = ass.OvertimeLayers().ToList();
			var firstLayer = orgLayers[0];
			var middleLayer = orgLayers[1];
			var lastLayer = orgLayers[2];

			target.MoveDown(ass, firstLayer);
			//new instances - cant check for equality
			var res = ass.OvertimeLayers().ToArray();
			res[0].Period.Should().Be.EqualTo(middleLayer.Period);
			res[1].Period.Should().Be.EqualTo(firstLayer.Period);
			res[2].Period.Should().Be.EqualTo(lastLayer.Period);
		}

		[Test]
		public void ShouldMoveDownPersonalLayer()
		{
			var target = new MoveLayerVertical();
			var ass = PersonAssignmentFactory.CreateAssignmentWithThreePersonalLayers();
			var orgLayers = ass.PersonalLayers().ToArray();
			var firstLayer = orgLayers[0];
			var middleLayer = orgLayers[1];
			var lastLayer = orgLayers[2];
			target.MoveDown(ass, firstLayer);
			//new instances - cant check for equality
			var res = ass.PersonalLayers().ToArray();
			res[0].Period.Should().Be.EqualTo(middleLayer.Period);
			res[1].Period.Should().Be.EqualTo(firstLayer.Period);
			res[2].Period.Should().Be.EqualTo(lastLayer.Period);
		}

		[Test]
		public void ShouldMoveUpPersonalLayerWhenOtherLayerIsAbove()
		{
			var activityToLookFor = new Activity("d");
			var target = new MoveLayerVertical();
			var ass = new PersonAssignment(new Person(), new Scenario("sd"), new DateOnly(2000, 1, 1));
			ass.AddActivity(new Activity("d"), new DateTimePeriod());
			ass.AddPersonalActivity(activityToLookFor, new DateTimePeriod());
			ass.AddPersonalActivity(new Activity("d"), new DateTimePeriod());

			target.MoveUp(ass, ass.PersonalLayers().Last());

			ass.PersonalLayers().Last().Payload.Should().Be.SameInstanceAs(activityToLookFor);
		}

		[Test]
		public void ShouldMoveDownOvertimeLayerWhenOtherLayerIsAbove()
		{
			var activityToLookFor = new Activity("d");
			var target = new MoveLayerVertical();
			var ass = new PersonAssignment(new Person(), new Scenario("sd"), new DateOnly(2000, 1, 1));
			ass.AddActivity(new Activity("d"), new DateTimePeriod());
			ass.AddOvertimeActivity(activityToLookFor, new DateTimePeriod(),null);
			ass.AddOvertimeActivity(new Activity("d"), new DateTimePeriod(), null);

			target.MoveDown(ass, ass.OvertimeLayers().First());

			ass.OvertimeLayers().Last().Payload.Should().Be.SameInstanceAs(activityToLookFor);
		}
	}
}