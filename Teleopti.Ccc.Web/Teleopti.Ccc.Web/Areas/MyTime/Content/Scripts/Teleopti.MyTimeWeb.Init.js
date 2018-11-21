function _initMomentLanguageWithFallback() {
	var ietfLanguageTag = $('html')
		.attr('lang')
		.toLowerCase();
	var baseLang = 'en'; //Base
	var languages = [ietfLanguageTag, ietfLanguageTag.split('-')[0], baseLang];

	for (var i = 0; i < languages.length; i++) {
		try {
			moment.locale(languages[i]);
			if (moment.locale() == languages[i]) return;
		} catch (e) {
			continue;
		}
	}
}

$(function() {
	Teleopti.MyTimeWeb.Schedule.Init();
	Teleopti.MyTimeWeb.Schedule.Month.Init();
	Teleopti.MyTimeWeb.Schedule.MobileStartDay.Init();
	Teleopti.MyTimeWeb.Schedule.MobileTeamSchedule.Init();
	Teleopti.MyTimeWeb.Schedule.MobileMonth.Init();
	Teleopti.MyTimeWeb.StudentAvailability.Init();
	Teleopti.MyTimeWeb.Preference.Init();
	Teleopti.MyTimeWeb.Request.Init();

	Teleopti.MyTimeWeb.Settings.Init();
	Teleopti.MyTimeWeb.Password.Init();
	Teleopti.MyTimeWeb.AppGuide.WFMApp.Init();
	Teleopti.MyTimeWeb.MyReport.Init();
	Teleopti.MyTimeWeb.BadgeLeaderBoardReport.Init();
	Teleopti.MyTimeWeb.MyAdherence.Init();
	Teleopti.MyTimeWeb.MyQueueMetrics.Init();
	Teleopti.MyTimeWeb.Common.Layout.Init();

	_initMomentLanguageWithFallback();
	Teleopti.MyTimeWeb.BadgeCountsDropdown.Init(); // !! Must be after _initMomentLanguageWithFallback
});
