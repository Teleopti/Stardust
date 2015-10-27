using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeRepositories.Rta
{
	public class FakeAdherenceDetailsReadModelReader : IAdherenceDetailsReadModelReader
	{
		private IEnumerable<AdherenceDetailsReadModel> _data = new AdherenceDetailsReadModel[] {};
		
		public FakeAdherenceDetailsReadModelReader()
		{
		}

		public FakeAdherenceDetailsReadModelReader(IEnumerable<AdherenceDetailsReadModel> data)
		{
			_data = data;
		}

		public void Has(AdherenceDetailsReadModel data)
		{
			_data = new[] {data};
		}

		public AdherenceDetailsReadModel Read(Guid personId, DateOnly date)
		{
			return _data.SingleOrDefault(r => r.PersonId == personId && r.Date == date.Date);
		}
	}
}