using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.SeatPlanning
{
	[Serializable]
	public class SeatMap : VersionedAggregateRootWithBusinessUnit, ISeatMap
	{
		private ISet<Location> locations = new HashSet<Location>();
		
		public virtual Location Location
		{
			get { return getLocation(); }
			set { setlocation(value); }
		}
		
		public virtual string SeatMapJsonData { get; set; }

		public virtual void CreateSeatMap(String seatMapJsonData, String locationName)
		{
			Location = new Location(){ Name = locationName};

			if (Location != null)
			{
				Location.SetParent(this);
			}

			SeatMapJsonData = seatMapJsonData;
		}

		public virtual SeatMap CreateChildSeatMap (LocationInfo location)
		{
			var childSeatMap = new SeatMap();
			childSeatMap.CreateSeatMap("{}", location.Name);
			Location.AddChild(childSeatMap.Location);

			return childSeatMap;
		}

		private void setlocation(Location location)
		{
			//We have to do it this way as cascading actions for one-to-one mappings aren't implemented in NHibernate
			locations.Clear();
			if (location != null)
			{
				location.SetParent(this);
				locations.Add(location);
			}
		}

		private Location getLocation()
		{
			//We have to do it this way as cascading actions for one-to-one mappings aren't implemented in NHibernate
			if (locations.Count == 0)
				return null;

			return locations.Single();
		}

		public virtual void UpdateSeatMapTemporaryId(Guid? temporaryId, Guid? persistedId)
		{
			SeatMapJsonData = SeatMapJsonData.Replace(temporaryId.ToString(), persistedId.ToString());
		}



	}
}