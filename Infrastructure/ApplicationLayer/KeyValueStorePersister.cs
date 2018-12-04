using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.LiteUnitOfWork.ReadModelUnitOfWork;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public class KeyValueStorePersister : IKeyValueStorePersister
	{
		private readonly ICurrentReadModelUnitOfWork _unitOfWork;

		public KeyValueStorePersister(ICurrentReadModelUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public void Update(string key, string value)
		{
			var updated = _unitOfWork.Current()
				.CreateSqlQuery("UPDATE [ReadModel].[KeyValueStore] SET [Value] = :value WHERE [Key] = :key")
				.SetParameter("key", key)
				.SetParameter("value", value)
				.ExecuteUpdate();
			if (updated == 0)
				_unitOfWork.Current()
					.CreateSqlQuery("INSERT INTO [ReadModel].[KeyValueStore] ([Key], [Value]) VALUES (:key, :value)")
					.SetParameter("key", key)
					.SetParameter("value", value)
					.ExecuteUpdate();
		}

		public string Get(string key)
		{
			return _unitOfWork.Current()
				.CreateSqlQuery("SELECT [Value] FROM [ReadModel].[KeyValueStore] WHERE [Key] = :key")
				.SetParameter("key", key)
				.UniqueResult<string>();
		}

		public void Delete(string key)
		{
			_unitOfWork.Current()
				.CreateSqlQuery("DELETE FROM [ReadModel].[KeyValueStore] WHERE [Key] = :key")
				.SetParameter("key", key)
				.ExecuteUpdate();
		}
	}
}