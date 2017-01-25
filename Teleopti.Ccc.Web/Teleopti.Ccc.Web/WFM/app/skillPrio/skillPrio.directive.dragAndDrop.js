(function() {
	'use strict';

	angular
		.module('wfm.skillPrio')
		.directive('draggable', draggable)
		.directive('droppable', droppable);

		function draggable() {
				var directive = {
						restrict: 'EA',
						scope: {
							draggable: '='
						},
						link: linkFunc,
				};

				return directive;

				function linkFunc(scope, element, attrs, ctrl) {
					var el = element[0];
					el.draggable = true;

					el.addEventListener(
						'dragstart',
						function(e) {
							e.dataTransfer.effectAllowed = 'move';
							this.classList.add('dragging');

							var parent_id = this.parentElement.attributes['parent-id'] ? this.parentElement.attributes['parent-id'].value : null;
							var source_id = this.attributes['node-id'] ? this.attributes['node-id'].value : null;
							var id = [source_id, parent_id].toString();
						  e.dataTransfer.setData('text', id);

							return false;
						}, false
					);

					el.addEventListener(
						'dragend',
						function(e) {
							this.classList.remove('dragging');
							return false;
						},
						false
					);
				}
		}

		function droppable() {
				var directive = {
						restrict: 'EA',
						scope: {
							droppable: '='
						},
						link: linkFunc,
				};

				return directive;

				function linkFunc(scope, element, attrs, ctrl) {
					var el = element[0];
					el.addEventListener(
						'dragover',
						function(e) {
							e.preventDefault();
							e.dataTransfer.dropEffect = 'move';
							this.classList.add('dragover');
							return false;
						}, false
					);

					el.addEventListener(
						'drop',
						function(e) {
							this.classList.remove('dragover');

							var idList = e.dataTransfer.getData('text');
							var id  = idList.split(',');
							var source_id = id[0];
							var parent_id = id[1];
							var dest_parent_id = this.attributes['parent-id'].value;
							
							scope.droppable.moveItem(parent_id,  source_id, dest_parent_id);
							scope.$apply();

							e.dataTransfer.clearData();
							return false;
						}, false
					);
				}
		}
})();
