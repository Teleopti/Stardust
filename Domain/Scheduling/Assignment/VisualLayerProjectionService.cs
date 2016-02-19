using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	/// <summary>
	/// Creates projections of activity layers
	/// </summary>
	/// <remarks>
	/// Created by: rogerkr
	/// Created date: 2008-01-29
	/// </remarks>
	public class VisualLayerProjectionService : IProjectionService
	{
		private readonly IPerson _person;
		private readonly ILayerCollection<IPayload> _layerCollectionOriginal;
		private static readonly IVisualLayerFactory standardVisualLayerFactory = new VisualLayerFactory();

		public VisualLayerProjectionService(IPerson person)
		{
			_person = person;
			_layerCollectionOriginal = new LayerCollection<IPayload>();
		}

		public void Add(IEnumerable<ILayer<IActivity>> layers, IVisualLayerFactory visualLayerFactory)
		{
			foreach (var layer in layers)
			{
				Add(layer, visualLayerFactory);
			}
		}

		public void Add(ILayer<IActivity> layer, IVisualLayerFactory visualLayerFactory)
		{
			_layerCollectionOriginal.Add(visualLayerFactory.CreateShiftSetupLayer(layer, _person));
		}

		public void Add(IVisualLayer layer)
		{
			_layerCollectionOriginal.Add(layer);
		}

		public IVisualLayerCollection CreateProjection()
		{
			IList<IVisualLayer> workingColl = new List<IVisualLayer>();
			var startTimeTemp = _layerCollectionOriginal.FirstStart();
			var endTimeTemp = _layerCollectionOriginal.LatestEnd();

			if (!startTimeTemp.HasValue || !endTimeTemp.HasValue)
				return new VisualLayerCollection(_person, workingColl, new ProjectionPayloadMerger());

			var currentTime = startTimeTemp.Value;
			var endTime = endTimeTemp.Value;
			while (currentTime < endTime)
			{
				var layerFound = false;
				for (var inverseLoop = _layerCollectionOriginal.Count - 1; inverseLoop >= 0; inverseLoop--)
				{
					var workingLayer = (IVisualLayer) _layerCollectionOriginal[inverseLoop];
					if (!workingLayer.Period.Contains(currentTime)) continue;

					var layerEndTime = findLayerEndTime(inverseLoop, workingLayer, currentTime);
					var newLayer = standardVisualLayerFactory.CreateResultLayer(workingLayer.Payload,
						workingLayer, new DateTimePeriod(currentTime, layerEndTime), workingLayer.PersonAbsenceId);
					workingColl.Add(newLayer);
					currentTime = layerEndTime;
					layerFound = true;
					break;
				}
				if (!layerFound)
					currentTime = findNextTimeSlot(currentTime);
			}
			return new VisualLayerCollection(_person, workingColl, new ProjectionPayloadMerger());
		}

		private DateTime findLayerEndTime(int currentLayerIndex, IVisualLayer workingLayer, DateTime currentTime)
		{
			var layerEndTime = workingLayer.Period.EndDateTime;
			if (currentLayerIndex == _layerCollectionOriginal.Count - 1) return layerEndTime;

			var orgLayerCount = _layerCollectionOriginal.Count;
			for (var higherPrioLoop = currentLayerIndex + 1; higherPrioLoop < orgLayerCount; higherPrioLoop++)
			{
				var higherPrioLayerPeriod = _layerCollectionOriginal[higherPrioLoop].Period;
				if (workingLayer.Period.Contains(higherPrioLayerPeriod.StartDateTime) &&
					higherPrioLayerPeriod.EndDateTime > currentTime && higherPrioLayerPeriod.StartDateTime < layerEndTime)
				{
					layerEndTime = higherPrioLayerPeriod.StartDateTime;
				}
			}
			return layerEndTime;
		}

		private DateTime findNextTimeSlot(DateTime currentTime)
		{
			var retTime = DateTime.MaxValue;
			foreach (var layer in _layerCollectionOriginal)
			{
				var layerTime = layer.Period.StartDateTime;
				if (layerTime > currentTime && layerTime < retTime)
					retTime = layerTime;
			}
			return retTime;
		}
	}
}