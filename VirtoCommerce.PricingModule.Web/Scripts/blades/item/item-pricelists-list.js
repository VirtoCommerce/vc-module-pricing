angular.module('virtoCommerce.pricingModule')
.controller('virtoCommerce.pricingModule.itemPricelistsListController', ['$scope', 'platformWebApp.bladeNavigationService', 'uiGridConstants', 'platformWebApp.uiGridHelper', 'virtoCommerce.pricingModule.prices', function ($scope, bladeNavigationService, uiGridConstants, uiGridHelper, prices) {
    $scope.uiGridConstants = uiGridConstants;
    var blade = $scope.blade;

    blade.refresh = function () {
    	blade.isLoading = true;
    	prices.getProductPricelists({ id: blade.itemId }, function (pricelists) {
    		blade.isLoading = false;
    		blade.currentEntities = [];
    		_.each(pricelists, function (x) {
    			var assignments = _.filter(x.assignments, function (assignment) {
    				return assignment.catalogId == blade.item.catalogId;
    			});
				//Make pricelist for each assignment assigned to product catalog
    			_.each(assignments, function (assignment) {
    				var pricelist = {
    					priority: assignment.priority
    				};
    				angular.extend(pricelist, x);
    				pricelist.assignments = [assignment];
    				blade.currentEntities.push(pricelist);
    			});
    		});

    		if (!pricelists.length) {
    			var newPricelistBlade = {
    				id: 'newPricelist',
    				controller: 'virtoCommerce.pricingModule.pricelistDetailController',
    				template: 'Modules/$(VirtoCommerce.Pricing)/Scripts/blades/pricelist-detail.tpl.html',
    				title: 'pricing.blades.pricelist-detail.title-new',
    				isNew: true,
    				saveCallback: function (pricelist) {
    					blade.refresh();
    				}
    			};
    			bladeNavigationService.showBlade(newPricelistBlade, blade);
    		}

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
    ];

    // ui-grid
    $scope.setGridOptions = function (gridOptions) {
        uiGridHelper.initialize($scope, gridOptions);
    };

    blade.refresh();
}]);
