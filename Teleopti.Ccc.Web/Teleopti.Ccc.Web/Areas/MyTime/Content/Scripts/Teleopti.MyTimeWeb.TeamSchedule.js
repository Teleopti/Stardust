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

Teleopti.MyTimeWeb.TeamSchedule = (function ($) {

	var portal = Teleopti.MyTimeWeb.Portal;
	var readyForInteraction = function () { };
	var completelyLoaded = function () { };
    var vm;

    var teamScheduleViewModel = function (urlDate) {
        var self = this;

        var periodData = $('#TeamSchedule-body').data('mytime-periodselection');
        this.selectedDate = ko.observable(moment(periodData.Date));
        if (urlDate) {
            this.selectedDate(moment(urlDate));
        }
        this.selectedTeam = ko.observable();
        this.availableTeams = ko.observableArray();
        
        this.displayDate = ko.observable(periodData.Display);

        this.selectableTeams = ko.computed(function() {
            if (self.availableTeams()[0] && self.availableTeams()[0].children) {
                var selectables = [];
                $.each(self.availableTeams(), function (index) {
                    $.merge(selectables, self.availableTeams()[index].children);
                });

                return selectables;
            }
            return self.availableTeams();
        });

        this.showGroupings = ko.computed(function () {
            if (self.availableTeams()[0] && self.availableTeams()[0].children) {
                return true;
            }
            return false;
        });
        
        this.showTeamPicker = ko.computed(function() {
            return self.availableTeams().length > 1;
        });

        this.nextDay = function() {
            self.selectedDate().add('days', 1);
            self.selectedDate.valueHasMutated();
        };

        this.previousDay = function () {
            self.selectedDate().add('days', -1);
            self.selectedDate.valueHasMutated();
        };

        this.today = function() {
            self.selectedDate(moment().startOf('day'));
        };

        this.findTeamById = function (teamId) {
            var foundTeam;
            ko.utils.arrayForEach(self.selectableTeams(), function (team) {
                if (team.id == teamId) {
                    foundTeam = team;
                }
            });
            return foundTeam;
        };
        
        this.findFirstTeam = function () {
            var firstTeam;
            ko.utils.arrayForEach(self.selectableTeams(), function (team) {
                if (firstTeam === undefined) {
                    firstTeam = team;
                }
            });
            return firstTeam;
        };

        this.dateAndTeamKey = ko.computed(function () {
            var selectedTeam = self.selectedTeam();
            var teamId = '';
            if (selectedTeam != undefined && selectedTeam != null) {
                teamId = selectedTeam.Value;
            }
            return self.selectedDate().format('YYYY-MM-DD') + '-' + teamId;
        });
    };

	function _initTeamPickerSelection() {
	    vm = new teamScheduleViewModel(_currentUrlDate());

	    $.ajax({
	        url: "MyTime/TeamSchedule/Teams/" + _currentUrlDate(),
	        dataType: "json",
	        type: "GET",
	        global: false,
	        cache: false,
	        success: function (data, textStatus, jqXHR) {
	            vm.availableTeams(data);

	            var teamId = _currentId();
	            var foundTeam = undefined;
	            if (vm.showTeamPicker()) {
	                if (teamId)
	                    foundTeam = vm.findTeamById(teamId);

	                if (foundTeam != undefined)
	                    vm.selectedTeam(foundTeam.id);
	                else
	                    _navigateTo(_getNavigateToDate());
	            } else {
	                if (teamId)
	                    foundTeam = vm.findTeamById(teamId);
	                
	                if (foundTeam != undefined)
	                    vm.selectedTeam(foundTeam.id);
	                else
	                    _navigateTo(_getNavigateToDate());
	            }
	            
	            vm.dateAndTeamKey.subscribe(function () {
	                var team = vm.selectedTeam();
	                if (team === undefined || team == null) return;
	                _navigateTo(vm.selectedDate().format('YYYY-MM-DD'), team);
	            });
	            
                readyForInteraction();
                completelyLoaded();
	        }
	    });
	}

	function _getNavigateToDate() {
	    var urlDate = _currentUrlDate();
	    if (urlDate) {
	        return moment(urlDate).format('YYYY-MM-DD');
	    }
	    
	    var periodData = $('#TeamSchedule-body').data('mytime-periodselection');
	    return periodData.Date;
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

	function _bindData() {
		var ajax = new Teleopti.MyTimeWeb.Ajax();
		ajax.Ajax({
			url: 'UserInfo/Culture',
			dataType: "json",
			type: 'GET',
			success: function (data) {
				$('.moment-datepicker').attr('data-bind', 'datepicker: selectedDate, datepickerOptions: { autoHide: true, weekStart: ' + data.WeekStart + ' }');
				ko.applyBindings(vm, $('#page')[0]);
			}
		});
	};
    
	function _cleanBindings() {
	    ko.cleanNode($('#page')[0]);
	};

	function _currentUrlDate() {
		return Teleopti.MyTimeWeb.Portal.ParseHash().dateHash;
	}
    
    function _currentId() {
        //return Teleopti.MyTimeWeb.Portal.ParseHash().parts[5];
        return $('#TeamSchedule-body').data('mytime-teamselection');
    }

	return {
		Init: function () {
		    portal.RegisterPartialCallBack('TeamSchedule/Index', Teleopti.MyTimeWeb.TeamSchedule.TeamSchedulePartialInit, Teleopti.MyTimeWeb.TeamSchedule.PartialDispose);
		},
		TeamSchedulePartialInit: function (readyForInteractionCallback, completelyLoadedCallback) {
			readyForInteraction = readyForInteractionCallback;
			completelyLoaded = completelyLoadedCallback;
			if ($('#TeamSchedule-body').length == 0) {
				readyForInteraction();
				completelyLoaded();
				return;
			}
			_initTeamPickerSelection();
			_bindData();
			_initAgentNameOverflow();
		},
		PartialDispose: function () {
		    _cleanBindings();
		}
	};

})(jQuery);