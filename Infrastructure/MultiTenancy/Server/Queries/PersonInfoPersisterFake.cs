using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries
{
	public class PersonInfoPersisterFake : IPersonInfoPersister
	{
		private readonly List<PersonInfo> _storage;

		public PersonInfoPersisterFake()
		{
			_storage = new List<PersonInfo>();
		}

		public void Persist(PersonInfo personInfo)
		{
			var existing = _storage.SingleOrDefault(x => x.ApplicationLogonInfo.LogonName == personInfo.ApplicationLogonInfo.LogonName);
			if (existing != null)
			{
				existing = personInfo;
			}
			else
			{
				_storage.Add(personInfo);
			}
		}

		public bool ValidateApplicationLogonNameIsUnique(PersonInfo personInfo)
		{
			return !_storage.Exists(x => x.ApplicationLogonInfo.LogonName == personInfo.ApplicationLogonInfo.LogonName && x.Id != personInfo.Id);
		}

		public bool ValidateIdenitityIsUnique(PersonInfo personInfo)
		{
			return !_storage.Exists(x => x.Identity == personInfo.Identity && x.Id != personInfo.Id);
		}

		public List<PersonInfo> PersistedData => _storage;
	}
}