using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Teleopti.Ccc.Domain.SeatPlanning;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Configurable
{
	public class SeatConfigurable : IDataSetup
	{
		private int _seatNumber;

		public string Name { get; set; }
		public int Priority { get; set; }

		public SeatConfigurable()
		{ }

		public SeatConfigurable(int seatNumber)
		{
			_seatNumber = seatNumber;
		}



		public void Apply(ICurrentUnitOfWork currentUnitOfWork)
		{
			var rep = new SeatMapLocationRepository(currentUnitOfWork);
			var rootLocation = rep.LoadRootSeatMap() as SeatMapLocation;
			var seats = new List<Seat>();

			if (rootLocation != null)
			{
				var maxPrioritySeat = rootLocation.Seats.OrderBy(seat => seat.Priority).First();
				var seatPriority = 0;

				while (_seatNumber > 0)
				{
					_seatNumber--;
					seatPriority++;
					var currentSeatPriority = maxPrioritySeat.Priority + seatPriority;
					seats.Add(rootLocation.AddSeat(currentSeatPriority.ToString(), currentSeatPriority));
				}
			}
			else
			{
				rootLocation = new SeatMapLocation();
				seats.Add(rootLocation.AddSeat(Name, Priority));
				rootLocation.SeatMapJsonData = "{\"objects\":[],\"background\":\"\"}";
				rootLocation.Name = "testLocation";
			}

			rootLocation.SetLocation(addSeatsToLocationJson(rootLocation.SeatMapJsonData, seats), rootLocation.Name);
			rep.Add(rootLocation);
		}





		private string addSeatsToLocationJson(string locationJson, IList<Seat> seats)
		{
			dynamic location = JsonConvert.DeserializeObject(locationJson);
			var left = 400;


			if (((JArray)location.objects).Any())
			{
				dynamic lastSeat = ((JArray)location.objects).Last;
				left = lastSeat.left + 40;
			}


			foreach (var seat in seats)
			{
				var seatObject = getSeatObject(seat.Id, seat.Priority, seat.Name, left);
				((JArray)location.objects).Add(JToken.FromObject(seatObject));
				left = left + 40;
			}

			return JsonConvert.SerializeObject(location);
		}



		private object getSeatObject(Guid? seatId, int seatPriority, string seatName, int seatPositionLeft)
		{
			return new
			{
				filters = new Array[0],
				backgroundColor = "",
				fillRule = "nonzero",
				globalCompositeOperation = "source-over",
				crossOrigin = "",
				alignX = "none",
				alignY = "none",
				meetOrSlice = "meet",
				fill = "rgb(0,0,0)",
				strokeWidth = 1,
				strokeLineCap = "butt",
				strokeLineJoin = "miter",
				type = "seat",
				originX = "left",
				originY = "top",
				src = "js/SeatManagement/Images/seat.svg",
				strokeMiterLimit = 10,
				opacity = 1,
				width = 35.9,
				height = 46.52,
				scaleX = 1,
				scaleY = 1,
				angle = 0,
				flipX = false,
				flipY = false,
				visible = true,


				id = seatId ?? Guid.Empty,
				name = seatName,
				priority = seatPriority,
				left = seatPositionLeft,
				top = 400
			};
		}
	}
}
