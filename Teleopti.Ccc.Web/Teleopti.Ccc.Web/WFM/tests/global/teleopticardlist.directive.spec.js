describe('directive: teleoptiCardList', function () {
	
	beforeEach(function(){
		module('wfm');
		module('teleopti.wfm.cardList');
		module('wfm.templates');
	});

	beforeEach(inject(function (_$httpBackend_, _$compile_, _$rootScope_, _$controller_) {

		$compile = _$compile_;
		$rootScope = _$rootScope_;

		$controller = _$controller_;

		var $httpBackend = _$httpBackend_;
		$httpBackend.expectGET("../api/Global/Language?lang=en").respond(200, 'mock');
		$httpBackend.expectGET("../api/Global/User/CurrentUser").respond(200, 'mock');
		
	}));

	it("should render card list directive correctly", function() {

		var element = basicDirectiveSetup();
		expect(element.html()).toContain('class="teleopti-card-list-container');
	});
	
	it("should transclude card header correctly", function () {

		var element = basicDirectiveSetup();

		var headerElement = element.find('card-header');
		expect(headerElement).toBeDefined();
		expect(headerElement.html()).toContain("Fred");
	});

	it("should transclude card body correctly", function() {

		var element = basicDirectiveSetup();

		expect(element.html()).toContain('md-card');
		expect(element.html()).toContain('teleopti-card-content');
		var bodyElement = element.find('card-body');
		expect(bodyElement.html()).toContain('12 Somewhere St');

	});

	function basicDirectiveSetup() {

		var scope = $rootScope.$new();

		scope.People = [{ name: 'Fred', address: '12 Somewhere St' }, { name: 'Bob', address: '13 Erehwon St' }];
		
		var element = "<teleopti-card-list>\
						<teleopti-card ng-repeat='person in People'>\
							<card-header>{{person.name}}</card-header>\
							<card-body>{{person.address}}</card-body>\
						</teleopti-card>\
					</teleopti-card-list>";
		element = $compile(element)(scope);
		scope.$digest();
		return element;
	}
});