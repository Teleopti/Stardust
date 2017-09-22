using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Analytics.Etl.Common.Transformer.ScheduleThreading
{
	internal class TimeInfo
	{
		public TimeInfoObject TotalTime { get; private set; }
		public TimeInfoObject ContractTime { get; private set; }
		public TimeInfoObject WorkTime { get; private set; }
		public TimeInfoObject PaidTime { get; private set; }
		public TimeInfoObject OverTime { get; private set; }

		public TimeInfo(IPayload payload, ILayer<IPayload> layer, IVisualLayerCollection layerCollection)
		{
			TotalTime = new TimeInfoObject();
			ContractTime = new TimeInfoObject();
			WorkTime = new TimeInfoObject();
			PaidTime = new TimeInfoObject();
			OverTime = new TimeInfoObject();

			TotalTime.Time = Convert.ToInt32(layer.Period.ElapsedTime().TotalMinutes);
			ContractTime.Time = (int)layerCollection.ContractTime().TotalMinutes;
			WorkTime.Time = (int)layerCollection.WorkTime().TotalMinutes;
			PaidTime.Time = (int)layerCollection.PaidTime().TotalMinutes;
			OverTime.Time = (int)layerCollection.Overtime().TotalMinutes;

			var meetingPayload = payload as IMeetingPayload;
			var activity = payload as IActivity;
			var absence = payload as IAbsence;
			if (meetingPayload != null)
			{
				activity = meetingPayload.Meeting.Activity;
			}
			if (absence != null)
			{
				TotalTime.AbsenceTime = TotalTime.Time;
				ContractTime.AbsenceTime = ContractTime.Time;
				WorkTime.AbsenceTime = WorkTime.Time;
				PaidTime.AbsenceTime = PaidTime.Time;
				OverTime.AbsenceTime = OverTime.Time;
			}

			if (activity != null)
			{
				TotalTime.ActivityTime = TotalTime.Time;
				ContractTime.ActivityTime = ContractTime.Time;
				WorkTime.ActivityTime = WorkTime.Time;
				PaidTime.ActivityTime = PaidTime.Time;
				OverTime.ActivityTime = OverTime.Time;
			}
		}
	}
}