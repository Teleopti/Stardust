(function() {
    angular.module('wfm.teamSchedule').service('CommandCheckService', CommandCheckService);

    CommandCheckService.$inject = ['$http', '$q', '$rootScope'];

    function CommandCheckService($http, $q, $rootScope) {
        var checkOverlappingUrl = "../api/TeamScheduleCommand/CheckOverlapppingCertainActivities";
        var overlappingPeopleList = [];
        var globalDeferred, globalResolveData;

        this.checkOverlappingCertainActivities = checkOverlappingCertainActivities;
        this.getOverlappingAgentList = getOverlappingAgentList;
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

            var overlapped = moment(requestData.StartTime).hours() == 14 && moment(requestData.EndTime).hours() == 15;

            if (overlapped) {
                $http.post(checkOverlappingUrl, requestData).then(function(data) {
                    globalResolveData = data;
                    overlappingPeopleList = fakeData;
                    $rootScope.$broadcast('teamSchedule.overlappingCheckComplete');
                }, function(error) {
                    globalResolveData = data;
                    overlappingPeopleList = fakeData;
                    $rootScope.$broadcast('teamSchedule.overlappingCheckComplete');
                });

            } else {
                $http.post(checkOverlappingUrl, requestData).then(function(data) {
                    overlappingPeopleList = [];
                }, function(error) {
                    globalDeferred.reject(error);
                });

            }
            return globalDeferred.promise;
        }

        function resolvePromise() {
            globalDeferred.resolve(globalResolveData);
        }

        function getOverlappingAgentList() {
            return overlappingPeopleList;
        }
    }
})();