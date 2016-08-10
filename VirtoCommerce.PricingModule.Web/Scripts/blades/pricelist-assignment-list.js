angular.module('virtoCommerce.pricingModule')
.controller('virtoCommerce.pricingModule.pricelistAssignmentListController', ['$scope', 'platformWebApp.bladeNavigationService', function ($scope, bladeNavigationService) {
    var blade = $scope.blade;
    var selectedNode = null;

    function initializeBlade(data) {
        blade.currentEntities = data;
        blade.isLoading = false;
    }

    $scope.selectNode = function (node, isNew) {
        selectedNode = node;
        $scope.selectedNodeId = selectedNode.id;

        var newBlade = {
            id: 'pricelistChildChild',
            controller: 'virtoCommerce.pricingModule.assignmentDetailController',
            template: 'Modules/$(VirtoCommerce.Pricing)/Scripts/blades/assignment-detail.tpl.html'
        };

        if (isNew) {
            angular.extend(newBlade, {
                isNew: true,
                data: selectedNode,
                title: 'pricing.blades.assignment-detail.new-title'
            });
        } else {
            angular.extend(newBlade, {
                currentEntityId: selectedNode.id,
                title: selectedNode.name,
                subtitle: 'pricing.blades.assignment-detail.subtitle'
            });
        }

        bladeNavigationService.showBlade(newBlade, blade);
    };

    blade.headIcon = 'fa-anchor';

    blade.toolbarCommands = [
      {
          name: "platform.commands.add", icon: 'fa fa-plus',
          executeMethod: function () {
              $scope.selectNode({ pricelistId: blade.currentEntity.id }, true);
          },
          canExecuteMethod: function () { return true; },
          permission: 'pricing:create'
      }
    ];

    $scope.$watch('blade.parentBlade.currentEntity.assignments', function (currentEntities) {
        initializeBlade(currentEntities);
    });

    // actions on load
    // $scope.$watch('blade.parentBlade.currentEntity.assignments' gets fired
}]);