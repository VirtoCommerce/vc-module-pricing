angular.module('virtoCommerce.pricingModule')
.controller('virtoCommerce.pricingModule.itemPricelistsListController', ['$scope', 'platformWebApp.bladeNavigationService', 'uiGridConstants', 'platformWebApp.uiGridHelper', function ($scope, bladeNavigationService, uiGridConstants, uiGridHelper) {
    $scope.uiGridConstants = uiGridConstants;
    var blade = $scope.blade;

    blade.refresh = function () {
        blade.isLoading = true;
        return blade.parentWidgetRefresh().then(function (results) {
            blade.isLoading = false;
            blade.currentEntities = results;
            return results;
        }, function (reason) {
            blade.isLoading = false;
        });
    }

    $scope.openBlade = function (data) {
        $scope.selectedNodeId = data.id;

        var newBlade = {
            id: "itemPrices",
            itemId: blade.itemId,
            priceListId: data.id,
            data: data,
            currency: data.currency,
            title: data.name,
            subtitle: 'pricing.blades.prices-list.subtitle',
            controller: 'virtoCommerce.pricingModule.pricesListController',
            template: 'Modules/$(VirtoCommerce.Pricing)/Scripts/blades/prices-list.tpl.html'
        };
        bladeNavigationService.showBlade(newBlade, blade);
    }

    blade.headIcon = 'fa-usd';
    blade.toolbarCommands = [
        {
            name: "platform.commands.refresh", icon: 'fa fa-refresh',
            executeMethod: blade.refresh,
            canExecuteMethod: function () {
                return true;
            }
        }
		//{
		//    name: "pricing.blades.pricelist-list.subtitle", icon: 'fa fa-usd',
		//    executeMethod: function () {
		//        var newBlade = {
		//            id: 'pricingList',
		//            title: 'pricing.blades.pricing-main.menu.pricelist-list.title',
		//            controller: 'virtoCommerce.pricingModule.pricelistListController',
		//            template: 'Modules/$(VirtoCommerce.Pricing)/Scripts/blades/pricelist-list.tpl.html'
		//        };
		//        bladeNavigationService.showBlade(newBlade, blade.parentBlade);
		//    },
		//    canExecuteMethod: function () { return true; },
		//    permission: 'pricing:access'
		//}
    ];

    // ui-grid
    $scope.setGridOptions = function (gridOptions) {
        uiGridHelper.initialize($scope, gridOptions);
    };

    blade.refresh();
}]);
