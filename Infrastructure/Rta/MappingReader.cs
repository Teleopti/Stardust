using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Rta
{
	public class MappingReader : IMappingReader
	{
		private readonly ICurrentUnitOfWork _unitOfWork;

		public MappingReader(ICurrentUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public IEnumerable<Mapping> Read()
		{
			return _unitOfWork.Session()
				.CreateSQLQuery(@"EXEC [dbo].[LoadAllRtaMappings]")
				.SetResultTransformer(Transformers.AliasToBean(typeof (readMapping)))
				.List<readMapping>();
		}

		public IEnumerable<Mapping> ReadFor(IEnumerable<string> stateCodes, IEnumerable<Guid?> activities)
		{
			return _unitOfWork.Session()
				.CreateSQLQuery(@"EXEC [dbo].[LoadRtaMappingsFor] :stateCode1, :stateCode2, :activity1, :activity2, :activity3")
				.SetParameter("stateCode1", stateCodes.ElementAtOrDefault(0))
				.SetParameter("stateCode2", stateCodes.ElementAtOrDefault(1))
				.SetParameter("activity1", activities.ElementAtOrDefault(0))
				.SetParameter("activity2", activities.ElementAtOrDefault(1))
				.SetParameter("activity3", activities.ElementAtOrDefault(2))
				.SetResultTransformer(Transformers.AliasToBean(typeof (readMapping)))
				.List<readMapping>();
		}

		private class readMapping : Mapping
		{
			public int? AdherenceInt
			{
				set
				{
					if (value.HasValue)
						Adherence = (Adherence) value.Value;
				}
			}
		}
	}
}