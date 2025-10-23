var moduleName = "virtoCommerce.pricingModule2";

if (AppDependencies !== undefined) {
    AppDependencies.push(moduleName);
}

angular.module(moduleName, [])
    .run(
        ['platformWebApp.dynamicTemplateService', 'platformWebApp.settings', 'platformWebApp.bladeNavigationService', 'platformWebApp.ui-grid.extension',
            function (dynamicTemplateService, settings, bladeNavigationService, gridOptionExtension) {

                dynamicTemplateService.ensureTemplateLoaded('Modules/$(VirtoCommerce.PricingModule2)/Scripts/item-prices2.tpl.html');

                // Register extension to add custom column permanently (data-independent) into the list
                gridOptionExtension.registerExtension("pricelist-grid", function (gridOptions) {
                    var customColumnDefs = [
                        {
                            name: 'recommendedPrice',
                            displayName: 'pricing.blades.prices-list.labels.recommended-price',
                            editableCellTemplate: 'recommended-price-cell-editor',
                            cellTemplate: 'recommended-price-cell-template',
                            enableCellEdit: true
                        }
                    ];

                    gridOptions.columnDefs = _.union(gridOptions.columnDefs, customColumnDefs);
                });
            }]);
