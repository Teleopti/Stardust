(function() {
	angular.module('wfm.teamSchedule').service('CommandCheckService', CommandCheckService);

	CommandCheckService.$inject = ['$http', '$q'];

	function CommandCheckService($http, $q) {
		var checkOverlappingUrl = "../api/TeamScheduleData/CheckOverlapppingCertainActivities";
		var checkOverlappingMoveActivityUrl = "../api/TeamScheduleData/CheckMoveActivityOverlapppingCertainActivities";
		var checkPersonalAccountsUrl = '../api/TeamScheduleData/CheckPersonalAccounts';

		var checkFailedList = [], commandRquestData;
		var commandCheckDeferred, commandCheckedStatus = false;

		this.checkOverlappingCertainActivities = checkOverlappingCertainActivities;
		this.checkMoveActivityOverlappingCertainActivities = checkMoveActivityOverlappingCertainActivities;
		this.checkPersonalAccounts = checkPersonalAccounts;

		this.getCheckFailedList = getCheckFailedList;
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
							checkFailedList = resp.data;
							commandCheckedStatus = true;
						}
					})
					.catch(function(e) {
						commandCheckDeferred.reject(e);
					});

				return commandCheckDeferred.promise;
			};
		}

		function getRequestData() {
			return commandRquestData;
		}

		function getCommandCheckStatus() {
			return commandCheckedStatus;
		}

		function resetCommandCheckStatus() {
			commandCheckedStatus = false;
		}

		function completeCommandCheck(option) {
			commandCheckDeferred.resolve(option);
		}

		function getCheckFailedList() {
			return checkFailedList;
		}

		function checkPersonalAccounts(requestData) {
			return getCheck(checkPersonalAccountsUrl)(requestData);
		}
	}
})();