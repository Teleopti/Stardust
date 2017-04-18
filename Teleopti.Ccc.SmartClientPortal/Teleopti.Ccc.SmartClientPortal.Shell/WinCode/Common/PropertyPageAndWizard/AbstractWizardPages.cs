using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.PropertyPageAndWizard
{
    /// <summary>
    /// Abstract base class for wizard managers
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2008-01-15
    /// </remarks>
    public abstract class AbstractWizardPages<T> : AbstractPropertyPages<T> where T : IAggregateRoot
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractWizardPages&lt;T&gt;"/> class.
        /// </summary>
        /// <remarks>
        /// Created by: henryg
        /// Created date: 2008-02-23
        /// </remarks>
        protected AbstractWizardPages(IRepositoryFactory repositoryFactory, IUnitOfWorkFactory unitOfWorkFactory) : base(repositoryFactory,unitOfWorkFactory)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractWizardPages&lt;T&gt;"/> class.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-15
        /// </remarks>
        protected AbstractWizardPages(T anAggregateRoot, IRepositoryFactory repositoryFactory, IUnitOfWorkFactory unitOfWorkFactory) : base(anAggregateRoot,repositoryFactory,unitOfWorkFactory)
        {
            AggregateRootObject = anAggregateRoot;
        }

        /// <summary>
        /// Adds to repository.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-15
        /// </remarks>
        public override void AddToRepository()
        {
            RepositoryObject.Add(AggregateRootObject);
        }

        /// <summary>
        /// Afters the save.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-15
        /// </remarks>
        protected virtual void OnAfterSave()
        {
            if (Saved != null)
            {
                Saved.Invoke(this, 
                    new AfterSavedEventArgs 
                    { 
                        SavedAggregateRoot = AggregateRootObject 
                    });
            }
        }

        /// <summary>
        /// Occurs when [after save].
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-16
        /// </remarks>
        public event EventHandler<AfterSavedEventArgs> Saved;

        /// <summary>
        /// Saves this instance.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-15
        /// </remarks>
        public override IEnumerable<IRootChangeInfo> Save()
        {
            IEnumerable<IRootChangeInfo> returnList = base.Save();
            if (returnList!=null) OnAfterSave();
            return returnList;
        }

        /// <summary>
        /// Gets a value indicating whether [mode is "create new" false if in "edit existing item"].
        /// </summary>
        /// <value><c>true</c> if [mode create new]; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-02-19
        /// </remarks>
        public override bool ModeCreateNew
        {
            get { return true; }
        }
    }
}
