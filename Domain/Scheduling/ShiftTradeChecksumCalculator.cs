using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
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

	        var personAssignment = _scheduleDay.PersonAssignment();
					if (personAssignment != null)
					{
						if (personAssignment.DayOff() != null)
						{
							checksum = getDayOffChecksum(personAssignment);
						}
						else
						{
							var shift = new EditableShiftMapper().CreateEditorShift(personAssignment);
							if (shift != null)
							{
								var visualLayerCollection = shift.ProjectionService().CreateProjection();
								checksum = checksum ^ getMainShiftChecksum(visualLayerCollection);
							}
						}
					}

            return checksum;
        }

        private static long getMainShiftChecksum(IEnumerable<IVisualLayer> layerCollection)
        {
            uint checksum = 0;
            foreach (IVisualLayer layer in layerCollection)
            {
                checksum = checksum ^ 
                    Crc32.Compute(SerializationHelper.SerializeAsBinary(layer.Period.StartDateTime.Ticks)) ^
                    Crc32.Compute(SerializationHelper.SerializeAsBinary(layer.Period.EndDateTime.Ticks)) ^ 
                    Crc32.Compute(SerializationHelper.SerializeAsBinary(layer.Payload.Id.GetValueOrDefault(Guid.Empty)));
            }
            return checksum;
        }

        private static long getDayOffChecksum(IPersonAssignment assignment)
        {
	        var dayOff = assignment.DayOff();
					return Crc32.Compute(SerializationHelper.SerializeAsBinary(dayOff.Anchor.Ticks)) ^
						Crc32.Compute(SerializationHelper.SerializeAsBinary(dayOff.Flexibility.Ticks)) ^
						Crc32.Compute(SerializationHelper.SerializeAsBinary(dayOff.TargetLength.Ticks)) ^
						Crc32.Compute(SerializationHelper.SerializeAsBinary(assignment.Date.Date.Ticks));
        }
    }
}
