# Algorithm Test

My implementations for some well-known problems, just for learning purpose.

Each algo has a seperate controller, post data to it to get a result.

## Knapsack controller.

Given an price limit and quantity limit, and a list of items, output items from input items that has the most price.

The output items quantity shouldn't exceed the quantity limit, and the output price shouldn't exceed the price limit.

### sample test

Following i/o will walk 11917 steps and takes an average of 7ms.

input:

```json
{
"priceLimit" :2998,
"quantityLimit" :6,
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
		},
		{
		  "id": 133,
		  "quantity": 2,
		  "unitPrice": 165.8
		},
		{
		  "id": 134,
		  "quantity": 1,
		  "unitPrice": 2000
		},
		{
		  "id": 135,
		  "quantity": 1,
		  "unitPrice": 56
		}
]
}
```

response:

```json
{
	"items": [{
		"id": 134,
		"unitPrice": 2000,
		"quantity": 1
	}, {
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
	"totalPrice": 2997.61,
	"totalQuantity": 5
}
```

## Calculator Controller

A simple calculator supports +/-/*/\/ and brackets.

Use a `List` to parse and calculate the result. The main idea is:

1. Parse and read each part of the expression to a class of `Argument`, which can be of type `Number`/`Operator`/`Brackets`
2. Don't start any computation until we meet the first close bracket. 
3. And when we meet a close bracket, calculate the sublist , which start from last open bracket to the close bracket, of the `Argument List`.
	- the calculation loops through this sublist for 2 times, first do multiply and divide, second do the plus and substract.
	- replace the sublist of the original `argument list` with a single argument of a number, which is the result we've just calculated from this sublist.
4. Go on parsing till the end, in this process, we may do as many times as step 3 as we encounter the number of bracket pair.
5. When we reach the end, do step 3 one more time. But this time, the input should be the full `Argument List`.

### sample input

request

```
//3*(4+5)*6/2-1/(2+6)
https://localhost:44308/api/Calculator?expr=3%2A%284%2B5%29%2A6%2F2-1%2F%282%2B6%29
```

response

```
80.875
```
