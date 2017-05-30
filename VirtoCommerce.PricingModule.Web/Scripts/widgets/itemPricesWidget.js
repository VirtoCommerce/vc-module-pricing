angular.module('virtoCommerce.pricingModule')
.controller('virtoCommerce.pricingModule.itemPricesWidgetController', ['$scope', '$filter', 'platformWebApp.bladeNavigationService', 'virtoCommerce.pricingModule.pricelists', 'virtoCommerce.pricingModule.prices', '$state', function ($scope, $filter, bladeNavigationService, pricelists, prices, $state) {
	var blade = $scope.blade;

	function refresh() {
		$scope.loading = true;
		return prices.getProductPrices({ id: blade.itemId }, function (productPrices) {
			$scope.loading = false;
			if (productPrices.length) {
				productPrices = _.groupBy(productPrices, 'currency');
				productPrices = _.max(_.values(productPrices), function (x) { return x.length; });
				var allPrices = _.union(_.pluck(productPrices, 'list'), _.pluck(productPrices, 'sale'));
				var minprice = _.min(allPrices);
				var maxprice = _.max(allPrices);
				var currency = _.any(productPrices) ? productPrices[0].currency : '';
				minprice = $filter('currency')(minprice, currency, 2);
				maxprice = $filter('currency')(maxprice, currency, 2);
				$scope.priceLabel = (minprice == maxprice ? minprice : minprice + ' - ' + maxprice);
			}
			return productPrices;
		});
	}

	$scope.openBlade = function () {
		if ($scope.loading)
			return;

		var productPricelistsBlade = {
			id: "itemPricelists",
			itemId: blade.itemId,
			item: blade.item,
			parentWidgetRefresh: refresh,
			title: blade.title,
			subtitle: 'pricing.blades.item-pricelists-list.subtitle',
			controller: 'virtoCommerce.pricingModule.itemPricelistsListController',
			template: 'Modules/$(VirtoCommerce.Pricing)/Scripts/blades/item/item-pricelists-list.tpl.html'
		};	
		bladeNavigationService.showBlade(productPricelistsBlade, blade);
		

	};

	refresh();
}]);