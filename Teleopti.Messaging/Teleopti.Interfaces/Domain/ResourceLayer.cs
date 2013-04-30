namespace Teleopti.Interfaces.Domain
{
	/// <summary>
	/// 
	/// </summary>
	public class ResourceLayer
	{
		/// <summary>
		/// 
		/// </summary>
		public double Resource { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public DateTimePeriod Period { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public IPayload Activity { get; set; }
	}
}