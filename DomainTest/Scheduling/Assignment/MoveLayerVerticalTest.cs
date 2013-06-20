using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
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
			var orgLayers = new List<IMainShiftLayer>(ass.MainShiftLayers);
			var firstLayer = orgLayers[0];
			var middleLayer = orgLayers[1];
			var lastLayer = orgLayers[2];

			target.MoveUp(ass, lastLayer);
			ass.MainShiftLayers.Should().Have.SameSequenceAs(firstLayer, lastLayer, middleLayer);
		}

		[Test]
		public void ShouldMoveUpOvertimeLayer()
		{
			var target = new MoveLayerVertical();
			var ass = PersonAssignmentFactory.CreateAssignmentWithThreeOvertimeLayers();
			var overtimeShift = ass.OvertimeShiftCollection.Single();
			var orgLayers = new List<IOvertimeShiftActivityLayer>(overtimeShift.LayerCollectionWithDefinitionSet());
			var firstLayer = orgLayers[0];
			var middleLayer = orgLayers[1];
			var lastLayer = orgLayers[2];

			target.MoveUp(ass, lastLayer);
			ass.OvertimeShiftCollection.Single().LayerCollection.Should().Have.SameSequenceAs(firstLayer, lastLayer, middleLayer);
		}

		[Test]
		public void ShouldMoveDownMainshiftLayer()
		{
			var target = new MoveLayerVertical();
			var ass = PersonAssignmentFactory.CreateAssignmentWithThreeMainshiftLayers();
			var orgLayers = new List<IMainShiftLayer>(ass.MainShiftLayers);
			var firstLayer = orgLayers[0];
			var middleLayer = orgLayers[1];
			var lastLayer = orgLayers[2];

			target.MoveDown(ass, firstLayer);
			ass.MainShiftLayers.Should().Have.SameSequenceAs(middleLayer, firstLayer, lastLayer);
		}

		[Test]
		public void ShouldMoveDownOvertimeLayer()
		{
			var target = new MoveLayerVertical();
			var ass = PersonAssignmentFactory.CreateAssignmentWithThreeOvertimeLayers();
			var overtimeShift = ass.OvertimeShiftCollection.Single();
			var orgLayers = new List<IOvertimeShiftActivityLayer>(overtimeShift.LayerCollectionWithDefinitionSet());
			var firstLayer = orgLayers[0];
			var middleLayer = orgLayers[1];
			var lastLayer = orgLayers[2];

			target.MoveDown(ass, firstLayer);
			ass.OvertimeShiftCollection.Single().LayerCollection.Should().Have.SameSequenceAs(middleLayer, firstLayer, lastLayer);
		}
	}
}