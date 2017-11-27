//using System;
//using System.Collections.Generic;
//using System.Linq;
//using Teleopti.Ccc.Domain.Notification;
//using Teleopti.Ccc.Domain.Repositories;

//namespace Teleopti.Ccc.TestCommon.FakeRepositories
//{
//	public class FakePersonScheduleChangeMessageRepository : IPersonScheduleChangeMessageRepository
//	{

//		private IList<PersonScheduleChangeMessage> messages = new List<PersonScheduleChangeMessage>();

//		public void Add(PersonScheduleChangeMessage scheduleChangeMessage)
//		{
//			messages.Add(scheduleChangeMessage);
//		}

//		public IEnumerable<PersonScheduleChangeMessage> PopMessages(Guid personId)
//		{
//			var results = messages.Where(m => m.PersonId == personId).ToList();
//			messages = messages.Where(m => m.PersonId != personId).ToList();
//			return results;
//		}
//	}
//}
