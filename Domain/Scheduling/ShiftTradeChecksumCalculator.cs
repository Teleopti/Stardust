﻿using System;
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
            {
                checksum = GetDayOffChecksum();
            }
            else
            {
                foreach (IPersonAssignment personAssignment in _scheduleDay.PersonAssignmentCollection())
                {
                    if (hasMainShift(personAssignment))
                    {
                        IVisualLayerCollection visualLayerCollection =
                            personAssignment.ToMainShift().ProjectionService().CreateProjection();
                        checksum = checksum ^ GetMainShiftChecksum(visualLayerCollection);
                    }
                }
            }

            return checksum;
        }

        private static bool hasMainShift(IPersonAssignment personAssignment)
        {
            return personAssignment.ToMainShift() != null;
        }

        private static long GetMainShiftChecksum(IEnumerable<IVisualLayer> layerCollection)
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

        private long GetDayOffChecksum()
        {
            return _scheduleDay.PersonDayOffCollection()[0].Checksum();
        }
    }
}
