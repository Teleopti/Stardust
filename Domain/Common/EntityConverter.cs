using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
    /// <summary>
    /// Base class for extending the domain entity
    /// </summary>
    public static class EntityConverter
    {

        /// <summary>
        /// Converts to Entity class.
        /// </summary>
        /// <param name="other">The other type.</param>
        /// <returns></returns>
        public static TEntity ConvertToEntity<TEntity, TOther>(TOther other)
            where TEntity : IEntity
            where TOther : EntityContainer<TEntity>
        {
            return other.ContainedEntity;
        }

        /// <summary>
        /// Converts the input Entity to Other type of class.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        public static TOther ConvertToOther<TEntity, TOther>(TEntity entity)
            where TEntity : IEntity
            where TOther : EntityContainer<TEntity>, new()
        {
            TOther ret = new TOther();
            ret.ContainedEntity = entity;
            return ret;
        }
    }
}