﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer
{
	public class TestWriteSideRepository<T> : IEnumerable<T>, IWriteSideRepository<T> where T : IAggregateRoot
	{
		private readonly IList<T> _entities = new List<T>();

		public void Add(T entity)
		{
			_entities.Add(entity);
		}

		public void Remove(T entity)
		{
			_entities.Remove(entity);
		}

		public T Get(Guid id)
		{
			return _entities.Single(e => e.Id.Equals(id));
		}

		public IEnumerator<T> GetEnumerator()
		{
			return _entities.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _entities.GetEnumerator();
		}
	}
}