using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.Repositories
{
    public interface IPersonRequestReadOnlyRepository
    {
        IList<IPersonRequestLightWeight> LoadOnPerson(Guid personId, int startRow, int endRow);
    }

    public interface IPersonRequestLightWeight
    {
         Guid Id { get; set; }
         DateTime StartDateTime { get; set; }
         DateTime EndDateTime { get; set; }
         string FirstName { get; set; }
         string LastName { get; set; }
         string EmplyomentNumber { get; set; }
         int RequestStatus { get; set; }
         string Subject { get; set; }
         string Message { get; set; }
         string DenyReason { get; set; }
         string Info { get; set; }
         string RequestType { get; set; }
         int ShiftTradeStatus { get; set; }
         string SavedByFirstName { get; set; }
         string SavedByLastName { get; set; }
         string SavedByEmplyomentNumber { get; set; }
    }
    
}