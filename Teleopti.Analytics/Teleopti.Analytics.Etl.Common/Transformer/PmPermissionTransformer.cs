using System;
using System.Collections.Generic;
using System.Data;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;
using Teleopti.Analytics.Etl.Common.Interfaces.PerformanceManager;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;


namespace Teleopti.Analytics.Etl.Common.Transformer
{
	public class PmPermissionTransformer : IPmPermissionTransformer
	{
		public IList<PmUser> GetUsersWithPermissionsToPerformanceManager(IList<IPerson> personCollection, IPmPermissionExtractor permissionExtractor, IUnitOfWorkFactory unitOfWorkFactory)
		{
			InParameter.NotNull("personCollection", personCollection);

			IList<PmUser> pmUserCollection = new List<PmUser>();

			foreach (IPerson person in personCollection)
			{

				IList<PmUser> personUsers = new List<PmUser>();

				foreach (IApplicationRole applicationRole in person.PermissionInformation.ApplicationRoleCollection)
				{
					if (applicationRole is IDeleteTag deleteTag && !deleteTag.IsDeleted)
					{
						personUsers.Add(getPermission(person.Id.GetValueOrDefault(), applicationRole, permissionExtractor, unitOfWorkFactory));
					}
				}

				PmUser pmUser = getMostGenerousPermissions(personUsers);

				if (pmUser != null)
				{
					// Current user belongs to at least one role with permissions to PM
					pmUserCollection.Add(pmUser);
				}

			}

			return pmUserCollection;
		}

		private static PmUser getMostGenerousPermissions(IEnumerable<PmUser> personUsers)
		{
			PmUser returnUser = null;

			foreach (var personUser in personUsers)
			{
				if (personUser == null)
					continue;

				if (returnUser == null || returnUser.AccessLevel < personUser.AccessLevel)
					returnUser = personUser;
			}

			return returnUser;
		}

		private static PmUser getPermission(Guid? id, IApplicationRole applicationRole, IPmPermissionExtractor permissionExtractor, IUnitOfWorkFactory unitOfWorkFactory)
		{
			PmUser pmUser = null;
			if (id == null)
				return null;

			PmPermissionType permissionLevel = permissionExtractor.ExtractPermission(applicationRole.ApplicationFunctionCollection, unitOfWorkFactory);

			if (permissionLevel != PmPermissionType.None)
			{
				// AccessLevel/permissionLevel: 
				// 0 = PM report permission denied 
				// 1 = Permission to view PM reports
				// 2 = Permission to create and view PM reports
				pmUser = new PmUser { PersonId = id.Value, AccessLevel = (int)permissionLevel };
			}
			return pmUser;
		}

		public void Transform(IEnumerable<PmUser> rootList, DataTable table)
		{
			InParameter.NotNull("rootList", rootList);
			InParameter.NotNull("table", table);

			foreach (var user in rootList)
			{
				DataRow row = table.NewRow();
				row["person_id"] = user.PersonId;
				row["access_level"] = user.AccessLevel;
				table.Rows.Add(row);
			}
		}

	}
}