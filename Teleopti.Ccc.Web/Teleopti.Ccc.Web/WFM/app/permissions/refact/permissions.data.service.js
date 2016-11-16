(function () {
  'use strict';

  angular
  .module('wfm.permissions')
  .service('permissionsDataService', permissionsDataService);

  permissionsDataService.$inject = ['PermissionsServiceRefact'];

  function permissionsDataService(PermissionsServiceRefact) {
    this.setSelectedRole = setSelectedRole;
    this.getSelectedRole = getSelectedRole;
    this.selectFunction = selectFunction;
    this.selectOrganization = selectOrganization;
    this.prepareData = prepareData;
    var selectedRole;

    function selectFunction (selectedRole, functions) {
      // var deferred = $q.defer();
      console.log('resultgfagdasgdagadgdagda');

      PermissionsServiceRefact.postFunctions.query({ Id: selectedRole.Id, Functions: functions }).$promise.then(function (result) {
        // deferred.resolve();
      });

      // return deferred.promise;
    }
    var data = {};

    function arrayCreator(orgData){
      var attributeName = orgData.Type + 's';
      if (!data[attributeName]) {
        data[attributeName]= [];
      }
      data[attributeName].push(orgData.Id);

      if(orgData.ChildNodes != null && orgData.ChildNodes.length > 0) {
        for (var i = 0; i < orgData.ChildNodes.length; i++) {
          arrayCreator(orgData.ChildNodes[i]);
        }
      }
      return data;
    }

    function prepareData(orgData) {
      var preparedObject = arrayCreator(orgData);
      preparedObject.Id = selectedRole.Id;

      return preparedObject;
    }

    function selectOrganization(orgData){
      var data = prepareData(orgData);
      PermissionsServiceRefact.assignOrganizationSelection.postData(data).$promise.then(function(result){
        if (result != null)
          console.log('result ',result);
      });
    }

    function setSelectedRole(role) {
      selectedRole = role;
    }

    function getSelectedRole() {
      return selectedRole;
    }

  }
})();
