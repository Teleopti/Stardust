using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Teleopti.Analytics.PM.PMServiceHost
{
	[DataContract]
	public class ResultDto
	{
		private readonly List<UserDto> _usersInserted = new List<UserDto>();
		private readonly List<UserDto> _usersUpdated = new List<UserDto>();
		private readonly List<UserDto> _usersDeleted = new List<UserDto>();
		private readonly List<UserDto> _validAnalyzerUsers = new List<UserDto>();

		[DataMember]
		public bool Success { get; set; }

		[DataMember]
		public List<UserDto> UsersInserted
		{
			get
			{
				return _usersInserted;
			}
		}

		[DataMember]
		public List<UserDto> UsersUpdated
		{
			get { return _usersUpdated; }
		}

		[DataMember]
		public List<UserDto> UsersDeleted
		{
			get { return _usersDeleted; }
		}

		[DataMember]
		public List<UserDto> ValidAnalyzerUsers
		{
			get { return _validAnalyzerUsers; }
		}

		[DataMember]
		public int AffectedUsersCount { get; set; }

		[DataMember]
		public string ErrorMessage { get; set; }

		[DataMember]
		public string ErrorType { get; set; }

		[DataMember]
		public bool IsWindowsAuthentication { get; set; }

		public void AddUsersInserted(UserDto user)
		{
			_usersInserted.Add(user);
		}

		public void AddUsersUpdated(UserDto user)
		{
			_usersUpdated.Add(user);
		}

		public void AddUsersDeleted(UserDto user)
		{
			_usersDeleted.Add(user);
		}

		public void AddValidAnalyzerUser(UserDto user)
		{
			_validAnalyzerUsers.Add(user);
		}
	}
}