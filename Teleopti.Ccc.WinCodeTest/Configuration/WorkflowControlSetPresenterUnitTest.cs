using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Configuration;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.WinCodeTest.Configuration
{
	[TestFixture]
	public class WorkflowControlSetPresenterUnitTest
	{
		private WorkflowControlSetPresenter _target;
		private MockRepository _mocks;
		private IWorkflowControlSetModel _model;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_target = new WorkflowControlSetPresenter(null, null, null);
			_model = _mocks.StrictMock<IWorkflowControlSetModel>();
			_target.SelectedModel = _model;
		}

		[Test]
		public void ShouldAddSkillToMatchList()
		{
			var skill = SkillFactory.CreateSkill("5 finger death punch");
			_mocks.Record();
			_model.AddSkillToMatchList(skill);
			_mocks.ReplayAll();
			_target.AddSkillToMatchList(skill);
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldRemoveSkillFromMatchList()
		{
			var skill = SkillFactory.CreateSkill("5 finger death punch");
			_mocks.Record();
			_model.RemoveSkillFromMatchList(skill);
			_mocks.ReplayAll();
			_target.RemoveSkillFromMatchList(skill);
			_mocks.VerifyAll();
		}
	}
}