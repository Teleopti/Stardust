using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teleopti.Interfaces.Infrastructure.Analytics
{
    public interface IAnalyticsDate
    {
		int DateId { get; set; }

		DateTime DateDate { get; set; }
    }
}
