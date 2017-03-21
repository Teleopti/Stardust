using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeOptionalColumnRepository : IOptionalColumnRepository
	{
		IList<IOptionalColumn> _optionalColumnList = new List<IOptionalColumn>();
		IList<IOptionalColumnValue> _personValueList = new List<IOptionalColumnValue>();

		public void Add(IOptionalColumn root)
		{
			_optionalColumnList.Add(root);
		}

		public void Remove(IOptionalColumn root)
		{
			throw new NotImplementedException();
		}

		public IOptionalColumn Get(Guid id)
		{
			return _optionalColumnList.SingleOrDefault(x => x.Id == id);
		}

		public IOptionalColumn Load(Guid id)
		{
			throw new NotImplementedException();
		}

		public IList<IOptionalColumn> LoadAll()
		{
			throw new NotImplementedException();
		}

		public IList<IOptionalColumn> GetOptionalColumns<T>()
		{
			throw new NotImplementedException();
		}

		public IList<IColumnUniqueValues> UniqueValuesOnColumn(Guid column)
		{
			throw new NotImplementedException();
		}

		public void AddPersonValues(IOptionalColumnValue personValue)
		{
			_personValueList.Add(personValue);
		}

		public List<IOptionalColumnValue> OptionalColumnValues(Guid optionalColumnId)
		{
			return _personValueList.Where(x => x.Parent.Id.Value == optionalColumnId).ToList();
		}
	}
}