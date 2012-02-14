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
			DateTime? ret = null;
			if (layerCollection == null)
				return ret;

			foreach (var layer in layerCollection.Where(layer => layer.WorkTime() > TimeSpan.Zero))
			{
				ret = layer.Period.StartDateTime;
				break;
			}
			return ret;
		}

		public DateTime? WorkTimeEnd(IEnumerable<IVisualLayer> layerCollection)
		{
			DateTime? ret = null;
			if (layerCollection == null)
				return ret;

			foreach (var layer in layerCollection.Where(layer => layer.WorkTime() > TimeSpan.Zero))
			{
				ret = layer.Period.EndDateTime;
			}
			return ret;
		}
	}
}