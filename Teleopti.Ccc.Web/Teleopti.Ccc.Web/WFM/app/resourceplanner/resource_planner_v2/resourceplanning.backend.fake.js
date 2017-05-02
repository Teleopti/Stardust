'use strict';
(function () {
    angular
        .module('wfm.resourceplanner')
        .service('fakeResourcePlanningBackend', function ($httpBackend) {

            var service = {
                clear : clear,
            }

            var filterResults = [];

            var paramsOf = function (url) {
                var result = {};
                var queryString = url.split("?")[1];
                if (queryString == null) {
                    return result;
                }
                var params = queryString.split("&");
                angular.forEach(params, function (t) {
                    var kvp = t.split("=");
                    if (result[kvp[0]] != null)
                        result[kvp[0]] = [].concat(result[kvp[0]], kvp[1]);
                    else
                        result[kvp[0]] = kvp[1];
                });
                return result;
            };

            var fakeGet = function (url, response) {
                $httpBackend.whenGET(url)
                    .respond(function (method, url, data, headers, params) {
                        var params2 = paramsOf(url);
                        return response(params2, method, url, data, headers, params);
                    });
            };

            function clear () {       
                filterResults = [];
            };

            return service;
        });
})();
