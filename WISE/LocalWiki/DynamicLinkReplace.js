function notTranslatedPrefixes() {
	return ['F01%253APM_',
		'PM_'];
}

function notTranslatedPages() {
	return ['f01%253APerformance_Manager.html',
		'Schedule_vs_Forecast_Intraday_Deviation.html',
		'Requested_Days_Count.html',
		'Requests_Count.html',
		'Quality_Questionnaire.html',
		'Percentage_Average_Score.html',
		'Percentage_Evaluations.html',
		'Point_Average_Score.html',
		'Point_Evaluations.html',
		'Pivot_Chart.html',
		'Pivot_Table.html',
		'Readiness.html',
		'Named_Sets.html',
		'Measure.html',
		'Grade_Average_Score.html',
		'Grade_Evaluations.html',
		'Forecast_Accuracy_After_Call_Work.html',
		'Forecast_Accuracy_Talk_Time.html',
		'Dimension.html',
		'Agents_Ready_FTE.html',
		'SDK.html',
		'SDK_Release_Notes.html',
		'Shift_Category_Dimension.html',
		'Scenario_Dimension.html',
		'Day_Off_Dimension.html',
		'Absence_Dimension.html',
		'Activity_Dimension.html',
		'OLAP.html',
		'OLAP_Cube.html',
		'Requested_Time.html',
		'Scheduled_Time_Hourly.html'];
}

function getPageName(link) {
	return link.substr(link.lastIndexOf('/') + 1);
}

function isLinkTranslated(link, forbiddenLinks) {
	link = getPageName(link);
	for (var i = 0, forbiddenLink; forbiddenLink = forbiddenLinks[i]; i++) {
		if (link === forbiddenLink) {
			return false;
		}
	}
	return true;
}

function isPrefixAllowed(link, forbiddenPrefixes) {
	var linkPage = getPageName(link);
	for (var i = 0, prefix; prefix = forbiddenPrefixes[i]; i++) {
		if (linkPage.indexOf(prefix) === 0) {
			return false;
		}
	}
	return true;
}

function endsWith(str, suffix) {
	return str.indexOf(suffix, str.length - suffix.length) !== -1;
}

function shouldReplaceLink(link, pageName) {
	// if pageName is sv.html and link is pointing to a sv.html 
	// then it is alerady pointing to a traslated page
	return !endsWith(link, pageName) &&
		isPrefixAllowed(link, notTranslatedPrefixes()) &&
		isLinkTranslated(link, notTranslatedPages());
}

function replaceLinks() {
	var url = window.location.pathname;
	var pageName = getPageName(url);

	for (var i = 0, link; link = document.links[i].href; i++) {
		if (shouldReplaceLink(link, pageName)) {
			var trimmedLink = endsWith(link, '1.html')
				? link.substr(0, link.length - 7)
				: link.substr(0, link.length - 5);

			document.links[i].href = trimmedLink + '\\' + pageName;
		}
	}
}