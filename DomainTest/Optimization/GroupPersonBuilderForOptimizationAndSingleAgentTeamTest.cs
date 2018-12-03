using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;


namespace Teleopti.Ccc.DomainTest.Optimization
{
	[TestFixture]
	public class GroupPersonBuilderForOptimizationAndSingleAgentTeamTest
	{
		private IGroupPersonBuilderForOptimization _target;

		[SetUp]
		public void Setup()
		{
			_target = new GroupPersonBuilderForOptimizationAndSingleAgentTeam();
		}

		[Test]
		public void ShouldReturnASingleAgentGroupOnAnyDate()
		{
			var person = new Person();
			person.SetId(Guid.Empty);
			var result = _target.BuildGroup(null, person, DateOnly.MinValue);

			result.GroupMembers.Count().Should().Be.EqualTo(1);
			result.GroupMembers.First().Should().Be.EqualTo(person);
		}
	}
}