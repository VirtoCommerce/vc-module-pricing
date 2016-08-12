angular.module('virtoCommerce.pricingModule')
    .factory('virtoCommerce.pricingModule.prices', ['$resource', function ($resource) {
        return $resource('api/products/:id/prices', { id: '@Id' }, {
            search: { url: 'api/catalog/products/prices/search' },
            getProductPrices: { isArray: true }, // is also used in other modules
            getProductPricelists: { url: 'api/catalog/products/:id/pricelists', isArray: true },
            update: { method: 'PUT' },
            remove: { method: 'DELETE', url: 'api/pricing/pricelists/:priceListId/products/prices' }
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