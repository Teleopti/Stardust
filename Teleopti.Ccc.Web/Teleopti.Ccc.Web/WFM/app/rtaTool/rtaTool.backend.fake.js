'use strict';
(function () {
  angular
    .module('wfm.rtaTool')
    .factory('FakeRtaBackend', fakeRtaBackend);

  fakeRtaBackend.$inject = ['$httpBackend'];

  function fakeRtaBackend($httpBackend) {

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
      function () {
        return [200, agents];
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