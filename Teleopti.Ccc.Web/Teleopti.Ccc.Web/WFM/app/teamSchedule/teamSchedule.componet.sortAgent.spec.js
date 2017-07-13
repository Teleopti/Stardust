fdescribe('sortAgent component tests', function () {
	var $componentController, $rootScope, $compile;
	beforeEach(module('wfm.templates'));

	beforeEach(function () {
		module("wfm.teamSchedule");
	});

	beforeEach(inject(function (_$componentController_, _$rootScope_, _$compile_) {
		$componentController = _$componentController_;
		$rootScope = _$rootScope_;
		$compile = _$compile_;
	}));

	it('should view all sorting options',function() {
			var ctrl = $componentController('sortAgent');
			ctrl.$onInit();
			expect(!!ctrl.selectedOption).toEqual(false);
			expect(ctrl.availableOptions.length).toEqual(5);
	});

	it('should select one item',function() {
		var ctrl = $componentController('sortAgent');
		ctrl.$onInit();
		ctrl.select(ctrl.availableOptions[0]);
			
		expect(ctrl.selectedOption).toEqual("FirstName");
		expect(ctrl.availableOptions[0].isSelected).toEqual(true);
	});

	it('should only select the latest when changing the selection',function() {
		var ctrl = $componentController('sortAgent');
		ctrl.$onInit();
		ctrl.select(ctrl.availableOptions[0]);
		ctrl.select(ctrl.availableOptions[2]);

		var selected = ctrl.availableOptions.filter(function(item) {
			return item.isSelected;
		});
			
		expect(selected.length).toEqual(1);
		expect(selected[0].key).toEqual(ctrl.availableOptions[2].key);
		expect(ctrl.selectedOption).toEqual(ctrl.availableOptions[2].key);
	});


	it('should emit sort option update event if the selected option is changed', function() {
		var selectedOption = '';
		var scope = $rootScope.$new();
		scope.$on('teamSchedule.update.sortOption',
			function (e, d) {
				selectedOption = d.option;
			});
		var ctrl = $componentController('sortAgent', {$scope:scope});
		ctrl.$onInit();
		ctrl.select(ctrl.availableOptions[0]);

		expect(selectedOption).toEqual(ctrl.availableOptions[0].key);
	});

	it('should init selected sort option when there is option', function () {
		var ctrl = $componentController('sortAgent', null, { ngModel: "StartTime" });
		ctrl.$onInit();
		var selected = ctrl.availableOptions.filter(function (item) {
			return item.isSelected;
		});
		expect(selected.length).toEqual(1);
		expect(selected[0].key).toEqual("StartTime");
		expect(ctrl.selectedOption).toEqual("StartTime");
	});

	it('should deselect the sort option when select the same option again', function() {
		var ctrl = $componentController('sortAgent');
		ctrl.$onInit();
		ctrl.select(ctrl.availableOptions[0]);
		ctrl.select(ctrl.availableOptions[0]);

		var selected = ctrl.availableOptions.filter(function (item) {
			return item.isSelected;
		});

		expect(selected.length).toEqual(0);
		expect(!!ctrl.selectedOption).toEqual(false);
	});

});