using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common.EntityBaseTypes
{
    /// <summary>
    /// Base class for bidirectional entities.
    /// </summary>
    public abstract class AggregateEntity : Entity, IAggregateEntity
    {
        private IEntity _parent;

        /// <summary>
        /// Gets the entity parent of this entity.
        /// </summary>
        /// <value>The parent.</value>
        public virtual IEntity Parent
        {
            get { return _parent; }
            private set { _parent = value; }
        }

        /// <summary>
        /// Gets the root of this entity's aggregate.
        /// </summary>
        /// <returns></returns>
        protected virtual IAggregateRoot Root()
        {
            IEntity parent = Parent;
            IAggregateRoot root = parent as IAggregateRoot;

            while (root == null)
            {
                IAggregateEntity internalParent = parent as IAggregateEntity;
                if (internalParent == null)
                {
                    throw new AggregateException("[" + ToString() + "]:s parent is null or not of type IAggregateEntity");
                }

                parent = internalParent.Parent;
                root = parent as IAggregateRoot;
            }
            return root;
        }

        /// <summary>
        /// Gets the root of this entity's aggregate.
        /// </summary>
        IAggregateRoot IAggregateEntity.Root()
        {
            return Root();
        }

        /// <summary>
        /// Sets the parent. Do not call this explicitly!
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-03-27
        /// </remarks>
        void IAggregateEntity.SetParent(IEntity parent)
        {
            SetParent(parent);
        }

        /// <summary>
        /// Sets the parent. Do not call this explicitly!
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-03-27
        /// </remarks>
        internal protected virtual void SetParent(IEntity parent)
        {
            Parent = parent;
        }
    }
}