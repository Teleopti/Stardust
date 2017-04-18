using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
	[TestFixture]
	public class PublishScheduleCommandTest
	{
		private PublishScheduleCommand _target;
		private MockRepository _mock;
		private IWorkflowControlSet _workflowControlSet;
		private IList<IWorkflowControlSet> _workflowControlSets;
		private IList<IWorkflowControlSet> _modifiedWorkflowControlSets;
		private DateOnly _publishToDate;
		private ICommonStateHolder _commonStateHolder;
		
		[SetUp]
		public void SetUp()
		{
			_mock = new MockRepository();
			_workflowControlSet = _mock.StrictMock<IWorkflowControlSet>();
			_workflowControlSets = new List<IWorkflowControlSet> { _workflowControlSet };
			_modifiedWorkflowControlSets = new List<IWorkflowControlSet>();
			_publishToDate = new DateOnly(2014, 1 ,1 );
			_commonStateHolder = _mock.StrictMock<ICommonStateHolder>();
			_target = new PublishScheduleCommand(_workflowControlSets, _publishToDate, _commonStateHolder);	
		}

		[Test]
		public void ShouldSetPublishToDate()
		{
			using (_mock.Record())
			{
				Expect.Call(_commonStateHolder.WorkflowControlSets).Return(_workflowControlSets);
				Expect.Call(_workflowControlSet.Equals(_workflowControlSet)).Return(true);
				Expect.Call(() => _workflowControlSet.SchedulePublishedToDate = _publishToDate.Date);
				Expect.Call(_commonStateHolder.ModifiedWorkflowControlSets).Return(_modifiedWorkflowControlSets);
			}

			using (_mock.Playback())
			{
				_target.Execute();
				Assert.AreEqual(1, _modifiedWorkflowControlSets.Count);
			}
		}
	}
}
