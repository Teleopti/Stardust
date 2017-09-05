'use strict';

describe('RtaOverviewController open/close site', function () {

  beforeEach(module('wfm.rta'));

  var $stateParams;
  var $fakeBackend;
  
  beforeEach(function () {
    module(function ($provide) {
      $provide.factory('$stateParams', function () {
        $stateParams = {};
        return $stateParams;
      });
      $provide.factory('skills', function () {
        return $fakeBackend.skills;
      });
      $provide.factory('skillAreas', function () {
        return $fakeBackend.skillAreas;
      });
    });
  });
  
  var
    $controllerBuilder,
    $scope,
    $state;
  beforeEach(inject(function (_FakeRtaBackend_, _ControllerBuilder_, _$state_) {
    $controllerBuilder = _ControllerBuilder_;
    $fakeBackend = _FakeRtaBackend_;
		$fakeBackend.clear();
    $state = _$state_;
    $state.current.name = 'rta-refact';
    $scope = $controllerBuilder.setup('RtaOverviewController39082');
  }));

  it('should open site', function () {
    $fakeBackend
      .withSiteAdherence({
        Id: 'parisId'
      })
      .withTeamAdherence({
        SiteId: 'parisId',
        Id: 'redId'
      });
    var c = $controllerBuilder.createController();
    var vm = c.vm;

    c.apply(function () {
      vm.siteCards[0].isOpen = true;
    });

    expect(vm.siteCards[0].isOpen).toEqual(true);
    expect(vm.siteCards[0].teams[0].Id).toEqual("redId");
  });

  it('should close site', function () {
    $fakeBackend
      .withSiteAdherence({
        Id: 'parisId'
      })
      .withTeamAdherence({
        SiteId: 'parisId',
        Id: 'redId'
      });
    var c = $controllerBuilder.createController();
    var vm = c.vm;

    c.apply(function () {
      vm.siteCards[0].isOpen = true;
    });
    c.apply(function () {
      vm.siteCards[0].isOpen = false;
    });

    expect(vm.siteCards[0].isOpen).toEqual(false);
  });

  it('should close site with selected team', function () {
    $stateParams.teamIds = ['redId'];
    $fakeBackend
      .withSiteAdherence({
        Id: 'parisId'
      })
      .withTeamAdherence({
        SiteId: 'parisId',
        Id: 'redId'
      });
    var c = $controllerBuilder.createController();
    var vm = c.vm;

    c.apply(function () {
      vm.siteCards[0].isOpen = false;
    });

    expect(vm.siteCards[0].isOpen).toEqual(false);
  });

});
