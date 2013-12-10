using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.GroupPageCreator
{
	public class Group
	{
		private readonly IList<IPerson> _groupMembers = new List<IPerson>();
		private string _name;

		public IEnumerable<IPerson> GroupMembers
		{
			get { return _groupMembers; }
		}

		public string Name
		{
			get { return _name; }
		}

		public void AddMember(IPerson person)
		{
			_groupMembers.Add(person);
		}

		public void SetName(string name)
		{
			_name = name;
		}

	}
}