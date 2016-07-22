using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Infrastructure.LiteUnitOfWork.ReadModelUnitOfWork;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Rta
{
	public class MappingReadModelReader : IMappingReader
	{
		public const string MagicString = "magic.string";
		private readonly ICurrentReadModelUnitOfWork _unitOfWork;

		public MappingReadModelReader(ICurrentReadModelUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public IEnumerable<Mapping> Read()
		{
			return _unitOfWork.Current()
				.CreateSqlQuery("SELECT * FROM [ReadModel].[RuleMappings]")
				.SetResultTransformer(Transformers.AliasToBean(typeof (internalModel)))
				.List<Mapping>();
		}

		public IEnumerable<Mapping> ReadFor(IEnumerable<string> stateCodes, IEnumerable<Guid?> activities)
		{
			if (stateCodes.IsEmpty())
				stateCodes = new string[] {null};
			if (activities.IsEmpty())
				activities = new Guid?[] {null};

			return _unitOfWork.Current()
				.CreateSqlQuery(@"
SELECT * FROM [ReadModel].[RuleMappings]
WHERE StateCode IN (:stateCodes) 
AND ActivityId IN (:activities)")
				.SetParameterList("stateCodes", stateCodes.Select(stateCode => stateCode ?? MagicString))
				.SetParameterList("activities", activities.Select(x => x ?? Guid.Empty))
				.SetResultTransformer(Transformers.AliasToBean(typeof (internalModel)))
				.List<internalModel>();
		}

		private class internalModel : Mapping
		{
			public new Guid? ActivityId
			{
				set { base.ActivityId = value == Guid.Empty ? null : value; }
			}
			public new string StateCode
			{
				set { base.StateCode = value == MagicString ? null : value; }
			}
			public new int Adherence
			{
				set { base.Adherence = (Adherence) value; }
			}
		}
	}
}