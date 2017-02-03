﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Collection;

namespace Teleopti.Ccc.TestCommon.FakeRepositories.Rta
{
	public class FakeExternalLogonReadModelPersister : IExternalLogonReader, IExternalLogonReadModelPersister
	{
		private IEnumerable<ExternalLogonReadModel> _data = Enumerable.Empty<ExternalLogonReadModel>();
		
		public virtual IEnumerable<ExternalLogonReadModel> Read()
		{
			return _data.Where(x => !x.Added).ToArray();
		}

		public void Delete(Guid personId)
		{
			_data
				.Where(x => x.PersonId == personId)
				.ForEach(x => x.Deleted = true);
		}

		public void Add(ExternalLogonReadModel model)
		{
			model.Added = true;
			_data = _data
				.Append(model)
				.ToArray();
		}

		public void Refresh()
		{
			_data = _data
				.Where(x => !x.Deleted)
				.ToArray()
				.ForEach(x => x.Added = false);
		}
	}
}