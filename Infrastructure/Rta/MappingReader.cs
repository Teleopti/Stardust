using System.Collections.Generic;
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