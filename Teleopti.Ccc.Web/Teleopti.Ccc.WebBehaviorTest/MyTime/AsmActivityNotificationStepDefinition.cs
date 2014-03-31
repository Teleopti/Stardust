using System;
using System.Globalization;
using System.Text;
using System.Xml;
using TechTalk.SpecFlow;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Browser = Teleopti.Ccc.WebBehaviorTest.Core.Browser;

namespace Teleopti.Ccc.WebBehaviorTest.MyTime
{
	[Binding]
	public class AsmActivityNotificationStepDefinition
	{
		[Given(@"Alert Time setting is '(.*)' seconds")]
		public void GivenAlertTimeSettingIs(int alertTime)
		{
			DataMaker.Data().Apply(new AlertTimeSettingConfigurable(alertTime));
		}


	}
}