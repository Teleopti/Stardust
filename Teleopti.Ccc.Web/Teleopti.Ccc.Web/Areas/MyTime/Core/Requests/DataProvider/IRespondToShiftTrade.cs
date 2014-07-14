using System;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;

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
		/// <returns>An updated view model</returns>
		RequestViewModel OkByMe(Guid requestId, string message);

		/// <summary>
		/// Rejects the shifttrade request
		/// </summary>
		/// <param name="requestId">The id of the request</param>
		/// <param name="message"></param>
		/// <returns>An updated view model</returns>
		RequestViewModel Deny(Guid requestId, string message);

		/// <summary>
		/// Resends the shifttrade that has been referred
		/// </summary>
		/// <param name="requestId"></param>
		/// <returns>An update viewmodel</returns>
		RequestViewModel ResendReferred(Guid requestId);
	}
}