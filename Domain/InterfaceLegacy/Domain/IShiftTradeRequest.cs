using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// Request for shift trade
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2008-10-10
    /// </remarks>
    public interface IShiftTradeRequest : IRequest
    {
        /// <summary>
        /// Gets the shift trade swap details.
        /// </summary>
        /// <value>The shift trade swap details.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2009-09-01
        /// </remarks>
        ReadOnlyCollection<IShiftTradeSwapDetail> ShiftTradeSwapDetails { get; }

		 /// <summary>
		 /// reference this offer to shift trade request when it come from bulletin board
		 /// </summary>
	    IShiftExchangeOffer Offer { get; set; }

	    /// <summary>
        /// Adds the shift trade swap detail.
        /// </summary>
        /// <param name="shiftTradeSwapDetail">The shift trade swap detail.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2009-09-03
        /// </remarks>
        void AddShiftTradeSwapDetail(IShiftTradeSwapDetail shiftTradeSwapDetail);

        /// <summary>
        /// Clears the shift trade swap details.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2009-09-03
        /// </remarks>
        void ClearShiftTradeSwapDetails();

        /// <summary>
        /// Gets the shift trade status.
        /// </summary>
        /// <param name="shiftTradeRequestStatusChecker">The shift trade request status checker.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2009-09-04
        /// </remarks>
        ShiftTradeStatus GetShiftTradeStatus(IShiftTradeRequestStatusChecker shiftTradeRequestStatusChecker);

        /// <summary>
        /// Gets the shift trade status.
        /// </summary>
        /// <value>The shift trade status.</value>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-09-19
        /// </remarks>
		void SetShiftTradeStatus(ShiftTradeStatus shiftTradeStatusToSet, IPersonRequestCheckAuthorization authorization);

        /// <summary>
        /// Gets all the people involved in this shift trade
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 6/1/2010
        /// </remarks>
        IEnumerable<IPerson> InvolvedPeople();

        ///<summary>
        ///Manually notify the person that should receive the shift trade request that it's available (only for use when saving in status mode New first, and then perform validation in another unit of work).
        ///</summary>
        void NotifyToPersonAfterValidation();

		/// <summary>
		/// Accepts this instance.
		/// </summary>
		/// <param name="acceptingPerson">The accepting person.</param>
		/// <param name="shiftTradeRequestSetChecksum">The shift trade request set checksum instance.</param>
		/// <param name="authorization">The authorization checker.</param>
		/// <remarks>
		/// Created by: peterwe
		/// Created date: 2009-06-17
		/// </remarks>
		void Accept(IPerson acceptingPerson, IShiftTradeRequestSetChecksum shiftTradeRequestSetChecksum, IPersonRequestCheckAuthorization authorization);


		/// <summary>
		/// Refers this instance.
		/// </summary>
		/// <remarks>
		/// Created by: henryg
		/// Created date: 2009-08-28
		/// </remarks>
		void Refer(IPersonRequestCheckAuthorization authorization);
	}
}