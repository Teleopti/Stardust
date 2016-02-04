using System;

namespace Teleopti.Interfaces.Messages.Denormalize
{
    /// <summary>
    /// Legacy message type, here only to be able to read old messages from denormalization queue.
    /// </summary>
    [Serializable, Obsolete("Legacy only")]
    public class ScheduleChanged : MessageWithLogOnContext
    {
        private readonly Guid _messageId = Guid.NewGuid();

        /// <summary>
        /// Gets the message identity.
        /// </summary>
        public override Guid Identity
        {
            get { return _messageId; }
        }

        /// <summary>
        /// Gets or sets the start date time.
        /// </summary>
        public DateTime StartDateTime { get; set; }

        /// <summary>
        /// Gets or sets the end date time.
        /// </summary>
        public DateTime EndDateTime { get; set; }

        /// <summary>
        /// Gets or sets the scenario id.
        /// </summary>
        public Guid ScenarioId { get; set; }

        /// <summary>
        /// Gets or sets the person id.
        /// </summary>
        public Guid PersonId { get; set; }

        ///<summary>
        /// Gets or sets the skip delete option to be used in the initial load.
        ///</summary>
        public bool SkipDelete { get; set; }
    }
}