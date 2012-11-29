using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.Robustness;
using Teleopti.Ccc.WebBehaviorTest.Pages.jQuery;
using Teleopti.Interfaces.Domain;
using WatiN.Core;
using WatiN.Core.Constraints;

namespace Teleopti.Ccc.WebBehaviorTest.Pages
{
	public class AdminWebPage : Page
	{
		[FindBy(Id = "content-placeholder")] public Element Placeholder;
	}
}