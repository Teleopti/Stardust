Teleopti.MyTimeWeb.TeamScheduleNew = (function ($) {

	var portal = Teleopti.MyTimeWeb.Portal;
	var readyForInteraction = function () { };
	var completelyLoaded = function () { };

	var ajax = new Teleopti.MyTimeWeb.Ajax();
	var vm;

	var endpoints = {
		loadCurrentDate: "../api/TeamSchedule/TeamScheduleCurrentDate",
		loadFilterTimes: "RequestsShiftTradeScheduleFilter/Get",
		loadMyTeam: "Requests/ShiftTradeRequestMyTeam",
		loadDefaultTeam: "../api/TeamSchedule/DefaultTeam",
		loadTeams: "Team/TeamsAndGroupsWithAllTeam",
		loadSchedule: "../api/TeamSchedule/TeamScheduleOld"
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