using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.WorkShiftFilters
{
	public interface ICommonMainShiftFilter
	{
		IList<IShiftProjectionCache> Filter(IList<IShiftProjectionCache> shiftList, IEffectiveRestriction effectiveRestriction);
	}
	
	public class CommonMainShiftFilter : ICommonMainShiftFilter
	{
		public IList<IShiftProjectionCache> Filter(IList<IShiftProjectionCache> shiftList, IEffectiveRestriction effectiveRestriction)
		{
			if (effectiveRestriction.CommonMainShift != null)
			{
				var shift = shiftList.FirstOrDefault(x => mainShiftsEqual(x.TheMainShift, effectiveRestriction.CommonMainShift));
				if (shift != null)
					return new List<IShiftProjectionCache> { shift };
				return null;
			}
			return shiftList;
		}

		private static bool mainShiftsEqual(IMainShift original, IMainShift current)
		{
			if (original.ShiftCategory.Id != current.ShiftCategory.Id)
				return false;
			if (original.LayerCollection.Count != current.LayerCollection.Count)
				return false;
			for (int layerIndex = 0; layerIndex < original.LayerCollection.Count; layerIndex++)
			{
				ILayer<IActivity> originalLayer = original.LayerCollection[layerIndex];
				ILayer<IActivity> currentLayer = current.LayerCollection[layerIndex];
				if (!originalLayer.Period.StartDateTime.TimeOfDay.Equals(currentLayer.Period.StartDateTime.TimeOfDay))
					return false;
				if (!originalLayer.Period.EndDateTime.TimeOfDay.Equals(currentLayer.Period.EndDateTime.TimeOfDay))
					return false;
				if (!originalLayer.Payload.Equals(currentLayer.Payload))
					return false;
			}
			return true;
		}

	}
}
