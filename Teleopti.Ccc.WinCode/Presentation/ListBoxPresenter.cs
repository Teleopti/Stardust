using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Presentation
{
    /// <summary>
    /// Base class for extending the domain entity
    /// </summary>
    /// <typeparam name="TEntity">The type of the domain.</typeparam>
    public abstract class ListBoxPresenter<TEntity> : EntityContainer<TEntity>, IListBoxPresenter where TEntity : IEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ListBoxPresenter{TEntity}"/> class.
        /// </summary>
        /// <param name="entity">The domain entity.</param>
        protected ListBoxPresenter(TEntity entity) : base(entity)
        {
        }

        #region IListBoxPresenter Members

        /// <summary>
        /// Gets the Name value. Usually this is the short name.
        /// </summary>
        /// <value>The key field.</value>
        /// <remarks>
        /// Usually this value goes to the name field of a control.
        /// </remarks>
        public abstract string DataBindText
        {
            get;
        }

        /// <summary>
        /// Gets the description or additional info value. Usually that is
        /// a longer description about the entity.
        /// </summary>
        /// <value>The description field.</value>
        /// <remarks>
        /// Usually this value goes to the tooltip to the control.
        /// </remarks>
        public abstract string DataBindDescriptionText
        {
            get;
        }

        /// <summary>
        /// Gets the inner domain object.
        /// </summary>
        /// <value>The domain object.</value>
        public object EntityObject
        {
            get { return ContainedEntity; }
        }

        #endregion

    }
}