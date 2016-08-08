(function() {
    angular.module('wfm.teamSchedule').service('CommandCheckService', CommandCheckService);

    CommandCheckService.$inject = ['$http', '$q'];

    function CommandCheckService($http, $q) {
        var checkOverlappingUrl = "../api/TeamScheduleData/CheckOverlapppingCertainActivities";
        var overlappingPeopleList = [];
        var commandCheckDeferred, commandCheckedStatus = false;

        this.checkOverlappingCertainActivities = checkOverlappingCertainActivities;
        this.getOverlappingAgentList = getOverlappingAgentList;
        this.getCommandCheckStatus = getCommandCheckStatus;
        this.resetCommandCheckStatus = resetCommandCheckStatus;
        this.completeCommandCheck = completeCommandCheck;

        function checkOverlappingCertainActivities(requestData) {
        	commandCheckDeferred = $q.defer();

	        $http.post(checkOverlappingUrl, requestData)
		        .then(function(resp) {		        	
		        	if (resp.data.length === 0) {
		        		commandCheckDeferred.resolve();
			        }
			        else {
		        		commandCheckedStatus = true;
		        		overlappingPeopleList = resp.data;				      
			        }
		        })
		        .catch(function(e) {
		        	commandCheckDeferred.reject(e);
		        });
            
	        return commandCheckDeferred.promise;
        }

        function getCommandCheckStatus(){
            return commandCheckedStatus;
        }

        function resetCommandCheckStatus(){
            commandCheckedStatus = false;
        }

        function completeCommandCheck() {
        	commandCheckDeferred.resolve();
	        resetCommandCheckStatus();
        }

        function getOverlappingAgentList() {
            return overlappingPeopleList;
        }
    }
})();