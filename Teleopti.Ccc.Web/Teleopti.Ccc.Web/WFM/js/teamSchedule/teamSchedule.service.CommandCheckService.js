(function() {
    angular.module('wfm.teamSchedule').service('CommandCheckService', CommandCheckService);

    CommandCheckService.$inject = ['$http', '$q'];

    function CommandCheckService($http, $q) {
        var checkOverlappingUrl = "../api/TeamScheduleCommand/CheckOverlapppingCertainActivities";
        var overlappingPeopleList = [];
        var globalDeferred, globalResolveData, commandCheckedStatus = false;

        this.checkOverlappingCertainActivities = checkOverlappingCertainActivities;
        this.getOverlappingAgentList = getOverlappingAgentList;
        this.getCommandCheckStatus = getCommandCheckStatus;
        this.resetCommandCheckStatus = resetCommandCheckStatus;
        this.resolvePromise = resolvePromise;

        var fakeData = [{
            Name: 'Daniel Billsus',
            PersonId: '4fd900ad-2b33-469c-87ac-9b5e015b2564'
        }, {
            Name: 'Teleopti Demo',
            PersonId: '10957ad5-5489-48e0-959a-9b5e015b2b5c'
        }, {
            Name: 'Bill Gates',
            PersonId: '826f2a46-93bb-4b04-8d5e-9b5e015b2577'
        }];

        function checkOverlappingCertainActivities(requestData) {
            globalDeferred = $q.defer();

            //Temporarily fake backend checking activity overlap rules(trigger when start hour is 14 and end hour is 15).
            var overlapped = moment(requestData.StartTime).hours() == 14 && moment(requestData.EndTime).hours() == 15;

            if (overlapped) {
                $http.post(checkOverlappingUrl, requestData).then(function(data) {
                    globalResolveData = data;
                    overlappingPeopleList = fakeData;
                    commandCheckedStatus = true;
                }, function(error) {
                    globalResolveData = data;
                    overlappingPeopleList = fakeData;
                    commandCheckedStatus = true;
                });
            } else {
                $http.post(checkOverlappingUrl, requestData).then(function(data) {
                    overlappingPeopleList = [];
                    globalDeferred.resolve(data);
                }, function(error) {
                    globalDeferred.reject(error);
                });
            }
            return globalDeferred.promise;
        }

        function getCommandCheckStatus(){
            return commandCheckedStatus;
        }

        function resetCommandCheckStatus(){
            commandCheckedStatus = false;
        }

        function resolvePromise() {
            globalDeferred.resolve(globalResolveData);
        }

        function getOverlappingAgentList() {
            return overlappingPeopleList;
        }
    }
})();