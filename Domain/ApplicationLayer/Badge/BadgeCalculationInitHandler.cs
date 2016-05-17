using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.General;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Badge
{
#pragma warning disable 618
	public class BadgeCalculationInitHandler : IHandleEvent<BadgeCalculationInitEvent>, IRunOnServiceBus
#pragma warning restore 618
	{
		private readonly ITeamGamificationSettingRepository _teamGamificationSettingRepository;
		private readonly IBusinessUnitRepository _buRepository;
		private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;
		private readonly INow _now;
		private readonly IEventPublisher _publisher;

		/// <summary>
		/// Get all timezones using businessunitrepository
		/// Foreach send CalculateTimeZoneMessage
		/// TODO:existed problem: new added timezone to the system will not be calculated now.
		/// </summary>
		public BadgeCalculationInitHandler(ITeamGamificationSettingRepository teamGamificationSettingRepository, IBusinessUnitRepository buRepository,
			ICurrentUnitOfWorkFactory unitOfWorkFactory, INow now, IEventPublisher publisher)
		{
	_teamGamificationSettingRepository = teamGamificationSettingRepository;
			_buRepository = buRepository;
			_unitOfWorkFactory = unitOfWorkFactory;
			_now = now;
			this._publisher = publisher;
		}

		public void Handle(BadgeCalculationInitEvent @event)
		{
			//an ugle solution to get rid of servicebus . delaysend
			
			List<TimeZoneInfo> timeZoneList;
			using (_unitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				//if (_alltoggles.Toggles().is Toggles.Portal_DifferentiateBadgeSettingForAgents_31318)
				{
					var settings = _teamGamificationSettingRepository.FindAllTeamGamificationSettingsSortedByTeam();
					if (settings == null || !settings.Any())
					{
						//_serviceBus.DelaySend(DateTime.Today.AddDays(1), @event);
						return;
					}
				}

				timeZoneList = _buRepository.LoadAllTimeZones().ToList();
			}

			foreach (var timeZoneInfo in timeZoneList)
			{
				calculateBadges(@event.LogOnBusinessUnitId, @event.LogOnDatasource,timeZoneInfo.Id);
				
			}
		}

		private  void calculateBadges(Guid businessUnitId, string dataSource,string timeZoneInfoId )
		{
			const int badgeCalculationDelayDays = -2;
			var today = _now.LocalDateTime();
			var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneInfoId);
			var todayForGivenTimeZone = TimeZoneInfo.ConvertTime(today, TimeZoneInfo.Local, timeZone);
			var calculationDateForGivenTimeZone = todayForGivenTimeZone.AddDays(badgeCalculationDelayDays).Date;



			_publisher.Publish(new CalculateBadgeMessage
			{
				LogOnDatasource = dataSource,
				LogOnBusinessUnitId = businessUnitId,
				Timestamp = DateTime.UtcNow,
				CalculationDate = calculationDateForGivenTimeZone,
				TimeZoneCode = timeZoneInfoId
			});
			
		}
	}
}