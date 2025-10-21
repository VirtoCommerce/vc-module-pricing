# Pricing Extension Sample Module

This example demonstrates the use of the Virto Commerce Extensibility Framework to extend the standard Pricing Module. It shows how to extend database contracts by adding a new property to the `Price` entity.

**Note:** This sample is intended for demo purposes, training sessions, etc. We recommend following a 3-tier architecture (Core, Data, Web, or XApi) for custom modules, as well as for Virto Commerce.

**Note:** The sample does not support the `vc-build` command.

## Extension Scenarios

1.  **Extending the Price Entity with RecommendedPrice:**
    This scenario demonstrates how to extend the database contract for the `Price` entity by adding a new `RecommendedPrice` property. This allows storing and managing an additional recommended price alongside standard prices (List and Sale) for products.

    *   **Model Extension:** The `VirtoCommerce.PricingModule2.Web.Models.Price2` class inherits from the base `VirtoCommerce.PricingModule.Core.Model.Price` class and adds the `RecommendedPrice` property (type `decimal?`).
    *   **Database Entity Extension:** The `VirtoCommerce.PricingModule2.Web.Entities.Price2Entity` class inherits from `VirtoCommerce.PricingModule.Data.Model.PriceEntity`. It includes the `RecommendedPrice` property and overrides the `ToModel`, `FromModel`, and `Patch` methods for correct mapping and persistence of this new property.
    *   **Database Context Integration:** The `VirtoCommerce.PricingModule2.Web.Repositories.Pricing2DbContext` class extends the standard `PricingDbContext` and registers `Price2Entity` in the Entity Framework Core model builder, ensuring proper mapping of the new entity and its property to the database. In the `Pricing2DbContextFactory` file, use your connection string.
    *   **Database Migration:** A database migration (`20251020164959_AddRecommendedPrice`) was created, which adds the `RecommendedPrice` column (of type `Money`) to the `Price` table in the database. A system `Discriminator` column is also added, indicating the extended type - `Price2Entity`.

    **How `RecommendedPrice` works:**
    *   **Storage:** `RecommendedPrice` is stored as a nullable `decimal` in the database. If a value is present in the database, it will be retrieved and available via the `RecommendedPrice` property in the `Price2` models and `Price2Entity` entities.
    *   **Calculation:** In this example, if `RecommendedPrice` is not set in the database (i.e., it is `null`), it is automatically calculated in the `FillRecommendedPrice` method of the `PricingEvaluatorService2` service (`vc-module-pricing/samples/VirtoCommerce.PricingModule2.Web/Services/PricingEvaluatorService2.cs`).
        *   The calculation is based on the existing price (`Sale` or `List`) and the global `RecommendedPricePercent` coefficient.
        *   If a `Sale` price is available, `RecommendedPrice` is calculated as `Sale * RecommendedPricePercent`.
        *   Otherwise, `RecommendedPrice` is calculated as `List * RecommendedPricePercent`.
    *   **Global Coefficient `RecommendedPricePercent`:** This coefficient is defined as a global variable in `VirtoCommerce.PricingModule2.Web.ModuleConstants.Settings.General.RecommendedPricePercent` (`vc-module-pricing/samples/VirtoCommerce.PricingModule2.Web/ModuleConstants.cs`). Its default value is `1.2m` (120%), but it can be changed via platform settings.

## Getting Started

### Prerequisites

1.  Download and run the latest Virto Commerce Edge Release with ECommerce Bundle.

### Installation

1.  Download and open the `VirtoCommerce.PricingModule` solution in Visual Studio 2022.
2.  Compile the `VirtoCommerce.PricingModule2.Web` project.
3.  Build the Admin UI code by running the following commands in the `VirtoCommerce.PricingModule2.Web` folder:

    ```cmd
    npm ci
    ```

    ```cmd
    npm run webpack:dev
    ```

4.  Rename `_module.manifest` to `module.manifest`.
5.  Install the module by creating a symbolic link in the Virto Commerce Modules folder using the following command (update the path `c:\vc-platform-3-demo\platform\modules` to your actual platform modules directory path):

    ```cmd
    mklink /D "c:\vc-platform-3-demo\platform\modules\_Pricing.Ext" "E:\GitHub\virtoworks\VirtoCommerce\vc-module-pricing\samples\VirtoCommerce.PricingModule2.Web"
    ```

6.  Run Virto Commerce Platform and enjoy the Virto Commerce Extensibility Framework with the extended pricing module.

