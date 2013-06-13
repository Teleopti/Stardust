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
				var indexOfPrevious = i - 1;
				var currLayer = clonedUnmergedCollection[i];
				var prevLayer = clonedUnmergedCollection[indexOfPrevious];
				if (currLayer.AdjacentTo(prevLayer))
				{
					clonedUnmergedCollection.Remove(currLayer);
					clonedUnmergedCollection.Remove(prevLayer);
					var newLayerPeriod = new DateTimePeriod(prevLayer.Period.StartDateTime,
																				prevLayer.Period.EndDateTime.Add(currLayer.Period.ElapsedTime()));
					clonedUnmergedCollection.Insert(indexOfPrevious, prevLayer.CloneWithNewPeriod(newLayerPeriod));
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