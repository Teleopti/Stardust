(function () {
    'use strict';

    angular
        .module('wfm.staffing')
        .service('ChartService', chartService);

    chartService.inject = ['$translate'];
    function chartService($translate) {
        this.staffingChartConfig = staffingChartConfig;

        ////////////////
        function staffingChartConfig(staffingData, isSuggestedData) {
            var staffing = generateOverUnderStaffing(staffingData.absoluteDifference);
            var scaffold = generateScaffold(staffingData);
            var groups = [[scaffold.over[0], staffing.over[0], scaffold.under[0], staffing.under[0]]];
            var hide = [scaffold.under[0], scaffold.over[0]];
            var columns = [
                staffingData.time,
                staffingData.scheduledStaffing,
                scaffold.over,
                scaffold.under,
                staffing.over,
                staffing.under
            ];

            if (!isSuggestedData) {
                var chartColors = generateColorObject(staffing, scaffold);
                var types = generateTypeObject(staffingData);
                columns.unshift(staffingData.forcastedStaffing);
            } else if (isSuggestedData) {
                var suggestedStaffing = generateOverUnderStaffing(staffingData.suggested.absoluteDifference, isSuggestedData);
                var suggestedScaffold = generateScaffold(staffingData.suggested, isSuggestedData);
                var chartColors = generateColorObject(staffing, scaffold, suggestedStaffing, suggestedScaffold);
                var types = generateTypeObject(staffingData, isSuggestedData);
                groups.push([suggestedScaffold.over[0], suggestedStaffing.over[0], suggestedScaffold.under[0], suggestedStaffing.under[0]]);
                hide.push(suggestedScaffold.under[0], suggestedScaffold.over[0]);
                columns.unshift(staffingData.suggested.forcastedStaffing);
                columns.push(
                    staffingData.suggested.scheduledStaffing,
                    suggestedScaffold.over,
                    suggestedScaffold.under,
                    suggestedStaffing.over,
                    suggestedStaffing.under
                );
            }

            var config = {
                bindto: '#staffingChart',
                point: {
                    show: false
                },
                legend: {
                    hide: hide
                },
                data: {
                    colors: chartColors,
                    order: 'null',
                    type: 'bar',
                    x: "x",
                    types: types,
                    columns: columns,
                    groups: groups
                },
                tooltip: {
                    contents: tooltip_contents
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
                    enabled: false,
                },
            };

            return config;
        };

        function tooltip_contents(d, defaultTitleFormat, defaultValueFormat, color) {
            var root = this, config = root.config, CLASS = root.CLASS,
                titleFormat = config.tooltip_format_title || defaultTitleFormat,
                nameFormat = config.tooltip_format_name || function (name) { return name; },
                valueFormat = config.tooltip_format_value || defaultValueFormat,
                text, i, title, value, name, bgcolor;

            // You can access all of data like this:
            for (i = 0; i < d.length; i++) {
                if (!(d[i] && (d[i].value || d[i].value === 0))) { continue; }

                if (d[i].name === 'OverStaffScaffold' || d[i].name === 'UnderStaffScaffold' || d[i].name === 'SuggestedOverStaffScaffold' || d[i].name === 'SuggestedUnderStaffScaffold') {
                    continue;
                }

                if (!text) {
                    title = config.axis_x_categories[d[i].index];
                    text = "<table class='" + CLASS.tooltip + "'>" + (title || title === 0 ? "<tr><th colspan='2'>" + title + "</th></tr>" : "");
                }

                name = nameFormat(d[i].name);
                value = valueFormat(d[i].value, d[i].ratio, d[i].id, d[i].index);
                bgcolor = root.levelColor ? root.levelColor(d[i].value) : color(d[i].id);

                text += "<tr class='" + CLASS.tooltipName + "-" + d[i].id + "'>";
                text += "<td class='name'><span style='background-color:" + bgcolor + "'></span>" + name + "</td>";
                text += "<td class='value'>" + value + "</td>";
                text += "</tr>";
            }

            return text + "</table>";
        };

        function generateOverUnderStaffing(absoluteDifference, isSuggestedData) {
            var staffing = {};
            staffing.over = [];
            staffing.under = [];

            for (var index = 0; index <= absoluteDifference.length; index++) {
                var value = absoluteDifference[index];
                if (value < 0) {
                    staffing.under.push(Math.abs(value.toFixed(1)));
                    staffing.over.push(null);
                } else if (value >= 0) {
                    staffing.over.push(value.toFixed(1));
                    staffing.under.push(null);
                }
            }

            if (isSuggestedData) {
                staffing.over.unshift($translate.instant('Suggested overstaffing'));
                staffing.under.unshift($translate.instant('Suggested understaffing'));
            } else {
                staffing.over.unshift($translate.instant('Overstaffing'));
                staffing.under.unshift($translate.instant('Understaffing'));
            }

            return staffing;
        }

        function generateScaffold(staffingData, isSuggestedData) {
            var scaffold = {};
            scaffold.under = staffingData.scheduledStaffing.concat();
            scaffold.under.shift();
            scaffold.over = staffingData.forcastedStaffing.concat();
            scaffold.over.shift();

            for (var index = 0; index <= staffingData.absoluteDifference.length; index++) {
                var value = staffingData.absoluteDifference[index];
                if (value < 0) {
                    scaffold.over[index] = 0;
                } else if (value >= 0) {
                    scaffold.under[index] = 0;
                }
            }

            if (isSuggestedData) {
                scaffold.under.unshift('SuggestedUnderStaffScaffold');
                scaffold.over.unshift('SuggestedOverStaffScaffold');
            } else {
                scaffold.under.unshift('UnderStaffScaffold');
                scaffold.over.unshift('OverStaffScaffold');
            }

            return scaffold;
        };

        function generateTypeObject(staffingData, isSuggestedData) {
            var types = {};
            var forcastingTypeKey = staffingData.forcastedStaffing[0];
            var staffingTypeKey = staffingData.scheduledStaffing[0];
            types[forcastingTypeKey] = 'line';
            types[staffingTypeKey] = 'line';

            if (isSuggestedData) {
                var suggestedStaffingTypeKey = staffingData.suggested.scheduledStaffing[0];
                types[suggestedStaffingTypeKey] = 'line';
            }

            return types;
        };

        function generateColorObject(staffing, scaffold, suggestedStaffing, suggestedScaffold) {
            var colors = {};
            var overstaffColorKey = staffing.over[0];
            var understaffColorKey = staffing.under[0];
            var overScaffoldKey = scaffold.over[0];
            var underScaffoldKey = scaffold.under[0];

            colors[overstaffColorKey] = '#0a84d6';
            colors[understaffColorKey] = '#D32F2F';
            colors[underScaffoldKey] = '#fff';
            colors[overScaffoldKey] = '#fff';

            if (suggestedStaffing && suggestedScaffold) {
                var suggestedOverstaffColorKey = suggestedStaffing.over[0];
                var suggestedUnderstaffColorKey = suggestedStaffing.under[0];
                var suggestedOverScaffoldKey = suggestedScaffold.over[0];
                var suggestedUnderScaffoldKey = suggestedScaffold.under[0]; 

                colors[suggestedOverstaffColorKey] = '#99D6FF';
                colors[suggestedUnderstaffColorKey] = '#EE8F7D';
                colors[suggestedUnderScaffoldKey] = '#fff';
                colors[suggestedOverScaffoldKey] = '#fff';
            }

            return colors;
        };
    }
})();