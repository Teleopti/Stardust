using System;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Configurable
{
	public class ExternalLogonConfigurable : IDataSetup
	{
        public string AcdLogOnOriginalId { get; set; }
		public int AcdLogOnMartId { get; set; }
        public int AcdLogOnAggId { get; set; }
        public string AcdLogOnName { get; set; }
        public int DataSourceId { get; set; }

		public IExternalLogOn ExternalLogOn;

        

		public void Apply(ICurrentUnitOfWork currentUnitOfWork)
		{
            ExternalLogOn = new ExternalLogOn
            {
                AcdLogOnAggId = AcdLogOnAggId,
                AcdLogOnMartId = AcdLogOnMartId,
                AcdLogOnName = AcdLogOnName,
                AcdLogOnOriginalId = AcdLogOnOriginalId,
                Active = true,
                DataSourceId = DataSourceId
            };

			new ExternalLogOnRepository(currentUnitOfWork).Add(ExternalLogOn);
		}
	}
}