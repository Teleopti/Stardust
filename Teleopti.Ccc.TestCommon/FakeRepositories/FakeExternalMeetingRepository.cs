using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeExternalMeetingRepository : IExternalMeetingRepository
	{
		private readonly List<ExternalMeeting> meetings = new List<ExternalMeeting>();
		
		public void Add(ExternalMeeting root)
		{
			meetings.Add(root);
		}

		public void Remove(ExternalMeeting root)
		{
			meetings.Remove(root);
		}

		public ExternalMeeting Get(Guid id)
		{
			return meetings.SingleOrDefault(x => x.Id == id);
		}

		public ExternalMeeting Load(Guid id)
		{
			return meetings.Single(x => x.Id == id);
		}

		public IEnumerable<ExternalMeeting> LoadAll()
		{
			return meetings;
		}
	}
}