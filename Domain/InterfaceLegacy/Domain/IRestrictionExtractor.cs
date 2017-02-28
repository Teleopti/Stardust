namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface IRestrictionExtractor
	{
		/// <summary>
		/// Extracts the specified schedule part.
		/// </summary>
		/// <param name="schedulePart">The schedule part.</param>
		IExtractedRestrictionResult Extract(IScheduleDay schedulePart);
	}
}