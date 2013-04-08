using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Teleopti.Ccc.WinCode.Scheduling;

namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
	[TestFixture]
	public class AgentPreferenceRemoveCommandTest
	{
		private AgentPreferenceRemoveCommand _removeCommand;

		[SetUp]
		public void Setup()
		{
			_removeCommand = new AgentPreferenceRemoveCommand();
		}
	}
}
