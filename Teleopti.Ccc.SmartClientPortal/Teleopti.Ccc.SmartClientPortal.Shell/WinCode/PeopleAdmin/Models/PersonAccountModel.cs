using System;
using System.Collections.Generic;
using System.Linq;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Domain.Tracking;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Models
{
    /// <summary>
    /// Adapter class for Person Account 
    /// </summary>
    /// <remarks>
    /// Created by: Dinesh Ranasinghe
    /// Created date: 2008-08-21
    /// </remarks>
    public class PersonAccountModel : IPersonAccountModel
    {
        private readonly ITraceableRefreshService _refreshService;
        private readonly IPersonAccountCollection _containedEntity;
        private IAccount _currentAccount;
	    private CommonNameDescriptionSetting _commonNameDescription;
		
        public PersonAccountModel(ITraceableRefreshService refreshService, IPersonAccountCollection personAccounts, IAccount account, CommonNameDescriptionSetting commonNameDescription)
            : this()
        {
            _refreshService = refreshService;
            _containedEntity = personAccounts;
            _currentAccount = account;
            _commonNameDescription = commonNameDescription;
        }
        
        protected PersonAccountModel()
        {
            UnitOfWorkFactory = Infrastructure.UnitOfWork.UnitOfWorkFactory.Current;
        }

        protected IUnitOfWorkFactory UnitOfWorkFactory { get; set; }

        private void RefreshAccount()
        {
            using (UnitOfWorkFactory.CreateAndOpenUnitOfWork())
            {
                _refreshService.Refresh(_currentAccount);
            }
        }

        public IPersonAccountCollection Parent => _containedEntity;

	    private bool CheckPersonAccountDayType()
        {
            return _currentAccount.GetType() == typeof(AccountDay);
        }

        public string FullName
        {
            get
            {
                //fel här, men gör som ukrairnarna
                if (_commonNameDescription == null)
                    return Parent.Person.ToString();
                return _commonNameDescription.BuildFor(Parent.Person);
            }
        }

        public DateOnly? AccountDate
        {
            get => _currentAccount?.StartDate;
	        set
            {
                if (_currentAccount != null && value.HasValue)
                {                   
                    _currentAccount.StartDate = value.Value;
                    RefreshAccount();
                }
            }
        }

        public object BalanceIn
        {
            get
            {
                if (_currentAccount == null) return null;

                if (CheckPersonAccountDayType())
                    return _currentAccount.BalanceIn.Days;

                return _currentAccount.BalanceIn;
            }
            set
            {
                if (_currentAccount != null)
                {
                    if (CheckPersonAccountDayType())
                        _currentAccount.BalanceIn = TimeSpan.FromDays((int)value);
                    else
                        _currentAccount.BalanceIn = (TimeSpan)value;
                    RefreshAccount();
                }
            }
        }

        public object Extra
        {
            get
            {
                if (_currentAccount == null) return null;
                if (CheckPersonAccountDayType())
                    return _currentAccount.Extra.Days;

                return _currentAccount.Extra;
            }
            set
            {
                if (_currentAccount != null)
                {
                    if (CheckPersonAccountDayType())
                        _currentAccount.Extra = TimeSpan.FromDays((int)value);
                    else 
                        _currentAccount.Extra = (TimeSpan) value;
                    RefreshAccount();
                }
            }
        }

        public object Accrued
        {
            get
            {
                if (_currentAccount == null) return null;
                if (CheckPersonAccountDayType())
                    return _currentAccount.Accrued.Days;

                return _currentAccount.Accrued;
            }

            set
            {
                if (_currentAccount != null)
                {
                    if (CheckPersonAccountDayType())
                        _currentAccount.Accrued = TimeSpan.FromDays((int)value);
                    else
                        _currentAccount.Accrued = (TimeSpan)value;
                    RefreshAccount();
                }
            }
        }

        public object Used
        {
            get
            {
                if (_currentAccount == null) return null;

                if (CheckPersonAccountDayType())
                    return _currentAccount.LatestCalculatedBalance.Days;

                return _currentAccount.LatestCalculatedBalance;
            }
        }

        public object BalanceOut
        {
            get
            {
                if (_currentAccount == null) return null;

                if (CheckPersonAccountDayType())
                    return _currentAccount.BalanceOut.Days;

                return _currentAccount.BalanceOut;
            }
            set
            {
                if (_currentAccount != null)
                {
                    if (CheckPersonAccountDayType())
                        _currentAccount.BalanceOut = TimeSpan.FromDays((int)value);
                    else
                        _currentAccount.BalanceOut = (TimeSpan)value;
                    RefreshAccount();
                }
            }
        }

        public object Remaining
        {
            get
            {
                if (_currentAccount == null) return null;

                if (CheckPersonAccountDayType())
                    return _currentAccount.Remaining.Days;

                return _currentAccount.Remaining;
            }
        }

        public bool CanGray => _currentAccount == null;

	    public int PersonAccountCount
        {
            get
            {
                var bosnierna = Parent.AllPersonAccounts().Count();
                //fattar inte, men gör som danskarna
                if (_currentAccount != null && bosnierna == 1)
                    return 0;
                return bosnierna;
            }
        }

        public bool ExpandState { get; set; }

	    public  IAccount CurrentAccount => _currentAccount;

	    public IAbsence TrackingAbsence
        {
			get => _currentAccount?.Owner.Absence;
			set
            {
                if (_currentAccount != null)
                {
                    throw new ArgumentException("There is no absence to work with.");
                }
            }
        }

        public string AccountType
        {
            get
            {
                if (_currentAccount == null)
                    return string.Empty;
                return _currentAccount.Owner.Absence.Tracker.Description.Name;
            }
        }

        public GridControl GridControl { get; set; }

        public bool CanBold { get; set; }

        public void ResetCanBoldPropertyOfChildAdapters()
        {
            if (GridControl != null)
            {
                var childAdapters = GridControl.Tag as IList<IPersonAccountChildModel>;
                if (childAdapters != null)
                {
                    for (int i = 0; i < childAdapters.Count; i++)
                    {
                        childAdapters[i].CanBold = false;
                    }
                }

                GridControl.Invalidate();
            }
        }
    }
}
