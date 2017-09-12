'use strict';

describe('rtaOverviewComponent', function () {
  var $controllerBuilder,
    $fakeBackend,
    $componentController,
    $httpBackend,
    $state,
    $sessionStorage;

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
      $provide.factory('skills', function () {
        return $fakeBackend.skills;
      });
      $provide.factory('skillAreas', function () {
        return $fakeBackend.skillAreas;
      });
    });
  });

  beforeEach(inject(function (_ControllerBuilder_, _FakeRtaBackend_, _$componentController_, _$httpBackend_, _$state_, _$sessionStorage_) {
    $controllerBuilder = _ControllerBuilder_;
    $fakeBackend = _FakeRtaBackend_;
    $componentController = _$componentController_;
    $httpBackend = _$httpBackend_;
    $state = _$state_;
    $sessionStorage = _$sessionStorage_;

    spyOn($state, 'go');
    // remove with RTA_RememberMyPartOfTheBusiness_39082
    scope = $controllerBuilder.setup('RtaOverviewController');


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

    $fakeBackend.clear();
    allSkills.forEach(function (skill) { $fakeBackend.withSkill(skill); });
    $fakeBackend.withSkillAreas(skillAreas);

  }));

  afterEach(function () {
		$sessionStorage.$reset();
	});

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
        Id: 'londonId'
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
        Id: 'londonId'
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

    var c = $controllerBuilder.createController();
    var vm = c.vm;
    ctrl = $componentController('rtaOverviewComponent', null, {
      siteCards: vm.siteCards
    });

    c.apply(function () {
      ctrl.siteCards[0].isOpen = true;
    });

    expect(ctrl.siteCards[0].teams[0].SiteId).toEqual('londonId');
    expect(ctrl.siteCards[0].teams[0].Name).toEqual('Green');
    expect(ctrl.siteCards[0].teams[0].AgentsCount).toEqual(11);
    expect(ctrl.siteCards[0].teams[0].InAlarmCount).toEqual(5);
    expect(ctrl.siteCards[0].teams[0].Color).toEqual('warning');
  });

  it('should display team data in site card with skill when clicked', function () {
    stateParams.skillIds = ['channelSalesId'];
    $fakeBackend
      .withSiteAdherence({
        Id: 'londonId',
        SkillId: 'channelSalesId',
        Name: 'London',
        AgentsCount: 11,
        InAlarmCount: 5,
        Color: 'warning'
      })
      .withTeamAdherence({
        Id: 'greenId',
        SiteId: 'londonId',
        SkillId: 'channelSalesId',
        Name: 'Green',
        AgentsCount: 11,
        InAlarmCount: 5,
        Color: 'warning'
      })
      .withTeamAdherence({
        Id: 'redId',
        SiteId: 'londonId',
        SkillId: 'invoiceId',
        Name: 'Red',
        AgentsCount: 8,
        InAlarmCount: 4,
        Color: 'warning'
      });
    var c = $controllerBuilder.createController();
    var vm = c.vm;
    ctrl = $componentController('rtaOverviewComponent', null, {
      siteCards: vm.siteCards
    });

    c.apply(function () {
      ctrl.siteCards[0].isOpen = true;
    });

    expect(ctrl.siteCards[0].teams.length).toEqual(1);
    expect(ctrl.siteCards[0].teams[0].SiteId).toEqual('londonId');
    expect(ctrl.siteCards[0].teams[0].Name).toEqual('Green');
    expect(ctrl.siteCards[0].teams[0].AgentsCount).toEqual(11);
    expect(ctrl.siteCards[0].teams[0].InAlarmCount).toEqual(5);
    expect(ctrl.siteCards[0].teams[0].Color).toEqual('warning');
  });

  it('should update team adherence when site is open', function () {
    $fakeBackend
      .withSiteAdherence({
        Id: 'londonId',
        InAlarmCount: 5,
        Color: 'warning'
      })
      .withTeamAdherence({
        SiteId: 'londonId',
        Id: 'greenId',
        InAlarmCount: 5,
        Color: 'warning'
      });

    var c = $controllerBuilder.createController();
    vm = c.vm;
    ctrl = $componentController('rtaOverviewComponent', null, {
      siteCards: vm.siteCards
    });

    c.apply(function () {
      ctrl.siteCards[0].isOpen = true;
    });
    c.apply(function () {
      $fakeBackend
        .clearTeamAdherences()
        .withTeamAdherence({
          SiteId: 'londonId',
          Id: 'greenId',
          InAlarmCount: 2,
          Color: 'good'
        });
    })
      .wait(5000);

    expect(ctrl.siteCards[0].teams[0].InAlarmCount).toEqual(2);
    expect(ctrl.siteCards[0].teams[0].Color).toEqual('good');
  });

  it('should not update team adherence when site is closed', function () {
    $fakeBackend
      .withSiteAdherence({
        Id: 'londonId',
        InAlarmCount: 5,
        Color: 'warning'
      })
      .withTeamAdherence({
        SiteId: 'londonId',
        InAlarmCount: 5,
        Color: 'warning'
      });

    var c = $controllerBuilder.createController();
    vm = c.vm;
    ctrl = $componentController('rtaOverviewComponent', null, {
      siteCards: vm.siteCards
    });

    c.apply(function () {
      ctrl.siteCards[0].isOpen = true;
    });
    $fakeBackend
      .clearTeamAdherences()
      .withTeamAdherence({
        SiteId: 'londonId',
        InAlarmCount: 2,
        Color: 'good'
      });
    c.wait(5000);
    c.apply(function () {
      ctrl.siteCards[0].isOpen = false;
    });
    $fakeBackend
      .clearTeamAdherences()
      .withTeamAdherence({
        SiteId: 'londonId',
        InAlarmCount: 10,
        Color: 'danger'
      });
    c.wait(5000);

    expect(ctrl.siteCards[0].teams[0].InAlarmCount).toEqual(2);
    expect(ctrl.siteCards[0].teams[0].Color).toEqual('good');
  });

  it('should update team adherence when site is open and skill is selected', function () {
    stateParams.skillIds = ['channelSalesId'];
    $fakeBackend
      .withSiteAdherence({
        Id: 'londonId',
        SkillId: 'channelSalesId',
        InAlarmCount: 5,
        Color: 'warning'
      })
      .withTeamAdherence({
        SiteId: 'londonId',
        SkillId: 'channelSalesId',
        InAlarmCount: 5,
        Color: 'warning'
      });
    var c = $controllerBuilder.createController();
    vm = c.vm;
    ctrl = $componentController('rtaOverviewComponent', null, {
      siteCards: vm.siteCards
    });

    c.apply(function () {
      ctrl.siteCards[0].isOpen = true;
    });
    $fakeBackend.clearTeamAdherences()
      .withTeamAdherence({
        SiteId: 'londonId',
        SkillId: 'channelSalesId',
        InAlarmCount: 2,
        Color: 'good'
      });
    c.wait(5000);

    expect(ctrl.siteCards[0].teams[0].InAlarmCount).toEqual(2);
    expect(ctrl.siteCards[0].teams[0].Color).toEqual('good');
  });

  it('should not update team adherence when site is closed and skill is selected', function () {
    stateParams.skillIds = ['channelSalesId'];
    $fakeBackend
      .withSiteAdherence({
        Id: 'londonId',
        SkillId: 'channelSalesId',
        InAlarmCount: 5,
        Color: 'warning'
      })
      .withTeamAdherence({
        SiteId: 'londonId',
        SkillId: 'channelSalesId',
        InAlarmCount: 5,
        Color: 'warning'
      });
    var c = $controllerBuilder.createController();
    vm = c.vm;
    ctrl = $componentController('rtaOverviewComponent', null, {
      siteCards: vm.siteCards
    });
    c.apply(function () {
      ctrl.siteCards[0].isOpen = true;
    });
    $fakeBackend.clearTeamAdherences()
      .withTeamAdherence({
        SiteId: 'londonId',
        SkillId: 'channelSalesId',
        InAlarmCount: 2,
        Color: 'good'
      });
    c.wait(5000);
    c.apply(function () {
      ctrl.siteCards[0].isOpen = false;
    });
    $fakeBackend
      .clearTeamAdherences()
      .withTeamAdherence({
        SiteId: 'londonId',
        InAlarmCount: 10,
        Color: 'danger'
      });
    c.wait(5000);

    expect(ctrl.siteCards[0].teams[0].InAlarmCount).toEqual(2);
    expect(ctrl.siteCards[0].teams[0].Color).toEqual('good');
  });

  it('should set agents state for site', function () {
    vm = $controllerBuilder.createController().vm;
    ctrl = $componentController('rtaOverviewComponent', null, {
      siteCards: vm.siteCards,
      agentsState: vm.agentsState
    });

    expect(ctrl.agentsState).toEqual('rta-agents({siteIds: card.site.Id})');
  });

  it('should set agents state for site with skill', function () {
    var c = $controllerBuilder.createController();
    var vm = c.vm;

    c.apply(function () {
      vm.filterOutput(channelSales);
    });

    ctrl = $componentController('rtaOverviewComponent', null, {
      siteCards: vm.siteCards,
      agentsState: vm.agentsState
    });

    expect(ctrl.agentsState).toEqual('rta-agents({siteIds: card.site.Id, skillIds: ["channelSalesId"]})');
  });

  it('should set agents state for site with skill area', function () {
    var c = $controllerBuilder.createController(undefined, skillAreas);
    var vm = c.vm;

    c.apply(function () {
      vm.filterOutput(skillArea1);
    });
    ctrl = $componentController('rtaOverviewComponent', null, {
      siteCards: vm.siteCards,
      agentsState: vm.agentsState
    });

    expect(ctrl.agentsState).toEqual('rta-agents({siteIds: card.site.Id, skillAreaId: "skillArea1Id"})');
  });

  it('should set agents state for site when clearing filter selection', function () {
    var c = $controllerBuilder.createController();
    var vm = c.vm;

    c.apply(function () {
      vm.filterOutput(channelSales);
    });
    c.apply(function () {
      vm.filterOutput();
    });

    ctrl = $componentController('rtaOverviewComponent', null, {
      siteCards: vm.siteCards,
      agentsState: vm.agentsState
    });

    expect(ctrl.agentsState).toEqual('rta-agents({siteIds: card.site.Id})');
  });

  it('should select site', function () {
    $fakeBackend
      .withSiteAdherence({
        Id: 'londonId'
      })
    var c = $controllerBuilder.createController()
    var vm = c.vm;
    ctrl = $componentController('rtaOverviewComponent', null, {
      siteCards: vm.siteCards,
      agentsState: vm.agentsState,
      getSelectedItems: vm.getSelectedItems
    });

    c.apply(function () {
      ctrl.selectItem(ctrl.siteCards[0]);
    });

    expect(ctrl.siteCards[0].isSelected).toEqual(true);
  });

  it('should unselect site', function () {
    $fakeBackend
      .withSiteAdherence({
        Id: 'londonId'
      });
    var c = $controllerBuilder.createController();
    var vm = c.vm;
    ctrl = $componentController('rtaOverviewComponent', null, {
      siteCards: vm.siteCards,
      agentsState: vm.agentsState,
      getSelectedItems: vm.getSelectedItems
    });

    c.apply(function () {
      ctrl.selectItem(ctrl.siteCards[0]);
    });
    c.apply(function () {
      ctrl.selectItem(ctrl.siteCards[0]);
    });

    expect(ctrl.siteCards[0].isSelected).toEqual(false);
  });

  it('should select team', function () {
    $fakeBackend
      .withSiteAdherence({
        Id: 'londonId'
      })
      .withTeamAdherence({
        SiteId: 'londonId',
        Id: 'greenId'
      });

    var c = $controllerBuilder.createController();
    var vm = c.vm;
    ctrl = $componentController('rtaOverviewComponent', null, {
      siteCards: vm.siteCards,
      agentsState: vm.agentsState,
      getSelectedItems: vm.getSelectedItems
    });

    c.apply(function () {
      ctrl.siteCards[0].isOpen = true;
    });
    c.apply(function () {
      ctrl.selectItem(ctrl.siteCards[0].teams[0]);
    });

    expect(ctrl.siteCards[0].teams[0].isSelected).toEqual(true);
  });

  it('should unselect team', function () {
    $fakeBackend
      .withSiteAdherence({
        Id: 'londonId'
      })
      .withTeamAdherence({
        SiteId: 'londonId',
        Id: 'greenId'
      });

    var c = $controllerBuilder.createController();
    var vm = c.vm;
    ctrl = $componentController('rtaOverviewComponent', null, {
      siteCards: vm.siteCards,
      agentsState: vm.agentsState,
      getSelectedItems: vm.getSelectedItems
    });

    c.apply(function () {
      ctrl.siteCards[0].isOpen = true;
    });
    c.apply(function () {
      ctrl.selectItem(ctrl.siteCards[0].teams[0]);
    });
    c.apply(function () {
      ctrl.selectItem(ctrl.siteCards[0].teams[0]);
    });

    expect(ctrl.siteCards[0].teams[0].isSelected).toEqual(false);
  });

  it('should select all teams under site when site is selected and opened', function () {
    $fakeBackend
      .withSiteAdherence({
        Id: 'londonId'
      })
      .withTeamAdherence({
        SiteId: 'londonId',
        Id: 'greenId'
      });

    var c = $controllerBuilder.createController();
    var vm = c.vm;
    ctrl = $componentController('rtaOverviewComponent', null, {
      siteCards: vm.siteCards,
      agentsState: vm.agentsState,
      getSelectedItems: vm.getSelectedItems
    });

    c.apply(function () {
      ctrl.siteCards[0].isOpen = true;
    });
    c.apply(function () {
      ctrl.selectItem(ctrl.siteCards[0]);
    });

    expect(ctrl.siteCards[0].teams[0].isSelected).toEqual(true);
  });

  it('should unselect all teams under site when site is unselected and opened', function () {
    $fakeBackend
      .withSiteAdherence({
        Id: 'londonId'
      })
      .withTeamAdherence({
        SiteId: 'londonId',
        Id: 'greenId'
      });

    var c = $controllerBuilder.createController();
    var vm = c.vm;
    ctrl = $componentController('rtaOverviewComponent', null, {
      siteCards: vm.siteCards,
      agentsState: vm.agentsState,
      getSelectedItems: vm.getSelectedItems
    });

    c.apply(function () {
      ctrl.siteCards[0].isOpen = true;
    });
    c.apply(function () {
      ctrl.selectItem(ctrl.siteCards[0]);
    });
    c.apply(function () {
      ctrl.selectItem(ctrl.siteCards[0]);
    });

    expect(ctrl.siteCards[0].teams[0].isSelected).toEqual(false);
  });

  it('should select site if all teams under it are selected and site is opened', function () {
    $fakeBackend
      .withSiteAdherence({
        Id: 'londonId'
      })
      .withTeamAdherence({
        SiteId: 'londonId',
        Id: 'greenId'
      });

    var c = $controllerBuilder.createController();
    var vm = c.vm;
    ctrl = $componentController('rtaOverviewComponent', null, {
      siteCards: vm.siteCards,
      agentsState: vm.agentsState,
      getSelectedItems: vm.getSelectedItems,
      selectedItems: vm.selectedItems
    });

    c.apply(function () {
      ctrl.siteCards[0].isOpen = true;
    });
    c.apply(function () {
      ctrl.selectItem(ctrl.siteCards[0].teams[0]);
    });

    expect(ctrl.siteCards[0].isSelected).toEqual(true);
  });

  it('should unselect site if at least one team under it is unselected and site is opened', function () {
    $fakeBackend
      .withSiteAdherence({
        Id: 'londonId'
      })
      .withTeamAdherence({
        SiteId: 'londonId',
        Id: 'greenId'
      })
      .withTeamAdherence({
        SiteId: 'londonId',
        Id: 'redId'
      });

    var c = $controllerBuilder.createController();
    var vm = c.vm;
    ctrl = $componentController('rtaOverviewComponent', null, {
      siteCards: vm.siteCards,
      agentsState: vm.agentsState,
      getSelectedItems: vm.getSelectedItems
    });


    c.apply(function () {
      ctrl.siteCards[0].isOpen = true;
    });
    c.apply(function () {
      ctrl.selectItem(ctrl.siteCards[0].teams[0]);
    });
    c.apply(function () {
      ctrl.selectItem(ctrl.siteCards[0].teams[1]);
    });
    c.apply(function () {
      ctrl.selectItem(ctrl.siteCards[0].teams[0]);
    });

    expect(ctrl.siteCards[0].isSelected).toEqual(false);
  });

  it('should select all teams under site when site is selected and initially closed', function () {
    $fakeBackend
      .withSiteAdherence({
        Id: 'londonId'
      })
      .withTeamAdherence({
        SiteId: 'londonId',
        Id: 'greenId'
      });

    var c = $controllerBuilder.createController();
    var vm = c.vm;
    ctrl = $componentController('rtaOverviewComponent', null, {
      siteCards: vm.siteCards,
      agentsState: vm.agentsState,
      getSelectedItems: vm.getSelectedItems
    });


    c.apply(function () {
      ctrl.selectItem(ctrl.siteCards[0]);
    });
    c.apply(function () {
      ctrl.siteCards[0].isOpen = true;
    });

    expect(ctrl.siteCards[0].teams[0].isSelected).toEqual(true);
  });

});
