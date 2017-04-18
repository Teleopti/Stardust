using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Forms;
using MbCache.Core;
using Microsoft.Practices.Composite.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts.Events;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts.Interfaces;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts.Views;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts.Presenters
{
    public class ExplorerPresenter : IExplorerPresenter
    {
        private readonly IExplorerViewModel _model;
        private readonly IExplorerView _view;
        private readonly IDataHelper _uowHelper;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IEventAggregator _eventAggregator;
        private readonly IMbCacheFactory _mbCacheFactory;
        
        private readonly INavigationPresenter _navigationPresenter;
        private readonly IGeneralPresenter _generalPresenter;
        private readonly IVisualizePresenter _visualizePresenter;


        public ExplorerPresenter(IExplorerView view, IDataHelper dataHelper, IRuleSetProjectionEntityService ruleSetProjectionEntityService, 
            IUnitOfWorkFactory unitOfWorkFactory, IEventAggregator eventAggregator, IMbCacheFactory mbCacheFactory, IExplorerViewModel model)
        {
            _view = view;
            _uowHelper = dataHelper;
            _unitOfWorkFactory = unitOfWorkFactory;
            _eventAggregator = eventAggregator;
            _mbCacheFactory = mbCacheFactory;
            _model = model;
            _navigationPresenter = new NavigationPresenter(this,dataHelper);
            _generalPresenter = new GeneralPresenter(this,dataHelper);
            _visualizePresenter = new VisualizePresenter(this, dataHelper, ruleSetProjectionEntityService);
            _eventAggregator.GetEvent<RuleSetChanged>().Subscribe(ruleSetChanged);
        }

        private void ruleSetChanged(IList<IWorkShiftRuleSet> obj)
        {
            _mbCacheFactory.Invalidate<IRuleSetProjectionEntityService>();
        }

        private void setDefaultSegment()
        {
            _model.DefaultSegment = DataWorkHelper.DefaultSegment();
        }

        private void loadAll()
        {
            using(var uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
            {
                
                Model.SetActivityCollection(DataWorkHelper.FindAllActivities(uow));
                Model.SetCategoryCollection(DataWorkHelper.FindAllCategories(uow));
                Model.SetRuleSetCollection(new ReadOnlyCollection<IWorkShiftRuleSet>(DataWorkHelper.FindRuleSets(uow)));
                Model.SetRuleSetBagCollection(new ReadOnlyCollection<IRuleSetBag>(DataWorkHelper.FindRuleSetBags(uow)));
                
            }
            
            loadClassTypeCollection();
            loadOperatorCollection();
            loadAccessibilityCollection();
        }

        public IExplorerView View
        {
            get
            {
                return _view;
            }
        }

        public IExplorerViewModel Model
        {
            get { return _model;}
        }

        public INavigationPresenter NavigationPresenter
        {
            get
            {
                return _navigationPresenter;
            }
        }

       public IGeneralPresenter GeneralPresenter
        {
            get
            {
                return _generalPresenter;
            }
        }

        public IVisualizePresenter VisualizePresenter
        {
            get
            {
                return _visualizePresenter;
            }
        }

        private void loadOperatorCollection()
        {
            Model.SetOperatorLimitCollection(new ReadOnlyCollection<string>(DataWorkHelper.FindAllOperatorLimits()));
        }

        private void loadClassTypeCollection()
        {
            IList<GridClassType> gridClassTypes = new List<GridClassType>();
            gridClassTypes.Add(new GridClassType(UserTexts.Resources.AbsoluteStart, typeof(ActivityAbsoluteStartExtender)));
            gridClassTypes.Add(new GridClassType(UserTexts.Resources.RelativeStart, typeof(ActivityRelativeStartExtender)));
            gridClassTypes.Add(new GridClassType(UserTexts.Resources.RelativeEnd, typeof(ActivityRelativeEndExtender)));
            Model.SetClassTypeCollection(gridClassTypes);
        }

        private void loadAccessibilityCollection()
        {
            Model.SetAccessibilityCollection(DataWorkHelper.FindAllAccessibilities());
        }

        public bool Validate()
        {
            return _generalPresenter.Validate();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Ccc.WinCode.Common.IViewBase.ShowErrorMessage(System.String,System.String)")]
        public bool Persist()
        {
            try
            {
                _uowHelper.PersistAll();
                _view.UpdateNavigationViewTreeIcons();
            }
            catch (OptimisticLockException)
            {
                _view.ShowErrorMessage(string.Concat(UserTexts.Resources.SomeoneElseHaveChanged + "." + UserTexts.Resources.PleaseTryAgainLater, " "), UserTexts.Resources.Message);

                loadAll();
                _view.RefreshChildViews();
                _view.ForceRefreshNavigationView();
                return false;
            }
            catch (DataSourceException dataSourceException)
            {
                _view.ShowDataSourceException(dataSourceException);
                return false;
            }
            return true;
        }

        public bool CheckForUnsavedData()
        {
            bool ret = true;
            if (_uowHelper.HasUnsavedData())
            {
                DialogResult response = _view.ShowConfirmationMessage(UserTexts.Resources.DoYouWantToSaveChangesYouMade, UserTexts.Resources.Shifts);
                switch (response)
                {
                    case DialogResult.No:
                        break;
                    case DialogResult.Yes:
                        return Persist();
                    default:
                        ret = false;
                        break;
                }
            }

            return ret;
        }

        public void Show(Form mainWindow)
        {
            setDefaultSegment();
            loadAll();
            _view.Show(this, mainWindow);
        }

       
        public IDataHelper DataWorkHelper
        {
            get
            {
                return _uowHelper;
            }
        }
    }
}
