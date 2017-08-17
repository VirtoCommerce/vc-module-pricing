angular.module('virtoCommerce.pricingModule')
.controller('virtoCommerce.pricingModule.itemPricelistsListController', ['$scope', 'platformWebApp.bladeNavigationService', 'uiGridConstants', 'virtoCommerce.pricingModule.prices', 'virtoCommerce.catalogModule.catalogs', function ($scope, bladeNavigationService, uiGridConstants, prices, catalogs) {
    $scope.uiGridConstants = uiGridConstants;
    var blade = $scope.blade;

    blade.refresh = function () {
    	blade.isLoading = true;
    	prices.getProductPricelists({ id: blade.itemId }, function (pricelists) {
    	    //Loading catalogs for pricelists because they do not contains them
    	    //Need to display name of catalog in product item pricelists grid
    	    catalogs.getCatalogs(function (catalogsList) {
    	        blade.isLoading = false;
    	        blade.currentEntities = [];
    	        _.each(pricelists, function (x) {
    	            if (x.prices.length > 0) {
    	                //Make pricelist for each assignment assigned to product catalog
    	                _.each(x.assignments, function (assignment) {
    	                    var pricelist = {
    	                        priority: assignment.priority
    	                    };
    	                    angular.extend(pricelist, x);
    	                    pricelist.assignments = [assignment];
    	                    pricelist.catalog = _.findWhere(catalogsList, { id: assignment.catalogId }).name;
    	                    blade.currentEntities.push(pricelist);
    	                });
    	            }
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
        $scope.gridOptions = gridOptions;
    };

    blade.refresh();
}]);
