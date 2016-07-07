using Teleopti.Ccc.WinCode.Payroll.Interfaces;

namespace Teleopti.Ccc.WinCode.Payroll
{
    public class ExplorerPresenter : IExplorerPresenter
    {
        #region Fields - Instance Members

        #region Fields - Instance Members - Private Fields

        private IExplorerViewModel _explorerViewModel;

        private IExplorerView _explorerView;

        private IPayrollHelper _helper;

        private IDefinitionSetPresenter _definitionSetPresenter;

        private IVisualizePresenter _visualizePresenter;
        private IMultiplicatorDefinitionPresenter _multiplicatorDefinitionPresenter;

        #endregion

        #endregion

        #region Methods - Instance Members

        #region Methods - Instance Members - Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ExplorerPresenter"/> class.
        /// </summary>
        /// <param name="helper">The helper.</param>
        /// <param name="view">The view.</param>
        /// <remarks>
        /// Created by: VirajS
        /// Created date: 2009-01-20
        /// </remarks>
        public ExplorerPresenter(IPayrollHelper helper, IExplorerView view)
        {
            CreateChildPresenters(helper, view);
            LoadData();
        }

        #endregion

        #region Methods - Instance Members - Public Methods

        /// <summary>
        /// Creates the child presenters.
        /// </summary>
        /// <param name="helper">The helper.</param>
        /// <param name="explorerView">The explorer view.</param>
        /// <remarks>
        /// Created by: VirajS
        /// Created date: 2009-01-20
        /// </remarks>
        private void CreateChildPresenters(IPayrollHelper helper, IExplorerView explorerView)
        {
            _explorerView = explorerView;
            _explorerViewModel = new ExplorerViewModel();

            _helper = helper;
            if (helper == null)
                _helper = new PayrollHelper(_explorerView.UnitOfWork);

            _definitionSetPresenter = new DefinitionSetPresenter(this);
            _multiplicatorDefinitionPresenter = new MultiplicatorDefinitionPresenter(this);
            _visualizePresenter = new VisualizePresenter(this);

        }

        /// <summary>
        /// Loads the data.
        /// </summary>
        private void LoadData()
        {

            _explorerViewModel.MultiplicatorCollection = Helper.LoadMultiplicatorList();
            _explorerViewModel.DefinitionSetCollection = Helper.LoadDefinitionSets();
        }

        /// <summary>
        /// Gets the view.
        /// </summary>
        /// <value>The view.</value>
        public IExplorerViewModel Model
        {
            get
            {
                return _explorerViewModel;
            }
        }

        /// <summary>
        /// Gets the definition set presenter.
        /// </summary>
        /// <value>The definition set presenter.</value>
        public IDefinitionSetPresenter DefinitionSetPresenter
        {
            get { return _definitionSetPresenter; }
        }

        public IMultiplicatorDefinitionPresenter MultiplicatorDefinitionPresenter
        {
            get { return _multiplicatorDefinitionPresenter; }
        }

        /// <summary>
        /// Gets the visualize presenter.
        /// </summary>
        /// <value>The visualize presenter.</value>
        public IVisualizePresenter VisualizePresenter
        {
            get { return _visualizePresenter; }
        }

        /// <summary>
        /// Gets the helper.
        /// </summary>
        /// <value>The helper.</value>
        public IPayrollHelper Helper
        {
            get
            {
                return _helper;
            }
        }

        #endregion

        #endregion
    }
}
