using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.ReadModel;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public interface IEtlReadModelRepository
	{
		IList<IScheduleChangedReadModel> ChangedDataOnStep(DateTime afterDate, IBusinessUnit currentBusinessUnit, string stepName);
		ILastChangedReadModel LastChangedDate(IBusinessUnit currentBusinessUnit, string stepName, DateTimePeriod period);
		void UpdateLastChangedDate(IBusinessUnit currentBusinessUnit, string stepName, DateTime thisTime);
		void WorkAroundFor27636();
	}
}