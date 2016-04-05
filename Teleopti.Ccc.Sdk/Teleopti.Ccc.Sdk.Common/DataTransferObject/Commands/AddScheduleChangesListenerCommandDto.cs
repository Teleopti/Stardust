using System.Runtime.Serialization;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands
{
	/// <summary>
	/// Command for adding a schedule changes listener 
	///
	/// The schedule changes will arrive as POST data to the endpoint you specify through the SDK. The function to get information about current subscriptions and listener endpoints is <see cref="GetScheduleChangesSubscriptionSettingsQueryDto"/> on the SchedulingService. The response on this function also contains the details needed to verify the signature of the incoming data. To add a new listener the command <see cref="AddScheduleChangesListenerCommandDto"/> should be executed. The url needs to be an absolute url (the complete address with http or https included) to the endpoint. DaysStartFromCurrentDate and DaysEndFromCurrentDate creates a filter on the range of schedule changes to listen for. A value of -1 for DaysStartFromCurrentDate means that changes from yesterday are included and a value of 1 for DaysEndFromCurrentDate means that changes for tomorrow are included. Please note that these dates are local dates in the viewpoint of the agents. DaysStartFromCurrentDate must be smaller than or equal to DaysEndFromCurrentDate. The name must be provided and will be used when the endpoint should be removed using the <see cref="RevokeScheduleChangesListenerCommandDto"/> command.
	///
	/// The schedule data sent from Teleopti WFM is formatted as json. In the http request there will be a header named Signature with a base64 encoded signature value. That signature can be used to verify the contents of the request body. Use the public key exposed in the SDK mentioned before in combination with the signature. The hashing algorithm used is SHA1. If the request body cannot be verified with the signature someone has tampered with the contents and the request should be discarded.
	///
	/// Only schedule information for the default scenario gets sent to the listener. You might get ScheduleDays outside your predefined filter, so always make sure you filter the schedule days on the recieving side as well.
	///
	/// The content of the messages will contain the following information:
	///
	/// IsDefaultScenario: (always true)
	/// ScenarioId: the scenario id
	/// PersonId: the person id
	/// LogOnDatasource: the tenant name
	/// LogOnBusinessUnitId: the business unit id
	/// ScheduleLoadTimestamp: a timestamp for when the data was loaded, schedule changes might not be pushed in chronological order
	/// ScheduleDays: a collection of the schedule days that were updated
	///	TeamId: the id of the team the person belongs to the given date
	///	SiteId: the id of the site the person belongs to the given date
	///	Date: the schedule date
	///	WorkTime: the amount of work time during the day(hh:mm:ss)
	///	ContractTime: the amount of work time during the day(hh:mm:ss)
	///	ShortName: the short name of the shift category/day off
	///	Name: the name of the shift category/day off
	///	Displaycolor: the argb value of the displaycolor for the shift category/day off
	///	IsWorkday: indication if this is considered as a work day or not
	///	IsFullDayAbsence: inidication if this is considered as a full day absence
	///	ShiftCategoryId: the shift category if applicable (zero padded id if not)
	///	NotScheduled: indication if the day isn't scheduled
	///	DayOff: null if this is not a day off, otherwise an object with the details below
	///		StartDateTime: the start time of the day off
	///		EndDateTime: the end time of the day off
	///		Anchor: the time of day the day off is anchored
	///	Shift: null if no shift, otherwise an object with the details below
	///		StartDateTime: the start time of the shift
	///		EndDateTime: the end time of the shift
	///		Layers: a collection of the individual layers of the shift, each having the details below
	///			PayloadId: the id of the activity or absence
	///			StartDateTime: the start time of the layer
	///			EndDateTime: the end time of the layer
	///			WorkTime: the amount of work time for the layer (hh:mm:ss)
	///			ContractTime: the amount of work time for the layer (hh:mm:ss)
	///			PaidTime: the amount of paid time for the layer (hh:mm:ss)
	///			Overtime: the amount of overtime for the layer (hh:mm:ss)
	///			IsAbsence: an indication if this is an absence instead of activity
	///			IsAbsenceConfidential: an indication if the absence should be handled with care for integrity reasons
	///			Name: the name of the activity or absence
	///			PayrollCode: the payroll code for the activity or absence
	///			DisplayColor: the argb color value for the layer
	///			MultiplicatorDefinitionSetId: the id of the multiplicator definition set for the layer in case of overtime
	/// </summary>
	[DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2016/03/")]
	public class AddScheduleChangesListenerCommandDto : CommandDto
	{
		/// <summary>
		/// Gets or sets the listener to add to the schedule changes subscriptions.
		/// </summary>
		[DataMember]
		public ScheduleChangesListenerDto Listener { get; set; }
	}
}