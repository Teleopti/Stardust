﻿using System.Collections.Generic;
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
			var orgLayers = new List<IMainShiftLayer>(ass.MainLayers);
			var firstLayer = orgLayers[0];
			var middleLayer = orgLayers[1];
			var lastLayer = orgLayers[2];

			target.MoveUp(ass, lastLayer);
			ass.MainLayers.Should().Have.SameSequenceAs(firstLayer, lastLayer, middleLayer);
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
		public void ShouldMoveUpPersonalLayer()
		{
			var target = new MoveLayerVertical();
			var ass = PersonAssignmentFactory.CreateAssignmentWithThreePersonalLayers();
			var orgLayers = ass.PersonalLayers.ToArray();
			var firstLayer = orgLayers[0];
			var middleLayer = orgLayers[1];
			var lastLayer = orgLayers[2];
			target.MoveUp(ass, lastLayer);
			//new instances - cant check for equality
			var res = ass.PersonalLayers.ToArray();
			res[0].Period.Should().Be.EqualTo(firstLayer.Period);
			res[1].Period.Should().Be.EqualTo(lastLayer.Period);
			res[2].Period.Should().Be.EqualTo(middleLayer.Period);
		}

		[Test]
		public void ShouldMoveDownMainshiftLayer()
		{
			var target = new MoveLayerVertical();
			var ass = PersonAssignmentFactory.CreateAssignmentWithThreeMainshiftLayers();
			var orgLayers = new List<IMainShiftLayer>(ass.MainLayers);
			var firstLayer = orgLayers[0];
			var middleLayer = orgLayers[1];
			var lastLayer = orgLayers[2];

			target.MoveDown(ass, firstLayer);
			ass.MainLayers.Should().Have.SameSequenceAs(middleLayer, firstLayer, lastLayer);
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

		[Test]
		public void ShouldMoveDownPersonalLayer()
		{
			var target = new MoveLayerVertical();
			var ass = PersonAssignmentFactory.CreateAssignmentWithThreePersonalLayers();
			var orgLayers = ass.PersonalLayers.ToArray();
			var firstLayer = orgLayers[0];
			var middleLayer = orgLayers[1];
			var lastLayer = orgLayers[2];
			target.MoveDown(ass, firstLayer);
			//new instances - cant check for equality
			var res = ass.PersonalLayers.ToArray();
			res[0].Period.Should().Be.EqualTo(middleLayer.Period);
			res[1].Period.Should().Be.EqualTo(firstLayer.Period);
			res[2].Period.Should().Be.EqualTo(lastLayer.Period);
		}
	}
}