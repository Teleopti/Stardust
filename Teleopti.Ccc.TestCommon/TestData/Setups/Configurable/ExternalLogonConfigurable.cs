using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;

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

			ExternalLogOnRepository.DONT_USE_CTOR(currentUnitOfWork).Add(ExternalLogOn);
		}
	}
}