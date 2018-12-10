using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	//rk: needs refactoring..
	//IVisualLayerFactory should only have one CreateLayer (maybe two overloaded)
	//and instead have multiple objects of IVisualLayerFactory (one for absence, meeting and so forth)
	public class VisualLayerFactory : IVisualLayerFactory
	{
		public virtual IVisualLayer CreateShiftSetupLayer(IActivity activity, DateTimePeriod period)
		{
			return new VisualLayer(activity, period, activity);
		}

		public virtual IVisualLayer CreateShiftSetupLayer(ILayer<IActivity> layer)
		{
			var ret = new VisualLayer(layer.Payload, layer.Period, layer.Payload);
			if (layer is OvertimeShiftLayer overtimeLayer)
			{
				ret.DefinitionSet = overtimeLayer.DefinitionSet;
			}
			return ret;
		}


		public IVisualLayer CreateMeetingSetupLayer(IMeetingPayload meetingPayload, IVisualLayer originalLayer, DateTimePeriod period)
		{
			return new VisualLayer(meetingPayload, period, meetingPayload.Meeting.Activity)
			{
				DefinitionSet = originalLayer.DefinitionSet
			};
		}


		public IVisualLayer CreateAbsenceSetupLayer(IAbsence absence, IVisualLayer originalLayer, DateTimePeriod period)
		{
			return new VisualLayer(absence, period, ((VisualLayer) originalLayer).HighestPriorityActivity)
			{
				HighestPriorityAbsence = absence,
				DefinitionSet = originalLayer.DefinitionSet
			};
		}

		public IVisualLayer CreateResultLayer(IPayload payload, IVisualLayer originalLayer, DateTimePeriod period)
		{
			var castedLayer = (VisualLayer) originalLayer;
			return new VisualLayer(payload, period, castedLayer.HighestPriorityActivity)
			{
				HighestPriorityAbsence = castedLayer.HighestPriorityAbsence,
				DefinitionSet = originalLayer.DefinitionSet,
			};
		}
	}
}
