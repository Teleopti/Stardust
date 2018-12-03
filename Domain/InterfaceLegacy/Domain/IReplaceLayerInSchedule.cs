namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface IReplaceLayerInSchedule
	{
		/// <summary>
		/// Replaces a layer <paramref name="layerToRemove" /> in <paramref name="scheduleDay"/>
		/// with a new layer containing sent data.
		/// </summary>
		void Replace(IScheduleDay scheduleDay, ILayer<IActivity> layerToRemove, IActivity newActivity, DateTimePeriod newPeriod);

		/// <summary>
		/// Replaces a layer <paramref name="layerToRemove" /> in <paramref name="scheduleDay"/>
		/// with a new layer containing sent data.
		/// </summary>
		void Replace(IScheduleDay scheduleDay, ILayer<IAbsence> layerToRemove, IAbsence newAbsence, DateTimePeriod newPeriod);
	}
}