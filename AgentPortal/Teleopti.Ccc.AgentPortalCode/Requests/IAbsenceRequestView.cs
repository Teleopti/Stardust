using System;
using System.Windows.Forms;
using Teleopti.Ccc.Sdk.Client.SdkServiceReference;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.AgentPortalCode.Requests
{
    public interface IAbsenceRequestView
    {
        /// <summary>
        /// Gets or sets the time picker time period.
        /// </summary>
        /// <value>The time picker time period.</value>
        /// <remarks>
        /// Created by: HenryG
        /// Created date: 2010-02-09
        /// </remarks>
        TimePeriod TimePickerTimePeriod{ get; set;}
        /// <summary>
        /// Gets or sets a value indicating whether [time pickers enabled].
        /// </summary>
        /// <value><c>true</c> if [time pickers enabled]; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by: HenryG
        /// Created date: 2010-02-09
        /// </remarks>
        bool TimePickersEnabled { get; set; }

        /// <summary>
        /// Gets or sets the subject.
        /// </summary>
        /// <value>The subject.</value>
        /// <remarks>
        /// Created by: HenryG
        /// Created date: 2010-02-11
        /// </remarks>
        string Subject { get; set; }

        /// <summary>
        /// Gets or sets the request date.
        /// </summary>
        /// <value>The request date.</value>
        /// <remarks>
        /// Created by: HenryG
        /// Created date: 2010-02-11
        /// </remarks>
        DateTime RequestDate { get; set; }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        /// <value>The status.</value>
        /// <remarks>
        /// Created by: HenryG
        /// Created date: 2010-02-11
        /// </remarks>
        string Status { get; set; }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>The message.</value>
        /// <remarks>
        /// Created by: HenryG
        /// Created date: 2010-02-11
        /// </remarks>
        string Message { get; set; }

        /// <summary>
        /// Gets or sets the type of the absence.
        /// </summary>
        /// <value>The type of the absence.</value>
        /// <remarks>
        /// Created by: HenryG
        /// Created date: 2010-02-11
        /// </remarks>
        AbsenceDto AbsenceType { get; set; }

        /// <summary>
        /// Gets or sets the selected date time.
        /// </summary>
        /// <value>The selected date time.</value>
        /// <remarks>
        /// Created by: HenryG
        /// Created date: 2010-02-11
        /// </remarks>
        DateTime SelectedStartDateTime { get; }

        /// <summary>
        /// Gets the selected end date time.
        /// </summary>
        /// <value>The selected end date time.</value>
        /// <remarks>
        /// Created by: HenryG
        /// Created date: 2010-02-11
        /// </remarks>
        DateTime SelectedEndDateTime { get; }

        /// <summary>
        /// Sets the date time picker.
        /// </summary>
        /// <param name="startTime">The start time.</param>
        /// <param name="endTime">The end time.</param>
        /// <remarks>
        /// Created by: HenryG
        /// Created date: 2010-02-11
        /// </remarks>
        void SetDateTimePickers(DateTime startTime, DateTime endTime);

        /// <summary>
        /// Initializes the date time pickers.
        /// </summary>
        /// <param name="startTime">The start time.</param>
        /// <param name="endTime">The end time.</param>
        /// <remarks>
        /// Created by: HenryG
        /// Created date: 2010-02-16
        /// </remarks>
        void InitializeDateTimePickers(DateTime startTime, DateTime endTime);
        
        /// <summary>
        /// Gets or sets a value indicating whether this instance is all day.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is all day; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// Created by: HenryG
        /// Created date: 2010-02-16
        /// </remarks>
        bool IsAllDay { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [delete button enabled].
        /// </summary>
        /// <value><c>true</c> if [delete button enabled]; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by: HenryG
        /// Created date: 2010-02-16
        /// </remarks>
        bool DeleteButtonEnabled { get; set; }

        /// <summary>
        /// Gets or sets the Deny Reason.
        /// </summary>
        /// <remarks>
        /// Created by: PeterWe
        /// Created date: 2010-02-16
        /// </remarks>
        string DenyReason { get; set; }

        /// <summary>
        /// Sets the Deny Reason.
        /// </summary>
        /// <value>true / false</value>
        /// <remarks>
        /// Created by: PeterWe
        /// Created date: 2010-02-16
        /// </remarks>
        void SetDenyReasonVisible(bool value);

        /// <summary>
        /// Sets the Deny Reason.
        /// </summary>
        /// <value>true / false</value>
        /// <remarks>
        /// Created by: PeterWe
        /// Created date: 2010-03-01
        /// </remarks>
        void SetFormReadOnly(bool value);

    	void ShowDeleteErrorMessage(string message);
    }
}