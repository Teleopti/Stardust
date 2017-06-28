
$(document).ready(function () {
	var vm,
		ajax,
		fakeActivities = [
			{
				Id: '90EA529A-EEA0-4E22-80AB-9B5E015AB3C6',
				Name: 'Phone'
			},
			{
				Id: 'CAD1BFBA-3A35-46BD-8AC2-9B5E015AB3C6',
				Name: 'Email'
			}
		],
		fakeDefinitionSets = [
			{
				Id: '29F7ECE8-D340-408F-BE40-9BB900B8A4CB',
				Name: 'Overtime paid'
			},
			{
				Id: '9019D62F-0086-44B1-A977-9BB900B8C361',
				Name: 'Overtime time'
			}];

	module('Teleopti.MyTimeWeb.Request.OvertimeRequestViewModel',
		{
			setup: function () {
				setup();
			}
		});

	function setup() {
		setupAjax();
		vm = new Teleopti.MyTimeWeb.Request.OvertimeRequestViewModel(ajax);
	}

	function setupAjax() {
		ajax = {
			Ajax: function (options) {
				if (options.url === '../api/Request/Activities') {
					options.success(fakeActivities);
				}
				if (options.url === '../api/MultiplicatorDefinitionSet/Overtime') {
					options.success(fakeDefinitionSets);
				}
			}
		};
	}

	test('should display activities', function () {
		equal(vm.Activities.length, fakeActivities.length);

		fakeActivities.forEach(function (activity, index) {
			equal(vm.Activities[index].Id, activity.Id);
			equal(vm.Activities[index].Name, activity.Name);
		});
	});

	test('should display overtime types', function () {
		equal(vm.MultiplicatorDefinitionSets.length, fakeDefinitionSets.length);

		fakeDefinitionSets.forEach(function (set, index) {
			equal(vm.MultiplicatorDefinitionSets[index].Id, set.Id);
			equal(vm.MultiplicatorDefinitionSets[index].Name, set.Name);
		});
	});
});