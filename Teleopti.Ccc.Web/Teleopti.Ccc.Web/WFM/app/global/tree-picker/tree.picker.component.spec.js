'use strict';

describe('treePickerComponent', function () {
  var
  $componentController,
  ctrl,
  mockedData,
  mockedOutputData,
  mockedOptions;

  beforeEach(function () {
    module('wfm.treePicker');
  });

  beforeEach(inject(function (_$componentController_) {
    $componentController = _$componentController_;

    mockedOutputData = [];

  }));

  describe('Organization Tree', function () {

    beforeEach(function () {
      mockedOptions = {
        orgPicker: true,
        topSelectOnly: false,
        singleSelectOnly: false
      };

      mockedData = {
        nodes: [
          {
            name: 'parent1',
            id: '1',
            nodes: [
              {
                name: 'child1',
                id: '2',
                nodes: [
                  {
                    name: 'grandchild1',
                    id: '3',
                    nodes: []
                  }
                ]
              }
            ]
          },
          {
            name: 'parent2',
            id: '4',
            nodes: [
              {
                name: 'child1',
                id: '5',
                nodes: [
                  {
                    name: 'grandchild1',
                    id: '6',
                    nodes: []
                  }
                ]
              },
              {
                name: 'child2',
                id: '7',
                nodes: [
                  {
                    name: 'grandchild2',
                    id: '8',
                    nodes: []
                  }
                ]
              }
            ]
          }
        ]
      };

    });

    it('should add selection method to each node', function () {
      ctrl = $componentController('treePicker', null, {
        data: mockedData,
        outputData: mockedOutputData,
        options: mockedOptions
      });

      ctrl.$onInit();

      expect(typeof ctrl.treeCollection.nodes[0].clickedNode).toBe('function');
      expect(typeof ctrl.treeCollection.nodes[1].clickedNode).toBe('function');
      expect(typeof ctrl.treeCollection.nodes[0].nodes[0].clickedNode).toBe('function');
      expect(typeof ctrl.treeCollection.nodes[1].nodes[0].clickedNode).toBe('function');
      expect(typeof ctrl.treeCollection.nodes[0].nodes[0].nodes[0].clickedNode).toBe('function');
    });

    it('should add open method to each node that has children', function () {
      ctrl = $componentController('treePicker', null, {
        data: mockedData,
        outputData: mockedOutputData,
        options: mockedOptions
      });

      ctrl.$onInit();

      expect(typeof ctrl.treeCollection.nodes[0].openNode).toBe('function');
      expect(typeof ctrl.treeCollection.nodes[1].openNode).toBe('function');
      expect(typeof ctrl.treeCollection.nodes[0].nodes[0].openNode).toBe('function');
      expect(typeof ctrl.treeCollection.nodes[1].nodes[0].openNode).toBe('function');
      expect(ctrl.treeCollection.nodes[0].nodes[0].nodes[0].openNode).toBe(undefined);
    });

    it('should add isSelectedInUI property to each node', function () {
      ctrl = $componentController('treePicker', null, {
        data: mockedData,
        outputData: mockedOutputData,
        options: mockedOptions
      });

      ctrl.$onInit();

      expect(ctrl.treeCollection.nodes[0].isSelectedInUI).toBe(false);
      expect(ctrl.treeCollection.nodes[1].isSelectedInUI).toBe(false);
      expect(ctrl.treeCollection.nodes[0].nodes[0].isSelectedInUI).toBe(false);
      expect(ctrl.treeCollection.nodes[1].nodes[0].isSelectedInUI).toBe(false);
      expect(ctrl.treeCollection.nodes[0].nodes[0].nodes[0].isSelectedInUI).toBe(false);
    });

    it('should add isOpenInUI property to each node', function () {
      ctrl = $componentController('treePicker', null, {
        data: mockedData,
        outputData: mockedOutputData,
        options: mockedOptions
      });

      ctrl.$onInit();

      expect(ctrl.treeCollection.nodes[0].isOpenInUI).toBe(false);
      expect(ctrl.treeCollection.nodes[1].isOpenInUI).toBe(false);
      expect(ctrl.treeCollection.nodes[0].nodes[0].isOpenInUI).toBe(false);
      expect(ctrl.treeCollection.nodes[1].nodes[0].isOpenInUI).toBe(false);
    });

    it('should see node selected', function () {
      ctrl = $componentController('treePicker', null, {
        data: mockedData,
        outputData: mockedOutputData,
        options: mockedOptions
      });

      ctrl.$onInit();
      ctrl.treeCollection.nodes[0].clickedNode(ctrl.treeCollection.nodes[0]);

      expect(ctrl.treeCollection.nodes[0].isSelectedInUI).toBe(true);
    });

    it('should see node unselected', function () {
      ctrl = $componentController('treePicker', null, {
        data: mockedData,
        outputData: mockedOutputData,
        options: mockedOptions
      });

      ctrl.$onInit();
      ctrl.treeCollection.nodes[0].clickedNode(ctrl.treeCollection.nodes[0]);
      ctrl.treeCollection.nodes[0].clickedNode(ctrl.treeCollection.nodes[0]);

      expect(ctrl.treeCollection.nodes[0].isSelectedInUI).toBe(false);
    });

    it('should see parent open', function () {
      ctrl = $componentController('treePicker', null, {
        data: mockedData,
        outputData: mockedOutputData,
        options: mockedOptions
      });

      ctrl.$onInit();
      ctrl.treeCollection.nodes[0].openNode(ctrl.treeCollection.nodes[0]);

      expect(ctrl.treeCollection.nodes[0].isOpenInUI).toBe(true);
    });

    it('should see parent closed', function () {
      ctrl = $componentController('treePicker', null, {
        data: mockedData,
        outputData: mockedOutputData,
        options: mockedOptions
      });

      ctrl.$onInit();
      ctrl.treeCollection.nodes[0].openNode(ctrl.treeCollection.nodes[0]);
      ctrl.treeCollection.nodes[0].openNode(ctrl.treeCollection.nodes[0]);

      expect(ctrl.treeCollection.nodes[0].isOpenInUI).toBe(false);
    });

    it('should handle selection logic multiple times for parent', function () {
      ctrl = $componentController('treePicker', null, {
        data: mockedData,
        outputData: mockedOutputData,
        options: mockedOptions
      });
      ctrl.$onInit();

      ctrl.treeCollection.nodes[0].clickedNode(ctrl.treeCollection.nodes[0]);
      ctrl.treeCollection.nodes[0].clickedNode(ctrl.treeCollection.nodes[0]);
      ctrl.treeCollection.nodes[0].clickedNode(ctrl.treeCollection.nodes[0]);

      expect(ctrl.treeCollection.nodes[0].isSelectedInUI).toEqual(true);
      expect(ctrl.treeCollection.nodes[0].nodes[0].isSelectedInUI).toEqual(true);
      expect(ctrl.treeCollection.nodes[0].nodes[0].nodes[0].isSelectedInUI).toEqual(true);
      expect(ctrl.outputData.length).toEqual(3);
      expect(ctrl.outputData[0]).toEqual(ctrl.treeCollection.nodes[0].id);
      expect(ctrl.outputData[1]).toEqual(ctrl.treeCollection.nodes[0].nodes[0].id);
      expect(ctrl.outputData[2]).toEqual(ctrl.treeCollection.nodes[0].nodes[0].nodes[0].id);
    });

    it('should handle selection logic multiple times for parents', function () {
      ctrl = $componentController('treePicker', null, {
        data: mockedData,
        outputData: mockedOutputData,
        options: mockedOptions
      });
      ctrl.$onInit();

      ctrl.treeCollection.nodes[0].clickedNode(ctrl.treeCollection.nodes[0]);
      ctrl.treeCollection.nodes[0].clickedNode(ctrl.treeCollection.nodes[1]);
      ctrl.treeCollection.nodes[0].clickedNode(ctrl.treeCollection.nodes[0]);

      expect(ctrl.treeCollection.nodes[0].isSelectedInUI).toEqual(false);
      expect(ctrl.treeCollection.nodes[0].nodes[0].isSelectedInUI).toEqual(false);
      expect(ctrl.treeCollection.nodes[0].nodes[0].nodes[0].isSelectedInUI).toEqual(false);
      expect(ctrl.treeCollection.nodes[1].isSelectedInUI).toEqual(true);
      expect(ctrl.treeCollection.nodes[1].nodes[0].isSelectedInUI).toEqual(true);
      expect(ctrl.treeCollection.nodes[1].nodes[0].nodes[0].isSelectedInUI).toEqual(true);
      expect(ctrl.treeCollection.nodes[1].nodes[1].isSelectedInUI).toEqual(true);
      expect(ctrl.treeCollection.nodes[1].nodes[1].nodes[0].isSelectedInUI).toEqual(true);
      expect(ctrl.outputData.length).toEqual(5);
      expect(ctrl.outputData[0]).toEqual(ctrl.treeCollection.nodes[1].id);
      expect(ctrl.outputData[1]).toEqual(ctrl.treeCollection.nodes[1].nodes[0].id);
      expect(ctrl.outputData[2]).toEqual(ctrl.treeCollection.nodes[1].nodes[0].nodes[0].id);
      expect(ctrl.outputData[3]).toEqual(ctrl.treeCollection.nodes[1].nodes[1].id);
      expect(ctrl.outputData[4]).toEqual(ctrl.treeCollection.nodes[1].nodes[1].nodes[0].id);
    });

    it('should select parent and child for child', function () {
      ctrl = $componentController('treePicker', null, {
        data: mockedData,
        outputData: mockedOutputData,
        options: mockedOptions
      });
      ctrl.$onInit();

      ctrl.treeCollection.nodes[0].nodes[0].clickedNode(ctrl.treeCollection.nodes[0].nodes[0]);

      expect(ctrl.treeCollection.nodes[0].isSelectedInUI).toEqual(true);
      expect(ctrl.treeCollection.nodes[0].nodes[0].isSelectedInUI).toEqual(true);
      expect(ctrl.treeCollection.nodes[0].nodes[0].nodes[0].isSelectedInUI).toEqual(true);
      expect(ctrl.outputData.length).toEqual(3);
      expect(ctrl.outputData[0]).toEqual(ctrl.treeCollection.nodes[0].nodes[0].id);
      expect(ctrl.outputData[1]).toEqual(ctrl.treeCollection.nodes[0].id);
      expect(ctrl.outputData[2]).toEqual(ctrl.treeCollection.nodes[0].nodes[0].nodes[0].id);
    });

    it('should select all parents when clicking a leaf', function () {
      ctrl = $componentController('treePicker', null, {
        data: mockedData,
        outputData: mockedOutputData,
        options: mockedOptions
      });
      ctrl.$onInit();

      ctrl.treeCollection.nodes[0].nodes[0].nodes[0].clickedNode(ctrl.treeCollection.nodes[0].nodes[0].nodes[0]);

      expect(ctrl.treeCollection.nodes[0].isSelectedInUI).toEqual(true);
      expect(ctrl.treeCollection.nodes[0].nodes[0].isSelectedInUI).toEqual(true);
      expect(ctrl.treeCollection.nodes[0].nodes[0].nodes[0].isSelectedInUI).toEqual(true);
      expect(ctrl.outputData.length).toEqual(3);
      expect(ctrl.outputData[1]).toEqual(ctrl.treeCollection.nodes[0].nodes[0].id);
      expect(ctrl.outputData[2]).toEqual(ctrl.treeCollection.nodes[0].id);
      expect(ctrl.outputData[0]).toEqual(ctrl.treeCollection.nodes[0].nodes[0].nodes[0].id);
    });
  });

  describe('TopSelectOnly Tree', function () {

    beforeEach(function () {
      mockedOptions = {
        orgPicker: false,
        topSelectOnly: true,
        singleSelectOnly: false
      };
      mockedData = {
        nodes: [
          {
            name: 'parent1',
            id: '1',
            nodes: [
              {
                name: 'child1',
                id: '2',
                nodes: [
                  {
                    name: 'grandchild1',
                    id: '3',
                    nodes: []
                  }
                ]
              }
            ]
          },
          {
            name: 'parent2',
            id: '4',
            nodes: [
              {
                name: 'child1',
                id: '5',
                nodes: [
                  {
                    name: 'grandchild1',
                    id: '6',
                    nodes: []
                  }
                ]
              },
              {
                name: 'child2',
                id: '7',
                nodes: [
                  {
                    name: 'grandchild2',
                    id: '8',
                    nodes: []
                  }
                ]
              }
            ]
          }
        ]
      };
    });

    it('should select all parents when clicking a leaf', function () {
      ctrl = $componentController('treePicker', null, {
        data: mockedData,
        outputData: mockedOutputData,
        options: mockedOptions
      });
      ctrl.$onInit();

      ctrl.treeCollection.nodes[0].nodes[0].nodes[0].clickedNode(ctrl.treeCollection.nodes[0].nodes[0].nodes[0]);

      expect(ctrl.treeCollection.nodes[0].isSelectedInUI).toEqual(true);
      expect(ctrl.treeCollection.nodes[0].nodes[0].isSelectedInUI).toEqual(true);
      expect(ctrl.treeCollection.nodes[0].nodes[0].nodes[0].isSelectedInUI).toEqual(true);
      expect(ctrl.outputData.length).toEqual(3);
      expect(ctrl.outputData[1]).toEqual(ctrl.treeCollection.nodes[0].nodes[0].id);
      expect(ctrl.outputData[2]).toEqual(ctrl.treeCollection.nodes[0].id);
      expect(ctrl.outputData[0]).toEqual(ctrl.treeCollection.nodes[0].nodes[0].nodes[0].id);
    });

    it('should keep selected parents when clicking selected leaf', function () {
      ctrl = $componentController('treePicker', null, {
        data: mockedData,
        outputData: mockedOutputData,
        options: mockedOptions
      });
      ctrl.$onInit();

      ctrl.treeCollection.nodes[0].nodes[0].nodes[0].clickedNode(ctrl.treeCollection.nodes[0].nodes[0].nodes[0]);
      ctrl.treeCollection.nodes[0].nodes[0].nodes[0].clickedNode(ctrl.treeCollection.nodes[0].nodes[0].nodes[0]);

      expect(ctrl.treeCollection.nodes[0].isSelectedInUI).toEqual(true);
      expect(ctrl.treeCollection.nodes[0].nodes[0].isSelectedInUI).toEqual(true);
      expect(ctrl.treeCollection.nodes[0].nodes[0].nodes[0].isSelectedInUI).toEqual(false);
      expect(ctrl.outputData.length).toEqual(2);
      expect(ctrl.outputData[0]).toEqual(ctrl.treeCollection.nodes[0].nodes[0].id);
      expect(ctrl.outputData[1]).toEqual(ctrl.treeCollection.nodes[0].id);
    });

    it('should select all parents when clicking a parent child', function () {
      ctrl = $componentController('treePicker', null, {
        data: mockedData,
        outputData: mockedOutputData,
        options: mockedOptions
      });
      ctrl.$onInit();

      ctrl.treeCollection.nodes[0].nodes[0].clickedNode(ctrl.treeCollection.nodes[0].nodes[0]);

      expect(ctrl.treeCollection.nodes[0].isSelectedInUI).toEqual(true);
      expect(ctrl.treeCollection.nodes[0].nodes[0].isSelectedInUI).toEqual(true);
      expect(ctrl.treeCollection.nodes[0].nodes[0].nodes[0].isSelectedInUI).toEqual(false);
      expect(ctrl.outputData.length).toEqual(2);
      expect(ctrl.outputData[0]).toEqual(ctrl.treeCollection.nodes[0].nodes[0].id);
      expect(ctrl.outputData[1]).toEqual(ctrl.treeCollection.nodes[0].id);
    });

    it('should deselect all children when clicking a parent child', function () {
      ctrl = $componentController('treePicker', null, {
        data: mockedData,
        outputData: mockedOutputData,
        options: mockedOptions
      });
      ctrl.$onInit();

      ctrl.treeCollection.nodes[0].nodes[0].nodes[0].clickedNode(ctrl.treeCollection.nodes[0].nodes[0].nodes[0]);
      ctrl.treeCollection.nodes[0].nodes[0].clickedNode(ctrl.treeCollection.nodes[0].nodes[0]);

      expect(ctrl.treeCollection.nodes[0].isSelectedInUI).toEqual(true);
      expect(ctrl.treeCollection.nodes[0].nodes[0].isSelectedInUI).toEqual(false);
      expect(ctrl.treeCollection.nodes[0].nodes[0].nodes[0].isSelectedInUI).toEqual(false);
      expect(ctrl.outputData.length).toEqual(1);
      expect(ctrl.outputData[0]).toEqual(ctrl.treeCollection.nodes[0].id);
    });

    it('should deselect all children when unselecting root', function () {
      ctrl = $componentController('treePicker', null, {
        data: mockedData,
        outputData: mockedOutputData,
        options: mockedOptions
      });
      ctrl.$onInit();

      ctrl.treeCollection.nodes[0].nodes[0].nodes[0].clickedNode(ctrl.treeCollection.nodes[0].nodes[0].nodes[0]);
      ctrl.treeCollection.nodes[0].clickedNode(ctrl.treeCollection.nodes[0]);

      expect(ctrl.treeCollection.nodes[0].isSelectedInUI).toEqual(false);
      expect(ctrl.treeCollection.nodes[0].nodes[0].isSelectedInUI).toEqual(false);
      expect(ctrl.treeCollection.nodes[0].nodes[0].nodes[0].isSelectedInUI).toEqual(false);
      expect(ctrl.outputData.length).toEqual(0);
    });

  });

  describe('singleSelectOnly Tree', function () {

    beforeEach(function () {
      mockedOptions = {
        orgPicker: false,
        topSelectOnly: false,
        singleSelectOnly: true
      };
      mockedData = {
        nodes: [
          {
            name: 'parent1',
            id: '1',
            nodes: [
              {
                name: 'child1',
                id: '2',
                nodes: [
                  {
                    name: 'grandchild1',
                    id: '3',
                    nodes: []
                  }
                ]
              }
            ]
          },
          {
            name: 'parent2',
            id: '4',
            nodes: [
              {
                name: 'child1',
                id: '5',
                nodes: [
                  {
                    name: 'grandchild1',
                    id: '6',
                    nodes: []
                  }
                ]
              },
              {
                name: 'child2',
                id: '7',
                nodes: [
                  {
                    name: 'grandchild2',
                    id: '8',
                    nodes: []
                  }
                ]
              }
            ]
          }
        ]
      };
    });

    it('should only be able to select one root', function () {
      ctrl = $componentController('treePicker', null, {
        data: mockedData,
        outputData: mockedOutputData,
        options: mockedOptions
      });
      ctrl.$onInit();

      ctrl.treeCollection.nodes[1].clickedNode(ctrl.treeCollection.nodes[1]);
      ctrl.treeCollection.nodes[0].clickedNode(ctrl.treeCollection.nodes[0]);

      expect(ctrl.treeCollection.nodes[0].isSelectedInUI).toEqual(true);
      expect(ctrl.treeCollection.nodes[0].nodes[0].isSelectedInUI).toEqual(true);
      expect(ctrl.treeCollection.nodes[0].nodes[0].nodes[0].isSelectedInUI).toEqual(true);
      expect(ctrl.outputData.length).toEqual(3);
      expect(ctrl.outputData[0]).toEqual(ctrl.treeCollection.nodes[0].id);
      expect(ctrl.outputData[1]).toEqual(ctrl.treeCollection.nodes[0].nodes[0].id);
      expect(ctrl.outputData[2]).toEqual(ctrl.treeCollection.nodes[0].nodes[0].nodes[0].id);
    });

    it('should select root when clicking a parent child', function () {
      ctrl = $componentController('treePicker', null, {
        data: mockedData,
        outputData: mockedOutputData,
        options: mockedOptions
      });
      ctrl.$onInit();

      ctrl.treeCollection.nodes[0].clickedNode(ctrl.treeCollection.nodes[0]);
      ctrl.treeCollection.nodes[1].nodes[0].clickedNode(ctrl.treeCollection.nodes[1].nodes[0]);

      expect(ctrl.outputData.length).toEqual(3);
      expect(ctrl.outputData[0]).toEqual(ctrl.treeCollection.nodes[1].nodes[0].id);
      expect(ctrl.outputData[1]).toEqual(ctrl.treeCollection.nodes[1].id);
      expect(ctrl.outputData[2]).toEqual(ctrl.treeCollection.nodes[1].nodes[0].nodes[0].id);
    });

    it('should select root when clicking a parent child', function () {
      ctrl = $componentController('treePicker', null, {
        data: mockedData,
        outputData: mockedOutputData,
        options: mockedOptions
      });
      ctrl.$onInit();

      ctrl.treeCollection.nodes[0].clickedNode(ctrl.treeCollection.nodes[0]);
      ctrl.treeCollection.nodes[1].nodes[0].clickedNode(ctrl.treeCollection.nodes[1].nodes[0]);

      expect(ctrl.outputData.length).toEqual(3);
      expect(ctrl.outputData[0]).toEqual(ctrl.treeCollection.nodes[1].nodes[0].id);
      expect(ctrl.outputData[1]).toEqual(ctrl.treeCollection.nodes[1].id);
      expect(ctrl.outputData[2]).toEqual(ctrl.treeCollection.nodes[1].nodes[0].nodes[0].id);
    });

    it('should be able to unselect node with same root', function () {
      ctrl = $componentController('treePicker', null, {
        data: mockedData,
        outputData: mockedOutputData,
        options: mockedOptions
      });
      ctrl.$onInit();

      ctrl.treeCollection.nodes[1].clickedNode(ctrl.treeCollection.nodes[1]);
      ctrl.treeCollection.nodes[1].nodes[0].nodes[0].clickedNode(ctrl.treeCollection.nodes[1].nodes[0].nodes[0]);

      expect(ctrl.outputData.length).toEqual(3);

      expect(ctrl.outputData[0]).toEqual(ctrl.treeCollection.nodes[1].id);
      expect(ctrl.outputData[1]).toEqual(ctrl.treeCollection.nodes[1].nodes[1].id);
      expect(ctrl.outputData[2]).toEqual(ctrl.treeCollection.nodes[1].nodes[1].nodes[0].id);
    });

    it('should be able to unselect leaf node with same root ', function () {
      ctrl = $componentController('treePicker', null, {
        data: mockedData,
        outputData: mockedOutputData,
        options: mockedOptions
      });
      ctrl.$onInit();

      ctrl.treeCollection.nodes[1].clickedNode(ctrl.treeCollection.nodes[1]);
      ctrl.treeCollection.nodes[1].nodes[0].nodes[0].clickedNode(ctrl.treeCollection.nodes[1].nodes[0].nodes[0]);
      ctrl.treeCollection.nodes[1].nodes[0].nodes[0].clickedNode(ctrl.treeCollection.nodes[1].nodes[0].nodes[0]);

      expect(ctrl.outputData.length).toEqual(5);

      expect(ctrl.outputData[0]).toEqual(ctrl.treeCollection.nodes[1].id);
      expect(ctrl.outputData[1]).toEqual(ctrl.treeCollection.nodes[1].nodes[1].id);
      expect(ctrl.outputData[2]).toEqual(ctrl.treeCollection.nodes[1].nodes[1].nodes[0].id);
      expect(ctrl.outputData[3]).toEqual(ctrl.treeCollection.nodes[1].nodes[0].nodes[0].id);
      expect(ctrl.outputData[4]).toEqual(ctrl.treeCollection.nodes[1].nodes[0].id);
    });

    it('should unselect leaf node with same root when clicking parent child', function () {
      ctrl = $componentController('treePicker', null, {
        data: mockedData,
        outputData: mockedOutputData,
        options: mockedOptions
      });
      ctrl.$onInit();

      ctrl.treeCollection.nodes[1].clickedNode(ctrl.treeCollection.nodes[1]);
      ctrl.treeCollection.nodes[1].nodes[0].clickedNode(ctrl.treeCollection.nodes[1].nodes[0]);

      expect(ctrl.outputData.length).toEqual(3);

      expect(ctrl.outputData[0]).toEqual(ctrl.treeCollection.nodes[1].id);
      expect(ctrl.outputData[1]).toEqual(ctrl.treeCollection.nodes[1].nodes[1].id);
      expect(ctrl.outputData[2]).toEqual(ctrl.treeCollection.nodes[1].nodes[1].nodes[0].id);
    });

    it('should select leaf siblings', function () {
      ctrl = $componentController('treePicker', null, {
        data: mockedData,
        outputData: mockedOutputData,
        options: mockedOptions
      });


      ctrl.data.nodes[1].nodes[1].nodes.push({name: 'grandchildX', id: '9', nodes: []})

      ctrl.$onInit();
      ctrl.treeCollection.nodes[1].nodes[1].nodes[1].clickedNode(ctrl.treeCollection.nodes[1].nodes[1].nodes[1]);
      ctrl.treeCollection.nodes[1].nodes[1].nodes[0].clickedNode(ctrl.treeCollection.nodes[1].nodes[1].nodes[0]);

      expect(ctrl.outputData.length).toEqual(4);

      expect(ctrl.outputData[0]).toEqual(ctrl.treeCollection.nodes[1].nodes[1].nodes[1].id);
      expect(ctrl.outputData[1]).toEqual(ctrl.treeCollection.nodes[1].nodes[1].id);
      expect(ctrl.outputData[2]).toEqual(ctrl.treeCollection.nodes[1].id);
      expect(ctrl.outputData[3]).toEqual(ctrl.treeCollection.nodes[1].nodes[1].nodes[0].id);
    });

    it('should unselect leaf sibling', function () {
      ctrl = $componentController('treePicker', null, {
        data: mockedData,
        outputData: mockedOutputData,
        options: mockedOptions
      });

      ctrl.data.nodes[1].nodes[1].nodes.push({name: 'grandchildX', id: '9', nodes: []})

      ctrl.$onInit();
      ctrl.treeCollection.nodes[1].nodes[1].nodes[1].clickedNode(ctrl.treeCollection.nodes[1].nodes[1].nodes[1]);
      ctrl.treeCollection.nodes[1].nodes[1].nodes[1].clickedNode(ctrl.treeCollection.nodes[1].nodes[1].nodes[1]);

      expect(ctrl.outputData.length).toEqual(0);
    });

    it('should select leaves when clicking parent child', function () {
      ctrl = $componentController('treePicker', null, {
        data: mockedData,
        outputData: mockedOutputData,
        options: mockedOptions
      });

      ctrl.data.nodes[1].nodes[1].nodes.push({name: 'grandchildX', id: '9', nodes: []})

      ctrl.$onInit();
      ctrl.treeCollection.nodes[1].nodes[1].nodes[1].clickedNode(ctrl.treeCollection.nodes[1].nodes[1].nodes[1]);
      ctrl.treeCollection.nodes[1].nodes[1].clickedNode(ctrl.treeCollection.nodes[1].nodes[1]);

      expect(ctrl.outputData.length).toEqual(0);
    });

    it('should deselect leaves when clicking parent child', function () {
      ctrl = $componentController('treePicker', null, {
        data: mockedData,
        outputData: mockedOutputData,
        options: mockedOptions
      });

      ctrl.data.nodes[1].nodes[1].nodes.push({name: 'grandchildX', id: '9', nodes: []})

      ctrl.$onInit();
      ctrl.treeCollection.nodes[1].nodes[1].nodes[1].clickedNode(ctrl.treeCollection.nodes[1].nodes[1].nodes[1]);
      ctrl.treeCollection.nodes[1].nodes[1].nodes[1].clickedNode(ctrl.treeCollection.nodes[1].nodes[1].nodes[1]);
      ctrl.treeCollection.nodes[1].nodes[1].clickedNode(ctrl.treeCollection.nodes[1].nodes[1]);

      expect(ctrl.outputData.length).toEqual(4);
      expect(ctrl.outputData[0]).toEqual(ctrl.treeCollection.nodes[1].nodes[1].id);
      expect(ctrl.outputData[1]).toEqual(ctrl.treeCollection.nodes[1].id);
      expect(ctrl.outputData[2]).toEqual(ctrl.treeCollection.nodes[1].nodes[1].nodes[0].id);
      expect(ctrl.outputData[3]).toEqual(ctrl.treeCollection.nodes[1].nodes[1].nodes[1].id);
    });

  });

});
