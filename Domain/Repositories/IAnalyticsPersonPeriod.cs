using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure.Analytics;

namespace Teleopti.Ccc.Domain.Repositories
{
    public interface IAnalyticsPersonPeriodRepository
    {

        IList<IAnalyticsPersonPeriod> GetPersonPeriods(Guid personCode);
        void AddPersonPeriod(IAnalyticsPersonPeriod personPeriod);
    }
}
