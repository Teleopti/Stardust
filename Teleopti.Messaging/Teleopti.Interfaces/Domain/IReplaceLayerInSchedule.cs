namespace Teleopti.Interfaces.Domain
{
	public interface IReplaceLayerInSchedule
	{
		/// <summary>
		/// Replaces a layer <paramref name="layerToRemove" /> in <paramref name="scheduleDay"/>
		/// with a new layer containing sent data.
		/// </summary>
		/// <returns>
		/// <code>true</code> if replace was successful.
		/// <code>false</code> if replace was unsuccessful, eg becuase <paramref name="layerToRemove"/> wasn't found.
		/// </returns>
		bool Replace<T>(IScheduleDay scheduleDay, ILayer<T> layerToRemove, T payload, DateTimePeriod period, int orderIndex);
	}
}