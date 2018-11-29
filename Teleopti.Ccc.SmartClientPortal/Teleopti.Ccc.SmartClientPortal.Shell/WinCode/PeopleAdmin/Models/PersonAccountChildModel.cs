using System;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Domain.Tracking;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Models
{
    public class PersonAccountChildModel : EntityContainer<IAccount>, IPersonAccountChildModel
    {
        private readonly ITraceableRefreshService _refreshService;
        private readonly IPersonAccountCollection _containedEntity;
        private IAccount _currentAccount;
        private CommonNameDescriptionSetting _commonNameDescription;
		private readonly IPersonAccountUpdater _personAccountUpdater;
        

        protected PersonAccountChildModel()
        {
            UnitOfWorkFactory = Infrastructure.UnitOfWork.UnitOfWorkFactory.Current;
        }

        public PersonAccountChildModel(ITraceableRefreshService refreshService, IPersonAccountCollection personAccounts, IAccount account, CommonNameDescriptionSetting commonNameDescription, IPersonAccountUpdater personAccountUpdater)
            : this()
        {
            _refreshService = refreshService;
            _containedEntity = personAccounts;
            _currentAccount = account;
            _commonNameDescription = commonNameDescription;
	        _personAccountUpdater = personAccountUpdater;
            base.ContainedEntity = account;
        }

        protected IUnitOfWorkFactory UnitOfWorkFactory { get; set; }

        /// <summary>
        /// Gets the parent.
        /// </summary>
        /// <value>The parent.</value>
        /// <remarks>
        /// Created by: Shiran Ginige
        /// Created date: 2008-06-09
        /// </remarks>
        public IPersonAccountCollection Parent
        {
            get { return _containedEntity; }
        }

        private void RefreshAccount()
        {
            using (UnitOfWorkFactory.CreateAndOpenUnitOfWork())
            {
                _refreshService.Refresh(_currentAccount);
            }
        }

        /// <summary>
        /// Gets the full name.
        /// </summary>
        /// <value>The full name.</value>
        /// <remarks>
        /// Created by: Shiran Ginige
        /// Created date: 2008-06-09
        /// </remarks>
        public string FullName
        {
            get
            {
                //fel här, men gör som tyskarna
                if (_commonNameDescription == null)
                    return Parent.Person.ToString();
                return _commonNameDescription.BuildFor(Parent.Person);
            }
        }

        /// <summary>
        /// Gets the balance in.
        /// </summary>
        /// <value>The balance in.</value>
        /// <remarks>
        /// Created by: Shiran Ginige
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

        private bool CheckPersonAccountDayType()
        {
            return _currentAccount.GetType() == typeof (AccountDay);
        }

        /// <summary>
        /// Gets the used.
        /// </summary>
        /// <value>The used.</value>
        /// <remarks>
        /// Created by: Shiran Ginige
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
        /// Created by: Shiran Ginige
        /// Created date: 2008-08-21
        /// </remarks>
        public object BalanceOut
        {
            get
            {
                if (_currentAccount == null) return null;

                if(CheckPersonAccountDayType())
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

        /// <summary>
        /// Gets the current accoount.
        /// </summary>
        /// <value>The current accoount.</value>
        /// <remarks>
        /// Created by: Savani Nirasha
        /// Created date: 9/5/2008
        /// </remarks>
        public IAccount CurrentAccount
        {
            get { return _currentAccount; }
        }

        /// <summary>
        /// Gets or sets the date.
        /// </summary>
        /// <value>The date.</value>
        /// <remarks>
        /// Created by: Shiran Ginige
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
					_personAccountUpdater.Update(_currentAccount.Owner.Person);
                }
            }
        }

        /// <summary>
        /// Gets or sets the extra.
        /// </summary>
        /// <value>The extra.</value>
        /// <remarks>
        /// Created by: Shiran Ginige
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
                        _currentAccount.Extra = (TimeSpan)value;
                    
                    RefreshAccount();
                }

            }
        }

        /// <summary>
        /// Gets or sets the accrued.
        /// </summary>
        /// <value>The accrued.</value>
        /// <remarks>
        /// Created by: Shiran Ginige
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
                        _currentAccount.Accrued = TimeSpan.FromDays((int) value);
                    else
                       _currentAccount.Accrued = (TimeSpan) value;
                }
                RefreshAccount();
            }
        }

        public bool CanBold
        {
            get ; 
            set ;
        }

        /// <summary>
        /// Gets a value indicating whether this instance can gray.
        /// </summary>
        /// <value><c>true</c> if this instance can gray; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by: Shiran Ginige
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
                 if (_currentAccount != null)
                 {
                     //fixa sen. Om du inte vet hur, fråga brasilianarna
                     throw new ArgumentException("Kan inte sätta absence ännu");
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

        public new IAccount ContainedEntity
        {
            get { return base.ContainedEntity; }
        }
    }
}
