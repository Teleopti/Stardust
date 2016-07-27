using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Infrastructure.LiteUnitOfWork.ReadModelUnitOfWork;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Rta
{
	public class MappingReadModelReader : IMappingReader
	{
		public const string MagicString = "magic.string";
		private readonly ICurrentReadModelUnitOfWork _unitOfWork;

		private int _version;
		private IEnumerable<Mapping> _cache = Enumerable.Empty<Mapping>();

		public MappingReadModelReader(ICurrentReadModelUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public IEnumerable<Mapping> Read()
		{
			var version = _unitOfWork.Current()
				.CreateSqlQuery("SELECT CONVERT(INT, [Value]) FROM [ReadModel].[KeyValueStore] WHERE [Key] = 'RuleMappingsVersion'")
				.UniqueResult<int>();

			if (version == _version && _cache.Any())
				return _cache;

			_version = version;
			_cache = _unitOfWork.Current()
				.CreateSqlQuery("SELECT * FROM [ReadModel].[RuleMappings]")
				.SetResultTransformer(Transformers.AliasToBean(typeof (internalModel)))
				.List<Mapping>();

			return _cache;
		}

		public IEnumerable<Mapping> ReadFor(IEnumerable<string> stateCodes, IEnumerable<Guid?> activities)
		{
			return (
				from m in Read()
				where
					stateCodes.Contains(m.StateCode) &&
					activities.Contains(m.ActivityId)
				select m
				).ToArray();
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
		}
	}
}