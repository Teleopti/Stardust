(function () {
    'use strict';

    angular
        .module('wfm.staffing')
        .controller('staffingController', staffingController);

    staffingController.inject = ['staffingService', '$filter', '$translate'];
    function staffingController(staffingService, $filter, $translate) {
        var vm = this;
        vm.staffingDataAvailable = true;
        vm.selectedSkill;
        vm.selectedSkillArea;
        vm.selectedSkillChange = selectedSkillChange;
        vm.querySearchSkills = querySearchSkills;
        vm.querySearchAreas = querySearchAreas;

        var allSkills = [];
        var allSkillAreas = [];
        getSkills();
        getSkillAreas();
        var getSkillStaffing = getSkillStaffing;
        var getSkillsForArea = getSkillAreaStaffing
        var staffingData = {};
        ////////////////
        function generateChart(skillId) {
            if (!skillId) return;
            var query = getSkillStaffing(skillId);
            query.$promise.then(function (result) {
                staffingData.time = [];
                staffingData.scheduledStaffing = [];
                staffingData.forcastedStaffing = [];
                if (staffingPrecheck(result.DataSeries)) {
                    staffingData.scheduledStaffing = result.DataSeries.ScheduledStaffing;
                    staffingData.forcastedStaffing = result.DataSeries.ForecastedStaffing;
                    staffingData.forcastedStaffing.unshift($translate.instant('ForecastedStaff'));
                    staffingData.scheduledStaffing.unshift($translate.instant('ScheduledStaff'));
                    angular.forEach(result.DataSeries.Time, function (value, key) {
                        staffingData.time.push($filter('date')(value, 'shortTime'));
                    }, staffingData.time);
                    staffingData.time.unshift('x');
                    generateChartForView();
                } else {
                    vm.staffingDataAvailable = false;
                }
            })
        }
        function staffingPrecheck(data) {
            if (!angular.equals(data, {}) && data != null) {
                if (data.Time && data.ScheduledStaffing && data.ForecastedStaffing) {
                    vm.staffingDataAvailable = true;
                    return true;
                }
            }
            vm.staffingDataAvailable = false;
            return false;
        }

        function getSkills() {
            var query = staffingService.getSkills.query();
            query.$promise.then(function (skills) {
                vm.selectedSkill = skills[0].Name;
                generateChart(skills[0].Id);
                allSkills = skills;
            })
        }

        function getSkillAreas() {
            var query = staffingService.getSkillAreas.get();
            query.$promise.then(function (response) {
                allSkillAreas = response.SkillAreas;
            })
        }

        function getSkillAreaStaffing(areaId) {
            if (!areaId) return;
            return staffingService.getSkillAreaStaffing.get(areaId);
        }
        function getSkillStaffing(skillId) {
            return staffingService.getSkillStaffing.get({ id: skillId })
        }
        function selectedSkillChange(skill) {
            if (skill == null) return;

            generateChart(skill.Id)
            vm.selectedSkill = skill;
        }


        function querySearchSkills(query) {
            var results = query ? allSkills.filter(createFilterFor(query)) : allSkills,
                deferred;
            return results;
        };

        function querySearchAreas(query) {
            var results = query ? allSkillAreas.filter(createFilterFor(query)) : allSkillAreas,
                deferred;
            return results;
        };

        function createFilterFor(query) {
            var lowercaseQuery = angular.lowercase(query);
            return function filterFn(item) {
                var lowercaseName = angular.lowercase(item.Name);
                return (lowercaseName.indexOf(lowercaseQuery) === 0);
            };
        };

        var generateChartForView = function () {
            c3.generate({
                bindto: '#staffingChart',
                data: {
                    x: "x",
                    columns: [
                        staffingData.time,
                        staffingData.forcastedStaffing,
                        staffingData.scheduledStaffing
                    ],
                    selection: {
                        enabled: true,
                    },
                },
                axis: {
                    x: {
                        label: {
                            text: $translate.instant('SkillTypeTime'),
                            position: 'outer-center'
                        },
                        type: 'category',
                        tick: {
                            culling: {
                                max: 24
                            },
                            fit: true,
                            centered: true,
                            multiline: false
                        }
                    }
                },
                zoom: {
                    enabled: true,
                },
            });

        }
    }
})();