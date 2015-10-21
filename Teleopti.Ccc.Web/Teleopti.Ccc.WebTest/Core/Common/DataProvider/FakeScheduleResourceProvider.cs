using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teleopti.Ccc.Domain.Outbound;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Common.DataProvider
{
    class FakeScheduleResourceProvider : IOutboundScheduledResourcesProvider
    {
        private readonly Dictionary<Tuple<DateOnly, string>, TimeSpan> scheduledTime = new Dictionary<Tuple<DateOnly, string>, TimeSpan>();
		private readonly Dictionary<Tuple<DateOnly, string>, TimeSpan> forecastedTime = new Dictionary<Tuple<DateOnly, string>, TimeSpan>(); 



        public void Load(IList<IOutboundCampaign> campaigns, DateOnlyPeriod period)
        {
            
        }

        public TimeSpan GetScheduledTimeOnDate(DateOnly date, ISkill skill)
        {
            var key = new Tuple<DateOnly, string>(date, skill.Name);
            return scheduledTime.ContainsKey(key) ? scheduledTime[key] : TimeSpan.Zero;
        }

        public TimeSpan GetForecastedTimeOnDate(DateOnly date, ISkill skill)
        {
			var key = new Tuple<DateOnly, string>(date, skill.Name);
			return forecastedTime.ContainsKey(key) ? forecastedTime[key] : TimeSpan.Zero;
        }

	    public void SetForecastedTimeOnDate(DateOnly date, ISkill skill, TimeSpan time)
	    {
			forecastedTime.Add(new Tuple<DateOnly, string>(date, skill.Name), time);
	    }

	    public void SetScheduledTimeOnDate(DateOnly date, ISkill skill, TimeSpan time)
        {
            scheduledTime.Add(new Tuple<DateOnly, string>(date, skill.Name), time);
        }
    }
}
