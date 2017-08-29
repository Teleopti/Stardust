using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.SeatPlanning;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Configurable
{
	public class SeatMapLocationConfigurable : IDataSetup
	{

		public SeatMapLocation SeatMapLocation;

		public String Name { get; set; }
		
		public void Apply(ICurrentUnitOfWork currentUnitOfWork)
		{

			SeatMapLocation = new SeatMapLocation()
			{
				Name = Name,
				SeatMapJsonData = "<fakedata/>"
			};

			new SeatMapLocationRepository(currentUnitOfWork).Add(SeatMapLocation);
		}



	}
}