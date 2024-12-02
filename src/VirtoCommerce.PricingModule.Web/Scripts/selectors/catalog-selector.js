angular.module('virtoCommerce.pricingModule')
    .controller('virtoCommerce.pricingModule.catalogSelectorController', ['$scope', 'virtoCommerce.catalogModule.catalogs', function ($scope, catalogs) {
        $scope.blade.fetchCatalogs = function (criteria) {
            return catalogs.search(criteria);
        }
    }]);
