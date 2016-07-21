using System;
using System.Collections.Generic;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Infrastructure.LiteUnitOfWork.ReadModelUnitOfWork;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Rta
{
	public class MappingReadModelReader : IMappingReader {

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
			return null;
		}

		private class internalModel : Mapping
		{
			public new int Adherence
			{
				set { base.Adherence = (Adherence) value; }
			}
		}
	}
}