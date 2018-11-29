using System;
using System.Collections.Generic;
using NHibernate;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Domain.Util;
using Teleopti.Ccc.Infrastructure.UnitOfWork;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class PeopleForShiftTradeFinder : IPeopleForShiftTradeFinder
	{
		private readonly ICurrentUnitOfWork _unitOfWork;

		public PeopleForShiftTradeFinder(ICurrentUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public IList<IPersonAuthorization> GetPeople(IPerson personFrom, DateOnly shiftTradeDate, IList<Guid> groupIdList,
			string name, NameFormatSetting nameFormat = NameFormatSetting.FirstNameThenLastName)
		{
			const string sql = "exec ReadModel.FindPeopleForShiftTrade @scheduleDate=:scheduleDate, "
							   + "@groupIdList=:groupIdList, @name=:name, "
							   + "@noSpaceInName = :noSpaceInName, "
							   + "@firstNameFirst = :firstNameFirst, "
							   + "@workflowControlSetId = :workflowControlSetId, "
							   + "@fromPersonId = :fromPersonId";

			var useChineseNameFormat = name != null && StringHelper.StringContainsChinese(name);
			var firstNameFirst = nameFormat == NameFormatSetting.FirstNameThenLastName;
			var personFromId = personFrom.Id.GetValueOrDefault();
			var workflowControlSetId = personFrom.WorkflowControlSet.Id.GetValueOrDefault();

			var result = new List<IPersonAuthorization>();

			var statelessSession = ((NHibernateUnitOfWork) _unitOfWork.Current()).Session.SessionFactory.OpenStatelessSession();
			groupIdList.Batch(100).ForEach(groupIdBatch =>
			{
				var groups = string.Join(",", groupIdBatch);
				var batchResult = statelessSession.CreateSQLQuery(sql)
					.SetDateOnly("scheduleDate", shiftTradeDate)
					.SetParameter("groupIdList", groups, NHibernateUtil.StringClob)
					.SetString("name", name)
					.SetBoolean("noSpaceInName", useChineseNameFormat)
					.SetBoolean("firstNameFirst", firstNameFirst)
					.SetGuid("workflowControlSetId", workflowControlSetId)
					.SetGuid("fromPersonId", personFromId)
					.SetResultTransformer(Transformers.AliasToBean(typeof(PersonSelectorShiftTrade)))
					.SetReadOnly(true)
					.List<IPersonAuthorization>();
				result.AddRange(batchResult);
			});

			return result;
		}


		public IList<IPersonAuthorization> GetPeople(IPerson personFrom, DateOnly shiftTradeDate, IList<Guid> peopleIdList)
		{
			const string sql = "exec ReadModel.FindPeopleForShiftTradeByPeopleIDs @scheduleDate=:scheduleDate,"
							+ "@peopleIdList=:peopleIdList,"
							+ "@workflowControlSetId = :workflowControlSetId,"
							+ "@fromPersonId = :fromPersonId";

		 
			var personFromId = personFrom.Id.GetValueOrDefault();
			var workflowControlSetId = personFrom.WorkflowControlSet.Id.GetValueOrDefault();

			var result = new List<IPersonAuthorization>();

			var statelessSession = ((NHibernateUnitOfWork)_unitOfWork.Current()).Session.SessionFactory.OpenStatelessSession();
			peopleIdList.Batch(2000).ForEach(groupIdBatch =>
			{
				var groups = string.Join(",", groupIdBatch);
				var batchResult = statelessSession.CreateSQLQuery(sql)
					.SetDateOnly("scheduleDate", shiftTradeDate)
					.SetParameter("peopleIdList", groups, NHibernateUtil.StringClob)
					.SetGuid("workflowControlSetId", workflowControlSetId)
					.SetGuid("fromPersonId", personFromId)
					.SetResultTransformer(Transformers.AliasToBean(typeof(PersonSelectorShiftTrade)))
					.SetReadOnly(true)
					.List<IPersonAuthorization>();
				result.AddRange(batchResult);
			});

			return result;
		}
	}
}
