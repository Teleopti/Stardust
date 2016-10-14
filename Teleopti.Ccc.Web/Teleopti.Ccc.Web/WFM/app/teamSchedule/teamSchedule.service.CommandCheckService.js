(function () {
	angular.module('wfm.teamSchedule').service('CommandCheckService', CommandCheckService);

	CommandCheckService.$inject = ['$http', '$q'];

	function CommandCheckService($http, $q) {
		var checkOverlappingUrl = "../api/TeamScheduleData/CheckOverlapppingCertainActivities";
		var checkOverlappingMoveActivityUrl = "../api/TeamScheduleData/CheckMoveActivityOverlapppingCertainActivities";
		var checkPersonalAccountsUrl = '../api/TeamScheduleData/CheckPersonalAccounts';

		var checkFailedAgentList = [], commandRquestData, currentCheckConfig;
		var commandCheckDeferred, commandCheckedStatus = false;

		var checkConfigs = {};
		populateAllCheckConfigs();
			
		this.checkAddActivityOverlapping = checkAddActivityOverlapping;
		this.checkAddPersonalActivityOverlapping = checkAddPersonalActivityOverlapping;
		this.checkMoveActivityOverlapping = checkMoveActivityOverlapping;
		this.checkPersonalAccounts = checkPersonalAccounts;
		this.getCheckFailedAgentList = getCheckFailedAgentList;
		this.getCommandCheckStatus = getCommandCheckStatus;
		this.resetCommandCheckStatus = resetCommandCheckStatus;
		this.completeCommandCheck = completeCommandCheck;
		this.getRequestData = getRequestData;
		this.getCheckConfig = getCheckConfig;

		function getCheckConfig() {
			return currentCheckConfig;
		}

		function checkAddActivityOverlapping(requestData) {
			commandRquestData = requestData;
			currentCheckConfig = checkConfigs.checkAddActivityOverlapping;
			return getCheck(checkOverlappingUrl)(requestData);
		}

		function checkAddPersonalActivityOverlapping(requestData) {
			commandRquestData = requestData;
			currentCheckConfig = checkConfigs.checkAddPersonalActivityOverlapping;
			return getCheck(checkOverlappingUrl)(requestData);
		}

		function checkMoveActivityOverlapping(requestData) {
			commandRquestData = requestData;
			currentCheckConfig = checkConfigs.checkMoveActivityOverlapping;
			return getCheck(checkOverlappingMoveActivityUrl)(requestData);
		}

		function checkPersonalAccounts(requestData) {
			commandRquestData = requestData;
			currentCheckConfig = checkConfigs.checkPersonalAccounts;
			return getCheck(checkPersonalAccountsUrl)(requestData);
		}

		function getCheck(url) {
			return function (requestData) {
				commandCheckDeferred = $q.defer();

				$http.post(url, requestData)
					.then(function (resp) {
						if (resp.data.length === 0) {
							commandCheckDeferred.resolve();
						} else {
							checkFailedAgentList = resp.data;
							commandCheckedStatus = true;
						}
					})
					.catch(function (e) {
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

		function getCheckFailedAgentList() {
			return checkFailedAgentList;
		}

		function populateAllCheckConfigs() {
			checkConfigs.checkAddActivityOverlapping = {
				subject: 'ThisActivityWillIntersectExistingActivitiesThatDoNotAllowOverlapping',
				body: 'TheFollowingAgentsHaveAffectedActivities',
				actionOptions: ['MoveNonoverwritableActivityForTheseAgents', 'DoNotModifyForTheseAgents', 'OverrideForTheseAgents'],
			};
			checkConfigs.checkAddPersonalActivityOverlapping = {
				subject: 'ThisActivityWillIntersectExistingActivitiesThatDoNotAllowOverlapping',
				body: 'TheFollowingAgentsHaveAffectedActivities',
				actionOptions: ['DoNotModifyForTheseAgents', 'OverrideForTheseAgents'],
			};
			checkConfigs.checkMoveActivityOverlapping = {
				subject: 'ThisActivityWillIntersectExistingActivitiesThatDoNotAllowOverlapping',
				body: 'TheFollowingAgentsHaveAffectedActivities',
				actionOptions: ['DoNotModifyForTheseAgents', 'OverrideForTheseAgents'],
			};
			checkConfigs.checkPersonalAccounts = {
				subject: 'TBD',
				body: 'TBD',
				actionOptions: ['DoNotModifyForTheseAgents', 'OverrideForTheseAgents'],
			};
		}
	}
})();