using System;
using System.Linq;
using Teleopti.Interfaces.Domain;

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
			IVisualLayer newStartLayer =null;
			IVisualLayer newEndLayer=null;
			int? layer2RemoveIndex =null;
			var result = clonedUnmergedCollection.ToList();
			for (var i = 0; i < result.Count; i++)
			{
				var visualLayer = (VisualLayer)result[i];
				var startLocal = visualLayer.Period.StartDateTimeLocal(_userZone);
				var endLocal = visualLayer.Period.EndDateTimeLocal(_userZone);
				if (spansTwoDays(startLocal, endLocal))
				{
					layer2RemoveIndex = i;
					var start = visualLayer.Period.StartDateTime;
					var end = visualLayer.Period.EndDateTime;
					var midnight = TimeZoneHelper.ConvertToUtc(endLocal.Date, _userZone);
					newStartLayer = cloneLayerWithNewPeriod(visualLayer, new DateTimePeriod(start, midnight));
					newEndLayer = cloneLayerWithNewPeriod(visualLayer, new DateTimePeriod(midnight, end));
					break;
				}
			}

			if (layer2RemoveIndex.HasValue)
			{
				result.RemoveAt(layer2RemoveIndex.Value);
				result.Insert(layer2RemoveIndex.Value, newEndLayer);
				result.Insert(layer2RemoveIndex.Value, newStartLayer);
			}
			return result.ToArray();
		}

		private static IVisualLayer cloneLayerWithNewPeriod(VisualLayer orgLayer, DateTimePeriod newPeriod)
		{
			var ret = new VisualLayer(orgLayer.Payload, newPeriod, orgLayer.HighestPriorityActivity,orgLayer.Person);
			ret.HighestPriorityAbsence = orgLayer.HighestPriorityAbsence;
			ret.DefinitionSet = orgLayer.DefinitionSet;

			return ret;
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