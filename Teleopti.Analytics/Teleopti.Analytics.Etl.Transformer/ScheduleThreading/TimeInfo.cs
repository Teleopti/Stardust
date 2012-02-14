using System;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.Transformer.ScheduleThreading
{
    internal class TimeInfo
    {
        public TimeInfoObject TotalTime { get; private set; }
        public TimeInfoObject ContractTime { get; private set; }
        public TimeInfoObject WorkTime { get; private set; }
        public TimeInfoObject PaidTime { get; private set; }
        public TimeInfoObject OverTime { get; private set; }

        public TimeInfo(IPayload payload, ILayer<IPayload> layer, IVisualLayerCollection projectedLayerCollection)
        {
            TotalTime = new TimeInfoObject();
            ContractTime = new TimeInfoObject();
            WorkTime = new TimeInfoObject();
            PaidTime = new TimeInfoObject();
            OverTime = new TimeInfoObject();

            var payloadLayer = (VisualLayer)layer;

            TotalTime.Time = Convert.ToInt32(layer.Period.ElapsedTime().TotalMinutes);
            ContractTime.Time = (int)payloadLayer.ContractTime().TotalMinutes;
            WorkTime.Time = (int)payloadLayer.WorkTime().TotalMinutes;
            PaidTime.Time = (int)payloadLayer.PaidTime().TotalMinutes;
            TimeSpan ot = projectedLayerCollection.FilterLayers(layer.Period).Overtime();
            OverTime.Time = (int)ot.TotalMinutes;

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