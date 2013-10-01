using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Rules
{
	public interface IWorkTimeStartEndExtractor
	{
		DateTime? WorkTimeStart(IEnumerable<IVisualLayer> layerCollection);
		DateTime? WorkTimeEnd(IEnumerable<IVisualLayer> layerCollection);
	}

	public class WorkTimeStartEndExtractor : IWorkTimeStartEndExtractor
	{
		public DateTime? WorkTimeStart(IEnumerable<IVisualLayer> layerCollection)
		{
			if (layerCollection == null) return null;

			var foundItem = layerCollection.FirstOrDefault(layer => layer.WorkTime() > TimeSpan.Zero);
			if (foundItem == null) return null;

			return foundItem.Period.StartDateTime;
		}

		public DateTime? WorkTimeEnd(IEnumerable<IVisualLayer> layerCollection)
		{
			if (layerCollection == null) return null;

			var foundItem = layerCollection.LastOrDefault(layer => layer.WorkTime() > TimeSpan.Zero);
			if (foundItem == null) return null;

			return foundItem.Period.EndDateTime;
		}
	}
}