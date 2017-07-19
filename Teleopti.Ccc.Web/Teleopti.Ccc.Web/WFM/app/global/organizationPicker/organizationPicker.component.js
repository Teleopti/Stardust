(function (angular) {
	'use strict';


	angular.module('wfm.organizationPicker')
		.component('organizationPicker', {
			templateUrl: [
				'$attrs', function ($attrs) {
					if (checkIsSingleMode($attrs)) {
						return 'app/global/organizationPicker/organizationPicker.single.tpl.html'
					}
					return 'app/global/organizationPicker/organizationPicker.tpl.html'
				}
			],
			controller: OrgPickerController,
			bindings: {
				onOpen: '&?',
				onClose: '&?',
				sitesAndTeams: '<',
				selectedText: '&?',
				selectedTeamIds: '<?',
			}
		});

	function checkIsSingleMode(attrs) {
		return ('single' in attrs)
	}

	OrgPickerController.$inject = ['$scope', '$translate', '$attrs', '$q', '$element', '$mdPanel'];

	function OrgPickerController($scope, $translate, $attrs, $q, $element, $mdPanel) {
		var singleMode = checkIsSingleMode($attrs)
		var searchText = ''
		var searchCache = {}
	
		var ctrl = this

		ctrl.longestName = ''

		ctrl.$onChanges = function (changesObj) {
			if (!!changesObj.sitesAndTeams && changesObj.sitesAndTeams.currentValue !== changesObj.sitesAndTeams.previousValue) {
				if (!angular.isArray(changesObj.sitesAndTeams.currentValue))
					return
				searchCache = {}
				searchText = ''
				populateGroupListAndNamemapAndFindLongestName(changesObj.sitesAndTeams.currentValue)
				ctrl.orgsInView = ctrl.searchForOrgsByName('')
				ctrl.updateSelectedTeamsInView()
			}
		}

		ctrl.$postLink = function () {
			setDefaultFocus();
		}

		ctrl.$onInit = function () {
			var menuPosition = $mdPanel.newPanelPosition().relativeTo($element).addPanelPosition($mdPanel.xPosition.ALIGN_START, $mdPanel.yPosition.BELOW)

			if (!ctrl.selectedText)
				ctrl.selectedText = ctrl.defaultSelectedTextFn

			if (!angular.isArray(ctrl.selectedTeamIds)) {
				ctrl.selectedTeamIds = []
			}

			ctrl.menuRef = $mdPanel.create({
				contentElement: $element.find('orgpicker-menu'),
				clickOutsideToClose: true,
				escapeToClose: true,
				zIndex: 40,
				trapFocus:true,
				attachTo: angular.element(document.body), // must-have for text inputs on ie11
				position: menuPosition,
				onOpenComplete: function () {
					$scope.$broadcast('$md-resize')

					if (singleMode && ctrl.selectedTeamIds[0]) {
						ctrl.topIndex = findTeamIndexInVirtualRepeatContainerById(ctrl.selectedTeamIds[0])
					}

					ctrl.updateSelectedTeamsInView()

					if (ctrl.onOpen)
						ctrl.onOpen()
				},
				onRemoving: (ctrl.onClose ? function () { ctrl.onClose() } : angular.noop),
				onCloseSuccess: function(){
							setDefaultFocus();
				},
			})
		}

		ctrl.openMenu = function (event) {
			// do not pass character to search input
			if (event.type === 'keypress')
				event.stopPropagation()

			ctrl.menuRef.open()
		}

		function setDefaultFocus(){
			var element = $element.find("md-select-value");
			element.focus();
		}

		function findTeamIndexInVirtualRepeatContainerById(teamId) {
			var index = -1
			for (var i = 0; i < ctrl.orgsInView.length; i++) {
				if (ctrl.orgsInView[i].m.id == teamId) {
					index = i
					break
				}
			}
			return index
		}

		function populateGroupListAndNamemapAndFindLongestName(rawSites) {
			ctrl.groupList = rawSites.map(function (rawSite) {
				var site = new Site(rawSite.Id, rawSite.Name)
				ctrl.nameMap[site.id] = site.name
				if (site.name.length > ctrl.longestName.length) {
					ctrl.longestName = site.name
				}
				rawSite.Children.forEach(function (team) {
					var t = new Team(team.Id, team.Name, site)
					ctrl.nameMap[t.id] = t.name
					site.teams.push(t)
					if (t.name.length > ctrl.longestName.length) {
						ctrl.longestName = t.name
					}
				})
				return site
			});
		}

		Object.defineProperties(this, {
			nameMap: { value: {} },
			searchText: {
				get: function () { return searchText },
				set: function (value) {
					searchText = value
					this.orgsInView = this.searchForOrgsByName(searchText)
					this.updateSelectedTeamsInView();
				}
			},
			defaultSelectedTextFn: {
				value: function () {
					return ctrl.selectedTeamIds.map(function (id) {
						return ctrl.nameMap[id]
					}).filter(function (name) {
						return !!name
					}).join(', ') || $translate.instant('SelectOrganization')
				}
			},
			clearAll: {
				value: function () {
					this.selectedTeamIds.splice(0)
					this.updateSelectedTeamsInView()
				}
			},
			updateSelectedTeamsInView: {
				value: function () {
					if (!angular.isArray(this.selectedTeamIds) || !angular.isArray(this.orgsInView))
						return
					for (var i = 0; i < this.orgsInView.length; i++) {
						var slaveSite = this.orgsInView[i]

						if (!slaveSite.m.isSite) {
							continue
						}

						slaveSite.selectedTeamIds.splice(0)
						slaveSite.m.selectedTeamIds.splice(0)

						for (var j = 0; j < slaveSite.teams.length; j++) {
							var slaveTeam = slaveSite.teams[j]
							if (this.selectedTeamIds.indexOf(slaveTeam.m.id) > -1) {
								slaveSite.selectedTeamIds.push(slaveTeam.m.id)
							}
						}

						for (var k = 0; k < slaveSite.m.teams.length; k++) {
							var team = slaveSite.m.teams[k];
							if (this.selectedTeamIds.indexOf(team.id) > -1) {
								slaveSite.m.selectedTeamIds.push(team.id)
							}
						}
					}
				}
			},
			collapseSite: {
				value: function (slaveSite) {
					var index = this.orgsInView.indexOf(slaveSite)
					if (slaveSite.collapsed) {
						var args = [index + 1, 0].concat(slaveSite.teams)
						this.orgsInView.splice.apply(this.orgsInView, args)
						slaveSite.collapsed = false
					} else {
						this.orgsInView.splice(index + 1, slaveSite.teams.length)
						slaveSite.collapsed = true
					}
				}
			},

			orgFocused: {
				value: function (org) {
					org.isFocused = true;
				}
			},

			orgBlurred: {
				value: function (org) {
					org.isFocused = false;
				}
			},

			toggleTeam: {
				value: function (slaveTeam) {
					slaveTeam.site.toggle(slaveTeam.m.id)
					slaveTeam.m.site.toggle(slaveTeam.m.id)

					var index = this.selectedTeamIds.indexOf(slaveTeam.m.id)
					if (index > -1) {
						this.selectedTeamIds.splice(index, 1)
					} else {
						if (singleMode)
							this.selectedTeamIds.splice(0, 1, slaveTeam.m.id)
						else
							this.selectedTeamIds.push(slaveTeam.m.id)
					}

					if (singleMode)
						this.menuRef.close()
				}
			},
			toggleSite: {
				value: function (slaveSite) {
					//event.stopPropagation()

					slaveSite.toggleAll()

					if (slaveSite.selectedTeamIds.length === slaveSite.teams.length) {
						slaveSite.selectedTeamIds.forEach(function (id) {
							if (this.selectedTeamIds.indexOf(id) === -1) {
								this.selectedTeamIds.push(id)
							}
						}, this)
					} else {
						slaveSite.teams.forEach(function (slaveTeam) {
							var index = this.selectedTeamIds.indexOf(slaveTeam.m.id)
							if (index > -1) {
								this.selectedTeamIds.splice(index, 1)
							}
						}, this)
					}
				}
			},
			searchForOrgsByName: {
				value: function (searchText) {
					var textIsEmpty = (searchText === '')
					var cached = searchCache[searchText]
					if (cached) {
						cached.filter(function (slave) { return slave.m.isSite })
							.forEach(function (slaveSite) {
								slaveSite.updateSelectedTeamIds()
								if (!textIsEmpty && slaveSite.collapsed) {
									var index = cached.indexOf(slaveSite)
									var args = [index+1, 0].concat(slaveSite.teams)
									cached.splice.apply(cached, args)
									slaveSite.collapsed = false
								}
							})
						return cached
					}
					var ret = []
					var re = new RegExp(searchText, 'i')
					this.groupList.forEach(function (site) {
						var s = new SlaveSite(site)
						s.collapsed = textIsEmpty
						var r = [s]
						var siteNameMatched = site.name.search(re) != -1
						site.teams.forEach(function (team) {
							var t
							if (siteNameMatched || textIsEmpty || team.name.search(re) != -1) {
								t = new SlaveTeam(team, s)
								s.addTeam(t)
								if (!s.collapsed) {
									r.push(t)
								}
							}
						})
						if (siteNameMatched || textIsEmpty || r.length > 1) {
							ret = ret.concat(r)
						}
					}, this)
					searchCache[searchText] = ret
					return ret
				}
			},
		})

	}

	function filterCollapsedTeams(slavesArray) {
		var newArray = []
		slavesArray.forEach(function (slave) {
			if (!slave.m.isSite) {
				return
			}
			var length = newArray.push(slave)
			if (!slave.collapsed) {
				newArray.splice.apply(newArray, [length, 0].concat(slave.teams))
			}
		})
		var args = [0, slavesArray.length].concat(newArray)
		slavesArray.splice.apply(slavesArray, args)
	}

	function Site(id, name) {
		var checked = false
		var selectedTeamIds = []
		var props = {
			id: { value: id },
			name: { value: name },
			isSite: { value: true },
			index: { writable: true, value: -1 },
			collapsed: { writable: true, value: true },
			expanded: { writable: true, value: false },
			checked: {
				get: function () { return selectedTeamIds.length === this.teams.length },
				set: function (value) {
					if (this.teams.length > 0) {
						this.teams.forEach(function (t) { t.checked = value })
					}
				}
			},
			teams: { value: [] },
			selectedTeamIds: {
				get: function () { return selectedTeamIds }
			},
			isChecked: {
				value: function () {
					return selectedTeamIds.length === this.teams.length
				}
			},
			isIndeterminate: {
				value: function () {
					return (selectedTeamIds.length !== 0 && selectedTeamIds.length !== this.teams.length)
				}
			},
			toggleAll: {
				value: function () {
					if (selectedTeamIds.length === this.teams.length) {
						selectedTeamIds = []
					} else if (selectedTeamIds.length === 0 || selectedTeamIds.length > 0) { // warum?
						selectedTeamIds = []
						this.teams.forEach(function (team) {
							selectedTeamIds.push(team.id)
						})
					}
				}
			},
			toggle: {
				value: function (id) {
					var index = selectedTeamIds.indexOf(id)
					if (index > -1) {
						selectedTeamIds.splice(index, 1)
					} else {
						selectedTeamIds.push(id)
					}
				}
			},
			selectTeam: {
				value: function (teamId) {
					var index = selectedTeamIds.indexOf(teamId)
					if (index === -1)
						selectedTeamIds.push(teamId)
				}
			},
			unselectTeam: {
				value: function (teamId) {
					var index = selectedTeamIds.indexOf(teamId)
					if (index > -1)
						selectedTeamIds.splice(index, 1)
				}
			},
			checkTeamIsSelected: {
				value: function (id) {
					return selectedTeamIds.indexOf(id) > -1
				}
			},
		}

		Object.defineProperties(this, props)
	}

	function Team(id, name, site) {
		var checked = false
		var props = {
			id: { value: id },
			name: { value: name },
			site: { value: site },
			index: { writable: true, value: -1 },
			checked: {
				get: function () { return checked },
				set: function (value) {
					checked = value
					this.site.toggle(this)
				}
			}
		}

		Object.defineProperties(this, props)
	}

	function SlaveTeam(masterTeam, slaveSite) {
		var props = {
			m: { value: masterTeam },
			site: { value: slaveSite },
		}
		Object.defineProperties(this, props)
	}

	function SlaveSite(masterSite) {
		var selectedTeamIds = []
		var props = {
			m: { value: masterSite },
			teams: { value: [] },
			selectedTeamIds: { get: function () { return selectedTeamIds } },
			collapsed: {
				value: false, writable: true
			},
			toggle: {
				value: function (id) {
					var index = selectedTeamIds.indexOf(id)
					if (index > -1) {
						selectedTeamIds.splice(index, 1)
					} else {
						selectedTeamIds.push(id)
					}
				}
			},
			toggleAll: {
				value: function () {
					if (selectedTeamIds.length === this.teams.length) {
						selectedTeamIds.splice(0)
						this.teams.forEach(function (slaveTeam) {
							this.m.unselectTeam(slaveTeam.m.id)
						}, this)
					} else {
						selectedTeamIds.splice(0)
						this.teams.forEach(function (slaveTeam) {
							selectedTeamIds.push(slaveTeam.m.id)
							this.m.selectTeam(slaveTeam.m.id)
						}, this)
					}
				}
			},
			expand: {
				value: function () {
					this.collapsed = false
				}
			},
			isChecked: {
				value: function () { return selectedTeamIds.length === this.teams.length }
			},
			isIndeterminate: {
				value: function () {
					return (selectedTeamIds.length !== 0 && selectedTeamIds.length !== this.teams.length)
				}
			},
			addTeam: {
				value: function (slaveTeam) {
					this.teams.push(slaveTeam)
					if (this.m.checkTeamIsSelected(slaveTeam.m.id)) {
						selectedTeamIds.push(slaveTeam.m.id)
					}
				}
			},
			updateSelectedTeamIds: {
				value: function () {
					selectedTeamIds.splice(0)
					this.teams.forEach(function (slaveTeam) {
						var id = slaveTeam.m.id
						if (this.m.checkTeamIsSelected(id))
							selectedTeamIds.push(id)
					}, this)
				}
			},
		}
		Object.defineProperties(this, props)
	}

})(angular);
