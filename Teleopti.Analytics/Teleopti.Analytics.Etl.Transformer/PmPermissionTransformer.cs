using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using log4net;
using Teleopti.Analytics.Etl.Interfaces.Common;
using Teleopti.Analytics.Etl.Interfaces.PerformanceManager;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Analytics.Etl.PerformanceManagerProxy;
using Teleopti.Analytics.PM.PMServiceHost;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using IJobResult = Teleopti.Analytics.Etl.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.Transformer
{
	public class PmPermissionTransformer : IPmPermissionTransformer
	{
		private readonly IPmProxyFactory _pmProxyFactory;
		readonly ILog _logger = LogManager.GetLogger(typeof(PmPermissionTransformer));

		private PmPermissionTransformer() { }

		public PmPermissionTransformer(IPmProxyFactory pmProxyFactory)
			: this()
		{
			_pmProxyFactory = pmProxyFactory;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public IList<UserDto> GetUsersWithPermissionsToPerformanceManager(IList<IPerson> personCollection, bool filterWindowsLogOn, IPmPermissionExtractor permissionExtractor, IUnitOfWorkFactory unitOfWorkFactory)
		{
			InParameter.NotNull("personCollection", personCollection);

			IList<UserDto> userDtoCollection = new List<UserDto>();

			foreach (IPerson person in personCollection)
			{
				string logOnName = filterWindowsLogOn ? getPersonWindowsLogOn(person) : getPersonApplicationLogOn(person);

				if (logOnName == null)
				{
					// User has no valid windows or application authetication info - no need to extract PM permission info
					continue;
				}

				IList<UserDto> personUsers = new List<UserDto>();

				foreach (IApplicationRole applicationRole in person.PermissionInformation.ApplicationRoleCollection)
				{
					var deleteTag = applicationRole as IDeleteTag;
					if (deleteTag != null && !deleteTag.IsDeleted)
					{
						personUsers.Add(getPermission(logOnName, applicationRole, filterWindowsLogOn, permissionExtractor, unitOfWorkFactory));
					}
				}

				UserDto userDto = getMostGenerousPermissions(personUsers);

				if (userDto != null)
				{
					// Current user has valid logon credentials and belongs to at least one role with permissions to PM
					userDtoCollection.Add(userDto);
				}

			}

			return userDtoCollection;
		}

		private static UserDto getMostGenerousPermissions(IEnumerable<UserDto> personUsers)
		{
			UserDto returnUser = null;

			foreach (var personUser in personUsers)
			{
				if (personUser == null)
					continue;

				if (returnUser == null || returnUser.AccessLevel < personUser.AccessLevel)
					returnUser = personUser;
			}

			return returnUser;
		}

		private static UserDto getPermission(string logOnName, IApplicationRole applicationRole, bool isWindowsLogOn, IPmPermissionExtractor permissionExtractor, IUnitOfWorkFactory unitOfWorkFactory)
		{
			UserDto userDto = null;
			if (string.IsNullOrEmpty(logOnName))
				return null;

			PmPermissionType permissionLevel = permissionExtractor.ExtractPermission(applicationRole.ApplicationFunctionCollection, unitOfWorkFactory);

			if (permissionLevel != PmPermissionType.None)
			{
				// AccessLevel/permissionLevel: 
				// 0 = PM report permission denied 
				// 1 = Permission to view PM reports
				// 2 = Permission to create and view PM reports
				userDto = new UserDto { UserName = logOnName, AccessLevel = (int)permissionLevel, IsWindowsLogOn = isWindowsLogOn };
			}
			return userDto;
		}

		private static string getPersonWindowsLogOn(IPerson person)
		{
			if (person.AuthenticationInfo == null) return null;
			return !person.AuthenticationInfo.Identity.Contains(@"\") ? null : person.AuthenticationInfo.Identity;
		}

		private static string getPersonApplicationLogOn(IPerson person)
		{
			if (person.ApplicationAuthenticationInfo == null)
				return null;

			return person.ApplicationAuthenticationInfo.ApplicationLogOnName;
		}

		public ResultDto SynchronizeUserPermissions(IList<UserDto> users, string olapServer, string olapDatabase)
		{
			InParameter.NotNull("users", users);

			ResultDto resultDto;
			IPmProxy proxy = _pmProxyFactory.CreateProxy();
			try
			{
				proxy.ResetUserLists();
				int batchCounter = 0;
				foreach (IEnumerable<UserDto> userDtos in users.Batch(100))
				{
					batchCounter += 1;
					_logger.DebugFormat("Before batch call number {0} (usersCount:{1}) to proxy.AddUsersToSynchronize(users.ToArray()).", batchCounter, users.Count);
					ResultDto batchResult = proxy.AddUsersToSynchronize(userDtos.ToArray());
					_logger.DebugFormat("After batch call number {0} (usersCount:{1}) to proxy.AddUsersToSynchronize(users.ToArray()).", batchCounter, users.Count);
					if (!batchResult.Success)
						return batchResult;
				}

				_logger.Debug("Before call to proxy.SynchronizeUsers(olapServer, olapDatabase).");
				resultDto = proxy.SynchronizeUsers(olapServer, olapDatabase);
				_logger.Debug("After call to proxy.SynchronizeUsers(olapServer, olapDatabase).");

				proxy.ResetUserLists();
				proxy.Close();
			}
			catch (TimeoutException timeoutException)
			{
				var message = new StringBuilder();
				message.Append("Timeout expired when trying to synchronize PM users. This is probably caused by too many users having permissions to Performance Manager. ");
				message.Append("The solution could be to reduce the number of users with such permissions and try to re-run the ETL Tool Permission job. ");
				message.Append("You can also try to increase the sendTimeout value, in the config file, for the binding named WsHttpBinding_IPMService. ");
				message.Append("Make that change in config file for both ETL Tool and ETL Service. ");
				message.Append("Another possible cause of the timeout is PM users with invalid Windows credentials entered in the Teleopti WFM People module.");
				throw new PmSynchronizeException(message.ToString(), timeoutException);
			}
			catch (Exception e)
			{
				_logger.ErrorFormat("Error trying to Synchronize users. Error message: {0}", e.Message);
				throw;
			}
			finally
			{
				_logger.Debug("In finally, before doing proxy.Abort() in SynchronizeUserPermissions");
				proxy.Abort();
			}

			return resultDto;
		}

		public ResultDto IsPmWindowsAuthenticated(string olapServer, string olapDatabase)
		{
			ResultDto resultDto;
			IPmProxy proxy = _pmProxyFactory.CreateProxy();
			try
			{
				_logger.Debug("Before call to proxy.IsWindowsAuthentication(olapServer, olapDatabase).");
				resultDto = proxy.IsWindowsAuthentication(olapServer, olapDatabase);
				_logger.Debug("After call to proxy.IsWindowsAuthentication(olapServer, olapDatabase).");
				proxy.Close();
			}
			catch (Exception e)
			{
				_logger.ErrorFormat("Error trying to call IsPmWindowsAuthenticated. Error message: {0}", e.Message);
				throw;
			}
			finally
			{
				_logger.Debug("In finally, before doing proxy.Abort() in IsPmWindowsAuthenticated");
				proxy.Abort();
			}

			return resultDto;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void Transform(IEnumerable<UserDto> rootList, DataTable table)
		{
			InParameter.NotNull("rootList", rootList);
			InParameter.NotNull("table", table);

			foreach (UserDto user in rootList)
			{
				DataRow row = table.NewRow();
				row["user_name"] = user.UserName;
				row["is_windows_logon"] = user.IsWindowsLogOn;
				table.Rows.Add(row);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
		public ResultDto JoinResults(IList<ResultDto> resultDtos)
		{
			InParameter.NotNull("resultDtos", resultDtos);
			var joinedResult = new ResultDto { Success = true };

			foreach (ResultDto resultDto in resultDtos)
			{
				joinedResult.AddRangeUsersInserted(resultDto.UsersInserted);
				joinedResult.AddRangeUsersUpdated(resultDto.UsersUpdated);
				joinedResult.AddRangeUsersDeleted(resultDto.UsersDeleted);
				joinedResult.AffectedUsersCount += resultDto.AffectedUsersCount;
				joinedResult.Success = resultDto.Success;
			}

			return joinedResult;
		}
	}
}