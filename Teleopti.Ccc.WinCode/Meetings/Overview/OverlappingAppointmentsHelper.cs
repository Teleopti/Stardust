using System.Collections.Generic;

namespace Teleopti.Ccc.WinCode.Meetings.Overview
{
	public interface IOverlappingAppointmentsHelper
	{
		bool HasTooManyOverlapping(IList<ISimpleAppointment> simpleAppointments);
		IList<ISimpleAppointment> ReduceOverlappingToFive(IList<ISimpleAppointment> appointments);
	}

	public class OverlappingAppointmentsHelper : IOverlappingAppointmentsHelper
	{
		public bool HasTooManyOverlapping(IList<ISimpleAppointment> simpleAppointments)
		{
			if (getFirstOverlappingWhenMoreThanFive(simpleAppointments) != null)
				return true;
			return false;
		}

		public IList<ISimpleAppointment> ReduceOverlappingToFive(IList<ISimpleAppointment> appointments)
		{
			if (appointments != null)
			{
				var nextAppointment = getFirstOverlappingWhenMoreThanFive(appointments);
				while (nextAppointment != null)
				{
					appointments.Remove(nextAppointment);
					nextAppointment = getFirstOverlappingWhenMoreThanFive(appointments);
				}
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
					appointmentsToMark.Clear();
					countOverlapping = 1;
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