---
author: NO VALUE SET
ms.author: NO VALUE SET # Microsoft employees only
ms.date: 4/15/2019
---
# PRICING MODULE

The ***Pricing Module*** is designed for storing, managing and evaluating product prices.
The Pricing Module is consisted of three main objects:

***Price*** – the purpose of this object is to store the item price in the system and support a static discount and a tier price. The price has the following key characteristics:

- Product (required) is an object that the current price relates to
- List price (required) – represents the base price for the product in currency of owned price. The List price is a pre-promotional regular price.  
- Sale price (optional) –is the discounted price of an item from the regular sale price
- Min quantity (min. 1) – represents lower inclusive limit for products quantity starting from which, this price will be valid. The min QTY is used for achieving Tier pricing functionality

***Pricelist*** – Plays the role of a container for prices with single currency. The Pricelist has the following key characteristics:

- Currency (required) – single currency for all prices included into the Price list  
- Prices – the list of prices included into the Price list  

***Pricelist*** Assignment allows associating the specific Pricelist with a catalog based on specific rules and conditions  

The Pricelist Assignment has the following key characteristics:

- Pricelist (required)  
- Catalog (required) - TBD
- Enable (start) and Expiration dates – the specified Enabled and Expiration dates determine the validation period of the pricelist assignment  
- Priority defines the priority of the Pricelist. The system will apply the pricelist that has the highest priority  
- Dynamic conditions of applicability -  

## Functional Requirements  

1. The system supports multiple Price lists, i.e. the same product item may have different price in different price lists.
1. Each Catalog can have its own list of Price Lists.
1. Every Price  List should have a unique Name-ID, and single currency.
1. Each Price list contains a list of product prices with single currency.
1. The system supports Tier Pricing and it is a way to encourage shoppers to buy larger quantities of a product by applying discounts based on the quantity ordered.
1. The system supports static discounts on products and allows the user to specify the discount on each item on the list.
1. In order to make the Price list active, it should be assigned to a specific Catalog in the system. The system allows the user to specify specific rules and conditions that can also be assigned to the PriceList. These specific rules and conditions will influence the price calculation and define to which user categories or Organizations the pricelist will be displayed. This is a so-called Dynamic Assignment.
1. The system allows expanding the Dynamic Assignment tree and it completely depends on the organization business needs. Any condition or rule can be added without changing the Module logic.
1. The system allows assigning one Price list to several different Catalogs. The Price list is integrated into the Catalog.
1. The system allows specify the Price list priority and therefore display on Storefront the Price list with higher priority.

## Scenarios  

1. [Create a New Price list](/Docs/create-new-price-list.md)
    1. Add Products to the New Price List
    1. Add Prices to products
    1. Add List Price
    1. Add Sale Price

## Specify Tier Price

View Price list via Catalog Module 
Add New Assignment 
Simple Assignment without rules and conditions 
Assignment with Rules and Conditions 

# MANAGING PRICING MODULE SETTINGS

## Indexing

## Web API

## Database Storage