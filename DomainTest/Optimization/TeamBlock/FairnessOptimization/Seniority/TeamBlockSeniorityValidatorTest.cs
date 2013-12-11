using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Rhino.Mocks;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization.TeamBlock.FairnessOptimization.Seniority
{
	[TestFixture]
	public class TeamBlockSeniorityValidatorTest
	{
		private MockRepository _mock;
		private ITeamBlockInfo _teamBlockInfo;
		private ITeamInfo _teamInfo;
		private TeamBlockSeniorityValidator _target;
		private IGroupPerson _groupPerson;
		private IPerson _person;
		private IList<IPerson> _persons;
		private IWorkflowControlSet _workflowControlSet;

		[SetUp]
		public void SetUp()
		{
			_mock = new MockRepository();
			_teamBlockInfo = _mock.StrictMock<ITeamBlockInfo>();
			_teamInfo = _mock.StrictMock<ITeamInfo>();
			_groupPerson = _mock.StrictMock<IGroupPerson>();
			_person = _mock.StrictMock<IPerson>();
			_persons = new List<IPerson>{_person};
			_workflowControlSet = _mock.StrictMock<IWorkflowControlSet>();
			_target = new TeamBlockSeniorityValidator();
		}

		[Test]
		public void ShouldReturnTrueWhenAllHaveSeniority()
		{
			using (_mock.Record())
			{
				Expect.Call(_teamBlockInfo.TeamInfo).Return(_teamInfo);
				Expect.Call(_teamInfo.GroupPerson).Return(_groupPerson);
				Expect.Call(_groupPerson.GroupMembers).Return(_persons);
				Expect.Call(_person.WorkflowControlSet).Return(_workflowControlSet);
				Expect.Call(_workflowControlSet.UseShiftCategoryFairness).Return(false);
			}

			using (_mock.Playback())
			{
				var result = _target.ValidateSeniority(_teamBlockInfo);
				Assert.IsTrue(result);
			}
		}

		[Test]
		public void ShouldReturnFalseWhenNotAllHaveSeniority()
		{
			using (_mock.Record())
			{
				Expect.Call(_teamBlockInfo.TeamInfo).Return(_teamInfo);
				Expect.Call(_teamInfo.GroupPerson).Return(_groupPerson);
				Expect.Call(_groupPerson.GroupMembers).Return(_persons);
				Expect.Call(_person.WorkflowControlSet).Return(_workflowControlSet);
				Expect.Call(_workflowControlSet.UseShiftCategoryFairness).Return(true);
			}

			using (_mock.Playback())
			{
				var result = _target.ValidateSeniority(_teamBlockInfo);
				Assert.IsFalse(result);
			}
		}
	}
}
