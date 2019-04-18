# Add Prices to products

## Scenario

![Add Prices to products](/docs/media/diagram-add-prices-to-products.png)

## Add List Price

1. The user navigates to **Pricing module** &rightarrow; **Price Lists** &rightarrow; selects the **Price list** and clicks on it
1. The system opens the **Manage Price List** screen
1. The user clicks the Prices widget and the system will display the Price List with possibility to add prices to the Price list
1. The user selects the product and the system will open the ‘Edit prices’ screen. The following UI elements will be displayed on the screen:
    1. Currency – label. The system displays the currency that was specified by user when the Price list was created
    1. List price input field - editable
    1. Sales price input field - editable
    1. Min. quantity input field - editable
    1. Modified- displays when the Price list was last modified
1. The user enters the price into the List Price input field and clicks the ‘Save’ button on the top of the screen
1. The system will save the entered price and display the product Price on the Price list in the Price range column of the table
![Price range column](/docs/media/screen-price-range-column.png)

## Add Sale Price

1. The user navigates to **Pricing module** &rightarrow; **Price Lists** &rightarrow; selects the **Price list** and clicks on it
1. The system opens the **Manage Price List** screen
1. The user clicks the Prices widget and the system will display the Price List with possibility to edit prices
1. The user selects the product and the system will open the **Edit prices** screen
1. The user enters the Sales price into the input field and clicks the **Save** button
1. The system will save the changes and display the Price range on the Price list. The Price range is consisted of List price and sale price
1. The Sale price finally will be displayed to the end user
![Sale price](/docs/media/screen-sale-price.png)

## Specify Tier Price

![Specify Tier Price](/docs/media/diagram-specify-tier-price.png)

1. The user navigates to **Pricing module** &rightarrow; **Price Lists** &rightarrow; selects the **Price list** and clicks on it
1. The system opens the **Manage Price List** screen
1. The user clicks the Prices widget and the system will display the Price List with possibility to edit prices
1. The user selects the product and the system will open the **Edit prices** screen
1. The user enters the List price (required), Sale price (Optional) and Min. quantity (required) into the correspondent input fields and clicks the **Save** button
1. The system will save the changes and the entered price(s) will be displayed in the Price range column. The Min. quantity will be displayed in the **#** column.
1. The system will display the tier price (price for min. Quantity) for the end user, i.e. the specified price (discount) will be available for the specified Min. Quantity
![Specified Price](/docs/media/screen-specified-price.png)
