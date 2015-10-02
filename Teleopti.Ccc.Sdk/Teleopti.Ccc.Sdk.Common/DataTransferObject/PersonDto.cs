using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject
{
	/// <summary>
	/// Represents a PersonDto object.
	/// </summary>
	[DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2010/04/")]
	[KnownType(typeof(PersonPeriodDto))]
	public class PersonDto : Dto
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="PersonDto"/> class.
		/// </summary>
		public PersonDto()
		{
			PersonPeriodCollection = new List<PersonPeriodDto>();
		}

		/// <summary>
		/// Gets or sets the name.
		/// </summary>
		/// <value>The name.</value>
		[DataMember]
		public string Name { get; set; }

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
		/// Gets or sets the culture language id.
		/// </summary>
		/// <value>The culture language id.</value>
		[DataMember]
		public int? CultureLanguageId { get; set; }

		/// <summary>
		/// Gets or sets the UI culture language id.
		/// </summary>
		/// <value>The UI culture language id.</value>
		[DataMember]
		public int? UICultureLanguageId { get; set; }

		/// <summary>
		/// Gets or sets the name of the application log on.
		/// </summary>
		/// <value>The name of the application log on.</value>
		[DataMember]
		public string ApplicationLogOnName { get; set; }

		/// <summary>
		/// Gets or sets the person period collection.
		/// </summary>
		/// <value>The person period collection.</value>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly"), DataMember]
		public IList<PersonPeriodDto> PersonPeriodCollection { get; set; }

		/// <summary>
		/// Gets or sets the time zone id.
		/// </summary>
		/// <value>The time zone id.</value>
		[DataMember]
		public string TimeZoneId { get; set; }

		/// <summary>
		/// Gets or sets the workflow control set.
		/// </summary>
		/// <value>The workflow control set.</value>
		[DataMember(IsRequired = false, Order = 1)]
		public WorkflowControlSetDto WorkflowControlSet { get; set; }

		/// <summary>
		/// Gets or sets the first name.
		/// </summary>
		/// <value>The first name.</value>
		[DataMember(IsRequired = false, Order = 2)]
		public string FirstName { get; set; }

		/// <summary>
		/// Gets or sets the last name.
		/// </summary>
		/// <value>The last name.</value>
		[DataMember(IsRequired = false, Order = 3)]
		public string LastName { get; set; }

		/// <summary>
		/// Gets or sets the name of the windows log on.
		/// </summary>
		/// <value>The name of the windows log on.</value>
		[DataMember(IsRequired = false, Order = 4)]
		[Obsolete(@"Use Identity instead in the format DOMAIN\USER if a windows identity.")]
		public string WindowsLogOnName { get; set; }

		/// <summary>
		/// Gets or sets the windows domain.
		/// </summary>
		/// <value>The windows domain.</value>
		[DataMember(IsRequired = false, Order = 5)]
		[Obsolete(@"Use Identity instead in the format DOMAIN\USER if a windows identity.")]
		public string WindowsDomain { get; set; }

		/// <summary>
		/// Gets or sets the application log on password.
		/// </summary>
		/// <value>The application log on password.</value>
		[DataMember(IsRequired = false, Order = 6)]
		public string ApplicationLogOnPassword { get; set; }

		/// <summary>
		/// Gets or sets the note.
		/// </summary>
		/// <value>The note.</value>
		[DataMember(IsRequired = false, Order = 6)]
		public string Note { get; set; }

		/// <summary>
		/// Gets or sets the termination date.
		/// </summary>
		/// <value>The termination date.</value>
		[DataMember(IsRequired = false, Order = 6)]
		public DateOnlyDto TerminationDate { get; set; }

		/// <summary>
		/// Gets or sets deleted flag.
		/// </summary>
		[DataMember(IsRequired = false, Order = 7)]
		public bool IsDeleted { get; set; }

		/// <summary>
		/// Gets or sets the Identity used in Windows, SSO, logon.
		/// </summary>
		/// <value>The Identity.</value>
		[DataMember(IsRequired = false, Order = 8)]
		public string Identity { get; set; }

		/// <summary>
		/// Gets or sets the first day of week.
		/// </summary>
		/// <value>The first day of week.</value>
		[DataMember(IsRequired = false, Order = 9)]
		public DayOfWeek FirstDayOfWeek { get; set; }
	}

}