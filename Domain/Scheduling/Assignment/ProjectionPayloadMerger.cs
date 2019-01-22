using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	/// <summary>
	/// The normal merger for a projection. 
	/// If two layers with same payload and definitionset intersects,
	/// they will be merged into a single one
	/// </summary>
	public class ProjectionPayloadMerger : ProjectionMerger
	{
		protected override IEnumerable<IVisualLayer> ModifyCollection(IEnumerable<IVisualLayer> clonedUnmergedCollection)
		{
			var result = clonedUnmergedCollection.ToList();
			for (var i = result.Count - 1; i > 0; i--)
			{
				var indexOfPrevious = i - 1;
				var currLayer = result[i];
				var prevLayer = result[indexOfPrevious];
				if (currLayer.Payload.OptimizedEquals(prevLayer.Payload)
						&& currLayer.Period.AdjacentTo(prevLayer.Period)
						&& currLayer.DefinitionSet == prevLayer.DefinitionSet)
				{
					result.Remove(currLayer);
					result.Remove(prevLayer);
					var newLayerPeriod = new DateTimePeriod(prevLayer.Period.StartDateTime,
					                                        prevLayer.Period.EndDateTime.Add(currLayer.Period.ElapsedTime()));
					result.Insert(indexOfPrevious, prevLayer.CloneWithNewPeriod(newLayerPeriod));
				}
			}
			return result.ToArray();
		}

		public override object Clone()
		{
			//simply create a new object to remove internal cache
			return new ProjectionPayloadMerger();
		}
	}
}