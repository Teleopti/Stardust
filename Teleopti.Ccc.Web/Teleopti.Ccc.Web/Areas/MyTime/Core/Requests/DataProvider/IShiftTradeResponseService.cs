using System;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider
{
	/// <summary>
	/// Handles response of shifttrades
	/// </summary>
	public interface IShiftTradeResponseService
	{
		/// <summary>
		/// Ok the shifttrade request
		/// </summary>
		/// <param name="id">The id of the shifttrade</param>
		void OkByMe(Guid id);
		
		/// <summary>
		/// Denies the shifttrade request
		/// </summary>
		/// <param name="id">The id of the shifttrade</param>
		void Reject(Guid id);
	}
}