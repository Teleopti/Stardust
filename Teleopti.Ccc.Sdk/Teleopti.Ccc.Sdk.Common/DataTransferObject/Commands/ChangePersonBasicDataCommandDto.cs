using System;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands
{
	/// <summary>
	/// This command change person basic data.
	/// </summary>
	[DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2011/10/")]
	public class ChangePersonBasicDataCommandDto : CommandDto
	{
		/// <summary>
		/// Gets or sets the mandatory person id.
		/// </summary>
		/// <value>The person id.</value>
		[DataMember]
		public Guid PersonId { get; set; }
		/// <summary>
		/// Gets or sets the first name.
		/// </summary>
		/// <value>The first name.</value>
		[DataMember]
		public string FirstName { get; set; }
		/// <summary>
		/// Gets or sets the last name.
		/// </summary>
		/// <value>The last name.</value>
		[DataMember]
		public string LastName { get; set; }
		/// <summary>
		/// Gets or sets the email.
		/// </summary>
		/// <value>The email.</value>
		[DataMember]
		public string Email { get; set; }
		/// <summary>
		/// Gets or sets the employment number.
		/// </summary>
		/// <value>The employment number.</value>
		[DataMember]
		public string EmploymentNumber { get; set; }
		/// <summary>
		/// Gets or sets the culture id.
		/// </summary>
		/// <value>The culture id.</value>
		[DataMember]
		public int? CultureLanguageId { get; set; }
		/// <summary>
		/// Gets or sets the ui culture id.
		/// </summary>
		/// <value>The ui culture id.</value>
		[DataMember]
		public int? UICultureLanguageId { get; set; }
		/// <summary>
		/// Gets or sets the note.
		/// </summary>
		/// <value>The note.</value>
		[DataMember]
		public string Note { get; set; }
		/// <summary>
		/// Gets or sets the first day of week.
		/// </summary>
		/// <value>The first day of week.</value>
		[DataMember]
		public DayOfWeek? WorkWeekStart { get; set; }
		/// <summary>
		/// Gets or sets the workflow control set id.
		/// </summary>
		/// <value>The workflow control set id.</value>
		[DataMember]
		public Guid? WorkflowControlSetId { get; set; }
		/// <summary>
		/// Gets or sets deleted flag.
		/// </summary>
		/// <value>The deleted flag.</value>
		[DataMember]
		public bool IsDeleted { get; set; }
	}
}