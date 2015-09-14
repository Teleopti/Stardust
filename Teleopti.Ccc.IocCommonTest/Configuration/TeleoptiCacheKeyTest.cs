using System;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.IocCommonTest.Configuration
{
	//just test edge cases here - normal flow is tested by other test cases
	[TestFixture]
	public class TeleoptiCacheKeyTest
	{
		[Test]
		public void ShouldUseBaseClassIfNoEntity()
		{
			var target = new exposer();
			target.TheKey(3).Should().Be.EqualTo("3");
		}

		[Test]
		public void ShouldUseGetHashCodeIfEffectiveRestrictionIsUsed()
		{
			//THIS ONE IS WRONG! Just uses old way for now. GetHashCode doesn't return unique value!

			var target = new exposer();
			var restriction = new EffectiveRestriction(new StartTimeLimitation(TimeSpan.FromHours(13), null),
			                                           new EndTimeLimitation(null, TimeSpan.FromHours(1)),
			                                           new WorkTimeLimitation(null, null), null, null, null,
			                                           new List<IActivityRestriction>());
			target.TheKey(restriction).Should().Be.EqualTo(restriction.GetHashCode().ToString(CultureInfo.InvariantCulture));
		}

		private class exposer : TeleoptiCacheKey
		{
			public string TheKey(object parameter)
			{
				return ParameterValue(parameter);
			}
		}
	}
}