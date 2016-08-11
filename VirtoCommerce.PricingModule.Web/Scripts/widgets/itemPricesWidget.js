angular.module('virtoCommerce.pricingModule')
.controller('virtoCommerce.pricingModule.itemPricesWidgetController', ['$scope', '$filter', 'platformWebApp.bladeNavigationService', 'virtoCommerce.pricingModule.pricelists', 'virtoCommerce.pricingModule.prices', 'virtoCommerce.pricingModule.pricelistAssignments', '$q', '$state',
    function ($scope, $filter, bladeNavigationService, pricelists, prices, pricelistAssignments, $q, $state) {
        var blade = $scope.blade;
        $scope.priceLabel = '...';
        var pricelists, catalogAssignments;

        // first check if catalog has any assingment
        pricelistAssignments.query({}, function (data) {
            catalogAssignments = _.where(data, { catalogId: bladeNavigationService.catalogsSelectedCatalog.id });
            if (_.any(catalogAssignments)) {
                pricelists = pricelists.query();

                catalogAssignments.sort(function (a, b) { return b.priority - a.priority; });
                refreshPricesCount(true);
            } else {
                $scope.noPricelist = true;
            }
        }, function (error) { bladeNavigationService.setError('Error ' + error.status, blade); });


        function refreshPricesCount(doInitialize) {
            $scope.priceLabel = '...';

            var prodPricesResult = prices.get({ id: blade.itemId });
            var deferred = $q.defer();

            $q.all([pricelists.$promise, prodPricesResult.$promise]).then(function () {
                var prodPrices = prodPricesResult.prices;

                // set priority and prices for catalog pricelists
                if (doInitialize) {
                    pricelists = _.filter(pricelists, function (x) {
                        return _.any(catalogAssignments, function (a) { return a.pricelistId === x.id; });
                    });
                    // set assignment priority to pricelist
                    _.each(pricelists, function (x) {
                        var pricelistAssignments = _.where(catalogAssignments, { pricelistId: x.id });
                        x.priority = _.pluck(pricelistAssignments, 'priority').sort().join(", ");
                    });
                }

                _.each(pricelists, function (list) {
                    list.prices = _.where(prodPrices, { pricelistId: list.id });
                    list.prices.sort(function (a, b) { return a.minQuantity - b.minQuantity; });
                });

                deferred.resolve(pricelists);

                // calculate $scope.priceLabel as list price for single item from highest priority pricelist                
                var thePrice;
                if (_.any(prodPrices)) {
                    var pricelistIds = _.pluck(catalogAssignments, 'pricelistId');

                    for (var i = 0; i < pricelistIds.length; i++) {
                        var list = _.findWhere(pricelists, { id: pricelistIds[i] });
                        if (_.any(list.prices)) {
                            thePrice = list.prices[0];
                            break;
                        }
                    }
                }

                if (thePrice) {
                    $scope.priceLabel = $filter('number')(thePrice.list, 2) + ' ' + thePrice.currency;
                } else {
                    $scope.priceLabel = 'N/A';
                }
            });

            return deferred.promise;
        }

        $scope.openBlade = function () {
            if ($scope.noPricelist) {
                bladeNavigationService.closeChildrenBlades(blade.parentBlade, function () {
                    $state.go('workspace.pricingModule');
                });
            } else if ($scope.priceLabel !== '...') {
                var newBlade = {
                    id: "itemPricelists",
                    itemId: blade.itemId,
                    parentWidgetRefresh: refreshPricesCount,
                    title: blade.title,
                    subtitle: 'pricing.blades.item-pricelists-list.subtitle',
                    controller: 'virtoCommerce.pricingModule.itemPricelistsListController',
                    template: 'Modules/$(VirtoCommerce.Pricing)/Scripts/blades/item/item-pricelists-list.tpl.html'
                };
                bladeNavigationService.showBlade(newBlade, blade);
            }
        };

    }]);