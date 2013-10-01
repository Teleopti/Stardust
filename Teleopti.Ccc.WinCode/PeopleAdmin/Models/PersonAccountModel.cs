using System;
using System.Collections.Generic;
using System.Linq;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;

namespace Teleopti.Ccc.WinCode.PeopleAdmin.Models
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
        private bool _expandState;
        private CommonNameDescriptionSetting _commonNameDescription;

        //This ctor is only used from tests
        public PersonAccountModel(ITraceableRefreshService refreshService, DateOnly selectedDate, IPersonAccountCollection personAccounts)
            : this()
        {
            _refreshService = refreshService;
            _containedEntity = personAccounts;
            //gör som bulgarerna - fel här! kan vara flera, en per absence
            _currentAccount = personAccounts.Find(selectedDate).FirstOrDefault();
        }

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

        /// <summary>
        /// Gets the parent.
        /// </summary>
        /// <value>The parent.</value>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-06-09
        /// </remarks>
        public IPersonAccountCollection Parent
        {
            get { return _containedEntity; }
        }

        private bool CheckPersonAccountDayType()
        {
            return _currentAccount.GetType() == typeof(AccountDay);
        }

        /// <summary>
        /// Gets the full name.
        /// </summary>
        /// <value>The full name.</value>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-06-09
        /// </remarks>
        public string FullName
        {
            get
            {
                //fel här, men gör som ukrairnarna
                if (_commonNameDescription == null)
                    return Parent.Person.ToString();
                return _commonNameDescription.BuildCommonNameDescription(Parent.Person);
            }
        }

        /// <summary>
        /// Gets or sets the date.
        /// </summary>
        /// <value>The date.</value>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-08-21
        /// </remarks>
        public DateOnly? AccountDate
        {
            get
            {
                if (_currentAccount == null) return null;
                return _currentAccount.StartDate;
            }
            set
            {
                if (_currentAccount != null && value.HasValue)
                {                   
                    _currentAccount.StartDate = value.Value;
                    RefreshAccount();
                }
            }
        }

        /// <summary>
        /// Gets the balance in.
        /// </summary>
        /// <value>The balance in.</value>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-08-21
        /// </remarks>
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

        /// <summary>
        /// Gets or sets the extra.
        /// </summary>
        /// <value>The extra.</value>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-08-21
        /// </remarks>
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

        /// <summary>
        /// Gets or sets the accrued.
        /// </summary>
        /// <value>The accrued.</value>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-08-21
        /// </remarks>
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

        /// <summary>
        /// Gets the used.
        /// </summary>
        /// <value>The used.</value>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-08-21
        /// </remarks>
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

        /// <summary>
        /// Gets the balance out.
        /// </summary>
        /// <value>The balance out.</value>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-08-21
        /// </remarks>
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

        /// <summary>
        /// Gets and sets the remaining value.
        /// </summary>
        /// <value>The remaining value.</value>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 2011-01-18
        /// </remarks>
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


        /// <summary>
        /// Gets a value indicating whether this instance can gray.
        /// </summary>
        /// <value><c>true</c> if this instance can gray; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-08-21
        /// </remarks>
        public bool CanGray
        {
            get
            {
                return _currentAccount == null;
            }
        }

        /// <summary>
        /// Gets the period count.
        /// </summary>
        /// <value>The period count.</value>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-08-21
        /// </remarks>
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

        /// <summary>
        /// Gets or sets a value indicating whether [expand state].
        /// </summary>
        /// <value><c>true</c> if [expand state]; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-06-10
        /// </remarks>
        public bool ExpandState
        {
            get { return _expandState; }
            set { _expandState = value; }
        }

        public  IAccount CurrentAccount
        {
            get { return _currentAccount; }
        }

        /// <summary>
        /// Gets the tracker description.
        /// </summary>
        /// <value>The tracker description.</value>
        /// <remarks>
        /// Created by: Savani Nirasha
        /// Created date: 8/28/2008
        /// </remarks>
        public IAbsence TrackingAbsence
        {
            get
            {
                if (_currentAccount == null)
                    return null;
                return _currentAccount.Owner.Absence;
            }
            set
            {
                //ta bort denna setter
                if (_currentAccount != null)
                {
                    throw new ArgumentException("There is no absence to work with.");
                }
            }
        }

        /// <summary>
        /// Gets the type of the account.
        /// </summary>
        /// <value>The type of the account.</value>
        /// <remarks>
        /// Created by: Savani Nirasha
        /// Created date: 8/28/2008
        /// </remarks>
        public string AccountType
        {
            get
            {
                if (_currentAccount == null)
                    return string.Empty;
                return _currentAccount.Owner.Absence.Tracker.Description.Name;
            }
        }

        /// <summary>
        /// Gets or sets the grid control.
        /// </summary>
        /// <value>The grid control.</value>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-08-21
        /// </remarks>
        public GridControl GridControl { get; set; }

        /// <summary>
        /// Gets the current person accoung by date.
        /// </summary>
        /// <param name="selectedDate">The selected date.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-08-21
        /// </remarks>
        public IAccount GetCurrentPersonAccountByDate(DateOnly selectedDate)
        {
            //fel här, kan vara fel, men gör som argentinarna
            _currentAccount = _containedEntity.Find(selectedDate).FirstOrDefault();
            return _currentAccount;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance can bold.
        /// </summary>
        /// <value><c>true</c> if this instance can bold; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-11-05
        /// </remarks>
        public bool CanBold
        {
            get; 
            set;
        }

        /// <summary>
        /// Resets the can bold property of child adapters.
        /// </summary>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-11-05
        /// </remarks>
        public void ResetCanBoldPropertyOfChildAdapters()
        {
            if (GridControl != null)
            {
                IList<IPersonAccountChildModel> childAdapters = GridControl.Tag as
                    IList<IPersonAccountChildModel>;

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
