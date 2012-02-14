using System;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
    /// <summary>
    /// Represents a optional column value
    /// </summary>
    /// <remarks>
    /// Created by: Viraj Siriwardana
    /// Created date: 2008-07-24
    /// </remarks>
    public class OptionalColumnValue : AggregateEntity, IOptionalColumnValue
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OptionalColumnValue"/> class.
        /// </summary>
        /// <remarks>
        /// Created by: Viraj Siriwardana
        /// Created date: 2008-07-24
        /// </remarks>
        protected OptionalColumnValue()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OptionalColumnValue"/> class.
        /// </summary>
        /// <param name="description">The name to set.</param>
        /// <remarks>
        /// Created by: Viraj Siriwardana
        /// Created date: 2008-07-24
        /// </remarks>
        public OptionalColumnValue(string description)
            : this()
        {
            _description = description;
        }

        private Guid? _referenceId;
        private string _description;

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>The description.</value>
        /// <remarks>
        /// Created by: Viraj Siriwardana
        /// Created date: 2008-07-24
        /// </remarks>
        public virtual string Description
        {
            get { return _description; }
            set { _description = value; }
        }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>The description.</value>
        /// <remarks>
        /// Created by: Viraj Siriwardana
        /// Created date: 2008-07-24
        /// </remarks>
        public virtual Guid? ReferenceId
        {
            get { return _referenceId; }
            set { _referenceId = value; }
        }
    }
}
