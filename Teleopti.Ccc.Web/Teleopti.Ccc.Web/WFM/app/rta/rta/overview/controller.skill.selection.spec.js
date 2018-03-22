'use strict';

rtaTester.describe('RtaOverviewController', function (it, fit, xit) {

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

	it('should go to sites by skill state', function (t) {
		allSkills.forEach(function (skill) {
			t.backend.withSkill(skill);
		});
		var vm = t.createController();

		t.apply(function () {
			vm.selectSkillOrSkillArea(vm.skills[0]);
		});

		expect(t.lastGoParams.skillIds).toEqual(['channelSalesId']);
	});

	it('should go to sites by skill area state', function (t) {
		t.backend.withSkillAreas(skillAreas);
		var vm = t.createController();

		vm.selectSkillOrSkillArea(vm.skillAreas[0]);

		expect(t.lastGoParams.skillAreaId).toEqual('skillArea1Id');
	});

	it('should go to sites with skill when changing selection from skill area to skill', function (t) {
		t.stateParams.skillAreaId = 'skillArea1Id';
		allSkills.forEach(function (skill) {
			t.backend.withSkill(skill);
		});
		t.backend.withSkillAreas(skillAreas);
		var vm = t.createController();

		t.apply(function () {
			vm.selectSkillOrSkillArea(vm.skills[0]);
		});

		expect(t.lastGoParams.skillAreaId).toEqual(undefined);
		expect(t.lastGoParams.skillIds).toEqual(['channelSalesId']);
	});

	it('should go to sites with skill area when changing selection from skill to skill area', function (t) {
		t.stateParams.skillIds = ['channelSalesId'];
		allSkills.forEach(function (skill) {
			t.backend.withSkill(skill);
		});
		t.backend.withSkillAreas(skillAreas);
		var vm = t.createController();

		t.apply(function () {
			vm.selectSkillOrSkillArea(skillArea1);
		});

		expect(t.lastGoParams.skillAreaId).toEqual('skillArea1Id');
		expect(t.lastGoParams.skillIds).toEqual(undefined);
	});

	it('should clear url when sending in empty input in filter', function (t) {
		t.stateParams.skillIds = ['channelSalesId'];
		allSkills.forEach(function (skill) {
			t.backend.withSkill(skill);
		});
		t.backend.withSkillAreas(skillAreas);
		var vm = t.createController();

		t.apply(function () {
			vm.selectSkillOrSkillArea(undefined);
		});

		expect(t.lastGoParams.hasOwnProperty('skillAreaId')).toEqual(true);
		expect(t.lastGoParams.skillAreaId).toEqual(undefined);
		expect(t.lastGoParams.hasOwnProperty('skillIds')).toEqual(true);
		expect(t.lastGoParams.skillIds).toEqual(undefined);
	});

	it('should not display site filtered by skill when skill is selected', function (t) {
		t.backend
			.withSkill({
				Id: 'skillId'
			})
			.withSiteAdherence({
				Id: 'londonId',
				SkillId: 'skillId'
			})
			.withSiteAdherence({
				Id: 'parisId',
				SkillId: 'anotherskill',
			});
		var vm = t.createController();

		t.backend
			.clearSiteAdherences()
			.withSiteAdherence({
				Id: 'londonId',
				SkillId: 'skillId'
			});
		t.apply(function () {
			vm.selectSkillOrSkillArea({
				Id: 'skillId'
			});
		});

		expect(vm.siteCards.length).toEqual(1);
		expect(vm.siteCards[0].Id).toEqual('londonId');
	});
	
});

