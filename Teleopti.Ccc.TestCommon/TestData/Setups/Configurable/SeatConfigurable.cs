using Teleopti.Ccc.Domain.SeatPlanning;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Configurable
{
	public class SeatConfigurable : IDataSetup
	{
		public string Name { get; set; }
		public int Priority { get; set; }

		public void Apply(IUnitOfWork now)
		{
			var location = new SeatMapLocation();
			location.SetLocation("{\"objects\":[{\"type\":\"seat\",\"originX\":\"left\",\"originY\":\"top\",\"left\":513.05,\"top\":426.24,\"width\":35.9,\"height\":46.52,\"fill\":\"rgb(0,0,0)\",\"stroke\":null,\"strokeWidth\":1,\"strokeDashArray\":null,\"strokeLineCap\":\"butt\",\"strokeLineJoin\":\"miter\",\"strokeMiterLimit\":10,\"scaleX\":1,\"scaleY\":1,\"angle\":0,\"flipX\":false,\"flipY\":false,\"opacity\":1,\"shadow\":null,\"visible\":true,\"clipTo\":null,\"backgroundColor\":\"\",\"fillRule\":\"nonzero\",\"globalCompositeOperation\":\"source-over\",\"src\":\"js/SeatManagement/Images/seat.svg\",\"filters\":[],\"crossOrigin\":\"\",\"alignX\":\"none\",\"alignY\":\"none\",\"meetOrSlice\":\"meet\",\"id\":\"44bc7ba7-c502-4c49-a7ad-e06e43059c93\",\"name\":\"1\",\"priority\":1}],\"background\":\"\"}", "TestLocation");
			location.AddSeat(Name, Priority);

			var rep = new SeatMapLocationRepository(now);
			rep.Add(location);
		}
	}
}
