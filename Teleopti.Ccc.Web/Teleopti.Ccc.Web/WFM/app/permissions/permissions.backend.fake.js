'use strict';
(function () {
  angular
    .module('wfm.permissions')
    .service('fakePermissionsBackend', function ($httpBackend) {

      var roles = [];
      var roleInfos = [];
      var role = { DescriptionText: 'test' };
      var applicationFunctions = [];
      var organizationSelection = {};

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

      fakeGet('../api/Permissions/Roles',
        function () {
          return [200, roles];
        });

      fakeGet('../api/Permissions/ApplicationFunctions',
        function () {
          return [200, applicationFunctions];
        });

      fakeGet('../api/Permissions/OrganizationSelection',
        function () {
          return [200, organizationSelection];
        });

      fakeGet(/\.\.\/api\/Permissions\/Roles\/(.*)\/?/,
        function(params2, method, url, data, headers, params) {
          var id = url.substr(-36);
          return [200, roleInfos.find(function(info) { return info.Id === id; })];
        });

      this.setName = function(id, name) {
        var r = roles.find(function(role){
          return role.Id === id;
        })
        if (r != null)  {
          r.DescriptionText = name;
        }
      }

      this.withApplicationFunction = function (applicationFunction) {
        applicationFunctions.push(applicationFunction);
        return this;
      };

      this.withOrganizationSelection = function (businessUnit, dynamicOptions) {
        organizationSelection['BusinessUnit'] = businessUnit;
        organizationSelection['DynamicOptions'] = dynamicOptions;

        return this;
      }

      this.withRole = function (role) {
        roles.push(role);
        return this;
      };

      this.withRoleInfo = function (roleInfo) {
        roleInfos.push(roleInfo);
        return this;
      };

      this.deleteAllAvailableOrgData = function () {
        roleInfos[0].AvailableBusinessUnits = [];
        roleInfos[0].AvailableSites = [];
        roleInfos[0].AvailableTeams = [];
      };

      this.getInfoForSelecteRole = function () {
        return roleInfos[0];
      }

      this.deleteUnselectedOrgData = function(id, type){
        if (type === 'Team' && roleInfos[0].AvailableTeams != null && roleInfos[0].AvailableTeams.length > 0) {
          var toBeDeleted = roleInfos[0].AvailableTeams.find(function(element){
            return element.Id == id;
          });
          roleInfos[0].AvailableTeams.splice(toBeDeleted, 1);
        }
      };

      this.clear = function () {
        roles = [];
        applicationFunctions = [];
        organizationSelection = {};
      };

    });
})();
