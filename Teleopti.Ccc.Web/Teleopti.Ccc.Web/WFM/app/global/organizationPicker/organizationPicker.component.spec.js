﻿'use strict';
describe('organizationPicker component tests',
	function() {

		var $componentController, $q, $compile, $rootScope, $document;

		beforeEach(module('wfm.templates', 'wfm.organizationPicker', 'ngMaterial'));

		beforeEach(function() {
			var fakeOrgPickerSvc = new FakeOrgPickerSvc();

			module(function($provide) {
				$provide.service('organizationPickerSvc',
					function() {
						return fakeOrgPickerSvc;
					});
			});
		});

		beforeEach(inject(function(_$componentController_,
			_$q_,
			_$rootScope_,
			_$compile_,
			_$document_) {
			$componentController = _$componentController_;
			$q = _$q_;
			$rootScope = _$rootScope_;
			$compile = _$compile_;
			$document = _$document_;
		}));

		function FakeOrgPickerSvc() {

			this.getAvailableHierarchy = function() {
				var data = {
					Children: [
						{
							Id: 'site1',
							Name: 'site1',
							Children: [
								{
									Id: 'team1',
									Name: 'team1'
								}
							]
						},
						{
							Id: "site2",
							Name: 'site2',
							Children: [
								{
									Id: 'team2',
									Name: 'team2'
								},
								{
									Id: 'team3',
									Name: 'team3'
								}
							]
						}
					],
					logonUserTeamId: 'logonUserTeamId'
				};
				return {
					then: function(cb) {
						cb({ data: data });
					}
				}
			}
		}


		it("should populate hierachy list",
			inject(function() {

				var bindings = {
					date: new Date('2015-09-01'),
					onOpen: function() {},
					onPick: angular.noop
				}

				var ctrl = $componentController('organizationPicker', null, bindings);
				ctrl.$onInit();

				expect(ctrl.groupList.length).toEqual(2);
				expect(ctrl.groupList[0].id).toEqual("site1");
				expect(ctrl.groupList[1].id).toEqual("site2");
				expect(ctrl.groupList[0].teams.length).toEqual(1);
				expect(ctrl.groupList[1].teams.length).toEqual(2);
			}));

		it("should extract the right abbreviation of the selected time zone ",
			inject(function() {

				var bindings = {
					date: new Date('2015-09-01'),
					onOpen: function() {},
					onPick: angular.noop
				}

				var ctrl = $componentController('organizationPicker', null, bindings);
				ctrl.$onInit();

				ctrl.onPickerOpen();
				ctrl.selectedTeamIds = ['team1'];
				var displayName = ctrl.formatSelectedDisplayName();
				expect(displayName).toEqual("team1");

				ctrl.selectedTeamIds = [];
				displayName = ctrl.formatSelectedDisplayName();
				expect(displayName).toEqual("Organization");
			}));

		it("Should trigger onPick when selection done",
			function() {
				var selectedTeams = [];
				var bindings = {
					date: new Date('2015-09-01'),
					onOpen: function() {},
					onPick: function(input) {
						selectedTeams = input.teams;
					}
				}

				var ctrl = $componentController('organizationPicker', null, bindings);
				ctrl.$onInit();

				ctrl.onPickerOpen();
				ctrl.selectedTeamIds = ['team1', 'team2'];
				ctrl.onPickerClose();
				expect(selectedTeams.length).toEqual(2);
			});

		it("should calculate selected team ids number correctly",
			function() {
				var selectedTeams = [];
				var bindings = {
					date: new Date('2015-09-01'),
					onOpen: function() {},
					onPick: function(input) {
						selectedTeams = input.teams;
					}
				}

				var ctrl = $componentController('organizationPicker', null, bindings);
				ctrl.$onInit();

				ctrl.onPickerOpen();
				ctrl.selectedTeamIds = ['team1', 'team2'];
				ctrl.onPickerClose();
				expect(selectedTeams.length).toEqual(2);
			});

		it("should be the single mode",
			function() {
				var scope = $rootScope.$new();
				var selectedTeams = [];
				var html = angular.element('<organization-picker on-pick=selectTeams(teams) single></organization-picker>');
				scope.selectTeams = function(teams) {
					selectedTeams = teams;
				}
				var container = $compile(html)(scope);
				scope.$digest();

				var select = container.find("md-select");

				select.triggerHandler("click");

				var openMenu = container.find("md-select-menu");
				var options = select.find("md-option");
				var opt = options[0].querySelector("div.md-text");
				angular.element(openMenu).triggerHandler({
					type: 'click',
					target: angular.element(opt),
					currentTarget: openMenu[0]
				});

				var ctrl = container.isolateScope().$ctrl;
				ctrl.onPickerClose();

				expect(selectedTeams.length).toEqual(1);
				expect(selectedTeams[0]).toEqual(angular.element(opt).text());

				var opt2 = options[1].querySelector("div.md-text");
				angular.element(openMenu).triggerHandler({
					type: 'click',
					target: angular.element(opt2),
					currentTarget: openMenu[0]
				});
				ctrl.onPickerClose();
				expect(selectedTeams.length).toEqual(1);
				expect(selectedTeams[0]).toEqual(angular.element(opt2).text());

			});

	});