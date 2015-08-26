(function() {
    'use strict';

    angular.module('outboundServiceModule').service('outboundActivityService', [
        '$http', '$q', outboundActivityService]);

    function outboundActivityService($http, $q) {

        var listActivityCommandUrl = '../api/Outbound/Campaign/Activities';

        this.listActivity = function () {
            var deferred = $q.defer();
            $http.get(listActivityCommandUrl).success(function (data) {
                deferred.resolve(data);
            });
            return deferred.promise;
        };

        this.nullActivity = function () {
            return {
                Id: null,
                Name: ''
            };
        };
    }


})();