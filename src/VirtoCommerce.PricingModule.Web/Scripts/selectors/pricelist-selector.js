angular.module('virtoCommerce.pricingModule')
.controller('virtoCommerce.pricingModule.pricelistSelectorController', ['$scope', 'virtoCommerce.pricingModule.pricelists', function ($scope, pricelists) {
    $scope.blade.fetchPricelists = function (criteria) {
        return pricelists.search(criteria);
    };
}]);
