
# Fishie Transactions

intigrating payment via bml transfer can be annoying
so i made this microservice so you can plug it into your existing project to keep track of and verify incoming payments


## API Reference

#### Getting Started
first you need to set the domain name for the bank api endpoint

```http
  POST /api/bank/setUrl
```

| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `url` | `string` | bank base endpoint|

next you need to login to your account via the api

```http
  POST /api/bank/login
```

| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `Username` | `string` | Your bml username |
| `Password` | `string` | Your API password |

checking to see if you are logged in

```http
  GET /api/bank/login
```
once logged in you need to select which accounts you want to check for payments but before you do that you need to get a list of Accounts


```http
  GET /api/bank/getAccounts
```

```http
  POST /api/bank/selectAccounts
```

| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `Accounts` | `List<string>` | List of accounts you want to check for incoming payments |

to verify you have selected the account you can call the following endpoint

```http
  GET /api/bank/getSelectedAccounts
```


#### Confirming Payments
```http
  POST /api/bank/confirmPayment
```

| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `Name` | `string` | Name of the user paying as listed on the bank |
| `Amount` | `float` | expacted amount |


#### Unconfirming Payments
```http
  POST /api/bank/unconfirmPayment
```

to obtain the hash you can either get it as the data returned by the /api/bank/confirmPayment or you can call /api/bank/getHistory

| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `hash` | `string` | hash for the transection |

#### Get History

this will return all transection along with its hash 

```http
  GET /api/bank/getHistory
```


