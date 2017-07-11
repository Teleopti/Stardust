'use strict';

describe('rtaOverviewComponent', function () {
  var $controllerBuilder,
    $fakeBackend,
    $componentController,
    $httpBackend;

  var ctrl,
    scope,
    stateParams,
    vm;

  var
    channelSales,
    phone,
    invoice,
    bts;

  var
    skills1,
    skills2,
    allSkills;

  var
    skillArea1,
    skillArea2;

  var skillAreas;

  var goodColor = '#C2E085';
  var warningColor = '#FFC285';
  var dangerColor = '#EE8F7D';

  beforeEach(function () {
    module('wfm.rta');
  });

  beforeEach(function () {
    module(function ($provide) {
      $provide.factory('$stateParams', function () {
        stateParams = {};
        return stateParams;
      });
    });
  });

  beforeEach(inject(function (_ControllerBuilder_, _FakeRtaBackend_, _$componentController_, _$httpBackend_) {
    $controllerBuilder = _ControllerBuilder_;
    $fakeBackend = _FakeRtaBackend_;
    $componentController = _$componentController_;
    $httpBackend = _$httpBackend_;

    $fakeBackend.clear();
    scope = $controllerBuilder.setup('RtaMainController');


    channelSales = {
      Name: 'Channel Sales',
      Id: 'channelSalesId'
    };

    phone = {
      Name: 'Phone',
      Id: 'phoneId'
    };

    invoice = {
      Name: 'Invoice',
      Id: 'invoiceId'
    };

    bts = {
      Name: 'BTS',
      Id: 'btsId'
    };

    skills1 = [channelSales, phone];
    skills2 = [invoice, bts];
    allSkills = [channelSales, phone, invoice, bts];

    skillArea1 = {
      Id: 'skillArea1Id',
      Name: 'SkillArea1',
      Skills: skills1
    };
    skillArea2 = {
      Id: 'skillArea2Id',
      Name: 'SkillArea2',
      Skills: skills2
    };
    skillAreas = [skillArea1, skillArea2];

  }));

  it('should display site card data', function () {
    $fakeBackend
      .withSiteAdherence({
        Id: 'londonId',
        Name: 'London',
        AgentsCount: 11,
        InAlarmCount: 5,
        Color: 'warning'
      });

    vm = $controllerBuilder.createController().vm;
    ctrl = $componentController('rtaOverviewComponent', null, {
      siteCards: vm.siteCards
    });

    expect(ctrl.siteCards[0].site.Id).toEqual('londonId');
    expect(ctrl.siteCards[0].site.Name).toEqual('London');
    expect(ctrl.siteCards[0].site.AgentsCount).toEqual(11);
    expect(ctrl.siteCards[0].site.InAlarmCount).toEqual(5);
    expect(ctrl.siteCards[0].site.Color).toEqual(warningColor);
  });

  it('should display site card with skill data', function () {
    stateParams.skillIds = ['channelSalesId'];
    $fakeBackend
      .withSiteAdherence({
        Id: 'londonId',
        Name: 'London',
        SkillId: 'channelSalesId',
        AgentsCount: 11,
        InAlarmCount: 5,
        Color: 'warning'
      })
      .withSiteAdherence({
        Id: 'parisId',
        Name: 'Paris',
        SkillId: 'phoneId',
        AgentsCount: 10,
        InAlarmCount: 5,
        Color: 'warning'
      });

    vm = $controllerBuilder.createController().vm;
    $httpBackend.flush();
    ctrl = $componentController('rtaOverviewComponent', null, {
      siteCards: vm.siteCards
    });

    expect(ctrl.siteCards.length).toEqual(1);
    expect(ctrl.siteCards[0].site.Id).toEqual('londonId');
    expect(ctrl.siteCards[0].site.Name).toEqual('London');
    expect(ctrl.siteCards[0].site.AgentsCount).toEqual(11);
    expect(ctrl.siteCards[0].site.InAlarmCount).toEqual(5);
    expect(ctrl.siteCards[0].site.Color).toEqual(warningColor);
  });

  it('should display site card with skill area data', function () {
    stateParams.skillAreaId = 'skillArea1Id';
    $fakeBackend
      .withSiteAdherence({
        Id: 'londonId',
        SkillId: 'channelSalesId',
        Name: 'London',
        AgentsCount: 11,
        InAlarmCount: 2,
        Color: 'good'
      })
      .withSiteAdherence({
        Id: 'parisId',
        SkillId: 'invoiceId',
        Name: 'Paris',
        AgentsCount: 7,
        InAlarmCount: 2,
        Color: 'good'
      });

    vm = $controllerBuilder.createController(undefined, skillAreas).vm;
    ctrl = $componentController('rtaOverviewComponent', null, {
      siteCards: vm.siteCards
    });

    expect(ctrl.siteCards.length).toEqual(1);
    expect(ctrl.siteCards[0].site.Id).toEqual('londonId');
    expect(ctrl.siteCards[0].site.Name).toEqual('London');
    expect(ctrl.siteCards[0].site.AgentsCount).toEqual(11);
    expect(ctrl.siteCards[0].site.InAlarmCount).toEqual(2);
    expect(ctrl.siteCards[0].site.Color).toEqual(goodColor);
  });

  it('should display closed site cards by default', function () {
    $fakeBackend
      .withSiteAdherence({
        Id: 'londonId',
        Name: 'London',
        AgentsCount: 11,
        InAlarmCount: 5,
        Color: 'warning'
      });

    var vm = $controllerBuilder.createController().vm;
    ctrl = $componentController('rtaOverviewComponent', null, {
      siteCards: vm.siteCards
    });

    expect(ctrl.siteCards[0].isOpen).toEqual(false);
  });

  it('should display open site cards when open in url', function () {
    stateParams.open = 'true';
    $fakeBackend
      .withSiteAdherence({
        Id: 'londonId',
        Name: 'London',
        AgentsCount: 11,
        InAlarmCount: 5,
        Color: 'warning'
      });

    var vm = $controllerBuilder.createController().vm;
    ctrl = $componentController('rtaOverviewComponent', null, {
      siteCards: vm.siteCards
    });

    expect(ctrl.siteCards[0].isOpen).toEqual(true);
  });

  it('should display team data in site card when clicked', function () {
    $fakeBackend
      .withSiteAdherence({
        Id: 'londonId',
        Name: 'London',
        AgentsCount: 11,
        InAlarmCount: 5,
        Color: 'warning'
      })
      .withTeamAdherence({
        SiteId: 'londonId',
        Name: 'Green',
        AgentsCount: 11,
        InAlarmCount: 5,
        Color: 'warning'
      });

    var vm = $controllerBuilder.createController().vm;
    ctrl = $componentController('rtaOverviewComponent', null, {
      siteCards: vm.siteCards
    });

    ctrl.siteCards[0].isOpen = true;
    ctrl.siteCards[0].fetchTeamData(ctrl.siteCards[0]);
    $httpBackend.flush();

    expect(ctrl.siteCards[0].teams[0].SiteId).toEqual('londonId');
    expect(ctrl.siteCards[0].teams[0].Name).toEqual('Green');
    expect(ctrl.siteCards[0].teams[0].AgentsCount).toEqual(11);
    expect(ctrl.siteCards[0].teams[0].InAlarmCount).toEqual(5);
    expect(ctrl.siteCards[0].teams[0].Color).toEqual('warning');
  });

  //  it('should display team data in site card with skill when clicked', function () {
  //   stateParams.skillIds = ['channelSalesId'];
  //   $fakeBackend
  //     .withSiteAdherence({
  //       Id: 'londonId',
  //       SkillId: 'channelSalesId',
  //       Name: 'London',
  //       AgentsCount: 11,
  //       InAlarmCount: 5,
  //       Color: 'warning'
  //     })
  //     .withTeamAdherence({
  //       SiteId: 'londonId',
  //       SkillId: 'channelSalesId',
  //       Name: 'Green',
  //       AgentsCount: 11,
  //       InAlarmCount: 5,
  //       Color: 'warning'
  //     });
  //   var vm = $controllerBuilder.createController().vm;
  //   ctrl = $componentController('rtaOverviewComponent', null, {
  //     siteCards: vm.siteCards
  //   });
  //   $httpBackend.flush();

  //   ctrl.siteCards[0].isOpen = true;
  //   ctrl.siteCards[0].fetchTeamData(ctrl.siteCards[0]);
  //   $httpBackend.flush();

  //   expect(ctrl.siteCards[0].teams[0].SiteId).toEqual('londonId');
  //   expect(ctrl.siteCards[0].teams[0].Name).toEqual('Green');
  //   expect(ctrl.siteCards[0].teams[0].AgentsCount).toEqual(11);
  //   expect(ctrl.siteCards[0].teams[0].InAlarmCount).toEqual(5);
  //   expect(ctrl.siteCards[0].teams[0].Color).toEqual('warning');
  // });

  it('should update team adherence when site is open', function () {
    $fakeBackend
      .withSiteAdherence({
        Id: 'londonId',
        Name: 'London',
        AgentsCount: 11,
        InAlarmCount: 5,
        Color: 'warning'
      })
      .withTeamAdherence({
        SiteId: 'londonId',
        Name: 'Green',
        AgentsCount: 11,
        InAlarmCount: 5,
        Color: 'warning'
      });

    var c = $controllerBuilder.createController();
    vm = c.vm;
    ctrl = $componentController('rtaOverviewComponent', null, {
      siteCards: vm.siteCards
    });
    ctrl.siteCards[0].isOpen = true;
    ctrl.siteCards[0].fetchTeamData(ctrl.siteCards[0]);
    $httpBackend.flush();
    c.apply(function () {
      $fakeBackend.clearTeamAdherences()
        .withTeamAdherence({
          SiteId: 'londonId',
          Name: 'Green',
          AgentsCount: 11,
          InAlarmCount: 2,
          Color: 'good'
        });
    })
      .wait(5000);

    expect(vm.siteCards[0].teams[0].InAlarmCount).toEqual(2);
    expect(vm.siteCards[0].teams[0].Color).toEqual('good');
  });

  it('should not update team adherence when site is closed', function () {
    $fakeBackend
      .withSiteAdherence({
        Id: 'londonId',
        Name: 'London',
        AgentsCount: 11,
        InAlarmCount: 5,
        Color: 'warning'
      })
      .withTeamAdherence({
        SiteId: 'londonId',
        AgentsCount: 11,
        InAlarmCount: 5,
        Color: 'warning'
      });

    var c = $controllerBuilder.createController();
    vm = c.vm;
    ctrl = $componentController('rtaOverviewComponent', null, {
      siteCards: vm.siteCards
    });
    ctrl.siteCards[0].isOpen = true;
    ctrl.siteCards[0].fetchTeamData(ctrl.siteCards[0]);
    $httpBackend.flush();
    c.apply(function () {
      $fakeBackend
        .clearTeamAdherences()
        .withTeamAdherence({
          SiteId: 'londonId',
          Name: 'Green',
          AgentsCount: 11,
          InAlarmCount: 2,
          Color: 'good'
        });
    })
      .wait(5000);

    ctrl.siteCards[0].isOpen = false;
    ctrl.siteCards[0].fetchTeamData(ctrl.siteCards[0]);

    c.apply(function () {
      $fakeBackend
        .clearTeamAdherences()
        .withTeamAdherence({
          SiteId: 'londonId',
          Name: 'Green',
          AgentsCount: 11,
          InAlarmCount: 10,
          Color: 'danger'
        });
    })
      .wait(5000);

    expect(ctrl.siteCards[0].teams[0].InAlarmCount).toEqual(2);
    expect(ctrl.siteCards[0].teams[0].Color).toEqual('good');
  });


  //IMPORTANT TEST!! FIX ME!

  // it('should not update teams adherence when sites are closed', function () {
  //   $fakeBackend
  //     .withSiteAdherence({
  //       Id: "londonGuid",
  //       Name: "London",
  //       AgentsCount: 11,
  //       InAlarmCount: 5,
  //       Color: "warning"
  //     })
  //     .withSiteAdherence({
  //       Id: "parisGuid",
  //       Name: "Paris",
  //       AgentsCount: 10,
  //       InAlarmCount: 4,
  //       Color: "warning"
  //     });

  //   var c = $controllerBuilder.createController();
  //   vm = c.vm;
  //   ctrl = $componentController('rtaOverviewComponent', null, {
  //     siteCards: vm.siteCards
  //   });

  //   ctrl.siteCards[0].isOpen = true;
  //   ctrl.siteCards[0].fetchTeamData(ctrl.siteCards[0]);
  //   $fakeBackend
  //     .withTeamAdherence({
  //       SiteId: "londonGuid",
  //       AgentsCount: 11,
  //       InAlarmCount: 5,
  //       Color: "warning"
  //     });
  //   $httpBackend.flush(); 
  //   ctrl.siteCards[1].isOpen = true;
  //   ctrl.siteCards[1].fetchTeamData(ctrl.siteCards[1]);
  //   $fakeBackend
  //     .clearTeamAdherences()
  //     .withTeamAdherence({
  //       SiteId: "parisGuid",
  //       AgentsCount: 10,
  //       InAlarmCount: 4,
  //       Color: "warning"
  //     });
  //   $httpBackend.flush();
  //   c.apply(function () {
  //     $fakeBackend
  //       .clearTeamAdherences()
  //       .withTeamAdherence({
  //         SiteId: "londonGuid",
  //         Name: "Green",
  //         AgentsCount: 11,
  //         InAlarmCount: 10,
  //         Color: "danger"
  //       });
  //   })
  //     .wait(5000);

  //   console.log(ctrl.siteCards);

  //   expect(ctrl.siteCards[0].teams[0].InAlarmCount).toEqual(2);
  //   expect(ctrl.siteCards[0].teams[0].Color).toEqual("good");
  //   expect(ctrl.siteCards[1].teams[0].InAlarmCount).toEqual(0);
  //   expect(ctrl.siteCards[1].teams[0].Color).toEqual("good");
  // });

});
