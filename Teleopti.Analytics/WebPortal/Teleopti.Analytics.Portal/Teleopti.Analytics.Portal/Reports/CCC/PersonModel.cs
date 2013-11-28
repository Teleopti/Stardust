using System;
using System.Data;
using Teleopti.Analytics.Portal.Utils;

namespace Teleopti.Analytics.Portal.Reports.Ccc
{
	internal class PersonModel : IEquatable<PersonModel>
	{
		private readonly bool _perDate;
		private readonly int _personId;
		public DataRow DataRow { get; set; }

		public PersonModel(DataRow dataRow, bool perDate)
		{
			_perDate = perDate;
			_personId = (int) dataRow["person_id"];
			DataRow = dataRow;
		}

		public string PersonName
		{
			get { return (string)DataRow["person_name"]; }
		}

		public string DateText
		{
			get { return ShiftStartDate.ToShortDateString(); }
		}

		private DateTime IntervalDate
		{
			get { return (DateTime)DataRow["date"]; }
		}

		public DateTime ShiftStartDate

		{
			get { return ((DateTime)DataRow["shift_startdate"]); }
		}

		public decimal? AdherenceTotal
		{
			get
			{
				if (DataRow["adherence_tot"] == DBNull.Value)
					return null;
				return (decimal)DataRow["adherence_tot"];
			}
		}

		public decimal? DeviationTotal
		{
			get
			{
				if (DataRow["deviation_tot_m"] == DBNull.Value)
					return null;
				return (decimal)DataRow["deviation_tot_m"];
			}
		}

		public decimal? TeamAdherenceTotal
		{
			get
			{
				if (DataRow["team_adherence_tot"] == DBNull.Value)
					return null;
				return (decimal)DataRow["team_adherence_tot"];
			}
		}

		public decimal? TeamDeviationTotal
		{
			get
			{
				if (DataRow["team_deviation_tot_m"] == DBNull.Value)
					return null;
				return (decimal)DataRow["team_deviation_tot_m"];
			}
		}

		public int FirstIntervalId { get { return (int)DataRow["interval_id"]; } }



		public bool Equals(PersonModel other)
		{
			if (ReferenceEquals(null, other))
			{
				return false;
			}
			if (ReferenceEquals(this, other))
			{
				return true;
			}
			return other._personId == _personId;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
			{
				return false;
			}
			if (ReferenceEquals(this, obj))
			{
				return true;
			}
			if (obj.GetType() != typeof (PersonModel))
			{
				return false;
			}
			return Equals((PersonModel) obj);
		}

		public override int GetHashCode()
		{
			if (_perDate)
				return ShiftStartDate.GetHashCode();
			return _personId;
		}

		public bool EndsOnNextDate { get; set; }

		public bool LoggedInOnTheDayBefore
		{
			get { return IntervalDate.IsEarlierThan(ShiftStartDate); }
		}
	}
}