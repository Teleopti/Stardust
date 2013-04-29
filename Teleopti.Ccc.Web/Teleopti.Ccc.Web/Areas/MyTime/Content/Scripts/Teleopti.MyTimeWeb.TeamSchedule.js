/// <reference path="~/Content/Scripts/jquery-1.9.1.js" />
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
            var team;
            ko.utils.arrayForEach(self.availableTeams(), function (t) {
                if (t.Value == teamId) {
                    team = t;
                }
            });
            return team;
        };
        
        this.findFirstTeam = function () {
            var team;
            ko.utils.arrayForEach(self.availableTeams(), function(t) {
                if (t.Value != '-') {
                    team = t;
                }
            });
            return team;
        };

        this.dateAndTeamKey = ko.computed(function() {
            var selectedTeam = self.selectedTeam();
            var teamId = '';
            if (selectedTeam != undefined && selectedTeam != null) {
                teamId = selectedTeam.Value;
            }
            return self.selectedDate().format('YYYY-MM-DD') + '-' + teamId;
        });
    };

	function _initPeriodSelection() {
		
	}

	function _initTeamPicker() {
		
	}

	function _initTeamPickerSelection() {

	    vm = new teamScheduleViewModel(_currentUrlDate());

	    $.ajax({
	        url: "MyTime/TeamSchedule/Teams/" + _currentUrlDate(),
	        dataType: "json",
	        type: "GET",
	        global: false,
	        cache: false,
	        success: function (data, textStatus, jqXHR) {
	            
	            var list = vm.availableTeams();
	            ko.utils.arrayForEach(data, function (t) {
	                if (t.Value != '-') {
	                    list.push(t);
	                }
	            });
	            vm.availableTeams.valueHasMutated();

	            var teamId = _currentId();
	            var foundTeam = undefined;
	            if (teamId)
	                foundTeam = vm.findTeamById(teamId);

	            if (foundTeam === undefined)
	                foundTeam = vm.findFirstTeam();

	            vm.selectedTeam(foundTeam);

                vm.dateAndTeamKey.subscribe(function() {
                    var team = vm.selectedTeam();
                    if (team === undefined || team == null) return;
                    _navigateTo(vm.selectedDate().format('YYYY-MM-DD'), team.Value);
                });
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

	function _bindData() {
	    ko.applyBindings(vm, $('#page')[0]);
	};

	function _currentUrlDate() {
		return Teleopti.MyTimeWeb.Portal.ParseHash().dateHash;
	}
    
    function _currentId() {
        return Teleopti.MyTimeWeb.Portal.ParseHash().parts[5];
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
			_bindData();
			_initAgentNameOverflow();
		}
	};

})(jQuery);

