using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.Win.Commands
{
	public class TeamBlockMoveTimeBetweenDaysCommand
	{
		private readonly IToggleManager _toggleManager;

		public TeamBlockMoveTimeBetweenDaysCommand(IToggleManager toggleManager)
		{
			_toggleManager = toggleManager;
		}

		public void Execute()
		{
			if (!_toggleManager.IsEnabled(Toggles.TeamBlockMoveTimeBetweenDays))
				return;
		}
	}
}
