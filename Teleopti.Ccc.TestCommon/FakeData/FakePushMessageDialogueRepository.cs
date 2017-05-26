using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakePushMessageDialogueRepository : IPushMessageDialogueRepository
	{
		private readonly List<IPushMessageDialogue> storage = new List<IPushMessageDialogue>();

		public void Add(IPushMessageDialogue root)
		{
			storage.Add(root);
		}

		public void Remove(IPushMessageDialogue root)
		{
			storage.Remove(root);
		}

		public IPushMessageDialogue Get(Guid id)
		{
			return storage.FirstOrDefault(s => s.Id == id);
		}

		public IPushMessageDialogue Load(Guid id)
		{
			throw new NotImplementedException();
		}

		public IList<IPushMessageDialogue> LoadAll()
		{
			throw new NotImplementedException();
		}

		public IList<IPushMessageDialogue> Find(IPushMessage pushMessage)
		{
			throw new NotImplementedException();
		}

		public IList<IPushMessageDialogue> FindAllPersonMessagesNotRepliedTo(IPerson person)
		{
			throw new NotImplementedException();
		}

		public void Remove(IPushMessage pushMessage)
		{
			throw new NotImplementedException();
		}

		public int CountUnread(IPerson receiver)
		{
			return storage.Count(s => !s.IsReplied);
		}

		public ICollection<IPushMessageDialogue> FindUnreadMessages(Paging paging, IPerson receiver)
		{
			return storage.Where(s => !s.IsReplied).ToArray();
		}
	}
}