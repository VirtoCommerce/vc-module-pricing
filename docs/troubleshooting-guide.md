# Troubleshooting incorrect prices in Storefront

Since the Pricing Module allows adding multiple prices of the same currency to one product situations can occur where prices displayed on the Storefront catalog are not the ones that are expected. 
For example you added a new price to your product but it doesn't show up on the Storefront:

![Wrong price](media/screen-price-wrong.png)

Here are the steps you can follow to find out why the correct price doesn't show:

1. Open the product with the wrong price in the Platform Catalog and open the Price widget.
1. Make sure the new price is in the Price List.
1. Open the new price *Assignments list* and make sure that the price has the assignment that conforms to the following conditions:
    1. The assignment *Priority* is highest among other price list assignments. Price with the highest priority assignments is used first.  
    1. The assignment *Catalog* references the correct catalog. For example if the Store your Storefront is configured to work with uses virtual catalog and the the assignment points to the actual catalog (where the product actually is) the price won't display. Alternatively You can set assignment *Store* instead of the *Catalog* to avoid confusion.
    1. If the assignment has special conditions make sure they're applicable. For example if the assignment have User Groups conditions a customer should be added in the correct User Groups.   
    1. Build the product index and check the storefront price

    ![Assignment Correct](media/screen-correcnt-assignment.png) ![Price Correct](media/screen-price-correct.png)
