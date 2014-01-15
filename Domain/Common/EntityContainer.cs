using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
    /// <summary>
    /// Base class for extending the entity
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    public class EntityContainer<TEntity> : Container<TEntity>, IEntity where TEntity : IEntity
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityContainer{TEntity}"/> class.
        /// </summary>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 12/13/2007
        /// </remarks>
        public EntityContainer()
        { 
            //
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityContainer{TEntity}"/> class.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 12/13/2007
        /// </remarks>
        public EntityContainer(TEntity entity) : base(entity)
        {
            //
        }

        /// <summary>
        /// Gets the entity.
        /// </summary>
        /// <value>The entity.</value>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 12/10/2007
        /// </remarks>
        public TEntity ContainedEntity
        {
            get { return base.Content; }
            set { base.Content = value; }
        }

        #region IEntity Members

        /// <summary>
        /// Gets the unique id for this entity.
        /// </summary>
        /// <value>The id.</value>
        public Guid? Id
        {
            get { return base.Content.Id; }
        }

        /// <summary>
        /// Sets the id.
        /// </summary>
        /// <param name="newId">The new ID.</param>
        public void SetId(Guid? newId)
        {
            ((IEntity)base.Content).SetId(newId);
        }

    	public void ClearId()
    	{
    		throw new NotImplementedException();
    	}

    	#endregion

        #region IEquatable<IEntity> Members

        ///<summary>
        ///Indicates whether the current object is equal to another object of the same type.
        ///</summary>
        ///
        ///<returns>
        ///true if the current object is equal to the other parameter; otherwise, false.
        ///</returns>
        ///
        ///<param name="other">An object to compare with this object.</param>
        public bool Equals(IEntity other)
        {
            if (other == null)
                return false;
            if (this == other)
                return true;
            if (!other.Id.HasValue || !Id.HasValue)
                return false;

            return (Id == other.Id);
        }

        #endregion
    }
}