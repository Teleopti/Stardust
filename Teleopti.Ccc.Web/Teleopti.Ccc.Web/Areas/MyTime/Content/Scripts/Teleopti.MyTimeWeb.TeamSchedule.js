﻿/// <reference path="~/Content/Scripts/jquery-1.9.1.js" />
/// <reference path="~/Content/jqueryui/jquery-ui-1.10.2.custom.js" />
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

	var portal = Teleopti.MyTimeWeb.Portal;
	var readyForInteraction = function () { };
	var completelyLoaded = function () { };
    var vm;

    var teamScheduleViewModel = function (urlDate) {
        var self = this;

        this.selectedDate = ko.observable(moment().startOf('day'));
        if (urlDate) {
            this.selectedDate(moment(urlDate));
        }
        this.selectedTeam = ko.observable();
        this.availableTeams = ko.observableArray();

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

        this.displayDate = ko.computed(function() {
            return self.selectedDate().format('YYYY-MM-DD');
        });

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
	            if (vm.showTeamPicker()) {
	                var foundTeam = undefined;
	                if (teamId)
	                    foundTeam = vm.findTeamById(teamId);

	                if (foundTeam === undefined)
	                    foundTeam = vm.findFirstTeam();

	                vm.selectedTeam(foundTeam.id);
	            } else {
	                vm.selectedTeam(teamId);
	            }
	            
	            vm.dateAndTeamKey.subscribe(function () {
	                var team = vm.selectedTeam();
	                if (team === undefined || team == null) return;
	                var theDate = vm.selectedDate();
	                _navigateTo(theDate.format('YYYY-MM-DD'), team);
	            });
	            
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
	    _unBindData();
		portal.NavigateTo("TeamSchedule/Index", date, teamid);
	}

	function _bindData() {
	    ko.applyBindings(vm, $('#page')[0]);
	};
    
	function _unBindData() {
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
			portal.RegisterPartialCallBack('TeamSchedule/Index', Teleopti.MyTimeWeb.TeamSchedule.TeamSchedulePartialInit);
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
		}
	};

})(jQuery);