﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class OptionalColumnCollectionChangedEvent: EventWithInfrastructureContext
	{
		private string _serializedOptionalColumn;

		[NonSerialized]
		private ICollection<Guid> _optionalColumnIdCollection;

		public ICollection<Guid> OptionalColumnIdCollection
		{
			get
			{
				if (_optionalColumnIdCollection != null) return _optionalColumnIdCollection;
				_optionalColumnIdCollection = string.IsNullOrEmpty(_serializedOptionalColumn)
					? new HashSet<Guid>()
					: new HashSet<Guid>(_serializedOptionalColumn.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(Guid.Parse));
				return _optionalColumnIdCollection;
			}
		}

		public void SetOptionalColumnIdCollection(ICollection<Guid> optionalColumnIdCollection)
		{
			if (optionalColumnIdCollection != null)
			{
				_serializedOptionalColumn = string.Join(",", new HashSet<Guid>(optionalColumnIdCollection).Select(p => p.ToString()));
			}
		}

		public string SerializedOptionalColumn
		{
			get
			{
				return _serializedOptionalColumn;
			}
			set
			{
				_serializedOptionalColumn = value;
			}
		}
	}
}