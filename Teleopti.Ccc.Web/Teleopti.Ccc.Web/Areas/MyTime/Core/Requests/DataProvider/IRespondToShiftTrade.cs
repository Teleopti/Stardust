using System;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider
{
	/// <summary>
	/// Handles response of shifttrades
	/// </summary>
	/// <remarks>
	/// Accepting or denying by the other person (the targetperson of the shifttrade)
	/// </remarks>
	public interface IRespondToShiftTrade
	{
		/// <summary>
		/// Respond OK to a shifttraderequest
		/// </summary>
		/// <param name="requestId">The id of the request</param>
		void OkByMe(Guid requestId);
		
		/// <summary>
		/// Rejects the shifttrade request
		/// </summary>
		/// <param name="requestId">The id of the request</param>
		void Deny(Guid requestId);
	}
}