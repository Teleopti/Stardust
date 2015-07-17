using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.GroupPageCreator
{
	public class Group
	{
		private readonly IList<IPerson> _groupMembers = new List<IPerson>();
		private string _name;

		/// <summary>
		/// The ID for main page, hardcoded in both code and database.
		/// </summary>
		public static readonly Guid PageMainId = new Guid("6CE00B41-0722-4B36-91DD-0A3B63C545CF");

		public Group()
		{}

		public Group(IList<IPerson> groupMembers, string name)
		{
			_groupMembers = groupMembers;
			_name = name;
		}

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