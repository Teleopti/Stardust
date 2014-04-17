
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

$(function () {
    Teleopti.MyTimeWeb.Schedule.Init();
    Teleopti.MyTimeWeb.Schedule.Month.Init();
    Teleopti.MyTimeWeb.Schedule.MobileWeek.Init();
	Teleopti.MyTimeWeb.StudentAvailability.Init();
	Teleopti.MyTimeWeb.Preference.Init();
	Teleopti.MyTimeWeb.Request.Init();
	Teleopti.MyTimeWeb.TeamSchedule.Init();
	Teleopti.MyTimeWeb.Settings.Init();
	Teleopti.MyTimeWeb.Password.Init();
	Teleopti.MyTimeWeb.MyReport.Init();
	Teleopti.MyTimeWeb.Common.Layout.Init();

	_initMomentLanguageWithFallback();
});