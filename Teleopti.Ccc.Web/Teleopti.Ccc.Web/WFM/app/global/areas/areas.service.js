(function() {
    'use strict';

    angular
        .module('wfm.areas') //restAreasService
        .factory('areasService', areasService); //AreasSvrc

    areasService.$inject = ['$resource'];

    /* @ngInject */
    function areasService($resource) {
      	var areas;
        var service = {
            getAreas: getAreas
        };

        return service;

        function getAreas(){
    			if(!areas){
    				areas = getAreasFromServer();
    			}
    			return areas;
    		}

        function getAreasFromServer(){
          return $resource('../api/Global/Application/Areas', {}, {
      			query: {
      				method: 'GET',
      				params: {},
      				isArray: true
      			}
      		}).query().$promise;

        }
    }
})();
