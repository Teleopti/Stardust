﻿using System;
using System.Collections.Generic;
using System.Globalization;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo.Requests
{
    /// <summary>
    /// Contains a new request for absence
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2008-06-05
    /// </remarks>
    public class AbsenceRequest : Request, IAbsenceRequest
    {
        private readonly IAbsence _absence;
        private string _typeDescription = string.Empty;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="AbsenceRequest"/> class.
        /// For NHibernate to use.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-06-05
        /// </remarks>
        protected AbsenceRequest()
        {
            _typeDescription = UserTexts.Resources.RequestTypeAbsence;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AbsenceRequest"/> class.
        /// </summary>
        /// <param name="absence">The absence.</param>
        /// <param name="period">The period.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-06-05
        /// </remarks>
        public AbsenceRequest(IAbsence absence, DateTimePeriod period) : base(period)
        {
            _absence = absence;
            _typeDescription = UserTexts.Resources.RequestTypeAbsence;
        }

     
        public virtual IAbsence Absence
        {
            get { return _absence; }
        }

        public override void Deny(IPerson denyPerson)
        {
            if (
                Period.StartDateTimeLocal(Person.PermissionInformation.DefaultTimeZone()).ToShortDateString().
                    ToString(Person.PermissionInformation.Culture()) ==
                Period.EndDateTimeLocal(Person.PermissionInformation.DefaultTimeZone()).ToShortDateString().ToString
                    (Person.PermissionInformation.Culture()))
            {
                TextForNotification = string.Format(Person.PermissionInformation.UICulture(),
                                                    UserTexts.Resources.AbsenceRequestForOneDayHasBeenDeniedDot,
                                                    Period.StartDateTimeLocal(
                                                        Person.PermissionInformation.DefaultTimeZone()).
                                                        ToShortDateString().ToString(
                                                            Person.PermissionInformation.Culture()));
            }
            else
            {
                TextForNotification = string.Format(Person.PermissionInformation.UICulture(),
                                                    UserTexts.Resources.AbsenceRequestHasBeenDeniedDot,
                                                    Period.StartDateTimeLocal(
                                                        Person.PermissionInformation.DefaultTimeZone()).
                                                        ToShortDateString().ToString(
                                                            Person.PermissionInformation.Culture()),
                                                    Period.EndDateTimeLocal(
                                                        Person.PermissionInformation.DefaultTimeZone()).
                                                        ToShortDateString().ToString(
                                                            Person.PermissionInformation.Culture()));
                }
        }

        public override void Accept(IPerson acceptingPerson, IShiftTradeRequestSetChecksum shiftTradeRequestSetChecksum, IPersonRequestCheckAuthorization authorization)
        {
            throw new NotImplementedException();
        }

        public override void Refer(IPersonRequestCheckAuthorization authorization)
        {
            throw new NotImplementedException();
        }

        public override string GetDetails(CultureInfo cultureInfo)
        {
            string text = Absence.Name;
            if (!Period.LocalStartDateTime.AddDays(1).AddSeconds(-1).Equals(Period.LocalEndDateTime))
            {
                text = string.Format(cultureInfo, "{0}, {1} - {2}",
                                     Absence.Name,
                                     Period.LocalStartDateTime.ToString("t",cultureInfo),
                                     Period.LocalEndDateTime.ToString("t", cultureInfo));
            }
            return text;
        }

        /// <summary>
        /// Approves this instance.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-06-05
        /// </remarks>
        protected internal override IEnumerable<IBusinessRuleResponse> Approve(IRequestApprovalService approvalService)
        {
            var result = approvalService.ApproveAbsence(_absence, Period, Person);
            if (result.IsEmpty())
            {

                if (Period.StartDateTimeLocal(Person.PermissionInformation.DefaultTimeZone()).ToShortDateString().ToString(Person.PermissionInformation.Culture()) == Period.EndDateTimeLocal(Person.PermissionInformation.DefaultTimeZone()).ToShortDateString().ToString(Person.PermissionInformation.Culture()))
                {
                    TextForNotification = string.Format(Person.PermissionInformation.UICulture(), UserTexts.Resources.AbsenceRequestForOneDayHasBeenApprovedDot, 
                        Period.StartDateTimeLocal(Person.PermissionInformation.DefaultTimeZone()).ToShortDateString().ToString(Person.PermissionInformation.Culture()));
                }
                else
                {
                    TextForNotification = string.Format(Person.PermissionInformation.UICulture(), UserTexts.Resources.AbsenceRequestHasBeenApprovedDot, 
                        Period.StartDateTimeLocal(Person.PermissionInformation.DefaultTimeZone()).ToShortDateString().ToString(Person.PermissionInformation.Culture()), 
                        Period.EndDateTimeLocal(Person.PermissionInformation.DefaultTimeZone()).ToShortDateString().ToString(Person.PermissionInformation.Culture()));
                }
            }
            return result;
        }

        /// <summary>
        /// Description for the request type
        /// </summary>
        public override string RequestTypeDescription
        {
            get{ return _typeDescription;}
            set{_typeDescription = value;}
        }

        /// <summary>
        /// Description for the absence
        /// </summary>
        public override Description RequestPayloadDescription
        {
            get{return _absence.Description;}
        }
    }
}