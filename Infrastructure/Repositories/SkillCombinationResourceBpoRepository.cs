using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class SkillCombinationResourceBpo
	{
		public DateTime StartDateTime { get; set; }
		public DateTime EndDateTime { get; set; }
		public double Resources { get; set; }
		public Guid SkillCombinationId { get; set; }
		public string Source { get; set; }
	}

	public interface ISkillCombinationResourceBpoRepository
	{
		void PersistSkillCombinationResourceBpo(DateTime utcDateTime, List<SkillCombinationResourceBpo> combinationResources);
		Dictionary<Guid, string> LoadSourceBpo(SqlConnection connection);

		/// <summary>
		/// Used for testing
		/// </summary>
		/// <returns></returns>
		IList<SkillCombinationResourceBpo> LoadBpoSkillCombinationResources();
	}

	public class SkillCombinationResourceBpoRepository : ISkillCombinationResourceBpoRepository
	{
		private readonly ICurrentUnitOfWork _currentUnitOfWork;
		private readonly INow _now;

		public SkillCombinationResourceBpoRepository(ICurrentUnitOfWork currentUnitOfWork, INow now)
		{
			_currentUnitOfWork = currentUnitOfWork;
			_now = now;
		}

		public void PersistSkillCombinationResourceBpo(DateTime utcDateTime, List<SkillCombinationResourceBpo> combinationResources)
		{
			var connectionString = _currentUnitOfWork.Current().Session().Connection.ConnectionString;
			using (var connection = new SqlConnection(connectionString))
			{
				//connection.OpenWithRetry(_retryPolicy);
				connection.Open();
				var bpoList = LoadSourceBpo(connection); 
				
				var dt = new DataTable();
				dt.Columns.Add("SkillCombinationId", typeof(Guid));
				dt.Columns.Add("StartDateTime", typeof(DateTime));
				dt.Columns.Add("EndDateTime", typeof(DateTime));
				dt.Columns.Add("InsertedOn", typeof(DateTime));
				dt.Columns.Add("Resources", typeof(double));
				dt.Columns.Add("SourceId", typeof(Guid));
				var insertedOn = _now.UtcDateTime();
				foreach (var skillCombinationResourceBpo in combinationResources)
				{
					
					var row = dt.NewRow();
					row["SkillCombinationId"] = skillCombinationResourceBpo.SkillCombinationId;
					row["StartDateTime"] = skillCombinationResourceBpo.StartDateTime;
					row["EndDateTime"] = skillCombinationResourceBpo.EndDateTime;
					row["InsertedOn"] = insertedOn;
					row["Resources"] = skillCombinationResourceBpo.Resources;
					
					var bpoId = Guid.NewGuid();
					if (bpoList.ContainsValue(skillCombinationResourceBpo.Source))
						bpoId = bpoList.Where(x => x.Value == skillCombinationResourceBpo.Source).First().Key;
					else
					{
						using (var transaction = connection.BeginTransaction())
						{
							using (var insertCommand = new SqlCommand(@"insert into ReadModel.[SourceBpo] (Id, Source) Values (@id,@source)", connection,transaction))
							{
								insertCommand.Parameters.AddWithValue("@id", bpoId);
								insertCommand.Parameters.AddWithValue("@source", skillCombinationResourceBpo.Source);
								//set lock time to null
								insertCommand.ExecuteNonQuery();
							}
							transaction.Commit();
							bpoList.Add(bpoId,skillCombinationResourceBpo.Source);
						}
						
					}
					row["SourceId"] = bpoId;
					dt.Rows.Add(row);
				}

				using (var transaction = connection.BeginTransaction())
				{
					using (var sqlBulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction))
					{
						sqlBulkCopy.DestinationTableName = "[ReadModel].[SkillCombinationResourceBpo]";
						sqlBulkCopy.WriteToServer(dt);
					}
					transaction.Commit();
				}

			}
		}

		public Dictionary<Guid,string> LoadSourceBpo(SqlConnection connection)
		{
			var bpoList = new Dictionary<Guid, string>();
			using (var command = new SqlCommand("select Id, Source from [ReadModel].[SourceBpo]", connection))
			{
				using (var reader = command.ExecuteReader())
				{
					if (reader.HasRows)
						while (reader.Read())
							bpoList.Add(reader.GetGuid(0), reader.GetString(1));
				}
			}
			return bpoList;
		}

		//may be just for testing if not then better not to use the current unit of work
		public IList<SkillCombinationResourceBpo> LoadBpoSkillCombinationResources()
		{
			var result = _currentUnitOfWork.Current().Session()
				.CreateSQLQuery(@"select SkillCombinationId,StartDateTime,EndDateTime,Resources,sb.source as Source
								FROM ReadModel.SkillCombinationResourceBpo scrb,  ReadModel.sourcebpo sb
									where sb.Id= scrb.SourceId")
				.SetResultTransformer(new AliasToBeanResultTransformer(typeof(SkillCombinationResourceBpo)))
				.List<SkillCombinationResourceBpo>();

			
			return result;
		}

		
	}
}