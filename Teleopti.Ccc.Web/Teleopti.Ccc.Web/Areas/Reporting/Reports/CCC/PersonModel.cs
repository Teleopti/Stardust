using System;
using System.Data;
using Teleopti.Ccc.Web.Areas.Reporting.Core;

namespace Teleopti.Ccc.Web.Areas.Reporting.Reports.CCC
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

		public string PersonName => (string)DataRow["person_name"];
		public string DateText => ShiftStartDate.ToShortDateString();
		private DateTime intervalDate => (DateTime)DataRow["date"];
		public DateTime ShiftStartDate => (DateTime)DataRow["shift_startdate"];
		public decimal? AdherenceTotal => DataRow["adherence_tot"] == DBNull.Value ? (decimal?) null : (decimal) DataRow["adherence_tot"];
		public decimal? DeviationTotal => DataRow["deviation_tot_m"] == DBNull.Value ? (decimal?) null : (decimal) DataRow["deviation_tot_m"];
		public decimal? TeamAdherenceTotal => DataRow["team_adherence_tot"] == DBNull.Value ? (decimal?) null : (decimal) DataRow["team_adherence_tot"];
		public decimal? TeamDeviationTotal => DataRow["team_deviation_tot_m"] == DBNull.Value ? (decimal?) null : (decimal) DataRow["team_deviation_tot_m"];
		public int FirstIntervalId => (int)DataRow["interval_id"];

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

		public override int GetHashCode() => _perDate ? ShiftStartDate.GetHashCode() : _personId;
		public bool EndsOnNextDate { get; set; }
		public bool LoggedInOnTheDayBefore => intervalDate.IsEarlierThan(ShiftStartDate);
	}
}