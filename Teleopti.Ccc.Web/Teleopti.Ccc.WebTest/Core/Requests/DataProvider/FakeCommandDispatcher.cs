using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;

namespace Teleopti.Ccc.WebTest.Core.Requests.DataProvider
{
	public class FakeCommandDispatcher : ICommandDispatcher
	{
		public void Execute(object command)
		{
			LatestCommand = command;
			var cmd = command as IRequestCommand;
			if (cmd!=null) cmd.ErrorMessages = new List<string>();
		}

		public object LatestCommand;
	}
}