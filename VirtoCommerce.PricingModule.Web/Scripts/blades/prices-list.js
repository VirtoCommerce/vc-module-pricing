angular.module('virtoCommerce.pricingModule')
.controller('virtoCommerce.pricingModule.pricesListController', ['$scope', 'virtoCommerce.pricingModule.prices', 'platformWebApp.objCompareService', 'platformWebApp.bladeNavigationService', 'platformWebApp.uiGridHelper', 'virtoCommerce.pricingModule.priceValidatorsService', function ($scope, prices, objCompareService, bladeNavigationService, uiGridHelper, priceValidatorsService) {
    $scope.uiGridConstants = uiGridHelper.uiGridConstants;
    var blade = $scope.blade;
    blade.updatePermission = 'pricing:update';

    blade.refresh = function () {
        blade.data.productId = blade.itemId;
        if (!blade.data.prices) {
            blade.data.prices = [];
        }

        //if (!_.any(blade.data.prices)) {
        //    addNewPrice(blade.data.prices);
        //}

        blade.currentEntities = angular.copy(blade.data.prices);
        blade.origEntity = blade.data.prices;
        blade.isLoading = false;
        priceValidatorsService.setAllPrices(blade.currentEntities);
    };

    blade.onClose = function (closeCallback) {
        bladeNavigationService.showConfirmationIfNeeded(isDirty(), canSave(), blade, $scope.saveChanges, closeCallback, "pricing.dialogs.prices-save.title", "pricing.dialogs.prices-save.message");
    };

    function isDirty() {
        return blade.currentEntities && !objCompareService.equal(blade.origEntity, blade.currentEntities) && blade.hasUpdatePermission()
    }

    function canSave() {
        return isDirty() && $scope.isValid();
    }

    $scope.cancelChanges = function () {
        $scope.bladeClose();
    };

    $scope.isValid = function () {
        return $scope.formScope && $scope.formScope.$valid &&
             _.all(blade.currentEntities, $scope.isListPriceValid) &&
             _.all(blade.currentEntities, $scope.isSalePriceValid) &&
             _.all(blade.currentEntities, $scope.isUniqueQty) &&
            (blade.currentEntities.length == 0 || _.some(blade.currentEntities, function (x) { return x.minQuantity == 1; }));
    }

    $scope.saveChanges = function () {
        blade.isLoading = true;

        angular.copy(blade.currentEntities, blade.origEntity);
        if (_.any(blade.currentEntities)) {
            prices.update({ id: blade.itemId }, blade.data, function (data) {
                // blade.parentBlade.refresh();
                $scope.bladeClose();
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

    $scope.setForm = function (form) { $scope.formScope = form; }

    blade.toolbarCommands = [
        {
            name: "platform.commands.save", icon: 'fa fa-save',
            executeMethod: $scope.saveChanges,
            canExecuteMethod: canSave,
            permission: blade.updatePermission
        },
        {
            name: "platform.commands.reset", icon: 'fa fa-undo',
            executeMethod: function () {
                angular.copy(blade.origEntity, blade.currentEntities);
            },
            canExecuteMethod: isDirty,
            permission: blade.updatePermission
        },
        {
            name: "platform.commands.add", icon: 'fa fa-plus',
            executeMethod: function () { addNewPrice(blade.currentEntities); },
            canExecuteMethod: function () { return true; },
            permission: blade.updatePermission
        },
        {
            name: "platform.commands.delete", icon: 'fa fa-trash-o',
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
        var newEntity = { productId: blade.itemId, list: 0, minQuantity: 1, currency: blade.currency, priceListId: blade.priceListId };
        targetList.push(newEntity);
    }

    $scope.isListPriceValid = priceValidatorsService.isListPriceValid;
    $scope.isSalePriceValid = priceValidatorsService.isSalePriceValid;
    $scope.isUniqueQty = priceValidatorsService.isUniqueQty;

    // actions on load
    blade.refresh();
}])

.factory('virtoCommerce.pricingModule.priceValidatorsService', [function () {
    var allPrices = {};
    return {
        setAllPrices: function (data) {
            allPrices = data;
        },
        isListPriceValid: function (data) {
            return data.list > 0;
        },
        isSalePriceValid: function (data) {
            return _.isUndefined(data.sale) || data.list >= data.sale;
        },
        isUniqueQty: function (data) {
            return Math.round(data.minQuantity) > 0 && _.all(allPrices, function (x) { return x === data || Math.round(x.minQuantity) !== Math.round(data.minQuantity) });
        }
    };
}])

.run(
  ['platformWebApp.gridOptionsService', 'virtoCommerce.pricingModule.priceValidatorsService', 'uiGridValidateService', function (gridOptionsService, priceValidatorsService, uiGridValidateService) {
      // ui-grid extensibility demo using gridOptionsService:
      //gridOptionsService.registerOptions('itemPrices',
      //  { enableGridMenu: false },
      //  ['currency', 'modifiedDate'],
      //  [
      //    { name: 'currency', editableCellTemplate: 'default-cellTextEditor', validators: { required: true }, cellTemplate: 'ui-grid/cellTitleValidator', enableCellEdit: true },
      //    { name: 'effectiveValue', displayName: 'test value', editableCellTemplate: 'default-cellTextEditor', validators: { required: true }, cellTemplate: 'priceCellTitleValidator', enableCellEdit: true }
      //  ]);

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
  }]);
