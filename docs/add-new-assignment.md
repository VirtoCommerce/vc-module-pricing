# Add New Assignment

## Scenario

![Add New Assignment](/docs/media/diagram-add-new-assignment.png)

## Simple Assignment without rules and conditions

1. The user selects **Price List** on the Price Lists screen and clicks on the **Assignments widget**
![Price list](/docs/media/screen-price-lists.png)
1. The system opens a new screen with an active **Add** button that enables the user to add a new Assignment to the selected Price List
1. The user clicks the **Add** button and the system opens the **New Price List Assignment** screen. The screen will display the following elements:
    1. Assignment Name input field - required
    1. Catalog drop down - required.
    1. Price List drop down- required
    1. Priority input field - required
    1. Description input field- optional
    1. Enable date – date picker optional
    1. Expiration date- date picker optional
    1. Under Eligible Shoppers:
        1. If any of these conditions are true:
            1. Any is a link
            1. Under Any selection is available: ALL, ANY
        1. **Add condition** button
    1. **Create** button becomes active only when required fields are filled out
1. The user fills out the required data:
    1. Enters the Assignment Name
    1. Selects the Catalog from the drop-down list
    1. Selects the Price List from the drop-down list
    1. Specifies the Priority (1, 2, 3, etc.). The system will then display on the Storefront the Price List with the highest priority
    1. The user can also fill out the fill out the **Description** field, but it’s optional
    1. The user can optionally specify the date range using the date pickers **from** - **to** to determine Price List availability. If the user leaves the fields blank, that will mean the Price List is always available
![New price list assignment](/docs/media/screen-new-price-list-assignment.png)
1. The system will create an assignment without additional rules and conditions
1. The system will display the number of assignments created in the Assignment widget
![Assignment](/docs/media/screen-assignment.png)

## Assignment with Rules and Conditions

1. In the created Assignment under Eligible Shoppers the user should first select if Any or All conditions are true by clicking the **Any** link and selecting the option
1. The system will display the selected option: Any or All
1. The user clicks the **Add condition** icon
1. The system will display the list of different conditions previously added to the system. Below is an example of different rules and conditions that can be assigned to the Price List
![List of different conditions](/docs/media/screen-list-of-different-conditions.png)
1. The user selects as many conditions and needed from the list
1. The system will display the selected conditions with possibility to delete them as shown on the screen shot bellow
![Selected conditions](/docs/media/screen-selected-conditions.png)
1. The user clicks the **Save** button on the top of the screen and the system will save the selected conditions
1. The conditions will be assigned to the Price List
