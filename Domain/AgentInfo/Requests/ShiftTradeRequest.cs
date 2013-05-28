using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo.Requests
{
    /// <summary>
    /// This will contains shift trade for request
    /// </summary>
    /// <remarks>
    /// Created by: Dinesh Ranasinghe
    /// Created date: 2008-09-19
    /// </remarks>
    public class ShiftTradeRequest : Request, IShiftTradeRequest
    {
        private readonly IList<IShiftTradeSwapDetail> _shiftTradeSwapDetails = new List<IShiftTradeSwapDetail>();
        private ShiftTradeStatus shiftTradeStatus = ShiftTradeStatus.OkByMe;
        private string _typeDescription = string.Empty;
        private IList<IPerson> _receiverOfNotification = new List<IPerson>();
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ShiftTradeRequest"/> class.
        /// </summary>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-09-19
        /// </remarks>
        protected ShiftTradeRequest()
        {
            _typeDescription = UserTexts.Resources.RequestTypeShiftTrade;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShiftTradeRequest"/> class.
        /// </summary>
        /// <param name="shiftTradeSwapDetails">The shift trade swap details.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2009-08-28
        /// </remarks>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public ShiftTradeRequest(IList<IShiftTradeSwapDetail> shiftTradeSwapDetails)
            : this()
        {
            _shiftTradeSwapDetails = new List<IShiftTradeSwapDetail>(shiftTradeSwapDetails);
            calculateAndSetPeriod();
            notifyPersonOfAvailableShiftTradeRequest(shiftTradeSwapDetails);
        }

        private void notifyPersonOfAvailableShiftTradeRequest(IEnumerable<IShiftTradeSwapDetail> shiftTradeSwapDetails)
        {
            if (!shiftTradeSwapDetails.IsEmpty())
            {
                var datePattern = PersonFrom.PermissionInformation.Culture().DateTimeFormat.ShortDatePattern;
                if (isShiftTradeRequestForOneDayOnly())
                {
                    SetNotification(string.Format(PersonFrom.PermissionInformation.UICulture(),
                                  UserTexts.Resources.ANewShiftTradeForOneDayHasBeenCreatedDot,
                                  Period.StartDateTimeLocal(PersonFrom.PermissionInformation.DefaultTimeZone()).ToString(datePattern)), new List<IPerson> { PersonTo });
                }
                else
                {
                    SetNotification(string.Format(PersonFrom.PermissionInformation.UICulture(),
                                  UserTexts.Resources.ANewShiftTradeHasBeenCreatedDot,
                                  Period.StartDateTimeLocal(PersonFrom.PermissionInformation.DefaultTimeZone()).ToString(datePattern),
                                  Period.EndDateTimeLocal(PersonFrom.PermissionInformation.DefaultTimeZone()).AddMinutes(-1).ToString(datePattern)), new List<IPerson> { PersonTo });
                }
                
                //SetNotification(
                //    isShiftTradeRequestForOneDayOnly()
                //        ? UserTexts.Resources.ANewShiftTradeHasBeenCreatedDot
                //        : UserTexts.Resources.ANewShiftTradeForOneDayHasBeenCreatedDot, new List<IPerson> {PersonTo});
            }
        }

        private void calculateAndSetPeriod()
        {
            if (_shiftTradeSwapDetails.Count>0)
            {
                DateOnly minDate = DateOnly.MaxValue, maxDate = DateOnly.MinValue;
                TimeZoneInfo timeZoneInfo = PersonFrom.PermissionInformation.DefaultTimeZone();
                foreach (IShiftTradeSwapDetail shiftTradeSwapDetail in _shiftTradeSwapDetails)
                {
                    shiftTradeSwapDetail.SetParent(this);
                    if (shiftTradeSwapDetail.DateFrom < minDate) minDate = shiftTradeSwapDetail.DateFrom;
                    if (shiftTradeSwapDetail.DateTo < minDate) minDate = shiftTradeSwapDetail.DateTo;
                    if (shiftTradeSwapDetail.DateFrom > maxDate) maxDate = shiftTradeSwapDetail.DateFrom;
                    if (shiftTradeSwapDetail.DateTo > maxDate) maxDate = shiftTradeSwapDetail.DateTo;
                }
                var period = new DateOnlyPeriod(minDate, maxDate).ToDateTimePeriod(timeZoneInfo);
                SetPeriod(period);
            }
            else
            {
                SetPeriod(new DateTimePeriod());
            }
        }

        public virtual ReadOnlyCollection<IShiftTradeSwapDetail> ShiftTradeSwapDetails
        {
            get { return new ReadOnlyCollection<IShiftTradeSwapDetail>(_shiftTradeSwapDetails); }
        }

        public virtual void AddShiftTradeSwapDetail(IShiftTradeSwapDetail shiftTradeSwapDetail)
        {
            verifyEditingShiftTradeIsAllowed();
            shiftTradeSwapDetail.SetParent(this);
            _shiftTradeSwapDetails.Add(shiftTradeSwapDetail);
            calculateAndSetPeriod();

            var datePattern = PersonFrom.PermissionInformation.Culture().DateTimeFormat.ShortDatePattern;

            if (isShiftTradeRequestForOneDayOnly())
            {
                SetNotification(string.Format(PersonFrom.PermissionInformation.UICulture(),
                                UserTexts.Resources.ANewShiftTradeForOneDayHasBeenCreatedDot,
                                Period.StartDateTimeLocal(PersonFrom.PermissionInformation.DefaultTimeZone()).ToString(datePattern))
                              , new List<IPerson> { shiftTradeSwapDetail.PersonTo });
            }
            else
            {
                SetNotification(string.Format(PersonFrom.PermissionInformation.UICulture(),
                              UserTexts.Resources.ANewShiftTradeHasBeenCreatedDot,
                              Period.StartDateTimeLocal(PersonFrom.PermissionInformation.DefaultTimeZone()).ToString(datePattern),
                              Period.EndDateTimeLocal(PersonFrom.PermissionInformation.DefaultTimeZone()).AddMinutes(-1).ToString(datePattern))
                              , new List<IPerson> { shiftTradeSwapDetail.PersonTo });
            }
            //SetNotification(_newShifTradeRequestHasBeenCreated, new List<IPerson> { shiftTradeSwapDetail.PersonTo });
        }

        public virtual void ClearShiftTradeSwapDetails()
        {
            verifyEditingShiftTradeIsAllowed();
            _shiftTradeSwapDetails.Clear();
            calculateAndSetPeriod();
        }

        private void verifyEditingShiftTradeIsAllowed()
        {
            if (shiftTradeStatus!=ShiftTradeStatus.OkByMe &&
                shiftTradeStatus!=ShiftTradeStatus.Referred)
            {
				throw new ShiftTradeRequestStatusException("Editing of shift trades is only allowed when in status modes OkByMe or Referred.");
            }
        }

        public virtual ShiftTradeStatus GetShiftTradeStatus(IShiftTradeRequestStatusChecker shiftTradeRequestStatusChecker)
        {
            shiftTradeRequestStatusChecker.Check(this);
            return shiftTradeStatus;
        }

        /// <summary>
        /// Gets the shift trade status.
        /// </summary>
        /// <value>The shift trade status.</value>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-09-19
        /// </remarks>
        public virtual void SetShiftTradeStatus(ShiftTradeStatus shiftTradeStatusToSet, IPersonRequestCheckAuthorization authorization)
        {
            VerifyShiftTradeStatusIsAllowed(shiftTradeStatusToSet);
            shiftTradeStatus = shiftTradeStatusToSet;
            switch (shiftTradeStatus)
            {
                case ShiftTradeStatus.NotValid:
                    ((IPersonRequest) Parent).Deny(null,null,authorization);
                    break;
                case ShiftTradeStatus.Referred:
                    ((IPersonRequest) Parent).ForcePending();
                    break;
            }
        }

        private void VerifyShiftTradeStatusIsAllowed(ShiftTradeStatus shiftTradeStatusToVerify)
        {
            if (shiftTradeStatus==ShiftTradeStatus.Referred && 
                shiftTradeStatusToVerify==ShiftTradeStatus.OkByBothParts)
            {
                throw new ShiftTradeRequestStatusException("The shift trade status OkByBothParts is not allowed when going from Referred.");
            }
        }

        /// <summary>
        /// Denies this instance.
        /// </summary>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-09-19
        /// </remarks>
        public override void Deny(IPerson denyPerson)
        {
            var list = new List<IPerson>(InvolvedPeople());
            list.Remove(denyPerson);
            //SetNotification(_shiftTradeRequestHasBeenDeniedDot, list);

            var datePattern = PersonFrom.PermissionInformation.Culture().DateTimeFormat.ShortDatePattern;

            if (isShiftTradeRequestForOneDayOnly())
            {
                SetNotification(string.Format(PersonFrom.PermissionInformation.UICulture(),
                                              UserTexts.Resources.ShiftTradeRequestForOneDayHasBeenDeniedDot,
                                              Period.StartDateTimeLocal(
                                                  PersonFrom.PermissionInformation.DefaultTimeZone()).ToString(
                                                      datePattern)), list);
            }
            else
            {
                SetNotification(string.Format(PersonFrom.PermissionInformation.UICulture(),
                              UserTexts.Resources.ShiftTradeRequestHasBeenDeniedDot,
                              Period.StartDateTimeLocal(PersonFrom.PermissionInformation.DefaultTimeZone()).ToString(datePattern),
                              Period.EndDateTimeLocal(PersonFrom.PermissionInformation.DefaultTimeZone()).AddMinutes(-1).ToString(datePattern)), list);
            }
        }

        /// <summary>
        /// Accepts this instance.
        /// </summary>
        /// <param name="acceptingPerson">The accepting person.</param>
        /// <param name="shiftTradeRequestSetChecksum">The shift trade request set checksum.</param>
        /// <param name="authorization">The authorization checker.</param>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2009-06-17
        /// </remarks>
        public override void Accept(IPerson acceptingPerson, IShiftTradeRequestSetChecksum shiftTradeRequestSetChecksum, IPersonRequestCheckAuthorization authorization)
        {
            string notification = "";
            var datePattern = PersonFrom.PermissionInformation.Culture().DateTimeFormat.ShortDatePattern;

            if (isShiftTradeRequestForOneDayOnly())
            {
                notification = string.Format(PersonFrom.PermissionInformation.UICulture(),
                              UserTexts.Resources.ShiftTradeRequestForOneDayHasBeenAcceptedDot,
                              Period.StartDateTimeLocal(PersonFrom.PermissionInformation.DefaultTimeZone()).ToString(datePattern));
            }
            else
            {
                notification = string.Format(PersonFrom.PermissionInformation.UICulture(),
                              UserTexts.Resources.ShiftTradeRequestHasBeenAcceptedDot,
                              Period.StartDateTimeLocal(PersonFrom.PermissionInformation.DefaultTimeZone()).ToString(datePattern),
                              Period.EndDateTimeLocal(PersonFrom.PermissionInformation.DefaultTimeZone()).AddMinutes(-1).ToString(datePattern));
            }

            InParameter.NotNull("acceptingPerson",acceptingPerson);
            ShiftTradeStatus okBothParts = ShiftTradeStatus.OkByBothParts;
            if (Person.Equals(acceptingPerson))
            {
                shiftTradeRequestSetChecksum.SetChecksum(this);
                okBothParts = ShiftTradeStatus.OkByMe;
                SetNotification(notification,new List<IPerson> {PersonTo});
            }
            else
            {
                SetNotification(notification, new List<IPerson> { PersonFrom });
            }
            SetShiftTradeStatus(okBothParts,authorization);
            //TextForNotification = _shiftTradeRequestHasBeenAcceptedDot;
            TextForNotification = notification;
        }

        public override void Refer(IPersonRequestCheckAuthorization authorization)
        {
            SetShiftTradeStatus(ShiftTradeStatus.Referred, authorization);
            //TextForNotification = _shiftTradeRequestHasBeenReferredDot;
            var datePattern = PersonFrom.PermissionInformation.Culture().DateTimeFormat.ShortDatePattern;

            if (!isShiftTradeRequestForOneDayOnly())
            {
                TextForNotification = string.Format(PersonFrom.PermissionInformation.UICulture(),
                              UserTexts.Resources.ShiftTradeRequestForOneDayHasBeenReferredDot,
                              Period.StartDateTimeLocal(PersonFrom.PermissionInformation.DefaultTimeZone()).ToString(datePattern));
            }
            else
            {
                TextForNotification = string.Format(PersonFrom.PermissionInformation.UICulture(),
                              UserTexts.Resources.ShiftTradeRequestHasBeenReferredDot,
                              Period.StartDateTimeLocal(PersonFrom.PermissionInformation.DefaultTimeZone()).ToString(datePattern),
                              Period.EndDateTimeLocal(PersonFrom.PermissionInformation.DefaultTimeZone()).AddMinutes(-1).ToString(datePattern));
            }
        }

        public override string GetDetails(CultureInfo cultureInfo)
        {
            string dates = string.Empty;
            string persons = string.Empty;

            foreach (IShiftTradeSwapDetail detail in ShiftTradeSwapDetails.OrderBy(d => d.DateFrom.Date))
            {
                if (string.IsNullOrEmpty(persons))
                    persons = string.Format(cultureInfo, "{0}, {1}", detail.PersonFrom.Name, detail.PersonTo.Name);

                dates += string.Format(cultureInfo, ", {0}", detail.DateFrom.Date.ToString("d",cultureInfo));
            }
            return string.Format(cultureInfo,"{0}{1}", persons, dates);
        }


        protected internal override IEnumerable<IBusinessRuleResponse> Approve(IRequestApprovalService approvalService)
        {
            var approveResult = approvalService.ApproveShiftTrade(this);
            if (approveResult.IsEmpty())
            {
                var datePattern = PersonFrom.PermissionInformation.Culture().DateTimeFormat.ShortDatePattern;
                //SetNotification(_shiftTradeRequestHasBeenApprovedDot, new List<IPerson>(InvolvedPeople()));
                if (isShiftTradeRequestForOneDayOnly())
                {
                    SetNotification(string.Format(PersonFrom.PermissionInformation.UICulture(),
                                  UserTexts.Resources.ShiftTradeRequestForOneDayHasBeenApprovedDot,
                                  Period.StartDateTimeLocal(PersonFrom.PermissionInformation.DefaultTimeZone()).ToString(datePattern))
                                  , new List<IPerson>(InvolvedPeople()));
                }
                else
                {
                    SetNotification(string.Format(PersonFrom.PermissionInformation.UICulture(),
                                  UserTexts.Resources.ShiftTradeRequestHasBeenApprovedDot,
                                  Period.StartDateTimeLocal(PersonFrom.PermissionInformation.DefaultTimeZone()).ToString(datePattern),
                                  Period.EndDateTimeLocal(PersonFrom.PermissionInformation.DefaultTimeZone()).AddMinutes(-1).ToString(datePattern)), new List<IPerson>(InvolvedPeople()));
                }
            }
            return approveResult;
        }

        /// <summary>
        /// Description for the request type
        /// </summary>
        public override string RequestTypeDescription
        {
            get { return _typeDescription; }
            set { _typeDescription = value; }
        }

    	public override RequestType RequestType
    	{
    		get { return RequestType.ShiftTradeRequest; }
    	}

    	/// <summary>
        /// Payload description
        /// </summary>
        public override Description RequestPayloadDescription
        {
            get { return new Description(); }
        }


        public override IList<IPerson> ReceiversForNotification
        {
            get
            {
                return _receiverOfNotification;
            }
        }
        private void SetNotification(string notification,IList<IPerson> receivers)
        {
            _receiverOfNotification = receivers;
            TextForNotification = notification;
        }


        #region PersonfromTo
        public override IPerson PersonFrom
        {
            get
            {
                return _shiftTradeSwapDetails.First().PersonFrom;
               
            }
        }

        public override IPerson PersonTo
        {
            get
            {
                return _shiftTradeSwapDetails.First().PersonTo;
            }
        }
        #endregion //PersonfromTo

        public virtual IEnumerable<IPerson> InvolvedPeople()
        {
            yield return PersonFrom;
            yield return PersonTo;
        }

        public virtual void NotifyToPersonAfterValidation()
        {
            notifyPersonOfAvailableShiftTradeRequest(_shiftTradeSwapDetails);
        }

        private bool isShiftTradeRequestForOneDayOnly ()
        {
            return _shiftTradeSwapDetails.Count <= 1;
        }
    }
}
