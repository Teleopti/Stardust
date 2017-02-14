using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.Requests.PerformanceTuningTest
{
	public class CalculateOvertimeSuggestionProviderTest : ISetup
	{
		public CalculateOvertimeSuggestionProvider CalculateOvertimeSuggestionProvider;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<CalculateOvertimeSuggestionProvider>().For<CalculateOvertimeSuggestionProvider>();
		}

		[Test]
		public void ShouldProvideCalculatedSuggestions()
		{
			var skillIds = new List<Guid>() {Guid.NewGuid()};
			var startDateTime = new DateTime(2017,02,14);
			var endDateTime = new DateTime(2017,02,14);
			CalculateOvertimeSuggestionProvider.GetOvertimeSuggestions(skillIds, startDateTime, endDateTime);
		}
	}

	public class CalculateOvertimeSuggestionProvider
	{
		public IList<SkillIntervalsForOvertime> GetOvertimeSuggestions(IList<Guid> skillIds, DateTime startDateTime, DateTime endDateTime )
		{
			return new List<SkillIntervalsForOvertime>();
		}
	}

	public class SkillIntervalsForOvertime 
	{
		public Guid SkillId { get; set; }
		public DateTime Time { get; set; }
		public double NewStaffing { get; set; }
	}
}
