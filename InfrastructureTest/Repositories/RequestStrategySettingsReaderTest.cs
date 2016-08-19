﻿using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[UnitOfWorkWithLoginTest]
	public class RequestStrategySettingsReaderTest
	{
		public IRequestStrategySettingsReader Reader;

		[Test]
		public void ShouldReadIntValue()
		{
			var val = Reader.GetIntSetting("AbsenceNearFuture", 8);
			val.Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldReturnDefaultIntValue()
		{
			var val = Reader.GetIntSetting("NÅTTSOMINTEFINNS", 8);
			val.Should().Be.EqualTo(8);
		}
	}
}