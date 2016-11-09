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
        var selectedRole;

        function selectFunction (selectedRole, functions) {
            // var deferred = $q.defer();
            PermissionsServiceRefact.postFunctions.query({ Id: selectedRole.Id, Functions: functions }).$promise.then(function (result) {
                console.log('result ', result);
                // deferred.resolve();
            });

            // return deferred.promise;
        }

        function setSelectedRole(role) {
            selectedRole = role;
        }

        function getSelectedRole() {
            return selectedRole;
        }

    }
})();