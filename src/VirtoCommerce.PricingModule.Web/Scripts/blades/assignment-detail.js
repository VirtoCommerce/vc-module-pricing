angular.module('virtoCommerce.pricingModule')
    .controller('virtoCommerce.pricingModule.assignmentDetailController', ['$scope', 'virtoCommerce.catalogModule.catalogs', 'virtoCommerce.pricingModule.pricelists', 'virtoCommerce.pricingModule.pricelistAssignments', 'virtoCommerce.storeModule.stores', 'platformWebApp.bladeNavigationService', 'virtoCommerce.coreModule.common.dynamicExpressionService', 'platformWebApp.dialogService', function ($scope, catalogs, pricelists, assignments, stores, bladeNavigationService, dynamicExpressionService, dialogService) {
    var blade = $scope.blade;
    blade.updatePermission = 'pricing:update';

    $scope.pageSize = 20;

    blade.refresh = function (parentRefresh) {
        if (blade.isNew) {
            assignments.getNew(initializeBlade);
        } else {
            assignments.get({ id: blade.currentEntityId }, initializeBlade);
            if (parentRefresh && angular.isFunction(blade.parentBlade.refresh)) {
                blade.parentBlade.refresh();
            }
        }
    };

    function initializeBlade(data) {
        if (blade.isNew) {
        	data = angular.extend(blade.data, data);
        	blade.data.pricelistId = blade.pricelistId;
        }

        if (data.dynamicExpression) {
            _.each(data.dynamicExpression.children, extendElementBlock);
            groupAvailableChildren(data.dynamicExpression.children[0]);
        }

        blade.currentEntity = angular.copy(data);
        blade.origEntity = data;
        blade.isLoading = false;

        if (!blade.isNew) {
            blade.toolbarCommands = [
                {
                    name: "platform.commands.save",
                    icon: 'fas fa-save',
                    executeMethod: $scope.saveChanges,
                    canExecuteMethod: canSave,
                    permission: blade.updatePermission
                },
                {
                    name: "platform.commands.reset",
                    icon: 'fa fa-undo',
                    executeMethod: function () {
                        angular.copy(blade.origEntity, blade.currentEntity);
                    },
                    canExecuteMethod: isDirty,
                    permission: blade.updatePermission
                }
            ];
        }
    }

    function isDirty() {
        return !angular.equals(blade.currentEntity, blade.origEntity) && blade.hasUpdatePermission();
    }

    function canSave() {
        return isDirty() && $scope.formScope && $scope.formScope.$valid && validateAssignment();
    }

    function validateAssignment() {
        return (!blade.currentEntity.storeId || !blade.currentEntity.catalogId) &&
            (blade.currentEntity.storeId || blade.currentEntity.catalogId)
    }

    $scope.setForm = function (form) { $scope.formScope = form; };

    $scope.cancelChanges = function () {
        $scope.bladeClose();
    };

    $scope.saveChanges = function () {
        // can't set dynamic "required" attribute in ui-scroll-drop-down template
        // so check if both "storeId" and "catalogId" are not null and only one of them is filled
        if (!validateAssignment()) {
            var dialog = {
                id: "assignmentSaveValidationError",
                title: "pricing.dialogs.assignment-save-validation.title",
                message: "pricing.dialogs.assignment-save-validation.message",
            }
            dialogService.showErrorDialog(dialog);
            return;
        }

        blade.isLoading = true;
        if (blade.currentEntity.dynamicExpression) {
            blade.currentEntity.dynamicExpression.availableChildren = undefined;
            _.each(blade.currentEntity.dynamicExpression.children, stripOffUiInformation);
        }

        if (blade.isNew) {
            assignments.save(blade.currentEntity, function (data) {
                blade.isNew = undefined;
                blade.currentEntityId = data.id;
                blade.refresh(true);
            }, function (error) {
                bladeNavigationService.setError('Error ' + error.status, blade);
            });
        } else {
            assignments.update(blade.currentEntity, function (data) {
                blade.refresh(true);
            }, function (error) {
                bladeNavigationService.setError('Error ' + error.status, blade);
            });
        }
    };

    blade.onClose = function (closeCallback) {
        bladeNavigationService.showConfirmationIfNeeded(isDirty(), canSave(), blade, $scope.saveChanges, closeCallback, "pricing.dialogs.assignment-save.title", "pricing.dialogs.assignment-save.message");
    };

    // datepicker
    $scope.datepickers = {
        str: false,
        end: false
    }

    $scope.open = function ($event, which) {
        $event.preventDefault();
        $event.stopPropagation();

        $scope.datepickers[which] = true;
    };

    // Dynamic ExpressionBlock
    function extendElementBlock(expressionBlock) {
        var retVal = dynamicExpressionService.expressions[expressionBlock.id];
        if (!retVal) {
            retVal = { displayName: 'unknown element: ' + expressionBlock.id };
        }

        _.extend(expressionBlock, retVal);

        if (!expressionBlock.children) {
            expressionBlock.children = [];
        }

        _.each(expressionBlock.children, extendElementBlock);
        _.each(expressionBlock.availableChildren, extendElementBlock);
        return expressionBlock;
    }

    function groupAvailableChildren(expressionBlock) {
        results = _.groupBy(expressionBlock.availableChildren, 'groupName');
        expressionBlock.availableChildren = _.map(results, function (items, key) { return { displayName: key, subitems: items }; });
    }

    function stripOffUiInformation(expressionElement) {
        expressionElement.availableChildren = undefined;
        expressionElement.displayName = undefined;
        expressionElement.getValidationError = undefined;
        expressionElement.groupName = undefined;
        expressionElement.newChildLabel = undefined;
        expressionElement.templateURL = undefined;

        _.each(expressionElement.children, stripOffUiInformation);
    };

    $scope.catalogDataSource = (criteria) => catalogs.search(criteria);
    $scope.pricelistDataSource = (criteria) => pricelists.search(criteria);
    $scope.storeDataSource = (criteria) => stores.search(criteria);

    blade.refresh();
}]);
