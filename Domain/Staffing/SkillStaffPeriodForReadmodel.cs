using System;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Staffing
{
	public class SkillStaffPeriodForReadmodel : SkillStaffPeriod
	{
		public Guid SkillId { get; set; }

		public SkillStaffPeriodForReadmodel(DateTimePeriod period, ITask taskData, ServiceAgreement serviceAgreementData) : base(period, taskData, serviceAgreementData)
		{}

		public double Forecast
		{
			get
			{
				SkillStaff skillStaff = (SkillStaff)Payload;
				return skillStaff.ForecastedIncomingDemand;
			}
			set
			{
				SkillStaff skillStaff = (SkillStaff)Payload;
				skillStaff.ForecastedIncomingDemand = value;
			}
		}
	}
}