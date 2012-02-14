using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Tracking
{
    
    public class SchedulePartTrackingDataProvider : ITrackingDataProvider
    {
        private IScheduleDay _part;

        public SchedulePartTrackingDataProvider(IScheduleDay schedulePart)
        {
            _part = schedulePart;
        }

        public IVisualLayerCollection CreateVisualLayerCollection()
        {
            return _part.ProjectionService().CreateProjection();
        }
    }
}
