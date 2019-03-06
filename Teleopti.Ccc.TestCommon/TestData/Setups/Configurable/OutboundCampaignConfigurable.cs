using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Outbound;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;


namespace Teleopti.Ccc.TestCommon.TestData.Setups.Configurable
{
	public class OutboundCampaignConfigurable : IDataSetup
	{
        public IOutboundCampaign Campaign;

		public string Name { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public string Skill { get; set; }
		public int CallListLen { get; set; }
		public int TargetRate { get; set; }
		public int ConnectRate { get; set; }
		public int RightPartyConnectRate { get; set; }
		public int ConnectAverageHandlingTime { get; set; }
		public int RightPartyAverageHandlingTime { get; set; }
		public int UnproductiveTime { get; set; }
		public string OpeningHourStart { get; set; }
		public string OpeningHourEnd { get; set; }

		public OutboundCampaignConfigurable()
		{
			CallListLen = 100;
			TargetRate = 50;
			ConnectRate = 20;
			RightPartyConnectRate = 20;
			ConnectAverageHandlingTime = 30;
			RightPartyAverageHandlingTime = 120;
			UnproductiveTime = 30;
		}

		public void Apply(ICurrentUnitOfWork currentUnitOfWork)
		{
			var skillRepository = SkillRepository.DONT_USE_CTOR(currentUnitOfWork);
			var skill = skillRepository.LoadAll().Single(x => x.Name == Skill);

			var startDateUtc = TimeZoneHelper.ConvertToUtc(StartDate, skill.TimeZone);
			var endDateUtc = TimeZoneHelper.ConvertToUtc(EndDate, skill.TimeZone);		

			Campaign = new Campaign()
			{
				Name = Name,
				CallListLen = CallListLen,
				TargetRate = TargetRate,
				Skill = skill,
				ConnectRate = ConnectRate,
				RightPartyConnectRate = RightPartyConnectRate,
				ConnectAverageHandlingTime = ConnectAverageHandlingTime,
				RightPartyAverageHandlingTime = RightPartyAverageHandlingTime,
				UnproductiveTime = UnproductiveTime,
				SpanningPeriod = new DateTimePeriod(startDateUtc, endDateUtc),
				BelongsToPeriod = new DateOnlyPeriod(new DateOnly(StartDate), new DateOnly(EndDate))
			};

			TimeSpan openingHourStart, openingHourEnd;

			if (TimeSpan.TryParse(OpeningHourStart, out openingHourStart) &&
				TimeSpan.TryParse(OpeningHourEnd, out openingHourEnd))
			{
				Campaign.WorkingHours = new Dictionary<DayOfWeek, TimePeriod>
				{
					{DayOfWeek.Monday, new TimePeriod(openingHourStart, openingHourEnd) },
					{DayOfWeek.Tuesday, new TimePeriod(openingHourStart, openingHourEnd) },
					{DayOfWeek.Wednesday, new TimePeriod(openingHourStart, openingHourEnd) },
					{DayOfWeek.Thursday, new TimePeriod(openingHourStart, openingHourEnd) },
					{DayOfWeek.Friday, new TimePeriod(openingHourStart, openingHourEnd) },
				};
			}

			new OutboundCampaignRepository(currentUnitOfWork).Add(Campaign);
		}

		public string GetWorkingHoursString()
		{
			var openingHourStart = string.Format("new Date((new Date()).toDateString() + ' {0}')", OpeningHourStart);
			var openingHourEnd = string.Format("new Date((new Date()).toDateString() + ' {0}')", OpeningHourEnd);
			var selectionArray = (from DayOfWeek day in Enum.GetValues(typeof (DayOfWeek)) select string.Format(" {{ WeekDay: {0}, Checked: true  }} ", (int)day)).ToArray();
			var selections = String.Format("[ {0} ]",  String.Join(", ", selectionArray));
			return String.Format("[{{ StartTime: {0}, EndTime: {1}, WeekDaySelections: {2} }}]", openingHourStart,
				openingHourEnd, selections);


		}
		

	}
}
