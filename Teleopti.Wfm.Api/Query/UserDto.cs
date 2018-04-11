using System;

namespace Teleopti.Wfm.Api.Query
{
	public class UserDto
	{
		public Guid Id { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string Identity { get; set; }
		public string Email { get; set; }
	}

	public class UserByEmailDto : IQueryDto
	{
		public string Email { get; set; }
	}
}