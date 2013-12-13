using System;

namespace Teleopti.Interfaces.Messages
{
	/// <summary>
    /// Generic message base class that will log on the raptor domain
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2010-03-30
    /// </remarks>
    [Serializable]
	public abstract class RaptorDomainMessage : IRaptorDomainMessageInfo
    {
        ///<summary>
        /// Definies an identity for this message (typically the Id of the root this message refers to.
        ///</summary>
        public abstract Guid Identity { get; }

		public Guid InitiatorId { get; set; }

		/// <summary>
        /// Gets or sets the datasource.
        /// </summary>
        /// <value>The datasource.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2010-02-24
        /// </remarks>
        public string Datasource { get; set; }

        /// <summary>
        /// Gets or sets the timestamp.
        /// </summary>
        /// <value>The timestamp.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2010-02-24
        /// </remarks>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the business unit id.
        /// </summary>
        /// <value>The business unit id.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2010-02-24
        /// </remarks>
        public Guid BusinessUnitId { get; set; }
    }
}