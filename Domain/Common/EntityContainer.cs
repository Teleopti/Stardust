using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
    /// <summary>
    /// Base class for extending the entity
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    public class EntityContainer<TEntity> : IEntity where TEntity : IEntity
    {
	    public EntityContainer()
        { 
        }

        public EntityContainer(TEntity entity)
        {
			ContainedEntity = entity;
        }

	    public TEntity ContainedEntity { get; set; }

        public Guid? Id
        {
            get { return ContainedEntity.Id; }
        }

        public void SetId(Guid? newId)
        {
            ((IEntity)ContainedEntity).SetId(newId);
        }

    	public void ClearId()
    	{
    		throw new NotImplementedException();
    	}

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
    }
}