'use strict';
describe('BusinessUnitsCtrl', function() {
	var $httpBackend,
			$controller;

  beforeEach(function () {
		module('wfm.businessunits');
	});

	beforeEach(inject(function (_$httpBackend_, _$controller_) {
		$httpBackend = _$httpBackend_;
		$controller = _$controller_;
	}));

  it('should get one businessunit without selected businessunit', inject(function(){
    var businessunits = [{Id:"928dd0bc-bf40-412e-b970-9b5e015aadea", Name:"Demo"}];
    var vm = $controller('BusinessUnitsCtrl');
    $httpBackend.whenGET('../api/BusinessUnit').respond(function(method,url,data)
		{
	    return [200, businessunits];
  	});

    vm.loadBusniessUnit();
    $httpBackend.flush();

    expect(vm.data.businessUnits[0].Name).toEqual('Demo');
  }));

  it('should change businessUnit by selected businessUnit', inject(function($window){
    var businessunits = [{Id:"928dd0bc-bf40-412e-b970-9b5e015aadea",Name:"Demo"},{Id:"928dd0bc-h24k-412e-jgl4-ahej542lzjej",Name:"Demo Changed"}];
    var selectedBu = {Id:"928dd0bc-h24k-412e-jgl4-ahej542lzjej",Name:"Demo Changed"};
    var vm = $controller('BusinessUnitsCtrl');
    $httpBackend.whenGET('../api/BusinessUnit').respond(function(method,url,data)
		{
	    return [200, businessunits];
  	});
    vm.changeBusinessUnit(selectedBu);
    vm.loadBusniessUnit();
    $httpBackend.flush();

    expect(vm.data.selectedBu.Name).toEqual('Demo Changed');
    $window.stop();
  }));
});
