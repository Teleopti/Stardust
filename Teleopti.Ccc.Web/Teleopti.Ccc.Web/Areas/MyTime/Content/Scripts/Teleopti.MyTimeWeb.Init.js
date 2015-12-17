function _initMomentLanguageWithFallback() {
	var ietfLanguageTag = $('html').attr('lang').toLowerCase();;
	var baseLang = 'en'; //Base
	var languages = [ietfLanguageTag, ietfLanguageTag.split('-')[0], baseLang];

	for (var i = 0; i < languages.length; i++) {
		try {
			moment.lang(languages[i]);
			if (moment.lang() == languages[i]) return;
		} catch (e) {
			continue;
		}
	}
}

function initAllModules() {
    Teleopti.MyTimeWeb.Schedule.Init();
    Teleopti.MyTimeWeb.Schedule.Month.Init();
    Teleopti.MyTimeWeb.Schedule.MobileWeek.Init();
	Teleopti.MyTimeWeb.StudentAvailability.Init();
	Teleopti.MyTimeWeb.Preference.Init();
	Teleopti.MyTimeWeb.Request.Init();

	if (Teleopti.MyTimeWeb.Common.IsToggleEnabled("MyTimeWeb_EnhanceTeamSchedule_32580")) {
		Teleopti.MyTimeWeb.TeamScheduleNew.Init();
	} else {
		Teleopti.MyTimeWeb.TeamScheduleOld.Init();
	}

	Teleopti.MyTimeWeb.Settings.Init();
	Teleopti.MyTimeWeb.Password.Init();
	Teleopti.MyTimeWeb.MyReport.Init();
	Teleopti.MyTimeWeb.BadgeLeaderBoardReport.Init();
	Teleopti.MyTimeWeb.MyAdherence.Init();
	Teleopti.MyTimeWeb.MyQueueMetrics.Init();
	Teleopti.MyTimeWeb.Common.Layout.Init();

	_initMomentLanguageWithFallback();
};