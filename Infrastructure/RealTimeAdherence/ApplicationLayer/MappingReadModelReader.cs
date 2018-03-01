using System;
using System.Collections.Generic;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Service;
using Teleopti.Ccc.Infrastructure.LiteUnitOfWork.ReadModelUnitOfWork;

namespace Teleopti.Ccc.Infrastructure.RealTimeAdherence.ApplicationLayer
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
				.CreateSqlQuery("SELECT * FROM [ReadModel].[RuleMappings] WITH (NOLOCK)")
				.SetResultTransformer(Transformers.AliasToBean(typeof(internalModel)))
				.List<Mapping>();
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
			public new int? Adherence
			{
				set { base.Adherence = (Adherence?) value; }
			}
			public bool Updated { get; set; }
		}
	}
}