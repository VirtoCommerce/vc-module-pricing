angular.module('virtoCommerce.pricingModule')
.controller('virtoCommerce.pricingModule.itemPricesWidgetController', ['$scope', '$filter', 'platformWebApp.bladeNavigationService', 'virtoCommerce.pricingModule.pricelists', 'virtoCommerce.pricingModule.prices', '$q', function ($scope, $filter, bladeNavigationService, pricelists, prices, $q) {
    var blade = $scope.blade;

    var pricelists = pricelists.query();

    function refresh() {
        $scope.priceRange = '...';

        var prodPrices = prices.get({ id: blade.itemId });
        var deferred = $q.defer();

        $q.all([pricelists.$promise, prodPrices.$promise]).then(function () {
            var prices = prodPrices.prices;
            if (_.any(prices)) {
                _.each(prices, function (p) {
                    var found = _.findWhere(pricelists, { id: p.pricelistId });
                    if (found) {
                        p.currency = found.currency;
                    }
                });

                prices = _.groupBy(prices, 'currency');
                prices = _.max(_.values(prices), function (x) { return x.length; });
                var allPrices = _.union(_.pluck(prices, 'list'), _.pluck(prices, 'sale'));
                var minprice = _.min(allPrices);
                var maxprice = _.max(allPrices);
                var currency = _.any(prices) ? ' ' + prices[0].currency : '';
                minprice = $filter('number')(minprice, 2);
                maxprice = $filter('number')(maxprice, 2);
                $scope.priceRange = (minprice == maxprice ? minprice : minprice + '-' + maxprice) + currency;
            } else {
                $scope.priceRange = 'N/A';
            }

            var processedPricelists = angular.copy(pricelists);
            _.each(processedPricelists, function (list) {
                list.prices = _.where(prodPrices.prices, { pricelistId: list.id });
            });

            deferred.resolve(processedPricelists);
        });

        return deferred.promise;
    }

    $scope.openBlade = function () {
        if ($scope.priceRange !== '...') {
            var newBlade = {
                id: "itemPricelists",
                itemId: blade.itemId,
                parentWidgetRefresh: refresh,
                title: blade.title,
                subtitle: 'pricing.blades.item-pricelists-list.subtitle',
                controller: 'virtoCommerce.pricingModule.itemPricelistsListController',
                template: 'Modules/$(VirtoCommerce.Pricing)/Scripts/blades/item/item-pricelists-list.tpl.html'
            };
            bladeNavigationService.showBlade(newBlade, blade);
        }
    };

    refresh();
}]);