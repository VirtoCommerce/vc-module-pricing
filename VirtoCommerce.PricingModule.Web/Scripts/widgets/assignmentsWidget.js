angular.module('virtoCommerce.pricingModule')
.controller('virtoCommerce.pricingModule.assignmentsWidgetController', ['$scope', 'platformWebApp.bladeNavigationService', 'virtoCommerce.pricingModule.pricelistAssignments', function ($scope, bladeNavigationService, assignments) {
	var blade = $scope.widget.blade;
	$scope.widget.assignmentsCount = 0;
	$scope.widget.loading = true;
	function refresh() {
		$scope.widget.loading = true;
		return assignments.search({
			priceListId: blade.currentEntityId,
			take: 0
		}, function (data) {
			$scope.widget.loading = false;
			$scope.widget.assignmentsCount = data.totalCount;
		});
	}

    $scope.openBlade = function () {
        var newBlade = {
            id: "pricelistChild",
            pricelistId: blade.currentEntity.id,
            currentEntity: blade.currentEntity,
            title: blade.title,
            parentWidgetRefresh: refresh,
            subtitle: 'pricing.blades.pricelist-assignment-list.subtitle',
            controller: 'virtoCommerce.pricingModule.assignmentListController',
            template: 'Modules/$(VirtoCommerce.Pricing)/Scripts/blades/assignment-list.tpl.html'
        };

        bladeNavigationService.showBlade(newBlade, blade);
    };
    refresh();
}]);
