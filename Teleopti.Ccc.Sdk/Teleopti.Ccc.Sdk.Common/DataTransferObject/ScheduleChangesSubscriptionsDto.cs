using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject
{
	/// <summary>
	/// The settings for subscriptions of schedule changes.
	/// </summary>
	[DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2016/03/")]
	public class ScheduleChangesSubscriptionsDto : Dto
	{
		public ScheduleChangesSubscriptionsDto()
		{
			Listeners = new List<ScheduleChangesListenerDto>();
		}

		/// <summary>
		/// The configured listeners for schedule changes
		/// </summary>
		[DataMember]
		public ICollection<ScheduleChangesListenerDto> Listeners { get; private set; }
	}
}