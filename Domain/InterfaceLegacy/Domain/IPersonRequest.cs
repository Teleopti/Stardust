using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// Request from a person
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2008-10-10
    /// </remarks>
    public interface IPersonRequest : ICloneableEntity<IPersonRequest>, 
                                        IAggregateRootBrokerConditions,
                                        IOriginator<IPersonRequest>,
                                        IChangeInfo,
                                        INotifyPropertyChanged,
																				ICreateInfo,
										IFilterOnBusinessUnit
    {
        /// <summary>
        /// Gets the date.
        /// </summary>
        /// <value>The date.</value>
        DateTime RequestedDate { get; }

        /// <summary>
        /// Gets the person.
        /// </summary>
        /// <value>The person.</value>
        IPerson Person { get; }

        /// <summary>
        /// Tries the set message and returns the result.
        /// </summary>
        /// <returns></returns>
        bool TrySetMessage(string message);

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>The message.</value>
        string GetMessage(ITextFormatter formatter);

        /// <summary>
        /// Tries the set broken business rules and returns the result.
        /// </summary>
        /// <returns></returns>
        bool TrySetBrokenBusinessRule(BusinessRuleFlags brokenRules);

		/// <summary>
		/// Gets broken business rule
		/// </summary>
		/// <returns></returns>
		BusinessRuleFlags? BrokenBusinessRules { get; }

        /// <summary>
        /// Gets or sets the request.
        /// </summary>
        /// <value>The current request.</value>
        IRequest Request { get; set; }

        /// <summary>
        /// Gets or sets the subject of the message.
        /// </summary>
        /// <value>The subject.</value>
        string Subject { set; }

    	/// <summary>
    	/// Get the formatted subject
    	/// </summary>
    	/// <param name="formatter"></param>
    	/// <returns></returns>
    	string GetSubject(ITextFormatter formatter);

        /// <summary>
        /// Gets a value indicating whether this instance is editable.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is editable; otherwise, <c>false</c>.
        /// </value>
        bool IsEditable { get; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="IPersonRequest"/> is changed and should be saved.
        /// </summary>
        /// <value><c>true</c> if changed; otherwise, <c>false</c>.</value>
        bool Changed { get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance is new.
        /// </summary>
        /// <value><c>true</c> if this instance is new; otherwise, <c>false</c>.</value>
        bool IsNew { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is pending.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is pending; otherwise, <c>false</c>.
        /// </value>
        bool IsPending { get; }

		/// <summary>
        /// Gets a value indicating whether this instance is denied.
        /// </summary>
        /// <value><c>true</c> if this instance is denied; otherwise, <c>false</c>.</value>
        bool IsDenied { get; }

		bool IsDeleted { get; }

		/// <summary>
		/// Gets a value indicating whether this instance is auto denied by system rules.
		/// </summary>
		/// <value><c>true</c> if this instance is auto denied; otherwise, <c>false</c>.</value>
		bool IsAutoDenied { get; }

		bool IsWaitlisted { get; }

		bool IsCancelled { get; }
			
        /// <summary>
        /// Gets a value indicating whether this instance is approved.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is approved; otherwise, <c>false</c>.
        /// </value>
        bool IsApproved { get; }

		/// <summary>
		/// Get a value indicating whether this instance is auto approved by service bus.
		/// </summary>
		/// <value><c>true</c> if this instance is auto approved; otherwise, <c>false</c>.</value>
		bool IsAutoAproved { get; }

        /// <summary>
        /// Gets the status text.
        /// </summary>
        /// <value>The status text.</value>
        string StatusText { get; }

		/// <summary>
		/// Gets the server time for when item was last updated 
		/// </summary>
		/// <value>Time for when item was updated on server</value>
		DateTime UpdatedOnServerUtc { get; }

        IList<IBusinessRuleResponse> Approve(IRequestApprovalService approvalService, IPersonRequestCheckAuthorization authorization, bool isAutoGrant = false);

	    void Deny(string denyReasonTextResourceKey, IPersonRequestCheckAuthorization authorization, IPerson denyPerson = null, PersonRequestDenyOption personRequestDenyOption = PersonRequestDenyOption.None);

        /// <summary>
        /// Answers the specified message.
        /// </summary>
        /// <param name="answerMessage">The answer message.</param>
        bool Reply(string answerMessage);

        /// <summary>
        /// Checks the length of the reply.
        /// </summary>
        /// <param name="answerMessage">The answer message.</param>
        /// <returns></returns>
        bool CheckReplyTextLength(string answerMessage);

        /// <summary>
        /// Sets the status to pending IF it was pending originally.
        /// </summary>
        void Pending();

        /// <summary>
        /// Forces the status to pending.
        /// </summary>
        void ForcePending();

        /// <summary>
        /// Tells this PersonRequest that it has been persisted.
        /// </summary>
        void Persisted();

        /// <summary>
        /// Gets the deny reason.
        /// </summary>
        /// <value>The deny reason.</value>
        string DenyReason { get; }

        ///<summary>
        /// Set the status to new to be able to perform validation after edits.
        ///</summary>
        void SetNew();

	    void Cancel (IPersonRequestCheckAuthorization authorization);

		bool IsAlreadyAbsent { get; }

		bool IsExpired { get; }

		bool InsufficientPersonAccount { get; }

	}
}