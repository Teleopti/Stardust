﻿using System;

namespace Teleopti.Interfaces.Messages.General
{
    /// <summary>
    /// 
    /// </summary>
	public class BadgeCalculationInitMessage : MessageWithLogOnInfo
    {
        private readonly Guid _messageId = Guid.NewGuid();

        ///<summary>
        /// Definies an identity for this message (typically the Id of the root this message refers to.
        ///</summary>
        public override Guid Identity
        {
            get { return _messageId; }
        }
    }
}
