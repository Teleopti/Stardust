using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	/// <summary>
	/// The normal merger for a projection. 
	/// If two layers with same payload and definitionset intersects,
	/// they will be merged into a single one
	/// </summary>
	public class ProjectionPayloadMerger : ProjectionMerger
	{
		protected override IList<IVisualLayer> ModifyCollection(IList<IVisualLayer> clonedUnmergedCollection)
		{
			for (var i = clonedUnmergedCollection.Count - 1; i > 0; i--)
			{
				var indexOfPrevious = i - 1;
				var currLayer = clonedUnmergedCollection[i];
				var prevLayer = clonedUnmergedCollection[indexOfPrevious];
				if (currLayer.Payload.OptimizedEquals(prevLayer.Payload)
						&& currLayer.AdjacentTo(prevLayer)
						&& currLayer.DefinitionSet == prevLayer.DefinitionSet)
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