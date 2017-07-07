fdescribe('sortAgent component tests', function () {
	var $componentController;
	beforeEach(function () {
		module("wfm.teamSchedule");
		module(function ($provide) {
			
		});
	});

	beforeEach(inject(function (_$componentController_) {
		$componentController = _$componentController_;
	}));

	it('should view all sorting options',function() {
			var bindings = {};
			var ctrl = $componentController('sortAgent', null, bindings);
			ctrl.$onInit();
			expect(ctrl.selectedOption).toEqual("");
			expect(ctrl.availableOptions.length).toEqual(5);
	});

	it('should select one item',function() {
		var bindings = {};
		var ctrl = $componentController('sortAgent', null, bindings);
		ctrl.$onInit();
		ctrl.select(ctrl.availableOptions[0]);
			
		expect(ctrl.selectedOption).toEqual("FirstName");
		expect(ctrl.availableOptions[0].isSelected).toEqual(true);
	});

	it('should only select the latest when changing the selection',function() {
		var bindings = {};
		var ctrl = $componentController('sortAgent', null, bindings);
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


	it('should call onUpdate if the selected option is changed', function() {
		var isUpdateCalled = false;
		var bindings = {
			onUpdate: function() {
				isUpdateCalled = true;
			}
		};
		var ctrl = $componentController('sortAgent', null, bindings);
		ctrl.$onInit();
		ctrl.select(ctrl.availableOptions[0]);

		expect(isUpdateCalled).toEqual(true);
	});

	it('should keep the selected sorting option when switching from week view to day view', function() {
		var bindings = {};
		var ctrl = $componentController('sortAgent', null, bindings);
		ctrl.$onInit();
		ctrl.select(ctrl.availableOptions[2]);
		var selected = ctrl.availableOptions.filter(function(item) {
			return item.isSelected;
		});
		expect(selected.length).toEqual(1);
		expect(selected[0].key).toEqual(ctrl.availableOptions[2].key);

		//not finished yet


	});


});