angular.module('virtoCommerce.pricingModule')
    .factory('virtoCommerce.pricingModule.prices', ['$resource', function ($resource) {
        return $resource('api/products/:id/prices', { id: '@Id' }, {
            getProductPrices: { isArray: true }, // is also used in other modules
            update: { method: 'PUT' },
            updateAll: { url: 'api/products/prices', method: 'PUT' }
        });
    }])
    .factory('virtoCommerce.pricingModule.productPrices', ['$resource', function ($resource) {
        return $resource('api/catalog/products/:id/prices', {}, {
            search: { url: 'api/catalog/products/prices/search' },
        });
    }])
    .factory('virtoCommerce.pricingModule.pricelists', ['$resource', function ($resource) {
        return $resource('api/pricing/pricelists/:id', {}, {
            update: { method: 'PUT' }
        });
    }])
    .factory('virtoCommerce.pricingModule.pricelistAssignments', ['$resource', function ($resource) {
        return $resource('api/pricing/assignments/:id', { id: '@Id' }, {
            getNew: { url: 'api/pricing/assignments/new' },
            update: { method: 'PUT' }
        });
    }]);