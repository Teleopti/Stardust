﻿using WatiN.Core;

namespace Teleopti.Ccc.WebBehaviorTest.Pages.Common
{
	public interface IEditTextRequestPage
	{
		Button AddTextRequestButton { get; }
		Div RequestDetailSection { get; }
		TextField TextRequestDetailSubjectInput { get; }
		TextField TextRequestDetailFromDateInput { get; }
		TextField TextRequestDetailFromTimeTextField { get; }
		TextField TextRequestDetailToDateTextField { get; }
		TextField TextRequestDetailToTimeTextField { get; }
		TextField TextRequestDetailMessageTextField { get; }
		TextField TextRequestDetailEntityId { get; }
		Div ValidationErrorText { get; }

		Button OkButton { get; set; }
		Element CancelButton { get; }
	}
}
