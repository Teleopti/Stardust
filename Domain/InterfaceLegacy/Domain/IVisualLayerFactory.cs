namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface IVisualLayerFactory
	{
		IVisualLayer CreateShiftSetupLayer(IActivity activity, DateTimePeriod period);

		IVisualLayer CreateShiftSetupLayer(ILayer<IActivity> layer);

		IVisualLayer CreateResultLayer(IPayload payload, IVisualLayer originalLayer, DateTimePeriod period);

		IVisualLayer CreateMeetingSetupLayer(IMeetingPayload meetingPayload, IVisualLayer originalLayer, DateTimePeriod period);

		IVisualLayer CreateAbsenceSetupLayer(IAbsence absence, IVisualLayer originalLayer, DateTimePeriod period);
	}
}
