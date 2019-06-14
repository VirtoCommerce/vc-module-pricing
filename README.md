---
author: Maksim Kopnov
date: 4/15/2019
---
# Pricing Module

## Overview

The **Pricing Module** is designed for storing, managing and evaluating product prices.

The Pricing Module is consisted of three main objects:

- Price
- Pricelist
- Pricelist Assignment

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

## Functional Requirements  

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

## Scenarios  

1. [Create a New Price list](/docs/create-new-price-list.md)
    1. [Add Products to the New Price List](/docs/add-products-to-the-new-price-list.md)
    1. [Add Prices to products](/docs/add-prices-to-products.md)
        1. [Add List Price](/docs/add-prices-to-products.md#add-list-price)
        1. [Add Sale Price](/docs/add-prices-to-products.md#add-sale-price)
        1. [Specify Tier Price](/docs/add-prices-to-products.md#specify-tier-price)
1. [View Price list via Catalog Module](/docs/view-price-list-via-catalog-module.md)
1. [Add New Assignment](/docs/add-new-assignment.md)
    1. [Simple Assignment without rules and conditions](/docs/add-new-assignment.md#simple-assignment-without-rules-and-conditions)
    1. [Assignment with Rules and Conditions](/docs/add-new-assignment.md#assignment-with-rules-and-conditions)
1. [Managing Pricing module settings](/docs/view-price-list-via-catalog-module.md)

## Web API

Web API documentation for each module is built out automatically and can be accessed by following the link bellow:
<https://admin-demo.virtocommerce.com/docs/ui/index#/Pricing%2520module>

## Database Model

![DB model](/docs/media/diagram-db-model.png)

## License

Copyright (c) Virto Solutions LTD.  All rights reserved.

Licensed under the Virto Commerce Open Software License (the "License"); you
may not use this file except in compliance with the License. You may
obtain a copy of the License at

<http://virtocommerce.com/opensourcelicense>

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or
implied.
