using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;

namespace Teleopti.Ccc.DomainTest.Scheduling.Restrictions
{
	[TestFixture]
	public class AgentDayPreferenceExtractorTest
	{

		public AgentDayPreferenceExtractor _target;

		[SetUp]
		public void Setup()
		{
			_target = new AgentDayPreferenceExtractor();
		}
	}
}
