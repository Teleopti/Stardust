using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Foundation;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.PropertyPageAndWizard
{
    /// <summary>
    /// Abstract base class for dialogues showing properties in several different views
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2008-01-15
    /// </remarks>
    public abstract class AbstractPropertyPages<T> : IAbstractPropertyPages, IDisposable where T : IAggregateRoot
    {
        private T _aggregateRoot;
        private IPropertyPage _currentPage;
        private List<IPropertyPage> _propertyPages;
        private Form _owningForm;
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private IUnitOfWork _unitOfWork;
    	private ILazyLoadingManager _lazyManager;

    	/// <summary>
        /// Gets or sets the owner.
        /// </summary>
        /// <value>The owner.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-15
        /// </remarks>
        public Form Owner
        {
            get { return _owningForm; }
            set { _owningForm = value; }
        }

        /// <summary>
        /// Gets the minimum size.
        /// Defaults to 550x400.
        /// Suitable design-time size for a containing UserControl is 300x300 pixels.
        /// Override this Property if your Wizard has other requirements.
        /// </summary>
        /// <value>The minimum size.</value>
        /// <remarks>
        /// Created by: henryg
        /// Created date: 2008-01-28
        /// </remarks>
        public virtual Size MinimumSize
        {
            get
            {
                return new Size(550, 400);
            }
        }

        /// <summary>
        /// Creates the new root.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-15
        /// </remarks>
        public abstract T CreateNewRoot();

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-15
        /// </remarks>
        public abstract string Name { get; }
        /// <summary>
        /// Gets the window text.
        /// </summary>
        /// <value>The window text.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-06-02
        /// </remarks>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-06-02
        /// </remarks>
        public abstract string WindowText { get; }

        protected AbstractPropertyPages(T anAggregateRoot, IRepositoryFactory repositoryFactory, IUnitOfWorkFactory unitOfWorkFactory)
            : this(repositoryFactory, unitOfWorkFactory)
        {
            _aggregateRoot = anAggregateRoot;
        }

        protected AbstractPropertyPages(IRepositoryFactory repositoryFactory, IUnitOfWorkFactory unitOfWorkFactory)
        {
            _repositoryFactory = repositoryFactory;
            _unitOfWorkFactory = unitOfWorkFactory;
        }

        /// <summary>
        /// Initializes the specified pages.
        /// </summary>
        /// <param name="pages">The pages.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-16
        /// </remarks>
        public void Initialize(IList<IPropertyPage> pages, ILazyLoadingManager lazyManager)
        {
            InParameter.NotNull("pages", pages);
        	_lazyManager = lazyManager;
            _propertyPages = new List<IPropertyPage>(pages);
        }

        /// <summary>
        /// Gets the pages.
        /// </summary>
        /// <value>The pages.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-15
        /// </remarks>
        public ReadOnlyCollection <IPropertyPage> Pages
        {
            get
            {
                return new ReadOnlyCollection<IPropertyPage>(_propertyPages);
            }
        }

        /// <summary>
        /// Gets the first page.
        /// </summary>
        /// <value>The first page.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-15
        /// </remarks>
        public IPropertyPage FirstPage
        {
            get { return _propertyPages[0]; }
        }

        /// <summary>
        /// Gets or sets the current page.
        /// </summary>
        /// <value>The current page.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-15
        /// </remarks>
        public IPropertyPage CurrentPage
        {
            get
            {
                if (_currentPage == null)
                {
                    _currentPage = FirstPage;
                }
                return _currentPage;
            }
            set { _currentPage = value; }
        }

        /// <summary>
        /// Gets the previous page.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-15
        /// </remarks>
        public IPropertyPage PreviousPage()
        {
            if (CurrentPage.Depopulate(AggregateRootObject))
            {
                CurrentPage = FindMyPreviousPage(CurrentPage);
                CurrentPage.Populate(AggregateRootObject);
            }
            return CurrentPage;
        }

        /// <summary>
        /// Gets the next page.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-15
        /// </remarks>
        public IPropertyPage NextPage()
        {
            if (CurrentPage.Depopulate(AggregateRootObject))
            {
                CurrentPage = FindMyNextPage(CurrentPage);
                CurrentPage.Populate(AggregateRootObject);
            }
            return CurrentPage;
        }

        /// <summary>
        /// Determines whether [is on first].
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if [is on first]; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-15
        /// </remarks>
        public bool IsOnFirst()
        {
            return (CurrentPage == _propertyPages[0]);
        }

        /// <summary>
        /// Determines whether [is on last].
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if [is on last]; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-15
        /// </remarks>
        public bool IsOnLast()
        {
            return (CurrentPage == _propertyPages[_propertyPages.Count - 1]);
        }

        /// <summary>
        /// Occurs when [name changed].
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-14
        /// </remarks>
        public event EventHandler<WizardNameChangedEventArgs> NameChanged;

        /// <summary>
        /// Triggers the name changed.
        /// </summary>
        /// <param name="eventArgs">The <see cref="WizardNameChangedEventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-14
        /// </remarks>
        public void TriggerNameChanged(WizardNameChangedEventArgs eventArgs)
        {
            if (NameChanged != null)
            {
                NameChanged.Invoke(this, eventArgs);
            }
        }

        /// <summary>
        /// Adds to repository.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-15
        /// </remarks>
        public virtual void AddToRepository()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Saves this instance.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-15
        /// </remarks>
        public virtual IEnumerable<IRootChangeInfo> Save()
        {
            if (CurrentPage.Depopulate(AggregateRootObject))
            {
                using (var uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
                {
                    IEnumerable<IRootChangeInfo> changesMade = UnitOfWork.PersistAll();
                    uow.PersistAll();
                    return changesMade;
                }
            }
            return null;
        }

        /// <summary>
        /// Gets the page names.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-15
        /// </remarks>
        public String[] GetPageNames()
        {
            return _propertyPages
                .Select(pp => pp.PageName)
                .ToArray<string>();
        }

        /// <summary>
        /// Gets or sets the aggregate root object.
        /// </summary>
        /// <value>The aggregate root object.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-15
        /// </remarks>
        public T AggregateRootObject
        {
            get
            {
                if (_aggregateRoot == null)
                {
                    _aggregateRoot = CreateNewRoot();
                }
                return _aggregateRoot;
            }
            protected set { _aggregateRoot = value; }
        }

        /// <summary>
        /// Finds my next page.
        /// </summary>
        /// <param name="propertyPage">The property page.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-15
        /// </remarks>
        public IPropertyPage FindMyNextPage(IPropertyPage propertyPage)
        {
            return FindPageByOffset(propertyPage,1);
        }

        /// <summary>
        /// Finds my previous page.
        /// </summary>
        /// <param name="propertyPage">The property page.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-15
        /// </remarks>
        public IPropertyPage FindMyPreviousPage(IPropertyPage propertyPage)
        {
            return FindPageByOffset(propertyPage, -1);
        }

        /// <summary>
        /// Finds the page by offset.
        /// </summary>
        /// <param name="propertyPage">The property page.</param>
        /// <param name="offset">The offset.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-15
        /// </remarks>
        private IPropertyPage FindPageByOffset(IPropertyPage propertyPage, int offset)
        {
            int myIndex = _propertyPages.IndexOf(propertyPage);
            IPropertyPage page = _propertyPages.ElementAtOrDefault(myIndex + offset);
            if (page == null) page = propertyPage;

            return page;
        }

        /// <summary>
        /// Shows the page.
        /// </summary>
        /// <param name="propertyPage">The property page.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-15
        /// </remarks>
        public IPropertyPage ShowPage(IPropertyPage propertyPage)
        {
            bool okToSwitchPage = true;
            if (_currentPage != null)
            {
                okToSwitchPage = CurrentPage.Depopulate(AggregateRootObject);
            }
            if (okToSwitchPage)
            {
                CurrentPage = propertyPage;
                CurrentPage.Populate(AggregateRootObject);
            }
            return CurrentPage;
        }

        /// <summary>
        /// Gets a value indicating whether [mode is "create new" false if in "edit existing item"].
        /// </summary>
        /// <value><c>true</c> if [mode create new]; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-02-19
        /// </remarks>
        public virtual bool ModeCreateNew
        {
            get { return false; }
        }

        /// <summary>
        /// Gets the unit of work.
        /// </summary>
        /// <value>The unit of work.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-15
        /// </remarks>
        protected IUnitOfWork UnitOfWork
        {
            get
            {
                if (_unitOfWork == null)
                {
                    _unitOfWork = _unitOfWorkFactory.CreateAndOpenUnitOfWork();
                }
                return _unitOfWork;
            }
        }

        /// <summary>
        /// Gets the repository factory.
        /// </summary>
        /// <value>The repository factory.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-15
        /// </remarks>
        protected IRepositoryFactory RepositoryFactory
        {
            get { return _repositoryFactory; }
        }

        /// <summary>
        /// Gets the repository object.
        /// </summary>
        /// <value>The repository object.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-15
        /// </remarks>
        public abstract IRepository<T> RepositoryObject { get; }

        /// <summary>
        /// Loads a working copy of AggregateRoot.
        /// Typically used for Properties dialogs and such.
        /// Overload this method if you need to load other entities aswell.
        /// For example using a Workload would require to load corresponding Skill too.
        /// </summary>
        /// <remarks>
        /// Created by: henryg
        /// Created date: 2008-02-23
        /// </remarks>
        /// <remarks>
        /// Created by: henryg
        /// Created date: 2008-02-23
        /// </remarks>
        public virtual void LoadAggregateRootWorkingCopy()
        {
            if (_aggregateRoot.Id.HasValue)
            {
                _aggregateRoot = RepositoryObject.Get(_aggregateRoot.Id.Value);
				_lazyManager.Initialize(_aggregateRoot);
            }
        }

        #region IDisposable Members

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-15
        /// </remarks>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-15
        /// </remarks>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // free managed resources
                if (_owningForm != null)
                {
                    _owningForm.Dispose();
                }
                if (_unitOfWork != null)
                {
                    _unitOfWork.Dispose();
                    _unitOfWork = null;
                }

                //Free all intialized pages
                if (_propertyPages != null)
                {
                    foreach (IPropertyPage propertyPage in _propertyPages)
                    {
                        propertyPage.Dispose();
                    }
                }
            }
        }

        #endregion
    }
}
