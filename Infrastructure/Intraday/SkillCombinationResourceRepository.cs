﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Repositories;
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

			var dt = new DataTable();
			dt.Columns.Add("Id", typeof(Guid));
			dt.Columns.Add("SkillId", typeof(Guid));
			dt.Columns.Add("InsertedOn", typeof(DateTime));

			var insertedOn = _now.UtcDateTime();

			var combinationId = Guid.NewGuid();
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

		private Dictionary<string, Guid> loadSkillCombination()
		{
			var result = _currentUnitOfWork.Current().Session()
				.CreateSQLQuery("select Id, SkillId from [ReadModel].[SkillCombination]")
				.SetResultTransformer(Transformers.AliasToBean<internalSkillCombination>())
				.List<internalSkillCombination>();

			var dictionary = result.GroupBy(x => x.Id).ToDictionary(k => keyFor(k.Select(x => x.SkillId)), v => v.Key);
			return dictionary;
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
			var skillCombinations = loadSkillCombination();

			var connectionString = _currentUnitOfWork.Current().Session().Connection.ConnectionString;
			using (var connection = new SqlConnection(connectionString))
			{
				connection.Open();

				var purgeTime = dataLoaded.AddMinutes(-updateReadModelInterval*2);
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


				foreach (var skillCombinationResource in skillCombinationResources)
				{
					var key = keyFor(skillCombinationResource.SkillCombination);
					Guid id;
					if (!skillCombinations.TryGetValue(key, out id))
					{
						id = persistSkillCombination(skillCombinationResource.SkillCombination);
						skillCombinations.Add(key, id);
					}



					using (var transaction = connection.BeginTransaction(IsolationLevel.Serializable))
					{
						using (var deleteCommand = new SqlCommand($@"
						DELETE FROM [ReadModel].[SkillCombinationResource] 
						WHERE SkillCombinationId = @id 
						AND StartDateTime = @startTime", connection, transaction))
						{
							deleteCommand.Parameters.AddWithValue("@id", id);
							deleteCommand.Parameters.AddWithValue("@startTime", skillCombinationResource.StartDateTime);
							deleteCommand.ExecuteNonQuery();
						}

						using (var deleteCommand = new SqlCommand($@"
						DELETE FROM ReadModel.SkillCombinationResourceDelta
						WHERE InsertedOn < @dataLoaded
						AND SkillCombinationId = @id
						AND StartDateTime = @startTime", connection, transaction))
						{
							deleteCommand.Parameters.AddWithValue("@dataLoaded", dataLoaded);
							deleteCommand.Parameters.AddWithValue("@id", id);
							deleteCommand.Parameters.AddWithValue("@startTime", skillCombinationResource.StartDateTime);
							deleteCommand.ExecuteNonQuery();
						}


						using (var insertCommand = new SqlCommand(@"
							INSERT INTO [ReadModel].[SkillCombinationResource] (SkillCombinationId, StartDateTime, EndDateTime, Resource, InsertedOn, BusinessUnit)
							VALUES (@SkillCombinationId, @StartDateTime, @EndDateTime, @Resource, @InsertedOn, @BusinessUnit)", connection, transaction))
						{
							insertCommand.Parameters.AddWithValue("@SkillCombinationId", id);
							insertCommand.Parameters.AddWithValue("@StartDateTime", skillCombinationResource.StartDateTime);
							insertCommand.Parameters.AddWithValue("@EndDateTime", skillCombinationResource.EndDateTime);
							insertCommand.Parameters.AddWithValue("@Resource", skillCombinationResource.Resource);
							insertCommand.Parameters.AddWithValue("@InsertedOn", _now.UtcDateTime());
							insertCommand.Parameters.AddWithValue("@BusinessUnit", bu);

							insertCommand.ExecuteNonQuery();
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

		public void PersistChange(SkillCombinationResource skillCombinationResource)
		{
			var skillCombinations = loadSkillCombination();
			Guid id;
			if (skillCombinations.TryGetValue(keyFor(skillCombinationResource.SkillCombination), out id))
			{
				_currentUnitOfWork.Current().Session()
					.CreateSQLQuery("INSERT INTO [ReadModel].[SkillCombinationResourceDelta] (SkillCombinationId, StartDateTime, EndDateTime, InsertedOn, DeltaResource) VALUES (:SkillCombinationId, :StartDateTime, :EndDateTime, :TimeStamp, :DeltaResource)")
					.SetParameter("SkillCombinationId", id)
					.SetParameter("StartDateTime", skillCombinationResource.StartDateTime)
					.SetParameter("EndDateTime", skillCombinationResource.EndDateTime)
					.SetParameter("TimeStamp", DateTime.UtcNow)
					.SetParameter("DeltaResource", skillCombinationResource.Resource)
					.ExecuteUpdate();

				_currentUnitOfWork.Current().PersistAll();

				var numberResources = _currentUnitOfWork.Current().Session().CreateSQLQuery("SELECT COUNT(*) FROM [ReadModel].[SkillCombinationResource] WHERE StartDateTime = :StartDateTime AND SkillCombinationId = :id")
					.SetParameter("StartDateTime", skillCombinationResource.StartDateTime)
					.SetParameter("id", id)
					.UniqueResult<int>();

				if (numberResources == 0)
					PersistSkillCombinationResource(GetLastCalculatedTime(), new List<SkillCombinationResource>
													{
														new SkillCombinationResource
														{
															StartDateTime = skillCombinationResource.StartDateTime,
															EndDateTime = skillCombinationResource.EndDateTime,
															Resource = 0,
															SkillCombination = skillCombinationResource.SkillCombination
														}
													});

			}
		}

		public DateTime GetLastCalculatedTime()
		{
			var bu = _currentBusinessUnit.Current().Id.GetValueOrDefault();
			var latest =_currentUnitOfWork.Current().Session()
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


	public class SkillCombinationResourceRepositoryEmpty : SkillCombinationResourceRepository
	{
		public override void PersistSkillCombinationResource(DateTime dataLoaded, IEnumerable<SkillCombinationResource> skillCombinationResources)
		{

		}

		public SkillCombinationResourceRepositoryEmpty(INow now, ICurrentUnitOfWork currentUnitOfWork, ICurrentBusinessUnit currentBusinessUnit, IRequestStrategySettingsReader requestStrategySettingsReader) 
			: base(now, currentUnitOfWork, currentBusinessUnit, requestStrategySettingsReader)
		{
		}
	}

}