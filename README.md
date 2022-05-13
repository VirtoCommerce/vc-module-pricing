# Overview

[![CI status](https://github.com/VirtoCommerce/vc-module-pricing/workflows/Module%20CI/badge.svg?branch=dev)](https://github.com/VirtoCommerce/vc-module-pricing/actions?query=workflow%3A"Module+CI") [![Quality gate](https://sonarcloud.io/api/project_badges/measure?project=VirtoCommerce_vc-module-pricing&metric=alert_status&branch=dev)](https://sonarcloud.io/dashboard?id=VirtoCommerce_vc-module-pricing) [![Reliability rating](https://sonarcloud.io/api/project_badges/measure?project=VirtoCommerce_vc-module-pricing&metric=reliability_rating&branch=dev)](https://sonarcloud.io/dashboard?id=VirtoCommerce_vc-module-pricing) [![Security rating](https://sonarcloud.io/api/project_badges/measure?project=VirtoCommerce_vc-module-pricing&metric=security_rating&branch=dev)](https://sonarcloud.io/dashboard?id=VirtoCommerce_vc-module-pricing) [![Sqale rating](https://sonarcloud.io/api/project_badges/measure?project=VirtoCommerce_vc-module-pricing&metric=sqale_rating&branch=dev)](https://sonarcloud.io/dashboard?id=VirtoCommerce_vc-module-pricing)

The **Pricing Module** is designed for storing, managing and evaluating product prices.

The Pricing Module is consisted of three main objects:

1. Price
1. Price list
1. Price list Assignment

### Price

The purpose of this object is to store the item price in the system and support a static discount and a tier price. The price has the following key characteristics:

- **Product** (required) - is an object that the current price relates to.
- **List price** (required) – represents the base price for the product in currency of owned price. The List price is a pre-promotional regular price.  
- **Sale price** (optional) – is the discounted price of an item from the regular sale price.
- **Min quantity** (min. 1) – represents lower inclusive limit for products quantity starting from which, this price will be valid. The min QTY is used for achieving Tier pricing functionality.

### Pricelist

The Pricelist plays the role of a container for prices with single currency. The Pricelist has the following key characteristics:

- **Currency** (required) – single currency for all prices included into the Price list.  
- **Prices** – the list of prices included into the Price list.  

### Pricelist Assignment

The Pricelist Assignment allows associating the specific Pricelist with a catalog based on specific rules and conditions.  

The Pricelist Assignment has the following key characteristics:

- **Pricelist** (required) - relation to selected pricelist.  
- **Catalog** (required) - relation to selected catalog.
- **Enable** (start) and **Expiration dates** – the specified Enabled and Expiration dates determine the validation period of the pricelist assignment.  
- **Priority**  defines the priority of the Pricelist. The system will apply the pricelist that has the highest priority.  
- **Dynamic Assignment** of applicability.

## Key Features

1. The system supports multiple **Pricelists**, i.e. the same product item may have different price in different price lists.
1. Each **Catalog** can have its own list of **Pricelists**.
1. Every **Pricelists** should have a unique Name-ID, and single currency.
1. Each **Pricelists** contains a list of product prices with single currency.
1. The system supports **Tier Pricing** and it is a way to encourage shoppers to buy larger quantities of a product by applying discounts based on the quantity ordered.
1. The system supports static discounts on products and allows the user to specify the discount on each item on the list.
1. In order to make the **Pricelists** active, it should be assigned to a specific **Catalog** in the system. The system allows the user to specify specific rules and conditions that can also be assigned to the **Pricelists**. These specific rules and conditions will influence the price calculation and define to which user categories or Organizations the pricelist will be displayed. This is a so-called **Dynamic Assignment**.
1. The system allows expanding the **Dynamic Assignment** tree and it completely depends on the organization business needs. Any condition or rule can be added without changing the Module logic.
1. The system allows assigning one **Pricelists** to several different **Catalogs**. The **Pricelists** is integrated into the **Catalog**.
1. The system allows specify the **Pricelists** priority and therefore display on **Storefront** the **Pricelists** with higher priority.

## Documentation

1. [Pricing Module Document](/docs/index.md)

1. [View on Github](https://github.com/VirtoCommerce/vc-module-pricing)

1. [Web API Documentation](<https://admin-demo.virtocommerce.com/docs/ui/index#/Pricing%2520module>)

## References

1. Deploy: https://virtocommerce.com/docs/latest/developer-guide/deploy-module-from-source-code/
1. Installation: https://www.virtocommerce.com/docs/latest/user-guide/modules/
1. Home: https://virtocommerce.com
1. Community: https://www.virtocommerce.org
1. [Download Latest Release](https://github.com/VirtoCommerce/vc-module-pricing/releases)

## License

Copyright (c) Virto Solutions LTD.  All rights reserved.

Licensed under the Virto Commerce Open Software License (the "License"); you
may not use this file except in compliance with the License. You may
obtain a copy of the License at

<http://virtocommerce.com/open-source-license>

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or
implied.
