using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.UserTexts;

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
		private string _typeDescription;
		private IList<IPerson> _receiverOfNotification = new List<IPerson>();
		private IShiftExchangeOffer _offer;
		/// <summary>
		/// Initializes a new instance of the <see cref="ShiftTradeRequest"/> class.
		/// </summary>
		/// <remarks>
		/// Created by: Dinesh Ranasinghe
		/// Created date: 2008-09-19
		/// </remarks>
		protected ShiftTradeRequest()
		{
			_typeDescription = Resources.RequestTypeShiftTrade;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ShiftTradeRequest"/> class.
		/// </summary>
		/// <param name="shiftTradeSwapDetails">The shift trade swap details.</param>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2009-08-28
		/// </remarks>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage",
			"CA2214:DoNotCallOverridableMethodsInConstructors")]
		public ShiftTradeRequest(IList<IShiftTradeSwapDetail> shiftTradeSwapDetails)
			: this()
		{
			_shiftTradeSwapDetails = new List<IShiftTradeSwapDetail>(shiftTradeSwapDetails);
			calculateAndSetPeriod();
			notifyPersonOfAvailableShiftTradeRequest(shiftTradeSwapDetails);
		}

		private void notifyPersonOfAvailableShiftTradeRequest(IEnumerable<IShiftTradeSwapDetail> shiftTradeSwapDetails)
		{
			if (shiftTradeSwapDetails.IsEmpty()) return;

			var notification = getNotificationForAddingSwapDetail();
			setNotification(notification, new List<IPerson> {PersonTo});
		}

		private void calculateAndSetPeriod()
		{
			if (_shiftTradeSwapDetails.Any())
			{
				DateOnly minDate = DateOnly.MaxValue, maxDate = DateOnly.MinValue;
				foreach (var shiftTradeSwapDetail in _shiftTradeSwapDetails)
				{
					shiftTradeSwapDetail.SetParent(this);
					if (shiftTradeSwapDetail.DateFrom < minDate) minDate = shiftTradeSwapDetail.DateFrom;
					if (shiftTradeSwapDetail.DateTo < minDate) minDate = shiftTradeSwapDetail.DateTo;
					if (shiftTradeSwapDetail.DateFrom > maxDate) maxDate = shiftTradeSwapDetail.DateFrom;
					if (shiftTradeSwapDetail.DateTo > maxDate) maxDate = shiftTradeSwapDetail.DateTo;
				}
				var period = getPeriodEncompassingShiftTradeSwapDetails(minDate, maxDate);

				SetPeriod(period);
			}
			else
			{
				SetPeriod(new DateTimePeriod());
			}
		}

		private DateTimePeriod getPeriodEncompassingShiftTradeSwapDetails(DateOnly startDate, DateOnly endDate)
		{
			var timeZoneInfo = PersonFrom.PermissionInformation.DefaultTimeZone();
			var dateOnlyPeriod = new DateOnlyPeriod(startDate, endDate).ToDateTimePeriod(timeZoneInfo);
			// Remove one minute so is end of last day
			return dateOnlyPeriod.ChangeEndTime(TimeSpan.FromMinutes(-1));
		}

		#region Methods to get notification

		private string getNotificationTemplate(string resourceKey, CultureInfo culture)
		{
			return Resources.ResourceManager.GetString(resourceKey, culture) ??
					Resources.ResourceManager.GetString(resourceKey);
		}

		private string getNotificationForAccept()
		{
			var personFromInfo = PersonFrom.PermissionInformation;
			var culture = personFromInfo.Culture();
			var language = personFromInfo.UICulture();
			var timezone = personFromInfo.DefaultTimeZone();
			var datePattern = culture.DateTimeFormat.ShortDatePattern;

			string notification;
			if (isShiftTradeRequestForOneDayOnly())
			{
				var notificationTemplate = getNotificationTemplate(nameof(Resources.ShiftTradeRequestForOneDayHasBeenAcceptedDot), language);
				notification = string.Format(culture, notificationTemplate,
					Period.StartDateTimeLocal(timezone).ToString(datePattern));
			}
			else
			{
				var notificationTemplate = getNotificationTemplate(nameof(Resources.ShiftTradeRequestHasBeenAcceptedDot), language);
				notification = string.Format(culture, notificationTemplate,
					Period.StartDateTimeLocal(timezone).ToString(datePattern),
					Period.EndDateTimeLocal(timezone).ToString(datePattern));
			}
			return notification;
		}

		private string getNotificationForAddingSwapDetail()
		{
			var personToInfo = PersonTo.PermissionInformation;
			var culture = personToInfo.Culture();
			var language = personToInfo.UICulture();
			var timezone = personToInfo.DefaultTimeZone();
			var datePattern = culture.DateTimeFormat.ShortDatePattern;

			string notification;
			if (isShiftTradeRequestForOneDayOnly())
			{
				var notificationTemplate = getNotificationTemplate(nameof(Resources.ANewShiftTradeForOneDayHasBeenCreatedDot), language);
				notification = string.Format(culture, notificationTemplate,
					Period.StartDateTimeLocal(timezone).ToString(datePattern));
			}
			else
			{
				var notificationTemplate = getNotificationTemplate(nameof(Resources.ANewShiftTradeHasBeenCreatedDot), language);
				notification = string.Format(culture, notificationTemplate,
					Period.StartDateTimeLocal(timezone).ToString(datePattern),
					Period.EndDateTimeLocal(timezone).ToString(datePattern));
			}
			return notification;
		}

		private string getNotificationForDeny()
		{
			var personFromInfo = PersonFrom.PermissionInformation;
			var culture = personFromInfo.Culture();
			var language = personFromInfo.UICulture();
			var timezone = personFromInfo.DefaultTimeZone();
			var datePattern = culture.DateTimeFormat.ShortDatePattern;

			string notification;
			if (isShiftTradeRequestForOneDayOnly())
			{
				var notificationTemplate = getNotificationTemplate(nameof(Resources.ShiftTradeRequestForOneDayHasBeenDeniedDot), language);
				notification = string.Format(culture, notificationTemplate,
					Period.StartDateTimeLocal(timezone).ToString(datePattern));
			}
			else
			{
				var notificationTemplate = getNotificationTemplate(nameof(Resources.ShiftTradeRequestHasBeenDeniedDot), language);
				notification = string.Format(culture, notificationTemplate,
					Period.StartDateTimeLocal(timezone).ToString(datePattern),
					Period.EndDateTimeLocal(timezone).ToString(datePattern));
			}
			return notification;
		}

		private string getNotificationForRefer()
		{
			var personFromInfo = PersonFrom.PermissionInformation;
			var culture = personFromInfo.Culture();
			var language = personFromInfo.UICulture();
			var timezone = personFromInfo.DefaultTimeZone();
			var datePattern = culture.DateTimeFormat.ShortDatePattern;

			string notification;
			if (!isShiftTradeRequestForOneDayOnly())
			{
				var notificationTemplate = getNotificationTemplate(nameof(Resources.ShiftTradeRequestForOneDayHasBeenReferredDot), language);
				notification = string.Format(culture, notificationTemplate,
					Period.StartDateTimeLocal(timezone).ToString(datePattern));
			}
			else
			{
				var notificationTemplate = getNotificationTemplate(nameof(Resources.ShiftTradeRequestHasBeenReferredDot), language);
				notification = string.Format(culture, notificationTemplate,
					Period.StartDateTimeLocal(timezone).ToString(datePattern),
					Period.EndDateTimeLocal(timezone).ToString(datePattern));
			}
			return notification;
		}

		private string getNotificationForApprove()
		{
			var personFromInfo = PersonFrom.PermissionInformation;
			var culture = personFromInfo.Culture();
			var language = personFromInfo.UICulture();
			var timezone = personFromInfo.DefaultTimeZone();
			var datePattern = culture.DateTimeFormat.ShortDatePattern;

			string notification;
			if (isShiftTradeRequestForOneDayOnly())
			{
				var notificationTemplate = getNotificationTemplate(nameof(Resources.ShiftTradeRequestForOneDayHasBeenApprovedDot), language);
				notification = string.Format(culture, notificationTemplate,
					Period.StartDateTimeLocal(timezone).ToString(datePattern));
			}
			else
			{
				var notificationTemplate = getNotificationTemplate(nameof(Resources.ShiftTradeRequestHasBeenApprovedDot), language);
				notification = string.Format(culture, notificationTemplate,
					Period.StartDateTimeLocal(timezone).ToString(datePattern),
					Period.EndDateTimeLocal(timezone).ToString(datePattern));
			}
			return notification;
		}

		#endregion

		public virtual ReadOnlyCollection<IShiftTradeSwapDetail> ShiftTradeSwapDetails =>
			new ReadOnlyCollection<IShiftTradeSwapDetail>(_shiftTradeSwapDetails);

		public virtual void AddShiftTradeSwapDetail(IShiftTradeSwapDetail shiftTradeSwapDetail)
		{
			verifyEditingShiftTradeIsAllowed();
			shiftTradeSwapDetail.SetParent(this);
			_shiftTradeSwapDetails.Add(shiftTradeSwapDetail);
			calculateAndSetPeriod();

			var notification = getNotificationForAddingSwapDetail();
			setNotification(notification, new List<IPerson>
			{
				shiftTradeSwapDetail.PersonTo
			});
		}

		public virtual void ClearShiftTradeSwapDetails()
		{
			verifyEditingShiftTradeIsAllowed();
			_shiftTradeSwapDetails.Clear();
			calculateAndSetPeriod();
		}

		private void verifyEditingShiftTradeIsAllowed()
		{
			if (shiftTradeStatus != ShiftTradeStatus.OkByMe &&
				shiftTradeStatus != ShiftTradeStatus.Referred)
			{
				throw new ShiftTradeRequestStatusException(
					"Editing of shift trades is only allowed when in status modes OkByMe or Referred.");
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
		public virtual void SetShiftTradeStatus(ShiftTradeStatus shiftTradeStatusToSet,
			IPersonRequestCheckAuthorization authorization)
		{
			verifyShiftTradeStatusIsAllowed(shiftTradeStatusToSet);
			shiftTradeStatus = shiftTradeStatusToSet;
			switch (shiftTradeStatus)
			{
				case ShiftTradeStatus.NotValid:
					((IPersonRequest) Parent).Deny(null, authorization);
					break;
				case ShiftTradeStatus.Referred:
					((IPersonRequest) Parent).ForcePending();
					break;
			}
		}

		private void verifyShiftTradeStatusIsAllowed(ShiftTradeStatus shiftTradeStatusToVerify)
		{
			if (shiftTradeStatus == ShiftTradeStatus.Referred &&
				shiftTradeStatusToVerify == ShiftTradeStatus.OkByBothParts)
			{
				throw new ShiftTradeRequestStatusException(
					"The shift trade status OkByBothParts is not allowed when going from Referred.");
			}
		}

		public override void Deny(IPerson denyPerson)
		{
			var list = new List<IPerson>(InvolvedPeople());
			list.Remove(denyPerson);

			if (Offer != null)
			{
				Offer.Status = ShiftExchangeOfferStatus.Pending;
			}

			var notification = getNotificationForDeny();
			setNotification(notification, list);
		}

		public override void Cancel()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Accepts the shift trade request.
		/// </summary>
		/// <param name="acceptingPerson">The accepting person.</param>
		/// <param name="shiftTradeRequestSetChecksum">The shift trade request set checksum.</param>
		/// <param name="authorization">The authorization checker.</param>
		/// <remarks>
		/// Created by: peterwe
		/// Created date: 2009-06-17
		/// </remarks>
		public virtual void Accept(IPerson acceptingPerson, IShiftTradeRequestSetChecksum shiftTradeRequestSetChecksum,
			IPersonRequestCheckAuthorization authorization)
		{
			InParameter.NotNull(nameof(acceptingPerson), acceptingPerson);

			var notification = getNotificationForAccept();
			var okBothParts = ShiftTradeStatus.OkByBothParts;

			if (Person.Equals(acceptingPerson))
			{
				shiftTradeRequestSetChecksum.SetChecksum(this);
				okBothParts = ShiftTradeStatus.OkByMe;
				setNotification(notification, new List<IPerson> {PersonTo});
			}
			else
			{
				setNotification(notification, new List<IPerson> {PersonFrom});
			}
			SetShiftTradeStatus(okBothParts, authorization);
			TextForNotification = notification;
		}

		public virtual void Refer(IPersonRequestCheckAuthorization authorization)
		{
			SetShiftTradeStatus(ShiftTradeStatus.Referred, authorization);
			TextForNotification = getNotificationForRefer();
		}

		public override string GetDetails(CultureInfo cultureInfo)
		{
			var dates = string.Empty;
			var persons = string.Empty;

			foreach (var detail in ShiftTradeSwapDetails.OrderBy(d => d.DateFrom.Date))
			{
				if (string.IsNullOrEmpty(persons))
					persons = string.Format(cultureInfo, "{0}, {1}", detail.PersonFrom.Name, detail.PersonTo.Name);

				dates += string.Format(cultureInfo, ", {0}", detail.DateFrom.Date.ToString("d", cultureInfo));
			}
			return string.Format(cultureInfo, "{0}{1}", persons, dates);
		}

		protected internal override IEnumerable<IBusinessRuleResponse> Approve(IRequestApprovalService approvalService)
		{
			var businessRuleResponses = approvalService.Approve(this).ToArray();
			if (businessRuleResponses.Any())
			{
				return businessRuleResponses;
			}

			if (Offer != null)
			{
				Offer.Status = ShiftExchangeOfferStatus.Completed;
			}

			var notification = getNotificationForApprove();
			setNotification(notification, new List<IPerson>(InvolvedPeople()));

			return businessRuleResponses;
		}

		/// <summary>
		/// Description for the request type
		/// </summary>
		public override string RequestTypeDescription
		{
			get { return _typeDescription; }
			set { _typeDescription = value; }
		}

		public override RequestType RequestType => RequestType.ShiftTradeRequest;

		public virtual IShiftExchangeOffer Offer
		{
			get { return _offer; }
			set { _offer = value; }
		}

		public override Description RequestPayloadDescription => new Description();

		public override IList<IPerson> ReceiversForNotification => _receiverOfNotification;

		private void setNotification(string notification, IList<IPerson> receivers)
		{
			_receiverOfNotification = receivers;
			TextForNotification = notification;
		}

		#region PersonfromTo

		public override IPerson PersonFrom => _shiftTradeSwapDetails.Any()
			? _shiftTradeSwapDetails.First().PersonFrom
			: null;

		public override IPerson PersonTo => _shiftTradeSwapDetails.Any()
			? _shiftTradeSwapDetails.First().PersonTo
			: null;

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

		private bool isShiftTradeRequestForOneDayOnly()
		{
			return _shiftTradeSwapDetails.Count <= 1;
		}
	}
}
