using System;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject
{
	public class PayrollBaseExportDto : Dto
	{
		/// <summary>
		/// Gets or sets the employment number.
		/// </summary>
		[DataMember]
		public string EmploymentNumber { get; set; }

		/// <summary>
		/// Gets or sets the first name.
		/// </summary>
		[DataMember]
		public string FirstName { get; set; }

		/// <summary>
		/// Gets or sets the last name.
		/// </summary>
		[DataMember]
		public string LastName { get; set; }

		/// <summary>
		/// Gets or sets the Business Unit name.
		/// </summary>
		[DataMember]
		public string BusinessUnitName { get; set; }

		/// <summary>
		/// Gets or sets the site name.
		/// </summary>
		[DataMember]
		public string SiteName { get; set; }

		/// <summary>
		/// Gets or sets the team name.
		/// </summary>
		[DataMember]
		public string TeamName { get; set; }

		/// <summary>
		/// Gets or sets the Contract name.
		/// </summary>
		[DataMember]
		public string ContractName { get; set; }

		/// <summary>
		/// Gets or sets the Part-time percentage name.
		/// </summary>
		[DataMember]
		public string PartTimePercentageName { get; set; }

		/// <summary>
		/// Gets or sets the date.
		/// </summary>
		[DataMember]
		public DateTime Date { get; set; }

		/// <summary>
		/// Gets or sets the Part-time percentage number (XXXXX, last two is decimals) 
		/// </summary>
		[DataMember]
		public double PartTimePercentageNumber { get; set; }

		/// <summary>
		/// Gets or sets the payroll code
		/// </summary>
		[DataMember]
		public string PayrollCode { get; set; }

		/// <summary>
		/// Gets or sets the time
		/// </summary>
		[DataMember]
		public TimeSpan Time { get; set; }

		/// <summary>
		/// Gets or sets the start date.
		/// </summary>
		[DataMember]
		public DateTime StartDate { get; set; }

		/// <summary>
		/// Gets or sets the end date.
		/// </summary>
		[DataMember]
		public DateTime EndDate { get; set; }

		/// <summary>
		/// Gets or sets the shift category name.
		/// </summary>
		[DataMember]
		public string ShiftCategoryName { get; set; }

		/// <summary>
		/// Gets or sets the shift contract time.
		/// </summary>
		[DataMember]
		public TimeSpan ContractTime { get; set; }

		/// <summary>
		/// Gets or sets the work time.
		/// </summary>
		[DataMember]
		public TimeSpan WorkTime { get; set; }

		/// <summary>
		/// Gets or sets the paid time.
		/// </summary>
		[DataMember]
		public TimeSpan PaidTime { get; set; }

		/// <summary>
		/// Gets or sets the absence payroll code.
		/// </summary>
		[DataMember]
		public string AbsencePayrollCode { get; set; }

		/// <summary>
		/// Gets or sets the Day off payroll code.
		/// </summary>
		[DataMember]
		public string DayOffPayrollCode { get; set; }
	}
}
