﻿using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Wfm.Adherence.Test.InfrastructureTesting
{
	public class DatabaseTestAttribute : InfrastructureTestAttribute
	{
		protected override void BeforeTest()
		{
			InfrastructureTestStuff.BeforeWithLogon();
			base.BeforeTest();
		}

		protected override void AfterTest()
		{
			base.AfterTest();
			InfrastructureTestStuff.AfterWithLogon();
		}
	}
}