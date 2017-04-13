using System.Windows.Input;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCode.Common.Commands
{
    /// <summary>
    /// CommandModel that executes with a UnitOfWork
    /// </summary>
    /// <remarks>
    /// Creates and opens a UnitOfWork when executed
    /// Created by: henrika
    /// Created date: 2009-05-20
    /// </remarks>
    public abstract class RepositoryCommandModel :CommandModel
    {
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;

        protected RepositoryCommandModel(IUnitOfWorkFactory unitOfWorkFactory)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
        }

        public sealed override void OnExecute(object sender, ExecutedRoutedEventArgs e)
        {
            using(IUnitOfWork uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
            {
                OnExecute(uow,sender,e);
            }
        }

        /// <summary>
        /// Called when [execute].
        /// </summary>
        /// <param name="uow">The uow.</param>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Creates and opens a UnitOfWork
        /// Created by: henrika
        /// Created date: 2009-05-20
        /// </remarks>
        public abstract void OnExecute(IUnitOfWork uow,object sender,ExecutedRoutedEventArgs e);
    }
}
