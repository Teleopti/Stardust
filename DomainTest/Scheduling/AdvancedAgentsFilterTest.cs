using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.DomainTest.Scheduling
{
	[TestFixture]
	public class AdvancedAgentsFilterTest
	{
		private AdvancedAgentsFilter _target;

		[SetUp]
		public void Setup()
		{
			_target = new AdvancedAgentsFilter();	
		}

		[TestCase(true, ExpectedResult = 1)]
		[TestCase(false, ExpectedResult = 2)]
		public int ShouldFilterOnEmploymentType(bool filterUniques)
		{
			var agent1 = new Person();
			var agent2 = new Person();
			agent1.SetEmploymentNumber("1");
			agent2.SetEmploymentNumber("10");

			var result = _target.Filter(CultureInfoFactory.CreateSwedishCulture(), "1", new List<IPerson> {agent1, agent2}, new List<LogonInfoModel>(), filterUniques);

			return result.Count;
		}

		[TestCase(true, ExpectedResult = 1)]
		[TestCase(false, ExpectedResult = 2)]
		public int ShouldFilterOnFirstNameLastName(bool filterUniques)
		{
			var agent1 = new Person();
			var agent2 = new Person();
			agent1.SetName(new Name("agent", "agent10"));
			agent2.SetName(new Name("agent", "agent100"));

			var result = _target.Filter(CultureInfoFactory.CreateSwedishCulture(), "agent agent10", new List<IPerson> { agent1, agent2 }, new List<LogonInfoModel>(), filterUniques);

			return result.Count;
		}

		[TestCase(true, ExpectedResult = 1)]
		[TestCase(false, ExpectedResult = 2)]
		public int ShouldFilterOnLastNameFirstName(bool filterUniques)
		{
			var agent1 = new Person();
			var agent2 = new Person();
			agent1.SetName(new Name("agent10", "agent"));
			agent2.SetName(new Name("agent100", "agent"));

			var result = _target.Filter(CultureInfoFactory.CreateSwedishCulture(), "agent, agent10", new List<IPerson> { agent1, agent2 }, new List<LogonInfoModel>(), filterUniques);

			return result.Count;
		}

		[TestCase(true, ExpectedResult = 1)]
		[TestCase(false, ExpectedResult = 2)]
		public int ShouldFilterOnEmail(bool filterUniques)
		{
			var agent1 = new Person{Email = "email1"};
			var agent2 = new Person{Email = "email10"};

			var result = _target.Filter(CultureInfoFactory.CreateSwedishCulture(), "email1", new List<IPerson> { agent1, agent2 }, new List<LogonInfoModel>(), filterUniques);

			return result.Count;
		}

		[TestCase(true, ExpectedResult = 1)]
		[TestCase(false, ExpectedResult = 2)]
		public int ShouldFilterOnLogon(bool filterUniques)
		{
			var guid1 = Guid.NewGuid();
			var guid2 = Guid.NewGuid();
			var agent1 = new Person().WithId(guid1);
			var agent2 = new Person().WithId(guid2);
			var logonInfoModel1 = new LogonInfoModel{Identity = "identity1", PersonId = guid1};
			var logonInfoModel2 = new LogonInfoModel {Identity = "identity10", PersonId = guid2};

			var result = _target.Filter(CultureInfoFactory.CreateSwedishCulture(), "identity1", new List<IPerson> { agent1, agent2 }, new List<LogonInfoModel>{logonInfoModel1, logonInfoModel2}, filterUniques);

			return result.Count;
		}
	}
}
