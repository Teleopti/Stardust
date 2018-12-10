using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;


namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeBadgeCalculationRepository: IBadgeCalculationRepository
	{
		class ahtTable
		{
			public Guid PersonId { get; set; }
			public DateTime Date { get; set; }
			public TimeSpan Value { get; set; }
		}

		class answeredCallsTable
		{
			public Guid PersonId { get; set; }
			public DateTime Date { get; set; }
			public int Value { get; set; }
		}

		class adherenceTable
		{
			public Guid PersonId { get; set; }
			public DateTime Date { get; set; }
			public double Value { get; set; }
		}

		private List<ahtTable> _ahtTable = new List<ahtTable>();
		private List<answeredCallsTable> _answeredCallsTable = new List<answeredCallsTable>();
		private List<adherenceTable> _adherenceTable = new List<adherenceTable>();

		public void AddAht(DateTime date, TimeSpan ahtThreshold, Guid personId)
		{
			_ahtTable.Add(new ahtTable{PersonId = personId, Date = date, Value = ahtThreshold});
		}

		public void AddAnsweredCalls(DateTime date, int answeredCalls, Guid personId)
		{
			_answeredCallsTable.Add(new answeredCallsTable{PersonId = personId, Value = answeredCalls, Date = date});
		}

		public void AddAdherence(DateTime date, double adherence, Guid personId)
		{
			_adherenceTable.Add(new adherenceTable { PersonId = personId, Value = adherence, Date = date});
		}

		public Dictionary<Guid, int> LoadAgentsOverThresholdForAnsweredCalls(string timezoneCode, DateTime date, int answeredCallsThreshold,
			Guid businessUnitId)
		{
			var result = _answeredCallsTable.Where(x => new DateOnly(x.Date) == new DateOnly(date) && x.Value >= answeredCallsThreshold);
			return result.ToDictionary(key => key.PersonId, value => value.Value);
		}

		public Dictionary<Guid, double> LoadAgentsOverThresholdForAdherence(AdherenceReportSettingCalculationMethod adherenceCalculationMethod,
			string timezoneCode, DateTime date, Percent adherenceThreshold, Guid businessUnitId)
		{
			var result = _adherenceTable.Where(x => new DateOnly(x.Date) == new DateOnly(date) && x.Value >= adherenceThreshold.Value);
			return result.ToDictionary(key => key.PersonId, value => value.Value);
		}

		public Dictionary<Guid, double> LoadAgentsUnderThresholdForAht(string timezoneCode, DateTime date, TimeSpan ahtThreshold,
			Guid businessUnitId)
		{
			var result = _ahtTable.Where(x => new DateOnly(x.Date) == new DateOnly(date) && x.Value <= ahtThreshold);
			return result.ToDictionary(key => key.PersonId, value => value.Value.TotalSeconds);
		}
	}
}
