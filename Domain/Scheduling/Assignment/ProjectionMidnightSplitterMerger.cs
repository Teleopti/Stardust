using System;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	/// <summary>
	/// Does not merge at all, instead splits layers around midnight
	/// (in timezone userZone)
	/// </summary>
	public class ProjectionMidnightSplitterMerger : ProjectionMerger
	{
		private readonly TimeZoneInfo _userZone;

		public ProjectionMidnightSplitterMerger(TimeZoneInfo userZone)
		{
			_userZone = userZone;
		}

		protected override IVisualLayer[] ModifyCollection(IVisualLayer[] clonedUnmergedCollection)
		{
			var result = clonedUnmergedCollection.ToList();
			for (var i = 0; i < result.Count; i++)
			{
				var visualLayer = (VisualLayer)result[i];
				var startLocal = visualLayer.Period.StartDateTimeLocal(_userZone);
				var endLocal = visualLayer.Period.EndDateTimeLocal(_userZone);
				if (spansTwoDays(startLocal, endLocal))
				{
					int layer2RemoveIndex =i;
					var start = visualLayer.Period.StartDateTime;
					var end = visualLayer.Period.EndDateTime;
					var midnight = TimeZoneHelper.ConvertToUtc(startLocal.Date.AddDays(1), _userZone);
					var newStartLayer =cloneLayerWithNewPeriod(visualLayer, new DateTimePeriod(start, midnight));
					var newEndLayer=cloneLayerWithNewPeriod(visualLayer, new DateTimePeriod(midnight, end));

					result.RemoveAt(layer2RemoveIndex);
					result.Insert(layer2RemoveIndex, newEndLayer);
					result.Insert(layer2RemoveIndex, newStartLayer);
				}
			}
			return result.ToArray();
		}

		private static IVisualLayer cloneLayerWithNewPeriod(VisualLayer orgLayer, DateTimePeriod newPeriod)
		{
			return new VisualLayer(orgLayer.Payload, newPeriod, orgLayer.HighestPriorityActivity)
			{
				HighestPriorityAbsence = orgLayer.HighestPriorityAbsence,
				DefinitionSet = orgLayer.DefinitionSet
			};
		}
		
		private static bool spansTwoDays(DateTime startLocal, DateTime endLocal)
		{
			return startLocal.Day != endLocal.AddMilliseconds(-1).Day;
		}

		public override object Clone()
		{
			return new ProjectionMidnightSplitterMerger(_userZone);
		}
	}
}