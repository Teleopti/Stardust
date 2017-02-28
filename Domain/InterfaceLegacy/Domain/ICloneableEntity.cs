using System;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Interface for Entities with generic cloning with or without IEntity.Id set.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ICloneableEntity<out T> : ICloneable
    {

        /// <summary>
        /// Returns a clone of this T with IEntitiy.Id set to null.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-05-27
        /// </remarks>
        T NoneEntityClone();

        /// <summary>
        /// Returns a clone of this T with IEntitiy.Id as this T.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-05-27
        /// </remarks>
        T EntityClone();
    }
}