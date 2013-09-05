﻿using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public interface IMessageSender
	{
		void Execute(IMessageBrokerIdentifier messageBrokerIdentifier, IEnumerable<IRootChangeInfo> modifiedRoots);
	}
}