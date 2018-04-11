﻿using System.Collections.Generic;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;
using Teleopti.Analytics.Etl.Common.JobHistory;

namespace Teleopti.Analytics.Etl.ConfigTool.Gui.ViewModel
{
	public class JobHistoryTreeViewModel : List<JobHistoryViewModel>
	{
		private IJobHistory _selectedItem;

		public JobHistoryTreeViewModel()
		{
			CopyExceptionCommand = new CopyErrorCommand(this);
		}

		public CopyErrorCommand CopyExceptionCommand { get; private set; }

		public IJobHistory SelectedItem
		{
			get { return _selectedItem; }
			set
			{
				_selectedItem = value;
				CopyExceptionCommand.SetCanExecute();
			}
		}
	}
}
