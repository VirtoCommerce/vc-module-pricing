angular.module('virtoCommerce.pricingModule')
    .controller('virtoCommerce.pricingModule.pricelistItemListController', ['$scope', 'virtoCommerce.pricingModule.prices', '$filter', 'platformWebApp.bladeNavigationService', 'uiGridConstants', 'platformWebApp.uiGridHelper', 'platformWebApp.bladeUtils', 'platformWebApp.dialogService', '$translate', 'platformWebApp.settings', function ($scope, prices, $filter, bladeNavigationService, uiGridConstants, uiGridHelper, bladeUtils, dialogService, $translate, settings) {
        $scope.uiGridConstants = uiGridConstants;
        $scope.noProductRowName = $translate.instant('pricing.blades.pricelist-item-list.labels.no-product-row-name');
        var blade = $scope.blade;
        var exportDataRequest = {
            exportTypeName: 'VirtoCommerce.PricingModule.Data.ExportImport.ExportablePrice',
            dataQuery: {
                exportTypeName: 'PriceExportDataQuery'
            }
        };
        blade.csvExportProvider = 'CsvExportProvider';
        blade.csvExportDelimiter = ';';
        blade.csvPropertyInfos = [
            {
                fullName: "Code",
                group: "TabularPrice",
                displayName: "SKU",
                isRequired: false,
            },
            {
                fullName: "ProductName",
                group: "TabularPrice",
                displayName: "Product Name",
                isRequired: false,
            },
            {
                fullName: "Currency",
                group: "TabularPrice",
                displayName: "Currency",
                isRequired: false,
            },
            {
                fullName: "List",
                group: "TabularPrice",
                displayName: "List price",
                isRequired: false,
            },
            {
                fullName: "Sale",
                group: "TabularPrice",
                displayName: "Sales price",
                isRequired: false,
            },
            {
                fullName: "MinQuantity",
                group: "TabularPrice",
                displayName: "Min quantity",
                isRequired: false,
            },
            {
                fullName: "ModifiedDate",
                group: "TabularPrice",
                displayName: "Modified",
                isRequired: false,
            },
            {
                fullName: "StartDate",
                group: "TabularPrice",
                displayName: "Valid from",
                isRequired: false,
            },
            {
                fullName: "EndDate",
                group: "TabularPrice",
                displayName: "Valid to",
                isRequired: false,
            },
            {
                fullName: "CreatedDate",
                group: "TabularPrice",
                displayName: "Created date",
                isRequired: false,
            },
            {
                fullName: "CreatedBy",
                group: "TabularPrice",
                displayName: "Created by",
                isRequired: false,
            },
            {
                fullName: "ModifiedBy",
                group: "TabularPrice",
                displayName: "Modified By",
                isRequired: false,
            }
        ];

        blade.refresh = function () {
            blade.isLoading = true;

            settings.getValues({ id: 'Pricing.SimpleExport.LimitOfLines' }, (value) => {
                $scope.exportLimit = value[0];
            });

            prices.search(getSearchCriteria(), function (data) {
                blade.currentEntities = $scope.preparePrices(data.results);
                $scope.pageSettings.totalItems = data.totalCount;

                blade.isLoading = false;
            }, function (error) { bladeNavigationService.setError('Error ' + error.status, blade); });
        };

        $scope.preparePrices = function(data) {
            _.each(data, (item) => {
                if(!item.product) {
                    item.product = { name :  $scope.noProductRowName};
                }
            });
            return data;
        }

        $scope.selectNode = function (node) {
            $scope.selectedNodeId = node.productId;

            var newBlade = {
                id: 'productItemPrices',
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
            $scope.selectedNodeId = null;
            var selectedProducts = [];
            var newBlade = {
                id: "CatalogItemsSelect",
                title: "Select items for pricing", //catalogItemSelectController: hard-coded title
                controller: 'virtoCommerce.catalogModule.catalogItemSelectController',
                template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/common/catalog-items-select.tpl.html',
                breadcrumbs: [],
                toolbarCommands: [
                    {
                        name: "pricing.commands.add-selected", icon: 'fas fa-plus',
                        executeMethod: function (blade) {
                            addProductsToPricelist(selectedProducts, blade);
                        },
                        canExecuteMethod: function () {
                            return selectedProducts.length > 0;
                        }
                    }]
            };

            newBlade.options = {
                checkItemFn: function (listItem, isSelected) {
                    if (listItem.type === 'category') {
                        newBlade.error = 'Categories are not supported';
                        listItem.selected = undefined;
                    } else {
                        if (isSelected) {
                            if (_.all(selectedProducts, function (x) { return x.id !== listItem.id; })) {
                                selectedProducts.push(listItem);
                            }
                        }
                        else {
                            selectedProducts = _.reject(selectedProducts, function (x) { return x.id === listItem.id; });
                        }
                        newBlade.error = undefined;
                    }
                }
            };

            bladeNavigationService.showBlade(newBlade, blade);
        }

        function addProductsToPricelist(products, theBlade) {
            theBlade.isLoading = true;

            // search for possible duplicating prices
            prices.search({
                priceListId: blade.currentEntityId,
                productIds: _.pluck(products, 'id')
            }, function (data) {
                var newItems = _.filter(products, function (product) {
                    return _.all(data.results, function (x) {
                        return x.productId !== product.id;
                    })
                });

                var newProductPrices = _.map(newItems, function (x) {
                    return {
                        // productId: x.id,
                        prices: [{ productId: x.id, list: 0, minQuantity: 1, currency: blade.currency, priceListId: blade.currentEntityId }]
                    };
                });

                prices.update(newProductPrices, function () {
                    bladeNavigationService.closeBlade(theBlade);
                    blade.refresh();
                    blade.parentWidgetRefresh();
                }, function (error) {
                    bladeNavigationService.setError('Error ' + error.status, blade);
                });
            }, function (error) { bladeNavigationService.setError('Error ' + error.status, blade); });
        }

        $scope.deleteList = function (list) {
            var dialog = {
                id: "confirmDeleteItem",
                title: "pricing.dialogs.pricelist-item-list-delete.title",
                message: "pricing.dialogs.pricelist-item-list-delete.message",
                callback: function (remove) {
                    if (remove) {
                        bladeNavigationService.closeChildrenBlades(blade, function () {
                            prices.remove({ priceListId: blade.currentEntityId, productIds: _.pluck(list, 'productId') }, function () {
                                blade.refresh();
                                blade.parentWidgetRefresh();
                            },
                                function (error) { bladeNavigationService.setError('Error ' + error.status, blade); });
                        });
                    }
                }
            }
            dialogService.showConfirmationDialog(dialog);
        }

        blade.toolbarCommands = [
            {
                name: "platform.commands.refresh", icon: 'fa fa-refresh',
                executeMethod: blade.refresh,
                canExecuteMethod: function () { return true; }
            },
            {
                name: "platform.commands.add", icon: 'fas fa-plus',
                executeMethod: openAddEntityWizard,
                canExecuteMethod: function () { return true; },
                permission: blade.updatePermission
            },
            {
                name: "platform.commands.delete", icon: 'fas fa-trash-alt',
                executeMethod: function () {
                    $scope.deleteList($scope.gridApi.selection.getSelectedRows());
                },
                canExecuteMethod: function () {
                    return $scope.gridApi && _.any($scope.gridApi.selection.getSelectedRows());
                },
                permission: blade.updatePermission
            },
            {
                name: "platform.commands.export",
                icon: 'fa fa-upload',
                canExecuteMethod: function () {
                    return true;
                },
                executeMethod: function () {
                    $scope.selectedRows = $scope.gridApi.selection.getSelectedRows();
                    $scope.isAllSelected = $scope.gridApi.selection.getSelectAllState() || !$scope.selectedRows.length;
                    exportDataRequest.dataQuery.isAllSelected = $scope.isAllSelected;
                    exportDataRequest.dataQuery.objectIds = [];
                    if (!$scope.isAllSelected && $scope.selectedRows) {
                        var priceIds = _.pluck(_.flatten(_.pluck($scope.selectedRows, 'prices')), 'id');
                        exportDataRequest.dataQuery.objectIds = priceIds;
                    }

                    exportDataRequest.dataQuery.productIds = [];

                    if ((exportDataRequest.dataQuery.productIds && exportDataRequest.dataQuery.productIds.length)
                        || (!$scope.isAllSelected)) {
                        exportDataRequest.dataQuery.productIds = _.map($scope.selectedRows, function (product) {
                            return product.productId;
                        });
                    }

                    var searchCriteria = getSearchCriteria();

                    if ($scope.isAllSelected || (searchCriteria.pricelistIds && searchCriteria.pricelistIds.length > 0) || searchCriteria.keyword !== '') {
                        exportDataRequest.dataQuery.isAnyFilterApplied = true;
                    }

                    angular.extend(exportDataRequest.dataQuery, searchCriteria);
                    showExportDialog();
                }
            }
        ];

        function getSearchCriteria() {
            var result = {
                pricelistIds: [blade.currentEntityId],
                keyword: filter.keyword,
                sort: uiGridHelper.getSortExpression($scope),
                skip: ($scope.pageSettings.currentPage - 1) * $scope.pageSettings.itemsPerPageCount,
                take: $scope.pageSettings.itemsPerPageCount
            }
            return result;
        }

        function showExportDialog() {
            const selectedItemsCount = $scope.isAllSelected ? $scope.pageSettings.totalItems : $scope.selectedRows.length;
            const validationError = selectedItemsCount > $scope.exportLimit;
            var dialog = {
                id: "priceExportDialog",
                exportAll: $scope.isAllSelected ? true : false,
                totalItemsCount: $scope.pageSettings.totalItems,
                selectedItemsCount,
                exportLimit: $scope.exportLimit,
                validationError,
                advancedExport: function () {
                    if (exportDataRequest.providerConfig) {
                        delete exportDataRequest.providerConfig;
                    }

                    this.no();
                    var newBlade = {
                        id: 'priceExport',
                        title: 'pricing.blades.exporter.priceTitle',
                        subtitle: 'pricing.blades.exporter.priceSubtitle',
                        controller: 'virtoCommerce.exportModule.exportSettingsController',
                        template: 'Modules/$(VirtoCommerce.Export)/Scripts/blades/export-settings.tpl.html',
                        exportDataRequest: exportDataRequest
                    };
                    bladeNavigationService.showBlade(newBlade, blade);
                },
                callback: function (confirm) {
                    if (confirm) {
                        exportDataRequest.providerConfig = {};
                        exportDataRequest.providerConfig.delimiter = blade.csvExportDelimiter;
                        exportDataRequest.providerName = blade.csvExportProvider;
                        exportDataRequest.dataQuery.includedProperties = blade.csvPropertyInfos;
                        delete exportDataRequest.dataQuery.skip;
                        delete exportDataRequest.dataQuery.take;
                        delete exportDataRequest.dataQuery.IsPreview;
                        blade.isExporting = true;
                        var progressBlade = {
                            id: 'exportProgress',
                            title: 'export.blades.export-progress.title',
                            controller: 'virtoCommerce.exportModule.exportProgressController',
                            template: 'Modules/$(VirtoCommerce.Export)/Scripts/blades/export-progress.tpl.html',
                            exportDataRequest: exportDataRequest,
                            onCompleted: function () {
                                blade.isExporting = false;
                            }
                        };
                        bladeNavigationService.showBlade(progressBlade, blade);
                    }
                }
            }
            dialogService.showDialog(dialog, 'Modules/$(VirtoCommerce.Pricing)/Scripts/dialogs/priceExport-dialog.tpl.html', 'platformWebApp.confirmDialogController');
        }

        $scope.getPriceRange = function (priceGroup) {
            var retVal;
            var allPrices = _.union(_.pluck(priceGroup.prices, 'list'), _.pluck(priceGroup.prices, 'sale'));
            var minprice = $filter('currency')(_.min(allPrices), '', 2);
            var maxprice = $filter('currency')(_.max(allPrices), '', 2);
            retVal = (minprice === maxprice ? minprice : minprice + '-' + maxprice);

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
            $scope.gridOptions = gridOptions;

            gridOptions.onRegisterApi = function (gridApi) {
                gridApi.core.on.sortChanged($scope, function () {
                    if (!blade.isLoading) blade.refresh();
                });
            };

            bladeUtils.initializePagination($scope);
        };

        //No need to call this because page 'pageSettings.currentPage' is watched!!! It would trigger subsequent duplicated req...
        //blade.refresh();
    }]);
