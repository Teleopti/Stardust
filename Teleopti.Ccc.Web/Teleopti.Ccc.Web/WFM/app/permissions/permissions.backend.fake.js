'use strict';
(function () {
  angular
    .module('wfm.permissions')
    .service('fakePermissionsBackend', function ($httpBackend) {

      var roles = [];
      var role = { DescriptionText: 'test' };

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

      fakeGet(/\.\.\/api\/Permissions(.*)/,
        function () {
          return [200, roles];
        });

      this.showRoles = function () {
        return roles;
      };

      this.editName = function(name){
        role.DescriptionText = name;
        return this;
      }

      this.clear = function () {
        roles = [];
      };

      this.withRole = function (role) {
        roles.push(role);
        return this;
      };

    });
})();
