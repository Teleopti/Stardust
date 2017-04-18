using System.Collections.Generic;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Payroll.Interfaces;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Payroll
{
    public abstract class CommonViewHolder<T> : ICommon<T>
    {
        private readonly IExplorerPresenter _explorerPresenter = null;
        private List<T> _viewCollection = null;
        private readonly IPayrollHelper _helper;

        /// <summary>
        /// Gets the view.
        /// </summary>
        /// <value>The view.</value>
        public virtual List<T> ModelCollection
        {
            get
            {
                return _viewCollection;
            }
            set
            {
                _viewCollection = value;
            }
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

        /// <summary>
        /// Gets the global presenter.
        /// </summary>
        /// <value>The global presenter.</value>
        public IExplorerPresenter ExplorerPresenter
        {
            get { return _explorerPresenter; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommonViewHolder&lt;T&gt;"/> class.
        /// </summary>
        protected CommonViewHolder(IExplorerPresenter explorerPresenter)
        {
            _explorerPresenter = explorerPresenter;
            _helper = _explorerPresenter.Helper;
            _viewCollection = new List<T>();
        }
    }
}
