﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class GroupPageChangedMessageSender : IMessageSender
	{
		private readonly IEnumerable<Type> _triggerInterfaces = new List<Type>
		                                                        	{
		                                                        		typeof (IGroupPage)
		                                                        	};

		private readonly ISendDenormalizeNotification _sendDenormalizeNotification;
		private readonly ISaveToDenormalizationQueue _saveToDenormalizationQueue;

		public GroupPageChangedMessageSender(ISendDenormalizeNotification sendDenormalizeNotification, ISaveToDenormalizationQueue saveToDenormalizationQueue)
		{
			_sendDenormalizeNotification = sendDenormalizeNotification;
			_saveToDenormalizationQueue = saveToDenormalizationQueue;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void Execute(IEnumerable<IRootChangeInfo> modifiedRoots)
		{
			var atLeastOneMessage = false;
			var affectedInterfaces = from r in modifiedRoots
			                         from i in r.Root.GetType().GetInterfaces()
			                         select i;
	        if (affectedInterfaces.Any(t => _triggerInterfaces.Contains(t)))
			{
                //get the group page ids
				var groupPage = modifiedRoots.Select(r => r.Root).OfType<IGroupPage>();
				foreach (var groupPageList in groupPage.Batch(25))
				{
					var idsAsString = (from p in groupPageList select p.Id.GetValueOrDefault()).ToArray();
					var message = new GroupPageChangedMessage();
					message.SetGroupPageIdCollection(idsAsString);
					_saveToDenormalizationQueue.Execute(message);
					atLeastOneMessage = true;
				}

				if (atLeastOneMessage)
				{
					_sendDenormalizeNotification.Notify();
				}
			}
		}
	}
}