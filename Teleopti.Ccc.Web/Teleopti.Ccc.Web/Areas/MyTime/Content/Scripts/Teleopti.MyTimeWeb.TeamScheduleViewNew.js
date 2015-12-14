/// <reference path="~/Content/Scripts/jquery-1.9.1.js" />
/// <reference path="~/Content/jqueryui/jquery-ui-1.10.2.custom.js" />
/// <reference path="~/Content/Scripts/jquery-1.9.1-vsdoc.js" />
/// <reference path="~/Content/Scripts/MicrosoftMvcAjax.debug.js" />
/// <reference path="Teleopti.MyTimeWeb.Portal.js"/>
/// <reference path="Teleopti.MyTimeWeb.Common.js"/>
/// <reference path="~/Content/Scripts/knockout-2.2.1.debug.js"/>

if (typeof (Teleopti) === 'undefined') {
	Teleopti = {};
	if (typeof (Teleopti.MyTimeWeb) === 'undefined') {
		Teleopti.MyTimeWeb = {};
	}
}

Teleopti.MyTimeWeb.TeamScheduleNew = (function ($) {

	var portal = Teleopti.MyTimeWeb.Portal;
	var readyForInteraction = function () { };
	var completelyLoaded = function () { };

	var ajax = new Teleopti.MyTimeWeb.Ajax();
	var vm;

	var endpoints = {
		loadCurrentDate: "TeamSchedule/TeamScheduleCurrentDate",
		loadFilterTimes: "RequestsShiftTradeScheduleFilter/Get",
		loadMyTeam: "Requests/ShiftTradeRequestMyTeam",
		loadDefaultTeam: "TeamSchedule/DefaultTeam",
		loadTeams: "Team/TeamsAndGroupsWithAllTeam",
		loadSchedule: Teleopti.MyTimeWeb.Common.IsToggleEnabled("MyTimeWeb_TeamScheduleNoReadModel_36210") ? "TeamSchedule/TeamSchedule" : "TeamSchedule/TeamSchedule"
	};

	function _bindData() {
		vm = Teleopti.MyTimeWeb.TeamScheduleViewModelFactory.createViewModel(endpoints, ajax);

		$(window).resize(function () {
			vm.redrawLayers();
		});
		ko.applyBindings(vm, $('#page')[0]);
	};

	function _cleanBindings() {
		//remove old layer's tooltip if it still exist
		$("[class='tooltip fade top in']").remove();
		$(window).unbind('resize');
		ko.cleanNode($('#page')[0]);
	};

	return {
		Init: function () {
			portal.RegisterPartialCallBack('TeamSchedule/Index', Teleopti.MyTimeWeb.TeamScheduleNew.TeamSchedulePartialInit, Teleopti.MyTimeWeb.TeamScheduleNew.PartialDispose);
		},
		TeamSchedulePartialInit: function (readyForInteractionCallback, completelyLoadedCallback) {
			readyForInteraction = readyForInteractionCallback;
			completelyLoaded = completelyLoadedCallback;
			if ($('#teamschedule-body-inner').length == 0) {
				readyForInteraction();
				completelyLoaded();
				return;
			}
			_bindData();
			readyForInteraction();
			completelyLoaded();
		},
		PartialDispose: function () {
			vm = {};
			_cleanBindings();
		}
	};

})(jQuery);