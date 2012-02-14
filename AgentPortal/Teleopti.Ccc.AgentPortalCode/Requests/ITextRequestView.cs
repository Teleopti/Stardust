using System;

namespace Teleopti.Ccc.AgentPortalCode.Requests
{
    public interface ITextRequestView
    {
        void InitializeDateTimePickers(DateTime startDateTime, DateTime endDateTime);
        string Subject { get; set; }
        string Message { get; set; }
        DateTime RequestDate { get; set; }
        string DenyReason { get; set; }
        string Status { get; set; }
        bool DeleteButtonEnabled { get; set; }
        void SetDenyReasonVisible(bool value);
        void SetFormReadOnly(bool value);
        DateTime StartDateTime { get; }
        DateTime EndDateTime { get; }
        void FixUtcDateTimes();
    	void ShowDeleteErrorMessage(string message);
    }
}
