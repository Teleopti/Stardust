using System;

namespace Teleopti.Ccc.Infrastructure.Repositories.Stardust
{
	public class JobFilterModel
	{
		public int From;
		public int To;
		public string DataSource;
		public string Type;
		public DateTime? FromDate;
		public DateTime? ToDate;
	}
}