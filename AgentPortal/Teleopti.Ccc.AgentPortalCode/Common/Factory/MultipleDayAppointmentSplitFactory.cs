using System;
using System.Collections.Generic;
using Teleopti.Ccc.AgentPortalCode.ScheduleControlDataProvider;

namespace Teleopti.Ccc.AgentPortalCode.Common.Factory
{
    /// <summary>
    /// Class that splits multiday appointments in to two or more 
    /// single appointments.
    /// </summary>
    public static class MultipleDayAppointmentSplitFactory
    {
        public static IList<ICustomScheduleAppointment> Split(IList<ICustomScheduleAppointment> sourceCollection)
        {
            IList<ICustomScheduleAppointment> splittedCollection = new List<ICustomScheduleAppointment>();
            foreach (ICustomScheduleAppointment appointment in sourceCollection)
            {
                if (StartAndEndOnSameDate(appointment) ||
                    !appointment.AllowMultipleDaySplit)
                {
                    appointment.SplitPartType = ScheduleAppointmentPartType.None;
                    splittedCollection.Add(appointment);

                    continue;
                }

                SplitMultipleDayAppointmentIntoParts(appointment, splittedCollection);
            }

            return splittedCollection;
        }

        private static void SplitMultipleDayAppointmentIntoParts(ICustomScheduleAppointment appointment, IList<ICustomScheduleAppointment> splittedCollection)
        {
            DateTime startDateTime = appointment.StartTime;
            DateTime endDateTime = appointment.EndTime;
            for (DateTime currentDateEnd = startDateTime.Date.AddDays(1); currentDateEnd <= endDateTime.Date.AddDays(1); currentDateEnd = currentDateEnd.AddDays(1))
            {
                ICustomScheduleAppointment splittedPart = appointment.Clone() as ICustomScheduleAppointment;
                if (splittedPart != null)
                {
                    splittedPart.StartTime = startDateTime;
                    splittedPart.EndTime = appointment.EndTime < currentDateEnd
                                               ?  appointment.EndTime
                                               : currentDateEnd.AddTicks(-1);

                    splittedPart.IsSplit = true;
                    splittedPart.SplitPartType = (splittedPart.StartTime == appointment.StartTime) ?
                                                                                                       ScheduleAppointmentPartType.First :
                                                                                                                                             appointment.EndTime < currentDateEnd ?
                                                                                                                                                                                      ScheduleAppointmentPartType.Last :
                                                                                                                                                                                                                           ScheduleAppointmentPartType.Middle;
                    splittedCollection.Add(splittedPart);
                }
                startDateTime = currentDateEnd;
            }
        }

        private static bool StartAndEndOnSameDate(ICustomScheduleAppointment appointment)
        {
            return appointment.StartTime.Date == appointment.EndTime.AddMilliseconds(-1).Date;
        }
    }
}
