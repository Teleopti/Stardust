using System;
using Teleopti.Ccc.Domain.Staffing;

namespace Teleopti.Ccc.Infrastructure.Repositories
{

	public class RemoveDeletedStaffingHeads : IRemoveDeletedStaffingHeads
	{
		private readonly UpdateStaffingLevelReadModelStartDate _updateStaffingLevelReadModelStartDate;

		public RemoveDeletedStaffingHeads(UpdateStaffingLevelReadModelStartDate updateStaffingLevelReadModelStartDate)
		{
			_updateStaffingLevelReadModelStartDate = updateStaffingLevelReadModelStartDate;
		}

		public DateTime GetStartDate(DateTime dateTime)
		{
			return _updateStaffingLevelReadModelStartDate.StartDateTime;
		}
	}

	public interface IRemoveDeletedStaffingHeads
	{
		DateTime GetStartDate(DateTime dateTime);
	}
}