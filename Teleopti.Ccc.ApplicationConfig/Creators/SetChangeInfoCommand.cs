using System;
using System.Reflection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.ApplicationConfig.Creators
{
	public class SetChangeInfoCommand
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
		public void Execute<T>(T item, IPerson person)
		{
			DateTime nu = DateTime.UtcNow;

			typeof(T)
				.GetField("_updatedBy", BindingFlags.NonPublic | BindingFlags.Instance)
				.SetValue(item, person);
			typeof(T)
				.GetField("_updatedOn", BindingFlags.NonPublic | BindingFlags.Instance)
				.SetValue(item, nu);
		}
	}
}