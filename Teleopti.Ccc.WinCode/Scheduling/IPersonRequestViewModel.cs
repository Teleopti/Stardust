using System;
using System.ComponentModel;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
    public interface IPersonRequestViewModel:INotifyPropertyChanged
    {
        bool IsWithinSchedulePeriod { get; }
        IPersonRequest PersonRequest { get; }
        DateOnlyPeriod RequestedDate { get; }
        string Name { get; }
        int Seniority { get; }
        string RequestType { get; }
        bool IsPending { get; }
        bool IsApproved { get; }
        bool IsDenied { get; }
        bool IsNew { get; }
        string Details { get; }
        string Subject { get; }
    	string GetSubject(ITextFormatter formatter);
    	string Message { get; }
    	string GetMessage(ITextFormatter formatter);
        DateTime LastUpdated { get; }
        bool CausesBrokenBusinessRule { get; set; }
        string RequestTypeOfToString { get; }
        string StatusText { get; }
        bool IsEditable { get; }
        bool IsSelected { get; set; }
        DateTime FirstDateInRequest { get; }
    }
}