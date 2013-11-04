/// <reference path="~/Content/Scripts/jquery-1.9.1.js" />
/// <reference path="~/Content/jqueryui/jquery-ui-1.10.1.custom.js" />
/// <reference path="~/Content/Scripts/jquery-1.9.1-vsdoc.js" />
/// <reference path="~/Content/Scripts/MicrosoftMvcAjax.debug.js" />
/// <reference path="Teleopti.MyTimeWeb.Portal.js"/>
/// <reference path="Teleopti.MyTimeWeb.Common.js"/>

if (typeof (Teleopti) === 'undefined') {
	Teleopti = {};
	if (typeof (Teleopti.MyTimeWeb) === 'undefined') {
		Teleopti.MyTimeWeb = {};
	}
}

Teleopti.MyTimeWeb.TeamSchedule = (function ($) {
	var ajax = new Teleopti.MyTimeWeb.Ajax();
	var toolBarViewModel = null;
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

	function _initViewModel() {
		var tab = $("#TeamScheduleTab")[0];
		if (tab) {
			toolBarViewModel = new (function () {
				this.SeperatorVisible = ko.observable(false);
			})();
			ko.applyBindings(toolBarViewModel, tab);
		}
	}

	function _initTeamPicker() {
		$('#Team-Picker').on("change", function (e) {
			var tid;
			if (e.val)
				tid = e.val;
			else {
				tid = e.target.value;
			}
			_navigateTo(_currentFixedDate(), tid);
		});
	}

	function _initTeamPickerSelection() {
		$('#Team-Picker').select2("destroy");
		ajax.Ajax({
			url: "TeamSchedule/Teams/" + _currentUrlDate(),
			dataType: "json",
			type: "GET",
			global: false,
			cache: false,
			success: function (data, textStatus, jqXHR) {
				var containerCss;
				if (data.length < 2) {
					containerCss = "team-select2-container team-select2-container-hidden";
				} else {
					containerCss = "team-select2-container";
					toolBarViewModel.SeperatorVisible(true);
				}
				$('#Team-Picker').select2(
					{
						data: data,
						containerCssClass: containerCss,
						dropdownCssClass: "team-select2-dropdown",
						formatResult: function (item) {
							return $('<div/>').text(item.text).html();
						},
						formatSelection: function(item) {
							return $('<div/>').text(item.text).html();
						}
					}
				);

				var teamId = $('#TeamSchedule-body').data('mytime-teamselection');
				if (!teamId)
					return;
				var selectables = [];
				if (data[0] && data[0].children) {
					$.each(data, function (index) {
						$.merge(selectables, data[index].children);
					});
				} else {
					selectables = data;
				}
				var team = $.grep(selectables, function (e) { return e.id == teamId; })[0];
				if (team) {
					$('#Team-Picker').select2("data", team);
				} else {
					var date = _currentFixedDate();
					if (date)
						_navigateTo(date);
				}

				readyForInteraction();
				completelyLoaded();
			}
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
			portal.RegisterPartialCallBack('TeamSchedule/Index', 
				Teleopti.MyTimeWeb.TeamSchedule.TeamSchedulePartialInit,
				Teleopti.MyTimeWeb.TeamSchedule.TeamSchedulePartialDispose
			);
			_initTeamPicker();
			_initViewModel();
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
		},
		TeamSchedulePartialDispose: function () {
			ajax.AbortAll();
		}
	};

})(jQuery);

