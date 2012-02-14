using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.ShiftCreator
{
    public class WorkShiftVisualLayerInfo : IWorkShiftVisualLayerInfo
    {
        private readonly IWorkShift _workShift;
        private readonly IVisualLayerCollection _visualLayerCollection;

        public WorkShiftVisualLayerInfo(IWorkShift workShift, IVisualLayerCollection visualLayerCollection)
        {
            _workShift = workShift;
            _visualLayerCollection = visualLayerCollection;
        }

        public IWorkShift WorkShift
        {
            get { return _workShift; }
        }

        public IVisualLayerCollection VisualLayerCollection
        {
            get { return _visualLayerCollection; }
        }
    }
}