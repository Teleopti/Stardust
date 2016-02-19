using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	//rk: needs refactoring..
	//IVisualLayerFactory should only have one CreateLayer (maybe two overloaded)
	//and instead have multiple objects of IVisualLayerFactory (one for absence, meeting and so forth)
	public class VisualLayerFactory : IVisualLayerFactory
	{
		public virtual IVisualLayer CreateShiftSetupLayer(IActivity activity, DateTimePeriod period, IPerson person)
		{
			return new VisualLayer(activity, period, activity, person);
		}

		public virtual IVisualLayer CreateShiftSetupLayer(ILayer<IActivity> layer, IPerson person)
		{
			var ret = new VisualLayer(layer.Payload, layer.Period, layer.Payload, person);
			var overtimeLayer = layer as IOvertimeShiftLayer;
			if (overtimeLayer != null)
			{
				ret.DefinitionSet = overtimeLayer.DefinitionSet;
			}
			return ret;
		}


		public IVisualLayer CreateMeetingSetupLayer(IMeetingPayload meetingPayload, IVisualLayer originalLayer, DateTimePeriod period)
		{
			return new VisualLayer(meetingPayload, period, meetingPayload.Meeting.Activity, originalLayer.Person)
			{
				DefinitionSet = originalLayer.DefinitionSet
			};
		}


		public IVisualLayer CreateAbsenceSetupLayer(IAbsence absence, IVisualLayer originalLayer, DateTimePeriod period,
			Guid? personAbsenceId)
		{
			return new VisualLayer(absence, period, ((VisualLayer) originalLayer).HighestPriorityActivity, originalLayer.Person,
				personAbsenceId)
			{
				HighestPriorityAbsence = absence,
				DefinitionSet = originalLayer.DefinitionSet
			};
		}

		public IVisualLayer CreateResultLayer(IPayload payload, IVisualLayer originalLayer, DateTimePeriod period,
			Guid? personAbsenceId = null)
		{
			var castedLayer = ((VisualLayer) originalLayer);
			return new VisualLayer(payload, period, castedLayer.HighestPriorityActivity, originalLayer.Person)
			{
				HighestPriorityAbsence = castedLayer.HighestPriorityAbsence,
				DefinitionSet = originalLayer.DefinitionSet,
				PersonAbsenceId = personAbsenceId
			};
		}
	}
}
