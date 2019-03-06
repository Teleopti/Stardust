using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Analytics.Etl.Common.Transformer
{
	//public class RequestTransformer : IEtlTransformer<IPersonRequest>
	public class RequestTransformer : IPersonRequestTransformer<IPersonRequest>
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		public void Transform(IEnumerable<IPersonRequest> rootList, int intervalsPerDay, DataTable table)
		{
			InParameter.NotNull("rootList", rootList);
			InParameter.NotNull("table", table);


			foreach (PersonRequest personRequest in rootList)
			{
				var personTimeZone = personRequest.Person.PermissionInformation.DefaultTimeZone();
				DateTimePeriod requestPeriod = personRequest.Request.Period;
				var numOfDays = (requestPeriod.ToDateOnlyPeriod(personTimeZone));

				var dayCollection = numOfDays.DayCollection();

				if (personRequest.Request is IShiftTradeRequest)
				{
					dayCollection =
						 ((IShiftTradeRequest)personRequest.Request).ShiftTradeSwapDetails.Select(x => x.DateFrom).
							  ToList();
				}

				var requestCreatedOn = personRequest.CreatedOn;
				DateTime convertedTimeFromUtc = TimeZoneInfo.ConvertTimeFromUtc(requestCreatedOn.GetValueOrDefault(), personTimeZone);

				foreach (var dateOnly in dayCollection)
				{
					DataRow row = table.NewRow();

					row["request_code"] = personRequest.Id;
					row["person_code"] = personRequest.Person.Id;

					// transforming request type into request type id.
					if (personRequest.Request is IAbsenceRequest)
					{
						row["request_type_code"] = 1;
						row["absence_code"] = ((IAbsenceRequest)personRequest.Request).Absence.Id;
					}
					else if (personRequest.Request is IShiftTradeRequest)
						row["request_type_code"] = 2;
					else
						row["request_type_code"] = 0;

					// transforming request status into request status id.
					if (personRequest.IsPending || personRequest.IsNew)
						row["request_status_code"] = 0;
					if (personRequest.IsApproved)
						row["request_status_code"] = 1;
					if (personRequest.IsDenied && !personRequest.IsWaitlisted)
						row["request_status_code"] = 2;
					if (personRequest.IsCancelled)
						row["request_status_code"] = 3;
					if (personRequest.IsWaitlisted)
						row["request_status_code"] = 4;

					row["request_date"] = dateOnly.Date;
					row["application_datetime"] = convertedTimeFromUtc;
					row["request_startdate"] = numOfDays.StartDate.Date;
					row["request_enddate"] = numOfDays.EndDate.Date;
					row["request_starttime"] = requestPeriod.StartDateTimeLocal(personTimeZone);
					row["request_endtime"] = requestPeriod.EndDateTimeLocal(personTimeZone);

					row["business_unit_code"] = personRequest.GetOrFillWithBusinessUnit_DONTUSE().Id;

					row["request_day_count"] = 1;
					row["request_start_date_count"] = dateOnly == numOfDays.StartDate ? 1 : 0;
					row["datasource_id"] = 1;
					row["insert_date"] = System.DateTime.Now;
					row["update_date"] = System.DateTime.Now;
					row["datasource_update_date"] = RaptorTransformerHelper.GetUpdatedDate(personRequest);
					row["is_deleted"] = personRequest.IsDeleted;

					table.Rows.Add(row);
				}
			}
		}

	}
}
