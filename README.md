# Algorithm Test

My implementations for some well-known problems, just for learning purpose.

Each algo has a seperate controller, post data to it to get a result.

## Knapsack controller.

Given an price limit and quantity limit, and a list of items, output items from input items that has the most price.

The output items quantity shouldn't exceed the quantity limit, and the output price shouldn't exceed the price limit.

### limitation

Current implementation is rather slow....

### sample data

input:

```json
{
"priceLimit" : 998,
"quantityLimit" : 5,
"items":[

		{
		  "id": 118,
		  "quantity": 2,
		  "unitPrice": 190
		},
		{
		  "id": 110,
		  "quantity": 2,
		  "unitPrice": 380.44
		},
		{
		  "id": 111,
		  "quantity": 2,
		  "unitPrice": 287.65
		},
		{
		  "id": 113,
		  "quantity": 2,
		  "unitPrice": 191.23
		},
		{
		  "id": 119,
		  "quantity": 2,
		  "unitPrice": 259.98
		},
		{
		  "id": 120,
		  "quantity": 3,
		  "unitPrice": 100.2
		},
		{
		  "id": 114,
		  "quantity": 2,
		  "unitPrice": 390.87
		}
]
}
```

response:

```json
{
	"items": [{
		"id": 111,
		"unitPrice": 287.65,
		"quantity": 1
	}, {
		"id": 119,
		"unitPrice": 259.98,
		"quantity": 2
	}, {
		"id": 118,
		"unitPrice": 190,
		"quantity": 1
	}],
	"totalPrice": 997.61,
	"totalQuantity": 4
}
```