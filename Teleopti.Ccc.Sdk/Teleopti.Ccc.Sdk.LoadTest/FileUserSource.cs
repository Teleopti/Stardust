using System;
using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Ccc.Sdk.LoadTest
{
	public class FileUserSource : IUserSource
	{
		private readonly IEnumerable<IUser> _users;

		public FileUserSource(string file)
		{
			_users = ReadFile(file);
		}

		private static IEnumerable<IUser> ReadFile(string file)
		{
			var lines = System.IO.File.ReadAllLines(file);
			var users = from l in lines 
			            let lineArr = l.Split(' ')
			            let userName = lineArr.ElementAt(0)
			            let password = lineArr.ElementAt(1)
			            select new User(userName, password);
			return users.ToArray();
		}

		public IUser GetUser(int testNumber)
		{
            var random = new Random((int) (testNumber ^ DateTime.UtcNow.Ticks));
			var userNumber = random.Next(0, _users.Count() - 1);
			return _users.ElementAt(userNumber);
		}
	}
}