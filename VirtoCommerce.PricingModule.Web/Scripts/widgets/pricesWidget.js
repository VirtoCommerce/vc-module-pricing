angular.module('virtoCommerce.pricingModule')
.controller('virtoCommerce.pricingModule.pricesWidgetController', ['$scope', 'platformWebApp.bladeNavigationService', 'virtoCommerce.pricingModule.productPrices', function ($scope, bladeNavigationService, productPrices) {
    var blade = $scope.widget.blade;

    function refresh() {
        $scope.priceCount = '...';

        productPrices.search({
            priceListId: blade.currentEntityId,
            take: 0
        }, function (data) {
            $scope.priceCount = data.totalCount;
        }//, function (error) { bladeNavigationService.setError('Error ' + error.status, blade); }
        );
    }

    //$scope.getPriceCount = function () {
    //    var retVal;
    //    // all prices count
    //    if (blade.currentEntity) {
    //        var pricelistPrices = _.flatten(_.pluck(blade.currentEntity.productPrices, 'prices'), true);
    //        retVal = pricelistPrices.length;
    //    } else {
    //        retVal = '';
    //    }
    //    return retVal;
    //}

    $scope.openBlade = function () {
        var newBlade = {
            id: "pricelistChild",
            currency: blade.currentEntity.currency,
            currentEntity: blade.currentEntity,
            currentEntityId: blade.currentEntityId,
            title: blade.title,
            subtitle: 'pricing.blades.pricelist-item-list.subtitle',
            controller: 'virtoCommerce.pricingModule.pricelistItemListController',
            template: 'Modules/$(VirtoCommerce.Pricing)/Scripts/blades/pricelist-item-list.tpl.html'
        };

        bladeNavigationService.showBlade(newBlade, blade);
    };

    refresh();
}]);