(function () {
  'use strict';

  angular
  .module('adminApp')
  .controller('etlConfigController', etlConfigController, ['$http', '$timeout']);

  function etlConfigController($http, tokenHeaderService, $timeout) {
    var vm = this;

    vm.tenants = [];

    (function init(){
      getTenants();
    })();

    function getTenants() {
      $http.get("./Etl/GetTenants", tokenHeaderService.getHeaders())
      .success(function (data) {
        vm.tenants  = data;
      });
    }

  }
})();
