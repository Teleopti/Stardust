﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Intraday
{
	public class SkillCombinationResourceRepository : ISkillCombinationResourceRepository
	{
		private readonly INow _now;
		private readonly ICurrentUnitOfWork _currentUnitOfWork;
		private readonly ICurrentBusinessUnit _currentBusinessUnit;
		private readonly IRequestStrategySettingsReader _requestStrategySettingsReader;
		private readonly object skillCombinationLock = new object();

		public SkillCombinationResourceRepository(INow now, ICurrentUnitOfWork currentUnitOfWork,
												  ICurrentBusinessUnit currentBusinessUnit, IRequestStrategySettingsReader requestStrategySettingsReader)
		{
			_now = now;
			_currentUnitOfWork = currentUnitOfWork;
			_currentBusinessUnit = currentBusinessUnit;
			_requestStrategySettingsReader = requestStrategySettingsReader;
		}

		private Guid persistSkillCombination(IEnumerable<Guid> skillCombination)
		{
			var combinationId = Guid.NewGuid();
			var dt = new DataTable();
			dt.Columns.Add("Id", typeof(Guid));
			dt.Columns.Add("SkillId", typeof(Guid));
			dt.Columns.Add("InsertedOn", typeof(DateTime));

			var insertedOn = _now.UtcDateTime();


			foreach (var skill in skillCombination)
			{
				var row = dt.NewRow();
				row["SkillId"] = skill;
				row["Id"] = combinationId;
				row["InsertedOn"] = insertedOn;
				dt.Rows.Add(row);
			}


			var connectionString = _currentUnitOfWork.Current().Session().Connection.ConnectionString;

			using (var connection = new SqlConnection(connectionString))
			{
				connection.Open();
				using (var transaction = connection.BeginTransaction())
				{
					using (var sqlBulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction))
					{
						sqlBulkCopy.DestinationTableName = "[ReadModel].[SkillCombination]";
						sqlBulkCopy.WriteToServer(dt);
					}
					transaction.Commit();
				}

			}
			return combinationId;
		}

		private static Dictionary<string, Guid> loadSkillCombination(SqlConnection connection, SqlTransaction transaction)
		{
			var skillCombinations = new Dictionary<string, Guid>();
			using (var command = new SqlCommand("select Id, SkillId from [ReadModel].[SkillCombination] order by id", connection, transaction))
			{
				var result = new List<internalSkillCombination>();
				using (var reader = command.ExecuteReader())
				{
					if (!reader.HasRows) return skillCombinations;
					while (reader.Read())
					{
						var internalSkillCombination = new internalSkillCombination()
						{
							Id = reader.GetGuid(0),
							SkillId = reader.GetGuid(1)
						};
						result.Add(internalSkillCombination);
					}
				}
				var groups = result.GroupBy(x => x.Id);
				foreach (var group in groups)
				{
					var key = keyFor(group.Select(x => x.SkillId));
					if (skillCombinations.ContainsKey(key))
						continue;
					skillCombinations.Add(key, group.Key);
				}
			}
			return skillCombinations;
		}

		private static string keyFor(IEnumerable<Guid> skillIds)
		{
			return string.Join("_", skillIds.OrderBy(x => x));
		}

		private class internalSkillCombination
		{
			public Guid Id { get; set; }
			public Guid SkillId { get; set; }
		}

		public virtual void PersistSkillCombinationResource(DateTime dataLoaded, IEnumerable<SkillCombinationResource> skillCombinationResources)
		{
			var updateReadModelInterval = _requestStrategySettingsReader.GetIntSetting("UpdateResourceReadModelIntervalMinutes", 60);
			var bu = _currentBusinessUnit.Current().Id.GetValueOrDefault();
			lock (skillCombinationLock)
			{


				var connectionString = _currentUnitOfWork.Current().Session().Connection.ConnectionString;
				using (var connection = new SqlConnection(connectionString))
				{
					connection.Open();

					var purgeTime = dataLoaded.AddMinutes(-updateReadModelInterval * 2);
					//Purge old intervals that is out of the scope
					using (var deleteCommand = new SqlCommand(@"
						DELETE FROM [ReadModel].[SkillCombinationResource] 
						WHERE StartDateTime < @purgeTime", connection))
					{
						deleteCommand.Parameters.AddWithValue("@purgeTime", purgeTime);
						deleteCommand.ExecuteNonQuery();
					}

					using (var deleteCommand = new SqlCommand($@"
						DELETE FROM [ReadModel].[SkillCombinationResourceDelta] 
						WHERE StartDateTime < @purgeTime", connection))
					{
						deleteCommand.Parameters.AddWithValue("@purgeTime", purgeTime);
						deleteCommand.ExecuteNonQuery();
					}

					var dt = new DataTable();
					dt.Columns.Add("SkillCombinationId", typeof(Guid));
					dt.Columns.Add("StartDateTime", typeof(DateTime));
					dt.Columns.Add("EndDateTime", typeof(DateTime));
					dt.Columns.Add("Resource", typeof(double));
					dt.Columns.Add("InsertedOn", typeof(DateTime));
					dt.Columns.Add("BusinessUnit", typeof(Guid));
					var insertedOn = _now.UtcDateTime();

					using (var transaction = connection.BeginTransaction(IsolationLevel.Serializable))
					{
						var skillCombinations = loadSkillCombination(connection, transaction);
						foreach (var skillCombinationResource in skillCombinationResources)
						{
							var key = keyFor(skillCombinationResource.SkillCombination);
							Guid id;

							if (!skillCombinations.TryGetValue(key, out id))
							{
								id = persistSkillCombination(skillCombinationResource.SkillCombination);
								skillCombinations.Add(key, id);
							}

							var row = dt.NewRow();
							row["SkillCombinationId"] = id;
							row["StartDateTime"] = skillCombinationResource.StartDateTime;
							row["EndDateTime"] = skillCombinationResource.EndDateTime;
							row["Resource"] = skillCombinationResource.Resource;
							row["InsertedOn"] = insertedOn;
							row["BusinessUnit"] = bu;
							dt.Rows.Add(row);
						}


						using (var deleteCommand = new SqlCommand($@"
						DELETE FROM ReadModel.SkillCombinationResourceDelta
						WHERE InsertedOn < @dataLoaded and 
						SkillCombinationId in 
								(select skillcombinationId from  [ReadModel].[SkillCombinationResource] 
								where businessunit = @buid)"
																  , connection, transaction))
						{
							deleteCommand.Parameters.AddWithValue("@buid", bu);
							deleteCommand.Parameters.AddWithValue("@dataLoaded", dataLoaded);
							deleteCommand.ExecuteNonQuery();
						}

						using (var deleteCommand = new SqlCommand($@"
						DELETE FROM [ReadModel].[SkillCombinationResource] 
						WHERE businessunit = @buid", connection, transaction))
						{
							deleteCommand.Parameters.AddWithValue("@buid", bu);
							deleteCommand.ExecuteNonQuery();
						}


						using (var sqlBulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction))
						{
							sqlBulkCopy.DestinationTableName = "[ReadModel].[SkillCombinationResource]";
							sqlBulkCopy.WriteToServer(dt);
						}

						transaction.Commit();
					}

				}
			}
		}



		public IEnumerable<SkillCombinationResource> LoadSkillCombinationResources(DateTimePeriod period)
		{
			var bu = _currentBusinessUnit.Current().Id.GetValueOrDefault();
			var result = _currentUnitOfWork.Current().Session()
				.CreateSQLQuery(
					@"SELECT r.SkillCombinationId, r.StartDateTime, r.EndDateTime, r.Resource + ISNULL(SUM(d.DeltaResource), 0) as Resource, c.SkillId from 
[ReadModel].[SkillCombinationResource] r INNER JOIN [ReadModel].[SkillCombination] c ON c.Id = r.SkillCombinationId 
LEFT JOIN [ReadModel].[SkillCombinationResourceDelta] d ON d.SkillCombinationId = r.SkillCombinationId AND d.StartDateTime = r.StartDateTime AND d.EndDateTime = r.EndDateTime
 WHERE r.StartDateTime < :endDateTime AND r.EndDateTime > :startDateTime AND r.BusinessUnit = :bu GROUP BY r.SkillCombinationId, r.StartDateTime, r.EndDateTime, r.Resource, c.SkillId")
				.SetDateTime("startDateTime", period.StartDateTime)
				.SetDateTime("endDateTime", period.EndDateTime)
				.SetParameter("bu", bu)
				.SetResultTransformer(new AliasToBeanResultTransformer(typeof(RawSkillCombinationResource)))
				.SetTimeout(10)
				.List<RawSkillCombinationResource>();

			var mergedResult =
				result.GroupBy(x => new {x.SkillCombinationId, x.StartDateTime, x.EndDateTime, x.Resource})
					.Select(
						x =>
							new SkillCombinationResourceWithCombinationId
							{
								StartDateTime = x.Key.StartDateTime.Utc(),
								EndDateTime = x.Key.EndDateTime.Utc(),
								Resource = x.Key.Resource,
								SkillCombinationId = x.Key.SkillCombinationId,
								SkillCombination = x.Select(s => s.SkillId).OrderBy(s => s)
							});

			return mergedResult;
		}

		public void PersistChanges(IEnumerable<SkillCombinationResource> deltas)
		{
			var connectionString = _currentUnitOfWork.Current().Session().Connection.ConnectionString;

			using (var connection = new SqlConnection(connectionString))
			{
				var skillCombinations = loadSkillCombination(connection, null);

				var dt = new DataTable();
				dt.Columns.Add("SkillCombinationId", typeof(Guid));
				dt.Columns.Add("StartDateTime", typeof(DateTime));
				dt.Columns.Add("EndDateTime", typeof(DateTime));
				dt.Columns.Add("InsertedOn", typeof(DateTime));
				dt.Columns.Add("DeltaResource", typeof(double));

				foreach (var delta in deltas)
				{
					Guid id;
					if (!skillCombinations.TryGetValue(keyFor(delta.SkillCombination), out id)) continue;

					var row = dt.NewRow();
					row["SkillCombinationId"] = id;
					row["StartDateTime"] = delta.StartDateTime;
					row["EndDateTime"] = delta.EndDateTime;
					row["InsertedOn"] = DateTime.UtcNow;
					row["DeltaResource"] = delta.Resource;
					dt.Rows.Add(row);

					var numberResources = _currentUnitOfWork.Current().Session().CreateSQLQuery("SELECT COUNT(*) FROM [ReadModel].[SkillCombinationResource] WHERE StartDateTime = :StartDateTime AND SkillCombinationId = :id")
						.SetParameter("StartDateTime", delta.StartDateTime)
						.SetParameter("id", id)
						.UniqueResult<int>();

					if (numberResources != 0) continue;

					var bu = _currentBusinessUnit.Current().Id.GetValueOrDefault();
					var lastUpdated = GetLastCalculatedTime();
					_currentUnitOfWork.Current().Session()
						.CreateSQLQuery(@"
							INSERT INTO [ReadModel].[SkillCombinationResource] (SkillCombinationId, StartDateTime, EndDateTime, Resource, InsertedOn, BusinessUnit)
							VALUES (:SkillCombinationId, :StartDateTime, :EndDateTime, :Resource, :InsertedOn, :BusinessUnit)")
						.SetParameter("SkillCombinationId", id)
						.SetParameter("StartDateTime", delta.StartDateTime)
						.SetParameter("EndDateTime", delta.EndDateTime)
						.SetParameter("Resource", 0)
						.SetParameter("InsertedOn", lastUpdated)
						.SetParameter("BusinessUnit", bu)
						.ExecuteUpdate();
				}


				connection.Open();
				using (var transaction = connection.BeginTransaction())
				{
					using (var sqlBulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction))
					{
						sqlBulkCopy.DestinationTableName = "[ReadModel].[SkillCombinationResourceDelta]";
						sqlBulkCopy.WriteToServer(dt);
					}
					transaction.Commit();
				}
			}
		}

		public DateTime GetLastCalculatedTime()
		{
			var bu = _currentBusinessUnit.Current().Id.GetValueOrDefault();
			var latest = _currentUnitOfWork.Current().Session()
				.CreateSQLQuery("SELECT top(1) InsertedOn from [ReadModel].SkillCombinationResource Where BusinessUnit = :bu order by InsertedOn desc ")
				.SetParameter("bu", bu)
				.UniqueResult<DateTime>();

			return latest;
		}

		protected string AddArrayParameters(SqlCommand sqlCommand, Guid[] array, string paramName)
		{
			var parameters = new string[array.Length];
			for (var i = 0; i < array.Length; i++)
			{
				parameters[i] = $"@{paramName}{i}";
				sqlCommand.Parameters.AddWithValue(parameters[i], array[i]);
			}

			return string.Join(", ", parameters);
		}
	}

	public class RawSkillCombinationResource
	{
		public Guid SkillCombinationId { get; set; }
		public DateTime StartDateTime { get; set; }
		public DateTime EndDateTime { get; set; }
		public double Resource { get; set; }
		public Guid SkillId { get; set; }
	}


	public class SkillCombinationResourceWithCombinationId : SkillCombinationResource
	{
		public Guid SkillCombinationId { get; set; }
	}

}