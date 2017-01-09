'use strict';
describe('favoriteSearch component tests', function () {

	var $componentController, rootScope, $compile, favoriteSearchDataService;


	beforeEach(module('wfm.templates'));
	beforeEach(module('wfm.teamSchedule'));

	beforeEach(function () {
		favoriteSearchDataService = new FakeFavoriteSearchDataService();

		module(function ($provide) {
			$provide.service('FavoriteSearchDataService',
				function () {
					return favoriteSearchDataService;
				});
		});
	});

	beforeEach(inject(function (_$componentController_, _$rootScope_, _$compile_) {
		$componentController = _$componentController_;
		rootScope = _$rootScope_;
		$compile = _$compile_;
	}));

	it("should populate favorite list", inject(function () {
		var scope = rootScope.$new();
		scope.getSearch = function () { };
		scope.applyFavorite = function () { };

		var element = angular.element('<favorite-search get-search="getSearch()" apply-favorite="applyFavorite()"></favorite-search>');
		element = $compile(element)(scope);
		scope.$digest();


		var ctrl = element.isolateScope().$ctrl;

		expect(ctrl.favoriteSearchList.length).toEqual(2);
		expect(ctrl.favoriteSearchList[0].Id).toEqual("1");
		expect(ctrl.favoriteSearchList[1].Id).toEqual("2");
		expect(ctrl.currentName).toEqual('test2');
	}));

	it('should save new favorite', function () {
		var scope = rootScope.$new();
		scope.getSearch = function () { return { teamIds: ["team1"], searchTerm: "agent" }; };
		scope.applyFavorite = function () { };

		var element = angular.element('<favorite-search get-search="getSearch()" apply-favorite="applyFavorite()"></favorite-search>');
		element = $compile(element)(scope);
		scope.$digest();


		var ctrl = element.isolateScope().$ctrl;

		ctrl.currentName = 'myNewSearch';
		ctrl.save();

		expect(ctrl.favoriteSearchList.length).toEqual(3);
		expect(ctrl.favoriteSearchList[0].Name).toEqual("myNewSearch");
		expect(ctrl.favoriteSearchList[0].TeamIds[0]).toEqual("team1");
		expect(ctrl.favoriteSearchList[0].SearchTerm).toEqual("agent");
	});

	it('should update current favorite search', function () {
		var scope = rootScope.$new();
		scope.getSearch = function () { return { teamIds: ["fakeTeamId"], searchTerm: "agent" }; };
		scope.applyFavorite = function () { };

		var element = angular.element('<favorite-search get-search="getSearch()" apply-favorite="applyFavorite()"></favorite-search>');
		element = $compile(element)(scope);
		scope.$digest();


		var ctrl = element.isolateScope().$ctrl;
		ctrl.isTest = true;
		ctrl.currentName = 'test2';
		ctrl.save();

		expect(ctrl.favoriteSearchList.length).toEqual(2);
		expect(ctrl.favoriteSearchList[1].TeamIds[0]).toEqual("fakeTeamId");
		expect(ctrl.favoriteSearchList[1].SearchTerm).toEqual("agent");
	});

	it('should make specific favorite default', function () {
		var scope = rootScope.$new();
		scope.getSearch = function () { return { teamIds: ["fakeTeamId"], searchTerm: "agent" }; };
		scope.applyFavorite = function () { };

		var element = angular.element('<favorite-search get-search="getSearch()" apply-favorite="applyFavorite()"></favorite-search>');
		element = $compile(element)(scope);
		scope.$digest();


		var ctrl = element.isolateScope().$ctrl;
		var newDefaultName = 'test1';
		ctrl.toggleDefault(newDefaultName);

		expect(ctrl.favoriteSearchList.length).toEqual(2);
		expect(ctrl.favoriteSearchList[0].IsDefault).toEqual(true);
		expect(ctrl.favoriteSearchList[1].IsDefault).toEqual(false);
	});


	it('should delete specific favorite', function () {
		var scope = rootScope.$new();
		scope.getSearch = function () { };
		scope.applyFavorite = function () { };

		var element = angular.element('<favorite-search get-search="getSearch()" apply-favorite="applyFavorite()"></favorite-search>');
		element = $compile(element)(scope);
		scope.$digest();


		var ctrl = element.isolateScope().$ctrl;
		var nameToBeDeleted = 'test1';
		ctrl.delete(nameToBeDeleted);

		expect(ctrl.favoriteSearchList.length).toEqual(1);
		expect(ctrl.favoriteSearchList[0].Id).toEqual('2');
	});


	function FakeFavoriteSearchDataService() {
		var fakeData = [
			{
				Id: '1',
				Name: 'test1',
				TeamIds: ['Team1Id', 'Team2Id'],
				SearchTerm: '',
				IsDefault: false
			},
			{
				Id: '2',
				Name: 'test2',
				TeamIds: ['Team1Id', 'Team2Id'],
				SearchTerm: '',
				IsDefault: true
			}
		];

		this.getFavoriteSearchList = function () {
			return {
				then: function (cb) { cb({ data: fakeData }); }
			}
		}

		this.add = function (currentName, currentSearch) {
			var result = {
				Id: 'id',
				Name: currentName,
				TeamIds: currentSearch.teamIds,
				SearchTerm: currentSearch.searchTerm,
				IsDefault: false
			};
			return {
				then: function (cb) { cb({ data: result }); }
			}
		}

		this.update = function () {
			return { then: function (cb) { cb(); } };
		}

		this.delete = function () {
			return { then: function (cb) { cb(); } };
		}

		this.changeDefault = function() {
			return { then: function (cb) { cb(); } };
		};

	}
});