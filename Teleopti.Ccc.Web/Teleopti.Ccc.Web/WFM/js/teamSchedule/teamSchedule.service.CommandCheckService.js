﻿(function() {
    angular.module('wfm.teamSchedule').service('CommandCheckService', CommandCheckService);

    CommandCheckService.$inject = ['$http', '$q'];

    function CommandCheckService($http, $q) {
    	var checkOverlappingUrl = "../api/TeamScheduleData/CheckOverlapppingCertainActivities";
	    var checkOverlappingMoveActivityUrl = "../api/TeamScheduleData/CheckMoveActivityOverlapppingCertainActivities";
        var overlappingPeopleList = [], commandRquestData;
        var commandCheckDeferred, commandCheckedStatus = false;

        this.checkOverlappingCertainActivities = checkOverlappingCertainActivities;
	    this.checkMoveActivityOverlappingCertainActivities = checkMoveActivityOverlappingCertainActivities;

        this.getOverlappingAgentList = getOverlappingAgentList;
        this.getCommandCheckStatus = getCommandCheckStatus;
        this.resetCommandCheckStatus = resetCommandCheckStatus;
        this.completeCommandCheck = completeCommandCheck;
        this.getRequestData = getRequestData;

        function checkOverlappingCertainActivities(requestData) {
	        return getCheck(checkOverlappingUrl)(requestData);
        }

        function checkMoveActivityOverlappingCertainActivities(requestData) {
            commandRquestData = requestData;
        	return getCheck(checkOverlappingMoveActivityUrl)(requestData);
        }

	    function getCheck(url) {
		    return function(requestData) {
			    commandCheckDeferred = $q.defer();

			    $http.post(url, requestData)
				    .then(function(resp) {
					    if (resp.data.length === 0) {
						    commandCheckDeferred.resolve();
					    } else {
						    commandCheckedStatus = true;
						    overlappingPeopleList = resp.data;
					    }
				    })
				    .catch(function(e) {
					    commandCheckDeferred.reject(e);
				    });

			    return commandCheckDeferred.promise;
		    };
	    }

        function getRequestData(){
            return commandRquestData;
        }

	    function getCommandCheckStatus(){
            return commandCheckedStatus;
        }

        function resetCommandCheckStatus(){
            commandCheckedStatus = false;
        }

        function completeCommandCheck() {
        	commandCheckDeferred.resolve();
        }

        function getOverlappingAgentList() {
            return overlappingPeopleList;
        }
    }
})();