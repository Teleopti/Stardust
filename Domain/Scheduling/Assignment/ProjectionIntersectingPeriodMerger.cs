using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	public class ProjectionIntersectingPeriodMerger : ProjectionMerger
	{
		protected override IList<IVisualLayer> ModifyCollection(IList<IVisualLayer> clonedUnmergedCollection)
		{
			for (var i = clonedUnmergedCollection.Count - 1; i > 0; i--)
			{
				var currLayer = clonedUnmergedCollection[i];
				var prevLayer = clonedUnmergedCollection[i - 1];
				if (currLayer.AdjacentTo(prevLayer))
				{
					prevLayer.ChangeLayerPeriodEnd(currLayer.Period.ElapsedTime());
					clonedUnmergedCollection.Remove(currLayer);
				}
			}
			return clonedUnmergedCollection;
		}

		public override object Clone()
		{
			//simply create a new object to remove internal cache
			return new ProjectionPayloadMerger();
		}
	}
}