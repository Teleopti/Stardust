/// <reference path="~/Content/Scripts/jquery-1.6.4.js" />
/// <reference path="~/Scripts/jquery-ui-1.8.11.js" />
/// <reference path="~/Scripts/jquery-1.5.1-vsdoc.js" />
/// <reference path="~/Scripts/MicrosoftMvcAjax.debug.js" />
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
	var common = Teleopti.MyTimeWeb.Common;

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
				source: "TeamSchedule/Teams",
				changed: function (event, item) {
					var teamId = item.item.value;
					_navigateTo(_currentFixedDate(), teamId);
				}
			})
			.parent()
			.hide()
			;
	}

	function _initTeamPickerSelection() {
		var teamId = $('#TeamSchedule-body').data('mytime-teamselection');
		var selectbox = $('#TeamSchedule-TeamPicker-select');
		selectbox.selectbox({
			source: "TeamSchedule/Teams/" + _currentUrlDate()
		});
		selectbox.selectbox('refresh', function () {
			if (selectbox.selectbox('selectableOptions').length < 2)
				selectbox.selectbox('hide');
			else
				selectbox.selectbox('show');
			if (selectbox.selectbox('contains', teamId))
				selectbox.selectbox('select', teamId);
			else
				_navigateTo(_currentFixedDate());
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
		return $('#TeamSchedule-body').data('mytime-periodselection').Date;
	}

	function _currentUrlDate() {
		return Teleopti.MyTimeWeb.Portal.ParseHash().dateHash;
	}

	return {
		Init: function () {
			portal.RegisterPartialCallBack('TeamSchedule/Index', Teleopti.MyTimeWeb.TeamSchedule.TeamSchedulePartialInit);
			_initTeamPicker();
		},
		TeamSchedulePartialInit: function () {
			_initPeriodSelection();
			_initTeamPickerSelection();
			common.Layout.ActivateTooltip();
			_initAgentNameOverflow();
		}
	};

})(jQuery);

$(function () { Teleopti.MyTimeWeb.TeamSchedule.Init(); });

