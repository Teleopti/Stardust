﻿using System;
using NUnit.Framework;
using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class TimeStepDefinitions
	{
		[Given(@"the time is '(.*)'")]
		[Given(@"the utc time is '(.*)'")]
		[When(@"the time is '(.*)'")]
		[When(@"the utc time is '(.*)'")]
		[SetCulture("sv-SE")]
		public void GivenCurrentTimeIs(string time)
		{
			CurrentTime.Set(time);
		}
	}
}