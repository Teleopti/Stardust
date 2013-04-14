﻿using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider
{
	public interface IPreferenceTemplateProvider
	{
		IEnumerable<IExtendedPreferenceTemplate> RetrievePreferenceTemplates();
	}
}