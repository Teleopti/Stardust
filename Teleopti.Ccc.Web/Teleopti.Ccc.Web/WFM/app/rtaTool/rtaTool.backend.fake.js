'use strict';
(function () {
  angular
    .module('wfm.rtaTool')
    .factory('FakeRtaToolBackend', FakeRtaToolBackend);

  FakeRtaToolBackend.$inject = ['$httpBackend'];

  function FakeRtaToolBackend($httpBackend) {

    var service = {
      clear: clear,
      withAgent: withAgent,
      withPhoneState: withPhoneState
    };

    ///////////////////////////////

    var agents = [];
    var phoneStates = [];

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

    var fake = function (url, response) {
      $httpBackend.whenGET(url)
        .respond(function (method, url, data, headers, params) {
          var params2 = paramsOf(url);
          return response(params2, method, url, data, headers, params);
        });
    };

    fake(/ToggleHandler\/AllToggles(.*)/,
      function () {
        return [200, []];
      });

    fake(/\.\.\/RtaTool\/PhoneStates\/For/,
      function () {
        return [200, phoneStates];
      });

    fake(/\.\.\/RtaTool\/Agents\/For(.*)/,
      function (params) {
        var ret = (function () {
          if (params.siteIds != null) {
            var agentsBySite = agents.filter(function (a) {
              return params.siteIds.indexOf(a.SiteId) >= 0
            });
            return agentsBySite;
          }
          if (params.teamIds != null) {
            var agentsByTeam = agents.filter(function (a) {
              return params.teamIds.indexOf(a.TeamId) >= 0
            });
            return agentsByTeam;
          }
          return agents;
        })();     
        return [200, ret];
      });

    function withAgent(agent) {
      agents.push(agent);
      return this;
    }

    function withPhoneState(phoneState) {
      phoneStates.push(phoneState);
      return this;
    }

    function clear() {
      agents = [];
      phoneStates = [];
    }


    return service;
  };
})();