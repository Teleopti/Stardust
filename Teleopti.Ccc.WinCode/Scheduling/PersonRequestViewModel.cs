﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Microsoft.Practices.Composite.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WinCode.Events;
using Teleopti.Ccc.WinCode.Scheduling.Requests;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
    /// <summary>
    /// Adapter for the request view in scheduler
    /// </summary>
    public class PersonRequestViewModel : IPersonRequestViewModel
    {
        private readonly IPersonRequest _personRequest;
        private readonly IShiftTradeRequestStatusChecker _shiftTradeRequestStatusChecker;
        private readonly IPersonAccountCollection _personAccountCollection;
        private bool _causesBrokenBusinessRule;
        private bool _isSelected;
        private bool _isWithinSchedulePeriod = true;
        private readonly IEventAggregator _eventAggregator;
        private ICccTimeZoneInfo _cccTimeZoneInfo;

        public bool IsWithinSchedulePeriod
        {
            get { return _isWithinSchedulePeriod; }
            private set
            {
                if (_isWithinSchedulePeriod != value)
                {
                    _isWithinSchedulePeriod = value;
                    NotifyPropertyChanged("IsWithinSchedulePeriod");
                }
            }
        }
        public PersonRequestViewModel(IPersonRequest personRequest, IShiftTradeRequestStatusChecker shiftTradeRequestStatusChecker,
            IPersonAccountCollection personAccountCollection, IEventAggregator eventAggregator, ICccTimeZoneInfo cccTimeZoneInfo)
        {
            _eventAggregator = eventAggregator;
            _personRequest = personRequest;
            _shiftTradeRequestStatusChecker = shiftTradeRequestStatusChecker;
            _personAccountCollection = personAccountCollection;
            _personRequest.PropertyChanged += PersonRequestViewModel_PropertyChanged;
            _cccTimeZoneInfo = cccTimeZoneInfo;
        }

        private void PersonRequestViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
           NotifyPropertyChanged(e.PropertyName);
        }

        public void ValidateIfWithinSchedulePeriod(DateTimePeriod schedulePeriod, IList<IPerson> persons)
        {
            IsWithinSchedulePeriod = schedulePeriod.Contains(PersonRequest.RequestedDate) && persons.Contains(PersonRequest.Person);
        }

        #region events

        public event EventHandler CausesBrokenBusinessRuleChanged;

        private void OnCausesBrokenBusinessRuleChanged()
        {
            if (CausesBrokenBusinessRuleChanged != null)
            {
                CausesBrokenBusinessRuleChanged(this, null);
            }
        }

        #endregion

        /// <summary>
        /// Get personRequest
        /// </summary>
        public IPersonRequest PersonRequest
        {
            get { return _personRequest; }
        }

        private IRequest Request
        {
            get { return _personRequest.Request; }
        }

        public string RequestedDate
        {
            get
            {
                return ResolveRequestDate();
            }
        }

        public DateTime FirstDateInRequest
        {
            get { return _personRequest.Request.Period.StartDateTime; }
        }

        public string Name
        {
            get { return string.Format(CultureInfo.CurrentUICulture,"{0}",_personRequest.Person.Name); }
        }

        public int Seniority
        {
            get { return _personRequest.Person.Seniority; }
        }

        public string RequestType
        {
            get { return ResolveTypeText(); }
        }

        public bool IsPending
        {
            get { return _personRequest.IsPending; }
        }

        public bool IsDenied
        {
            get { return _personRequest.IsDenied; }
        }

        public string Details
        {
            get
            { 
                return _personRequest.Request.GetDetails(CultureInfo.CurrentCulture);
            }
        }

        public string Subject
        {
            get { return _personRequest.Subject; }
        }

        public string Message
        {
            get { return _personRequest.Message; }
        }

        public DateTime LastUpdated
        {
            get { return _personRequest.UpdatedOn.Value; }
        }

        //Had to add a display-date (sorts on LastUpdated DateTime) bug in .Net Framwork regarding WPF culture
        public string LastUpdatedDisplay
        {
            get
            {
                if(!_personRequest.UpdatedOn.HasValue)
                    return "";
                var convertedDate = TimeZoneHelper.ConvertFromUtc(_personRequest.UpdatedOn.Value, _cccTimeZoneInfo);
                return convertedDate.ToString("g", CultureInfo.CurrentCulture);
            }
        }

        //It is a string for now, but I need to think
        public string Left
        {
            get
            {
                string left = string.Empty;

                IAbsenceRequest request = _personRequest.Request as IAbsenceRequest;
                if (request != null)
                {
                    var account = _personAccountCollection.Find(request.Absence);
                    if (account != null)
                    {
                        var personAccounts = account.Find(request.Period.ToDateOnlyPeriod(_personRequest.Person.PermissionInformation.DefaultTimeZone()));
                    
                        string parseLeft = string.Empty;
                        int colon = 1;
                        foreach (IAccount personAccount in personAccounts)
                        {
                            //personAccount.CalculateBalanceIn();
                            parseLeft += GetRemaining(personAccount);
                            if (!(string.IsNullOrEmpty(parseLeft)) && (personAccounts.Count() != colon))
                                parseLeft += " : ";
                            colon++;
                        }
                        left = parseLeft;
                    }
                }

                return left;
            }
        }

        private string GetRemaining(IAccount account)
        {
            string balance = string.Empty;
 
            if (CheckPersonAccountDayType(account))
                balance = string.Format(CultureInfo.CurrentCulture, "{0} {1}", ((AccountDay)account).Remaining.Days, Resources.Days);
            else
            {
                var span = ((AccountTime)account).Remaining;
                balance = string.Format(CultureInfo.CurrentCulture, "{0} {1}", span.TotalHours, Resources.Hours);
            }

            return balance;
        }
        
        private bool CheckPersonAccountDayType(IAccount account)
        {
            return account.GetType() == typeof(AccountDay);
        }

        private string ResolveTypeText()
        {
            string type = Resources.RequestTypeText;
            if (_personRequest.Request is IAbsenceRequest)
            {
                type = Resources.RequestTypeAbsence;
            }
            else if (_personRequest.Request is IShiftTradeRequest)
            {
                type = Resources.RequestTypeShiftTrade;
            }
            return type;
        }

        //Hmm. looks a bit vidrish, how do we do this better, move to domain in some way?
        private string ResolveRequestDate()
        {
            var localStart = _personRequest.Request.Period.LocalStartDateTime;
            var localEnd = _personRequest.Request.Period.LocalEndDateTime;
            string text = string.Format(CultureInfo.CurrentCulture, "{0} - {1}", localStart.ToShortDateString(), localEnd.ToShortDateString());
            if (localStart.Date.Equals(localEnd.AddMinutes(-1).Date))
            {
                text = localStart.ToShortDateString();
            }

            var shiftTradeRequest = _personRequest.Request as IShiftTradeRequest;
            if (shiftTradeRequest != null)
            {
                if (shiftTradeRequest.ShiftTradeSwapDetails.Count > 1)
                {
                    string multiple = Resources.MultipleValuesParanteses;
                    text = string.Format(CultureInfo.CurrentCulture, "{0} - {1}", localStart.ToShortDateString(), multiple);
                }
            }
            return text;
        }

        public bool CausesBrokenBusinessRule
        {
            get { return _causesBrokenBusinessRule; }
            set
            {
                _causesBrokenBusinessRule = value;
                OnCausesBrokenBusinessRuleChanged();
            }
        }

        /// <summary>
        /// Request type to string, for filtering
        /// </summary>
        public string RequestTypeOfToString
        {
            get { return _personRequest.Request.GetType().ToString(); }
        }

        /// <summary>
        /// Status to string, for filtering
        /// </summary>
        public string StatusText
        {
            get { return _personRequest.StatusText; }
        }

        public bool IsEditable
        {
            get
            {
                IShiftTradeRequest shiftTradeRequest = Request as IShiftTradeRequest;
                if (shiftTradeRequest != null)
                {
                    if (shiftTradeRequest.GetShiftTradeStatus(_shiftTradeRequestStatusChecker) != ShiftTradeStatus.OkByBothParts)
                        return false;
                }
                return (!_personRequest.IsApproved || !_personRequest.IsDenied) && _personRequest.IsEditable;
            }
        }

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    NotifyPropertyChanged("IsSelected");
                    new PersonRequestViewModelIsSelectedChanged(this).PublishEvent("PersonRequestViewModel",_eventAggregator);
                }
            }
        }

        public bool IsApproved { get
        {
            return _personRequest.IsApproved;
        }}

        public bool IsNew
        {
            get { return _personRequest.IsNew; }
        }

        private void NotifyPropertyChanged(string propertyName)
        {
        	var handler = PropertyChanged;
            if (handler!=null)
            {
            	handler(this,new PropertyChangedEventArgs(propertyName));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyStatusChanged()
        {
            NotifyPropertyChanged("StatusText");
            NotifyPropertyChanged("IsNew");
            NotifyPropertyChanged("IsPending");
            NotifyPropertyChanged("IsDenied");
            NotifyPropertyChanged("IsApproved");
            NotifyPropertyChanged("RequestedDate");
            NotifyPropertyChanged("Name");
            NotifyPropertyChanged("Seniority");
            NotifyPropertyChanged("RequestType");
            NotifyPropertyChanged("Details");
            NotifyPropertyChanged("Subject");
            NotifyPropertyChanged("Message");
            NotifyPropertyChanged("LastUpdatedDisplay");
            NotifyPropertyChanged("Left");
        }
    }
}
