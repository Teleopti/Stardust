using System;
using System.Collections.Generic;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    public class RequestHistoryReadOnlyRepository : IRequestHistoryReadOnlyRepository
    {
        private readonly IStatelessUnitOfWork _unitOfWork;
        
        public RequestHistoryReadOnlyRepository(IStatelessUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IList<IRequestHistoryLightWeight> LoadOnPerson(Guid personId, int startRow, int endRow)
        {
            return ((NHibernateStatelessUnitOfWork)_unitOfWork).Session.CreateSQLQuery(
                "exec [ReadModel].[RequestOnPerson] @person=:person, @start_row =:start_row, @end_row=:end_row")
                .SetGuid("person", personId)
                .SetInt32("start_row", startRow)
                .SetInt32("end_row", endRow)
                .SetResultTransformer(Transformers.AliasToBean(typeof(RequestHistoryLightWeight)))
                .SetReadOnly(true)
                .List<IRequestHistoryLightWeight>();     
        }
    }

    public class RequestHistoryLightWeight: IRequestHistoryLightWeight
    {
        public int TotalCount { get; set; }
        public Guid Id { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmploymentNumber { get; set; }
        public int RequestStatus { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public string DenyReason { get; set; }
        public string Info { get; set; }
        public string RequestType { get; set; }
        public int ShiftTradeStatus { get; set; }
        public string SavedByFirstName { get; set; }
        public string SavedByLastName { get; set; }
        public string SavedByEmploymentNumber { get; set; }
        public Int64 RowNumber { get; set; }

        public string RequestStatusText
        {
            get 
            {
                if (RequestStatus == 1)
                    return UserTexts.Resources.Denied;

                return UserTexts.Resources.Approved;
            }
        }

        public string RequestTypeText
        {
            get
            {
                if (RequestType == "TEXT")
                    return UserTexts.Resources.RequestTypeText;

                if (RequestType == "ABS")
                    return UserTexts.Resources.RequestTypeAbsence;

                return UserTexts.Resources.RequestTypeShiftTrade;
            }
        }
    }

}