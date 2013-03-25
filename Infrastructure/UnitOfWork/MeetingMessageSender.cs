﻿using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class MeetingMessageSender : IMessageSender
	{
		private readonly ISendDenormalizeNotification _sendDenormalizeNotification;
		private readonly ISaveToDenormalizationQueue _saveToDenormalizationQueue;

		public MeetingMessageSender(ISendDenormalizeNotification sendDenormalizeNotification, ISaveToDenormalizationQueue saveToDenormalizationQueue)
		{
			_sendDenormalizeNotification = sendDenormalizeNotification;
			_saveToDenormalizationQueue = saveToDenormalizationQueue;
		}

		public void Execute(IEnumerable<IRootChangeInfo> modifiedRoots)
		{
			var meetings =
				modifiedRoots.Select(r => new { ProvideCustomChangeInfo = r.Root as IProvideCustomChangeInfo, UpdateType = r.Status });
			meetings = meetings.Where(m => m.ProvideCustomChangeInfo != null);
			if (!meetings.Any()) return;

			var changedMeetings = meetings.Select(c =>
			{
				var tracker = new MeetingChangeTracker();
				tracker.TakeSnapshot((IMeeting)c.ProvideCustomChangeInfo.BeforeChanges());
				return new
				{

					Meeting = c,
					CustomChanges =
						tracker.CustomChanges((IMeeting)c.ProvideCustomChangeInfo,
											  c.UpdateType)
				};
			});

			foreach (var changedMeeting in changedMeetings)
			{
				foreach (var changedRoot in changedMeeting.CustomChanges)
				{
					var customChange = changedRoot.Root as ICustomChangedEntity;
					if (customChange == null) return;

					var message = new ScheduleChanged
					              	{
					              		ScenarioId =
					              			((IMeeting) changedMeeting.Meeting.ProvideCustomChangeInfo).
					              			Scenario.Id.GetValueOrDefault(),
					              		StartDateTime = customChange.Period.StartDateTime,
					              		EndDateTime = customChange.Period.EndDateTime,
					              		PersonId = customChange.MainRoot.Id.GetValueOrDefault()
					              	};
					_saveToDenormalizationQueue.Execute(message);
				}
			}

			_sendDenormalizeNotification.Notify();
		}
	}
}
