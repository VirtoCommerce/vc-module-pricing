angular.module('virtoCommerce.pricingModule')
    .controller('virtoCommerce.pricingModule.catalogSelectorController', ['$scope', 'virtoCommerce.catalogModule.catalogs', function ($scope, catalogs) {
        catalogs.search({ take: 1000 }, function (result) {
            $scope.catalogs = angular.copy(result.results);
        });
    }]);
