angular
	.module('createForecastReadModelModule')
	.factory('createForecastReadModelService', createForecastReadModelService);

createForecastReadModelService.$inject = ['$http'];

function createForecastReadModelService($http) {
	return {
		createForecastReadModel: createForecastReadModel
	};

	function createForecastReadModel(tenant, startDate, endDate) {
		return $http.post('/Stardust/TriggerSkillForecastCalculation', {"Tenant": tenant, "StartDate": startDate, "EndDate": endDate})
			.then(createForecastReadModelComplete)
			.catch(createForecastReadModelFailed);

		function createForecastReadModelComplete(response) {
			return response.data;
		}

		function createForecastReadModelFailed(error) {
			console.log('XHR Failed .' + error.data);
		}
	}
}