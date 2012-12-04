﻿using System;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos
{
	/// <summary>
	/// Query to get a collection of <see cref="SchedulePartDto"/> for one <see cref="PersonDto"/>.
	/// </summary>
	[DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2012/04/")]
	public class GetSchedulesByPersonQueryDto : QueryDto
	{
		/// <summary>
		/// Gets or sets the mandatory person id.
		/// </summary>
		[DataMember]
		public Guid PersonId { get; set; }

		/// <summary>
		/// Gets or sets the scenario id. Setting the scenario id to null will use default scenario.
		/// </summary>
		[DataMember]
		public Guid? ScenarioId { get; set; }

		/// <summary>
		/// Gets or sets the mandatory end date.
		/// </summary>
		[DataMember]
		public DateOnlyDto EndDate { get; set; }

		/// <summary>
		/// Gets or sets the mandatory start date.
		/// </summary>
		[DataMember]
		public DateOnlyDto StartDate { get; set; }

		/// <summary>
		/// Gets or sets the mandatory time zone id.
		/// </summary>
		[DataMember]
		public string TimeZoneId { get; set; }

		/// <summary>
		/// Gets or sets the optional special projection.
		/// </summary>
		[DataMember]
		public string SpecialProjection { get; set; }
	}
}