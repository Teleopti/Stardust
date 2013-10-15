using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
    public class ShiftTradeChecksumCalculator
    {
        private readonly IScheduleDay _scheduleDay;

        public ShiftTradeChecksumCalculator(IScheduleDay scheduleDay)
        {
            _scheduleDay = scheduleDay;
        }

        public long CalculateChecksum()
        {
            long checksum = -1; // Default checksum indicates not scheduled

	        if (_scheduleDay.SignificantPart() == SchedulePartView.DayOff)
		        checksum = getDayOffChecksum();
	        else
	        {
		        foreach (var personAssignment in _scheduleDay.PersonAssignmentCollection())
		        {
			        if (!hasMainShift(personAssignment)) continue;

			        var visualLayerCollection =
				        personAssignment.MainShift.ProjectionService().CreateProjection();
			        checksum = checksum ^ getMainShiftChecksum(visualLayerCollection);
		        }
	        }

	        return checksum;
        }

        private static bool hasMainShift(IPersonAssignment personAssignment)
        {
            return personAssignment.MainShift != null;
        }

        private static long getMainShiftChecksum(IEnumerable<IVisualLayer> layerCollection)
        {
            uint checksum = 0;
            foreach (var layer in layerCollection)
            {
                checksum = checksum ^ 
                    Crc32.Compute(SerializationHelper.SerializeAsBinary(layer.Period.StartDateTime.Ticks)) ^
                    Crc32.Compute(SerializationHelper.SerializeAsBinary(layer.Period.EndDateTime.Ticks)) ^ 
                    Crc32.Compute(SerializationHelper.SerializeAsBinary(layer.Payload.Id.GetValueOrDefault(Guid.Empty)));
            }
            return checksum;
        }

        private long getDayOffChecksum()
        {
            return _scheduleDay.PersonDayOffCollection()[0].Checksum();
        }
    }
}
