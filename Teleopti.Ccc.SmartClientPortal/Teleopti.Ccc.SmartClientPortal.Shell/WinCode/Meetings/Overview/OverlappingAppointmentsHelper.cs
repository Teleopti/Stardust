using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Overview
{
	public interface IOverlappingAppointmentsHelper
	{
		IEnumerable<ISimpleAppointment> ReduceOverlappingToFive(IEnumerable<ISimpleAppointment> appointments);
	}

	public class OverlappingAppointmentsHelper : IOverlappingAppointmentsHelper
	{
		public IEnumerable<ISimpleAppointment> ReduceOverlappingToFive(IEnumerable<ISimpleAppointment> appointments)
		{
			if (appointments != null)
			{
				var newAppointmentList = appointments.ToList();
				var nextAppointment = getFirstOverlappingWhenMoreThanFive(newAppointmentList);
				while (nextAppointment != null)
				{
					newAppointmentList.Remove(nextAppointment);
					nextAppointment = getFirstOverlappingWhenMoreThanFive(newAppointmentList);
				}
				appointments = newAppointmentList;
			}
			return appointments;
		}

		private static ISimpleAppointment getFirstOverlappingWhenMoreThanFive(IList<ISimpleAppointment> simpleAppointments)
		{
			int countOverlapping = 1;
			int lastIndex = simpleAppointments.Count - 1;
			var appointmentsToMark = new List<ISimpleAppointment>();
			foreach (var simpleAppointment in simpleAppointments)
			{
				var nextIndex = simpleAppointments.IndexOf(simpleAppointment) + 1;
				if (nextIndex > lastIndex) continue;
				var nextAppointment = simpleAppointments[nextIndex];
				if (nextAppointment.StartDateTime < simpleAppointment.EndDateTime.AddMinutes(FindRemainsToEvenHalfHour(simpleAppointment.EndDateTime.Minute)))
				{
					appointmentsToMark.Add(simpleAppointment);
					countOverlapping += 1;
				}
				else
				{
					var overlap = false;
					foreach (var appointment in appointmentsToMark)
					{
						if (nextAppointment.StartDateTime < appointment.EndDateTime.AddMinutes(FindRemainsToEvenHalfHour(appointment.EndDateTime.Minute)))
						{
							overlap = true;
							appointmentsToMark.Add(nextAppointment);
							break;
						}
					}

					if (overlap == false)
					{
						appointmentsToMark.Clear();
						countOverlapping = 1;
					}
				}

				if (countOverlapping > 5)
				{
					foreach (var appointment in appointmentsToMark)
					{
						appointment.OtherHasBeenDeleted = true;
					}
					return nextAppointment;
				}
			}
			return null;
		}

		
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2233:OperationsShouldNotOverflow", MessageId = "30-minute")]
		public static int FindRemainsToEvenHalfHour(int minute)
		{
			if (minute == 0) return 0;
			var result = 30 - minute;
			if (result < 0)
				return 30 + result;

			return result;
		}
	}
}