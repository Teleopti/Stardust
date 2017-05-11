'use strict';
(function () {
    angular
        .module('wfm.resourceplanner')
        .service('fakeResourcePlanningBackend', function ($httpBackend) {

            var LastScheduleResult = {};
            var LastScheduleStatus = {};
            var LastIntradayStatus = {};
            var planningPeriods = [];
            
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

            this.clear = function () {
                LastScheduleResult = {};
                LastScheduleStatus = {};
                LastIntradayStatus = {};
                planningPeriods = [];
            };

            fakeGet('../api/resourceplanner/agentgroup/aad945dd-be2c-4c6a-aa5b-30f3e74dfb5e/planningperiods',
                function () {
                    return [200, planningPeriods];
                });

            fakeGet('../api/resourceplanner/planningperiod/a557210b-99cc-4128-8ae0-138d812974b6/result',
                function () {
                    return [200, LastScheduleResult];
                });

            fakeGet('../api/resourceplanner/planningperiod/a557210b-99cc-4128-8ae0-138d812974b6/status',
                function () {
                    return [200, LastScheduleStatus];
                });
            
            fakeGet('../api/resourceplanner/planningperiod/a557210b-99cc-4128-8ae0-138d812974b6/intradaystatus',
                function () {
                    return [200, LastIntradayStatus];
                });

            this.withPlanningPeriods = function(planningPeriod) {
                planningPeriods.push(planningPeriod);
                return this;
            };

            this.withScheduleResult = function (fakeResult) {
                LastScheduleResult = fakeResult;
                return this;
            };

            this.withScheduleStatus = function (fakeResult) {
                LastScheduleStatus = fakeResult;
                return this;
            }

            this.withIntradayStatus = function (fakeResult) {
                LastIntradayStatus  = fakeResult;
                return this;
            }

        });
})();
