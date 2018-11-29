using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.ShiftCreator
{
	public class ShiftsFromMasterActivityBaseActivityService : IShiftFromMasterActivityService
	{
		public IList<IWorkShift> ExpandWorkShiftsWithMasterActivity(IWorkShift workShift, bool baseIsMaster)
		{
			if (!hasMasterActivity(workShift))
				return new[] {workShift};

			using (PerformanceOutput.ForOperation("Creating shifts from master activity"))
			{
				var finalList = new List<IWorkShift>();
				Stack<IWorkShift> stack = new Stack<IWorkShift>(createShiftsFromShift(workShift, true, baseIsMaster));
				while (stack.Count > 0)
				{
					var shiftToTest = stack.Pop();
					var thisResult = createShiftsFromShift(shiftToTest, false, false);
					if (!thisResult.Any())
						finalList.Add(cleanWorkShiftFromRedundantLayers(shiftToTest));

					foreach (var shift in thisResult)
					{
						stack.Push(shift);
					}						
				}

				return finalList;
			}
		}

		private List<IWorkShift> createShiftsFromShift(IWorkShift workShift, bool firstRun, bool baseIsMaster)
		{
			if (!hasMasterActivity(workShift))
				return new List<IWorkShift>();

			if(firstRun && workShift.LayerCollection.First().Payload is MasterActivity)
			{
				var visualLayerCollection = workShift.Projection;
				workShift.LayerCollection.Clear();
				foreach (var visualLayer in visualLayerCollection)
				{
					var newLayer = new WorkShiftActivityLayer((IActivity)visualLayer.Payload, visualLayer.Period);
					workShift.LayerCollection.Add(newLayer);
				}
			}

			for (int index = 0; index < workShift.LayerCollection.Count; index++)
			{
				var layer = workShift.LayerCollection[index];
				var masterActivity = layer.Payload as MasterActivity;
				if (masterActivity == null)
				{
					if (!baseIsMaster)
						firstRun = false;

					continue;
				}

				var resultForOneLayer = new List<IWorkShift>();
				foreach (var replaceActivity in masterActivity.ActivityCollection)
				{
					if (replaceActivity.IsDeleted || !replaceActivity.InContractTime || !replaceActivity.RequiresSkill)
						continue;

					var newLength = firstRun ? workShift.Projection.Period().GetValueOrDefault() : layer.Period;
					if (firstRun)
					{
						workShift.LayerCollection.Remove(layer);
						workShift.LayerCollection.Insert(0, layer);
					}

					var newShift = localCloneWithNewActivityOnLayer(workShift.LayerCollection, layer, replaceActivity, newLength,
						workShift.ShiftCategory);
					resultForOneLayer.Add(newShift);
				}

				return resultForOneLayer;				
			}

			return new List<IWorkShift>();
		}

		private IWorkShift localCloneWithNewActivityOnLayer(ILayerCollection<IActivity> layerCollection,
			ILayer<IActivity> layerToReplace, IActivity newActivity, DateTimePeriod newLength, IShiftCategory shiftCategory)
		{
			var resultingShift = new WorkShift(shiftCategory);
			foreach (var originalLayer in layerCollection)
			{
				WorkShiftActivityLayer newLayer;
				if (originalLayer.Equals(layerToReplace))
				{
					newLayer = new WorkShiftActivityLayer(newActivity, newLength);				
				}
				else
				{
					newLayer = new WorkShiftActivityLayer(originalLayer.Payload, originalLayer.Period);
				}

				resultingShift.LayerCollection.Add(newLayer);
			}

			return resultingShift;
		}

		private static bool hasMasterActivity(IWorkShift workShift)
		{
			return workShift.LayerCollection.Any(l => l.Payload is IMasterActivity);
		}

		private IWorkShift cleanWorkShiftFromRedundantLayers(IWorkShift workShift)
		{
			var resultingShift = new WorkShift(workShift.ShiftCategory);
			var firstLayer = workShift.LayerCollection.First();
			var firstPayLoad = firstLayer.Payload;
			foreach (var originalLayer in workShift.LayerCollection)
			{
				WorkShiftActivityLayer newLayer;
				if(originalLayer.Equals(firstLayer))
				{
					newLayer = new WorkShiftActivityLayer(originalLayer.Payload, originalLayer.Period);
					resultingShift.LayerCollection.Add(newLayer);
				}
				else
				{
					if(originalLayer.Payload.Equals(firstPayLoad) &&  firstLayer.Period.Contains(originalLayer.Period))
						continue;

					newLayer = new WorkShiftActivityLayer(originalLayer.Payload, originalLayer.Period);
					resultingShift.LayerCollection.Add(newLayer);
				}
			}

			return resultingShift;
		}
	}
}