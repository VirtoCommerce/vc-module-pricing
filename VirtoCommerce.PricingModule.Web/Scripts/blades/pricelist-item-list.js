angular.module('virtoCommerce.pricingModule')
.controller('virtoCommerce.pricingModule.pricelistItemListController', ['$scope', 'virtoCommerce.pricingModule.productPrices', '$filter', 'platformWebApp.bladeNavigationService', 'uiGridConstants', 'platformWebApp.uiGridHelper', 'platformWebApp.bladeUtils', function ($scope, productPrices, $filter, bladeNavigationService, uiGridConstants, uiGridHelper, bladeUtils) {
    $scope.uiGridConstants = uiGridConstants;
    var blade = $scope.blade;

    blade.refresh = function () {
        blade.isLoading = true;

        productPrices.search({
            priceListId: blade.currentEntityId,
            keyword: filter.keyword,
            sort: uiGridHelper.getSortExpression($scope),
            skip: ($scope.pageSettings.currentPage - 1) * $scope.pageSettings.itemsPerPageCount,
            take: $scope.pageSettings.itemsPerPageCount
        }, function (data) {
            blade.currentEntities = data.productPrices;
            $scope.pageSettings.totalItems = data.totalCount;

            blade.isLoading = false;
        }, function (error) { bladeNavigationService.setError('Error ' + error.status, blade); });
    };

    $scope.selectNode = function (node) {
        $scope.selectedNodeId = node.productId;

        var newBlade = {
            id: 'pricelistChildChild',
            itemId: node.productId,
            priceListId: blade.currentEntityId,
            data: node,
            currency: blade.currency,
            title: 'pricing.blades.prices-list.title',
            titleValues: { name: node.product.name },
            subtitle: 'pricing.blades.prices-list.subtitle',
            controller: 'virtoCommerce.pricingModule.pricesListController',
            template: 'Modules/$(VirtoCommerce.Pricing)/Scripts/blades/prices-list.tpl.html'
        };

        bladeNavigationService.showBlade(newBlade, blade);
    };

    function openAddEntityWizard() {
        var selectedProducts = [];
        var newBlade = {
            id: "CatalogItemsSelect",
            title: "Select items for pricing", //catalogItemSelectController: hard-coded title
            controller: 'virtoCommerce.catalogModule.catalogItemSelectController',
            template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/common/catalog-items-select.tpl.html',
            breadcrumbs: [],
            toolbarCommands: [
        {
            name: "pricing.commands.add-selected", icon: 'fa fa-plus',
            executeMethod: function (blade) {
                addProductsToPricelist(selectedProducts);
                bladeNavigationService.closeBlade(blade);
            },
            canExecuteMethod: function () {
                return selectedProducts.length > 0;
            }
        }]
        };

        newBlade.options = {
            checkItemFn: function (listItem, isSelected) {
                if (listItem.type == 'category') {
                    newBlade.error = 'Categories are not supported';
                    listItem.selected = undefined;
                } else {
                    if (isSelected) {
                        if (_.all(selectedProducts, function (x) { return x.id != listItem.id; })) {
                            selectedProducts.push(listItem);
                        }
                    }
                    else {
                        selectedProducts = _.reject(selectedProducts, function (x) { return x.id == listItem.id; });
                    }
                    newBlade.error = undefined;
                }
            }
        };

        bladeNavigationService.showBlade(newBlade, blade);
    }

    function addProductsToPricelist(products) {
        angular.forEach(products, function (product) {
            if (_.all(blade.currentEntities, function (x) { return x.productId != product.id; })) {
                var newPricelistItem =
                {
                    product: product,
                    productId: product.id,
                    prices: []
                };
                blade.currentEntities.push(newPricelistItem);
            }
        });

        // TODO: updateAll
        if (products.length === 1) {
            $scope.selectNode(_.last(blade.currentEntities));
        }
    }

    blade.headIcon = 'fa-usd';

    blade.toolbarCommands = [
            {
                name: "platform.commands.add", icon: 'fa fa-plus',
                executeMethod: function () {
                    openAddEntityWizard();
                },
                canExecuteMethod: function () {
                    return true;
                },
                permission: 'pricing:update'
            }
    ];

    $scope.getPriceRange = function (priceGroup) {
        var retVal;
        var allPrices = _.union(_.pluck(priceGroup.prices, 'list'), _.pluck(priceGroup.prices, 'sale'));
        var minprice = $filter('number')(_.min(allPrices), 2);
        var maxprice = $filter('number')(_.max(allPrices), 2);
        retVal = (minprice == maxprice ? minprice : minprice + '-' + maxprice);

        //else {
        //    retVal = 'NO PRICE';
        //}

        return retVal;
    }

    var filter = $scope.filter = {};
    filter.criteriaChanged = function () {
        if ($scope.pageSettings.currentPage > 1) {
            $scope.pageSettings.currentPage = 1;
        } else {
            blade.refresh();
        }
    };

    // ui-grid
    $scope.setGridOptions = function (gridOptions) {
        uiGridHelper.initialize($scope, gridOptions, function (gridApi) {
            uiGridHelper.bindRefreshOnSortChanged($scope);
        });
        bladeUtils.initializePagination($scope);
    };

    //No need to call this because page 'pageSettings.currentPage' is watched!!! It would trigger subsequent duplicated req...
    //blade.refresh();
}]);