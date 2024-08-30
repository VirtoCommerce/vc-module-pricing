angular.module('virtoCommerce.pricingModule')
    .controller('virtoCommerce.pricingModule.pricesListController', ['$scope', 'virtoCommerce.pricingModule.prices', 'platformWebApp.objCompareService', 'platformWebApp.bladeNavigationService', 'platformWebApp.uiGridHelper', 'virtoCommerce.pricingModule.priceValidatorsService', 'platformWebApp.ui-grid.extension', function ($scope, prices, objCompareService, bladeNavigationService, uiGridHelper, priceValidatorsService, gridOptionExtension) {
        $scope.uiGridConstants = uiGridHelper.uiGridConstants;
        var blade = $scope.blade;
        blade.updatePermission = 'pricing:update';

        blade.refresh = function () {
            blade.data.productId = blade.itemId;
            if (!blade.data.prices) {
                blade.data.prices = [];
            }

            blade.currentEntities = angular.copy(blade.data.prices);
            blade.origEntity = blade.data.prices;
            blade.isLoading = false;
            priceValidatorsService.setAllPrices(blade.currentEntities);
        };

        blade.onClose = function (closeCallback) {
            bladeNavigationService.showConfirmationIfNeeded(isDirty(), canSave(), blade, $scope.saveChanges, closeCallback, "pricing.dialogs.prices-save.title", "pricing.dialogs.prices-save.message");
        };

        function isDirty() {
            return blade.currentEntities && !objCompareService.equal(blade.origEntity, blade.currentEntities) && blade.hasUpdatePermission();
        }

        function canSave() {
            return isDirty() && $scope.isValid();
        }

        $scope.cancelChanges = function () {
            $scope.bladeClose();
        };

        $scope.isValid = function () {
            return blade.currentEntities.length === 0 ||
                $scope.formScope &&
                $scope.formScope.$valid &&
                _.all(blade.currentEntities, function (x) { return x.list }) &&
                _.all(blade.currentEntities, $scope.isListPriceValid) &&
                _.all(blade.currentEntities, $scope.isSalePriceValid) &&
                _.all(blade.currentEntities, $scope.isUniqueQty) &&
                $scope.hasSingleItemPrice();
        };

        $scope.saveChanges = function () {
            blade.isLoading = true;

            angular.copy(blade.currentEntities, blade.origEntity);
            if (_.any(blade.currentEntities)) {
                prices.update({ id: blade.itemId }, blade.data, function (data) {
                    $scope.bladeClose();
                    blade.parentBlade.refresh();
                },
                    function (error) { bladeNavigationService.setError('Error ' + error.status, $scope.blade); });
            }
            else {
                prices.remove({ priceListId: blade.priceListId, productIds: [blade.itemId] }, function () {
                    $scope.bladeClose();
                    blade.parentBlade.refresh();
                },
                    function (error) { bladeNavigationService.setError('Error ' + error.status, blade); });
            }
        };

        $scope.delete = function (listItem) {
            if (listItem) {
                blade.currentEntities.splice(blade.currentEntities.indexOf(listItem), 1);
            }
        };

        $scope.setForm = function (form) { $scope.formScope = form; };

        blade.toolbarCommands = [
            {
                name: "platform.commands.save", icon: 'fas fa-save',
                executeMethod: $scope.saveChanges,
                canExecuteMethod: canSave,
                permission: blade.updatePermission
            },
            {
                name: "platform.commands.reset", icon: 'fa fa-undo',
                executeMethod: function () {
                    angular.copy(blade.origEntity, blade.currentEntities);
                    $scope.validateGridData();
                },
                canExecuteMethod: isDirty,
                permission: blade.updatePermission
            },
            {
                name: "platform.commands.add", icon: 'fas fa-plus',
                executeMethod: function () { addNewPrice(blade.currentEntities); },
                canExecuteMethod: function () { return true; },
                permission: blade.updatePermission
            },
            {
                name: "platform.commands.delete", icon: 'fas fa-trash-alt',
                executeMethod: function () {
                    var selection = $scope.gridApi.selection.getSelectedRows();
                    angular.forEach(selection, function (listItem) {
                        blade.currentEntities.splice(blade.currentEntities.indexOf(listItem), 1);
                    });
                },
                canExecuteMethod: function () {
                    return $scope.gridApi && _.any($scope.gridApi.selection.getSelectedRows());
                },
                permission: blade.updatePermission
            }
        ];

        function addNewPrice(targetList) {
            var newEntity = { productId: blade.itemId, list: '', minQuantity: 1, currency: blade.currency, pricelistId: blade.priceListId };
            targetList.push(newEntity);
            $scope.validateGridData();
        }

        $scope.isListPriceValid = priceValidatorsService.isListPriceValid;
        $scope.isSalePriceValid = priceValidatorsService.isSalePriceValid;
        $scope.isUniqueQty = priceValidatorsService.isUniqueQty;
        $scope.hasSingleItemPrice = priceValidatorsService.hasSingleItemPrice;

        // ui-grid
        $scope.setGridOptions = function (gridId, gridOptions) {
            gridOptions.onRegisterApi = function (gridApi) {
                $scope.gridApi = gridApi;

                gridApi.edit.on.afterCellEdit($scope, function () {
                    //to process validation for all rows in grid.
                    //e.g. if we have two rows with the same count of min qty, both of this rows will be marked as error.
                    //when we change data to valid in one row, another one should become valid too.
                    //more info about ui-grid validation: https://github.com/angular-ui/ui-grid/issues/4152
                    $scope.validateGridData();
                });

                $scope.validateGridData();
            };

            $scope.gridOptions = gridOptions;
            gridOptionExtension.tryExtendGridOptions(gridId, gridOptions);
            return gridOptions;
        };

        $scope.validateGridData = function () {
            if ($scope.gridApi) {
                angular.forEach(blade.currentEntities, function (rowEntity) {
                    angular.forEach($scope.gridOptions.columnDefs, function (colDef) {
                        $scope.gridApi.grid.validate.runValidators(rowEntity, colDef, rowEntity[colDef.name], undefined, $scope.gridApi.grid);
                    });
                });
            }
        };

        $scope.datepickers = {};
        $scope.open = function ($event, which) {
            $event.preventDefault();
            $event.stopPropagation();
            $scope.datepickers[which] = true;
        };

        // actions on load
        blade.refresh();
    }])

    .factory('virtoCommerce.pricingModule.priceValidatorsService', [function () {
        var allPrices = [];

        function hasSingleItemPriceForPricelist(pricelistId) {
            var result = _.some(allPrices, function (x) {
                return x.pricelistId === pricelistId && isSingleItemPrice(x);
            });
            console.log('hasSingleItemPriceForPricelist', allPrices.length, pricelistId, result);
            return result;
        }

        function isSingleItemPrice(price) {
            var result = Math.round(price.minQuantity) === 1 && !price.startDate && !price.EndDate;
            console.log('isSingleItemPrice', price.pricelistId, price.minQuantity, price.startDate, price.EndDate, result);
            return result;
        }

        return {
            setAllPrices: function (prices) {
                allPrices = prices;
            },
            isListPriceValid: function (price) {
                return price.list >= 0;
            },
            isSalePriceValid: function (price) {
                return _.isUndefined(price.sale) || price.list >= price.sale;
            },
            isUniqueQty: function (price) {
                // Disable unique quantity test when time filtering is used.
                if (price.startDate || price.endData) {
                    return true;
                }
                return Math.round(price.minQuantity) > 0 && _.all(allPrices, function (x) { return x === price || Math.round(x.minQuantity) !== Math.round(price.minQuantity) || x.startDate || x.EndDate; });
            },
            isUniqueQtyForPricelist: function (price) {
                // Disable unique quantity test when time filtering is used.
                return Math.round(price.minQuantity) > 0 && _.filter(allPrices, function (x) { return x.pricelistId === price.pricelistId && x.minQuantity === price.minQuantity && !x.startDate && !x.EndDate && !price.startDate && !price.endDate; }).length <= 1;
            },
            hasSingleItemPrice: function (price) {
                if (price) {
                    return isSingleItemPrice(price) ||
                        allPrices.length === 0 ||
                        hasSingleItemPriceForPricelist(price.pricelistId);
                }
                var pricelistIds = _.uniq(_.map(allPrices, function (x) { return x.pricelistId; }));
                return pricelistIds.length === 0 || _.all(pricelistIds, hasSingleItemPriceForPricelist);
            }
        };
    }])

    .run(
        ['platformWebApp.ui-grid.extension', 'virtoCommerce.pricingModule.priceValidatorsService', 'uiGridValidateService', function (gridOptionExtension, priceValidatorsService, uiGridValidateService) {

            uiGridValidateService.setValidator('listValidator', function (argument) {
                return function (oldValue, newValue, rowEntity, colDef) {
                    // We should not test for existence here
                    return _.isUndefined(newValue) || priceValidatorsService.isListPriceValid(rowEntity);
                };
            }, function (argument) { return 'List price is invalid '; });

            uiGridValidateService.setValidator('saleValidator', function (argument) {
                return function (oldValue, newValue, rowEntity, colDef) {
                    return priceValidatorsService.isSalePriceValid(rowEntity);
                };
            }, function (argument) { return 'Sale price should not exceed List price'; });

            uiGridValidateService.setValidator('minQuantityValidator', function () {
                return function (oldValue, newValue, rowEntity, colDef) {
                    return priceValidatorsService.isUniqueQty(rowEntity);
                };
            }, function () { return 'Quantity value should be unique'; });

            uiGridValidateService.setValidator('minQuantityForPricelistValidator', function () {
                return function (oldValue, newValue, rowEntity, colDef) {
                    return priceValidatorsService.isUniqueQtyForPricelist(rowEntity);
                };
            }, function () { return 'Quantity value should be unique'; });

            uiGridValidateService.setValidator('singleItemPriceValidator', function () {
                return function (oldValue, newValue, rowEntity, colDef) {
                    return priceValidatorsService.hasSingleItemPrice(rowEntity);
                };
            }, function () { return 'At least one price with quantity 1 is required'; });
        }]);
