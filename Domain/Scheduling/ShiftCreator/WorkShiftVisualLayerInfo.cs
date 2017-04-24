using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.ShiftCreator
{
    public class WorkShiftVisualLayerInfo
    {
	    public WorkShiftVisualLayerInfo(IWorkShift workShift, IVisualLayerCollection visualLayerCollection)
        {
            WorkShift = workShift;
            VisualLayerCollection = visualLayerCollection;
        }

        public IWorkShift WorkShift { get; }

	    public IVisualLayerCollection VisualLayerCollection { get; }
    }
}