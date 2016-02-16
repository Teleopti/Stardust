using System;

namespace Teleopti.Ccc.Domain.MessageBroker.Legacy
{
	/// <summary>
	/// Static class containing consts used in Message Brokern.
	/// </summary>
	/// <remarks>
	/// Created by: ankarlp
	/// Created date: 2008-08-07
	/// </remarks>
	public static class Consts
	{
		/// <summary>
		/// Default character encoding.
		/// </summary>
		public const string DefaultCharEncoding = "utf-32";
		/// <summary>
		/// Max Wire Length is 1024.
		/// </summary>
		public const int MaxWireLength = 1024;
		/// <summary>
		/// Separator, new line.
		/// </summary>
		public const char Separator = '\n';
		/// <summary>
		/// Min Date for SQL Server.
		/// </summary>
		public static readonly DateTime MinDate = new DateTime(1753, 1, 1, 12, 0, 0);
		/// <summary>
		/// Max Date for SQL Server.
		/// </summary>
		public static readonly DateTime MaxDate = new DateTime(9999, 12, 31, 11, 59, 59);
	}
}