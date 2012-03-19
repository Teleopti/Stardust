using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.Repositories
{
    public interface IRequestHistoryReadOnlyRepository
    {
        IList<IRequestHistoryLightWeight> LoadOnPerson(Guid personId, int startRow, int endRow);
    }

    public interface IRequestHistoryLightWeight
    {
        int TotalCount { get; set; }
         Guid Id { get; set; }
         DateTime StartDateTime { get; set; }
         DateTime EndDateTime { get; set; }
         string FirstName { get; set; }
         string LastName { get; set; }
         string EmploymentNumber { get; set; }
         int RequestStatus { get; set; }
         string Subject { get; set; }
         string Message { get; set; }
         string DenyReason { get; set; }
         string Info { get; set; }
         string RequestType { get; set; }
         int ShiftTradeStatus { get; set; }
         string SavedByFirstName { get; set; }
         string SavedByLastName { get; set; }
         string SavedByEmploymentNumber { get; set; }
         DateTime LastUpdatedDateTime { get; set; }
         Int64 RowNumber { get; set; }
         string RequestStatusText { get; }
         string RequestTypeText { get; }
         string Dates { get; }
    }
    
}