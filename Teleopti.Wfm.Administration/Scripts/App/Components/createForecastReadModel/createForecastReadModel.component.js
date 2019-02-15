function createForecastReadModel($scope, tenantService, createForecastReadModelService) {
	var vm = this;

	vm.startDate = moment().subtract(8, 'd');
	vm.endDate = moment().add(56, 'd');

	vm.tenants = [];
	vm.selectedTenantName = '';

	vm.selectTenant = function(name) {
		vm.selectedTenantName = name;
	}

	vm.triggerCreateForecastReadModel = function() {
		createForecastReadModelService.createForecastReadModel(vm.selectedTenantName, vm.startDate, vm.endDate).then(function(data) {
			console.log(data);
		});
	}

	tenantService.getTenants().then(function(data) {
		vm.tenants = data;
	});
}

createForecastReadModel.$inject = ['$scope', 'tenantService', 'createForecastReadModelService'];

angular.module('createForecastReadModelModule').component('createForecastReadModel', {
	templateUrl: './Scripts/App/Components/createForecastReadModel/createForecastReadModel.template.html',
	controller: createForecastReadModel,
	bindings: {
		
	}
});