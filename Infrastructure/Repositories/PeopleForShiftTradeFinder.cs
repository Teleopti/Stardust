using System;
using System.Collections.Generic;
using System.Security.Claims;
using NHibernate;
using NHibernate.Impl;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Domain.Util;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;

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
							   + "@groupIdList=:groupIdList, @businessUnitId=:businessUnitId, @name=:name, "
							   + "@noSpaceInName = :noSpaceInName, "
							   + "@firstNameFirst = :firstNameFirst, "
							   + "@workflowControlSetId = :workflowControlSetId, "
							   + "@fromPersonId = :fromPersonId, "
							   + "@shiftStartUTC = :shiftStartUTC, "
							   + "@shiftEndUTC = :shiftEndUTC";

			var useChineseNameFormat = name != null && StringHelper.StringContainsChinese(name);
			var firstNameFirst = nameFormat == NameFormatSetting.FirstNameThenLastName;
			var personFromId = personFrom.Id.GetValueOrDefault();
			var workflowControlSetId = personFrom.WorkflowControlSet.Id.GetValueOrDefault();

			var result = new List<IPersonAuthorization>();

			//ROBTODO: Temporary - Person From Shift Period is currently not being passed to Get People, this is currently not being used by the query,
			//	but is a requested parameter to add for further optimisation
			var dummyDateTimePeriod = new DateTimePeriod(DateTime.Today.ToUniversalTime(), DateTime.Now.ToUniversalTime());
			var statelessSession = ((NHibernateUnitOfWork)_unitOfWork.Current()).Session.SessionFactory.OpenStatelessSession();
			groupIdList.Batch(100).ForEach(groupIdBatch =>
			{
				var groupIdString = string.Join(",", groupIdBatch);
				var batchResult = statelessSession.CreateSQLQuery(sql)
					.SetDateOnly("scheduleDate", shiftTradeDate)
					.SetParameter("groupIdList", groupIdString, NHibernateUtil.StringClob)
					.SetString("name", name)
					.SetBoolean("noSpaceInName", useChineseNameFormat)
					.SetBoolean("firstNameFirst", firstNameFirst)
					.SetGuid("businessUnitId", getBusinessUnitId())
					.SetGuid("workflowControlSetId", workflowControlSetId)
					.SetGuid("fromPersonId", personFromId)
					.SetDateTime("shiftStartUTC", dummyDateTimePeriod.StartDateTime)
					.SetDateTime("shiftEndUTC", dummyDateTimePeriod.EndDateTime)
					.SetResultTransformer(Transformers.AliasToBean(typeof(PersonSelectorShiftTrade)))
					.SetReadOnly(true)
					.List<IPersonAuthorization>();

				result.AddRange(batchResult);
			});
			return result;
		}

		private Guid getBusinessUnitId()
		{
			var filter = (FilterImpl)_unitOfWork.Session().GetEnabledFilter("businessUnitFilter");
			return !filter.Parameters.TryGetValue("businessUnitParameter", out object businessUnitId)
				? ((ITeleoptiIdentity) ClaimsPrincipal.Current.Identity).BusinessUnit.Id.GetValueOrDefault()
				: Guid.Parse(businessUnitId.ToString());
		}
	}
}
