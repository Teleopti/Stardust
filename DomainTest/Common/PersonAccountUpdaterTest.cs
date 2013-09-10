using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Common
{
	[TestFixture]
	public class PersonAccountUpdaterTest
	{
		private PersonAccountUpdater _target;
		private MockRepository _mocks;
		private IEnumerable<IAccount> _personAbsenceAccounts;
				
		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_personAbsenceAccounts = new List<IAccount>();
			//_target = new PersonAccountUpdater(_personAbsenceAccounts);
		}

		//[Test]
		//public void Should



	}
}
