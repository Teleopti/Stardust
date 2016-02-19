using System;

namespace Teleopti.Interfaces.Domain
{
	/// <summary>
	/// Factory for visual layers
	/// </summary>
	/// <remarks>
	/// Created by: rogerkr
	/// Created date: 2009-02-13
	/// </remarks>
	public interface IVisualLayerFactory
	{
		/// <summary>
		/// Creates the setup layer.
		/// </summary>
		/// <param name="activity">The activity.</param>
		/// <param name="period">The period.</param>
		/// <param name="person">The person.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: rogerkr
		/// Created date: 2009-02-13
		/// </remarks>
		IVisualLayer CreateShiftSetupLayer(IActivity activity, DateTimePeriod period, IPerson person);

		/// <summary>
		/// Creates the shift setup layer.
		/// </summary>
		/// <param name="layer">The layer.</param>
		/// <param name="person">The person.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: rogerkr
		/// Created date: 2009-03-17
		/// </remarks>
		IVisualLayer CreateShiftSetupLayer(ILayer<IActivity> layer, IPerson person);

		/// <summary>
		/// Creates the result layer.
		/// </summary>
		/// <param name="payload">The payload.</param>
		/// <param name="originalLayer">The original layer.</param>
		/// <param name="period">The period.</param>
		/// <param name="personAbsenceId">The ID of person absence for current layer</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: rogerkr
		/// Created date: 2009-02-13
		/// </remarks>
		IVisualLayer CreateResultLayer(IPayload payload, IVisualLayer originalLayer, DateTimePeriod period,
			Guid? personAbsenceId);

		/// <summary>
		/// Creates the result layer.
		/// </summary>
		/// <param name="meetingPayload">The meeting payload.</param>
		/// <param name="originalLayer">The original layer.</param>
		/// <param name="period">The period.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: rogerkr
		/// Created date: 2009-02-13
		/// </remarks>
		IVisualLayer CreateMeetingSetupLayer(IMeetingPayload meetingPayload, IVisualLayer originalLayer, DateTimePeriod period);

		/// <summary>
		/// Creates the result layer.
		/// </summary>
		/// <param name="absence">The absence.</param>
		/// <param name="originalLayer">The original layer.</param>
		/// <param name="period">The period.</param>
		/// <param name="personAbsenceId">The ID of person absence for current layer</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: rogerkr
		/// Created date: 2009-02-13
		/// </remarks>
		IVisualLayer CreateAbsenceSetupLayer(IAbsence absence, IVisualLayer originalLayer, DateTimePeriod period,
			Guid? personAbsenceId);
	}
}
