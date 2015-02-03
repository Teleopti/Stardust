using System.Collections.Generic;

// NOTE!!! DO NOT CHANGE NAMESPACE!
namespace Teleopti.Analytics.PM.PMServiceHost   // NOTE!!! DO NOT CHANGE NAMESPACE!
// NOTE!!! DO NOT CHANGE NAMESPACE!
{
	public class ResultDto
	{
		private readonly List<UserDto> _usersInserted = new List<UserDto>();
		private readonly List<UserDto> _usersUpdated = new List<UserDto>();
		private readonly List<UserDto> _usersDeleted = new List<UserDto>();
		private readonly List<UserDto> _validAnalyzerUsers = new List<UserDto>();

		public bool Success { get; set; }

		public List<UserDto> UsersInserted
		{
			get
			{
				return _usersInserted;
			}
		}

		public List<UserDto> UsersUpdated
		{
			get { return _usersUpdated; }
		}

		public List<UserDto> UsersDeleted
		{
			get { return _usersDeleted; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
		public List<UserDto> ValidAnalyzerUsers
		{
			get { return _validAnalyzerUsers; }
		}

		public int AffectedUsersCount { get; set; }

		public string ErrorMessage { get; set; }

		public string ErrorType { get; set; }

		public bool IsWindowsAuthentication { get; set; }

		public void AddRangeUsersInserted(IEnumerable<UserDto> users)
		{
			_usersInserted.AddRange(users);
		}

		public void AddRangeUsersUpdated(IEnumerable<UserDto> users)
		{
			_usersUpdated.AddRange(users);
		}

		public void AddRangeUsersDeleted(IEnumerable<UserDto> users)
		{
			_usersDeleted.AddRange(users);
		}
	}
}