using System;
using System.Collections.Generic;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
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

        public IList<IRequestHistoryLightweight> LoadOnPerson(Guid personId, int startRow, int endRow)
        {
            return ((NHibernateStatelessUnitOfWork)_unitOfWork).Session.CreateSQLQuery(
                "exec [ReadModel].[RequestOnPerson] @person=:person, @start_row =:start_row, @end_row=:end_row")
                .SetGuid("person", personId)
                .SetInt32("start_row", startRow)
                .SetInt32("end_row", endRow)
                .SetResultTransformer(Transformers.AliasToBean(typeof(RequestHistoryLightweight)))
                .SetReadOnly(true)
                .List<IRequestHistoryLightweight>();     
        }
    }

    public class RequestHistoryLightweight: IRequestHistoryLightweight
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
        public DateTime LastUpdatedDateTime { get; set; }
        public Int64 RowNumber { get; set; }

        public string RequestStatusText
        {
            get 
            {
                if (RequestStatus == 1 || RequestStatus == 4)
                    return UserTexts.Resources.Denied;

	            if (RequestStatus == 5)
		            return UserTexts.Resources.Waitlisted;

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

        public string Dates
        {
            get
            {
                var timeZone = TeleoptiPrincipal.CurrentPrincipal.Regional.TimeZone;
                return TimeZoneHelper.ConvertFromUtc(StartDateTime, timeZone) + Delimiter +
                       TimeZoneHelper.ConvertFromUtc(EndDateTime, timeZone);
            }
        }

        public string ShortDates
        {
            get
            {
                var timeZone = TeleoptiPrincipal.CurrentPrincipal.Regional.TimeZone;
                return TimeZoneHelper.ConvertFromUtc(StartDateTime, timeZone).ToShortDateString() + Delimiter +
                       TimeZoneHelper.ConvertFromUtc(EndDateTime, timeZone).ToShortDateString();
            }
        }

        public string LatestChangeDateTime
        {
            get
            {
                var timeZone = TeleoptiPrincipal.CurrentPrincipal.Regional.TimeZone;
                var culture = TeleoptiPrincipal.CurrentPrincipal.Regional.Culture;
                return TimeZoneHelper.ConvertFromUtc(LastUpdatedDateTime, timeZone).ToString(culture);
            }
        }

        private string Delimiter
        {
            get
            {
                var delimiter = " - ";
                if (RequestType == "TRADE")
                    delimiter = " ; ";
                return delimiter;
            }
        }
    }

}