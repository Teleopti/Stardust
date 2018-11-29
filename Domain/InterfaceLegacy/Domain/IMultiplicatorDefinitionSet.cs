using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    ///<summary>
    /// Interface for MultiplicatorDefinitions
    ///</summary>
    public interface IMultiplicatorDefinitionSet : IAggregateRoot, ICloneableEntity<IMultiplicatorDefinitionSet>,IChangeInfo, IBelongsToBusinessUnit
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-12-09
        /// </remarks>
        string Name { get; set; }

        /// <summary>
        /// Gets the definition collection.
        /// </summary>
        /// <value>The definition collection.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-12-09
        /// </remarks>
        IList<IMultiplicatorDefinition> DefinitionCollection { get; }

        /// <summary>
        /// Gets the type of the multiplicator.
        /// </summary>
        /// <value>The type of the multiplicator.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-12-09
        /// </remarks>
        MultiplicatorType MultiplicatorType { get; }

        /// <summary>
        /// Adds the definition.
        /// </summary>
        /// <param name="definition">The definition.</param>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-12-09
        /// </remarks>
        void AddDefinition(IMultiplicatorDefinition definition);

        /// <summary>
        /// Adds the definition at a specified position in the DefinitionCollection.
        /// </summary>
        /// <param name="definition">The definition.</param>
        /// <param name="orderIndex">The OrderIndex of the MultiplicatorDefinition in the DefinitionCollection.</param>
        /// <remarks>
        /// Created by: jonas n
        /// Created date: 2010-01-12
        /// </remarks>
        void AddDefinitionAt(IMultiplicatorDefinition definition, int orderIndex);

        /// <summary>
        /// Removes the definition.
        /// </summary>
        /// <param name="definition">The definition.</param>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-12-09
        /// </remarks>
        void RemoveDefinition(IMultiplicatorDefinition definition);

        /// <summary>
        /// Moves the definition up.
        /// </summary>
        /// <param name="definition">The definition.</param>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-12-09
        /// </remarks>
        void MoveDefinitionUp(IMultiplicatorDefinition definition);

        /// <summary>
        /// Moves the definition down.
        /// </summary>
        /// <param name="definition">The definition.</param>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-12-09
        /// </remarks>
        void MoveDefinitionDown(IMultiplicatorDefinition definition);

        /// <summary>
        /// Creates the projection for period.
        /// </summary>
        /// <param name="period">The period.</param>
        /// <param name="timeZoneInfo">The time zone info.</param>
        /// <returns>The layers expressed in UTC</returns>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-12-10
        /// </remarks>
        IList<IMultiplicatorLayer> CreateProjectionForPeriod(DateOnlyPeriod period, TimeZoneInfo timeZoneInfo);

        /// <summary>
        /// Indicates if this definition set is deleted.
        /// </summary>
        bool IsDeleted { get; }
    }
}
