using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
	public static class RepositoryExtensions
	{
		public static void AddRange<T>(this IRepository<T> repository, IEnumerable<T> entityCollection)
		{
			foreach (var entity in entityCollection)
			{
				repository.Add(entity);
			}
		}
	}
}