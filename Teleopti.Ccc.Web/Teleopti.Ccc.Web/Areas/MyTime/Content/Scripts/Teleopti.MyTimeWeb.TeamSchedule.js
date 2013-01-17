/// <reference path="~/Content/Scripts/jquery-1.8.3.js" />
/// <reference path="~/Content/jqueryui/jquery-ui-1.9.1.custom.js" />
/// <reference path="~/Content/Scripts/jquery-1.8.3-vsdoc.js" />
/// <reference path="~/Content/Scripts/MicrosoftMvcAjax.debug.js" />
/// <reference path="~/Content/Scripts/date.js" />
/// <reference path="Teleopti.MyTimeWeb.Portal.js"/>
/// <reference path="Teleopti.MyTimeWeb.Common.js"/>

if (typeof (Teleopti) === 'undefined') {
	Teleopti = {};
	if (typeof (Teleopti.MyTimeWeb) === 'undefined') {
		Teleopti.MyTimeWeb = {};
	}
}

Teleopti.MyTimeWeb.TeamSchedule = (function ($) {

	var portal = Teleopti.MyTimeWeb.Portal;
	var readyForInteraction = function () { };
	var completelyLoaded = function () { };

	function _initPeriodSelection() {
		var rangeSelectorId = '#TeamScheduleDateRangeSelector';
		var periodData = $('#TeamSchedule-body').data('mytime-periodselection');
		var teamId = $('#TeamSchedule-body').data('mytime-teamselection');
		var actionSuffix = "/" + teamId;
		portal.InitPeriodSelection(rangeSelectorId, periodData, actionSuffix);
	}

	function _initTeamPicker() {
		$('select.ui-selectbox-init')
			.removeClass('ui-selectbox-init')
			.selectbox({
				changed: function (event, item) {
					var teamId = item.item.value;
					_navigateTo(_currentFixedDate(), teamId);
				},
				rendered: function () {
					var teamId = $('#TeamSchedule-body').data('mytime-teamselection');
					if (!teamId)
						return;

					var self = $(this);

					if (self.selectbox('selectableOptions').length < 2)
						self.selectbox({ visible: false });
					else
						self.selectbox({ visible: true });
					if (self.selectbox('contains', teamId))
						self.selectbox('select', teamId);
					else {
						var date = _currentFixedDate();
						if (date)
							_navigateTo(date);
					}
					readyForInteraction();
					completelyLoaded();
				}
			})
			.parent()
			.hide()
			;
	}

	function _initTeamPickerSelection() {
		var selectbox = $('#TeamSchedule-TeamPicker-select');
		selectbox.selectbox({
			source: "MyTime/TeamSchedule/Teams/" + _currentUrlDate()
		});
	}

	function _initAgentNameOverflow() {
		$('.teamschedule-agent-name')
			.hoverIntent({
				interval: 200,
				timeout: 200,
				over: function () {
					if ($(this).hasHiddenContent())
						$(this).addClass('teamschedule-agent-name-hover');
				},
				out: function () {
					$(this).removeClass('teamschedule-agent-name-hover');
				}
			})
			;
	}

	function _navigateTo(date, teamid) {
		portal.NavigateTo("TeamSchedule/Index", date, teamid);
	}

	function _currentFixedDate() {
		if ($('#TeamSchedule-body').data('mytime-periodselection')) {
			return $('#TeamSchedule-body').data('mytime-periodselection').Date;
		} else {
			return undefined;
		}
	}

	function _currentUrlDate() {
		return Teleopti.MyTimeWeb.Portal.ParseHash().dateHash;
	}

	return {
		Init: function () {
			portal.RegisterPartialCallBack('TeamSchedule/Index', Teleopti.MyTimeWeb.TeamSchedule.TeamSchedulePartialInit);
			_initTeamPicker();
		},
		TeamSchedulePartialInit: function (readyForInteractionCallback, completelyLoadedCallback) {
			readyForInteraction = readyForInteractionCallback;
			completelyLoaded = completelyLoadedCallback;
			if ($('#TeamSchedule-body').length == 0) {
				readyForInteraction();
				completelyLoaded();
				return;
			}
			_initPeriodSelection();
			_initTeamPickerSelection();
			_initAgentNameOverflow();
		}
	};

})(jQuery);

