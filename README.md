# Sage50API Introduction

Welcome to our ASP.NET API project leveraging the power of the Sage50 SDK. This project serves as a bridge between your application and the Sage50 accounting software, enabling seamless integration and automation of essential financial processes. With our API, you can effortlessly retrieve, update, and synchronize data between your application and Sage50, streamlining your business operations. This readme.md provides an overview of the API's features, installation instructions, and usage guidelines to help you get started quickly and effectively.

## Web.config

To run the project successfully, ensure that the database file path is correctly set in the web.config file. Locate the <appSettings> section and update the DatabaseFilePath value to the appropriate path on your system.

```config
<appSettings>
    <add key="DatabaseFilePath" value="C:\Users\Public\Documents\Simply Accounting\2023\Samdata\Premium\Universl.SAI" />
</appSettings>
```

## Authorization

To access the database, make a POST request to the specified URL, providing the username, password, and multiuser parameters as query parameters. 

| Parameter | Type     | Description                                                                                                                                              |
| :--- |:---------|:---------------------------------------------------------------------------------------------------------------------------------------------------------|
| `username` | `string` | **Required**. Username to log on to database                                                                                                             |
| `password` | `string` | **Required**. Password for the username                                                                                                                  |
| `multiuser` | `bool`   | **Required**. Set to true to open database in multi-user mode, for First Step databases, this parameter is ignored and always opened in single-user mode |

When making the API request, include the following JSON body structure as an example:

```javascript 
{
  "requests": [
    {
      "action": "string",
      "body": {
        // Include various parameters and their corresponding values here
      }
    }
  ]
}
```

Make a POST request to the following API endpoint to access the PosterRequest functionality:
```javascript
http://localhost:49243/api/PosterRequest?username=sysadmin&password=sysadmin&multiuser=false
```

## Responses

The API response will be in the following format:

```javascript
{
    "job_duration": "0m 0s",
    "total_requests": 2,
    "responses": [
        {
            // Include the retrieved data fields and their corresponding values here
        }
    ]
}
```

The `job_duration` attribute describes the duration of the job execution time in minutes and seconds.

The `success` attribute describes the total number of requests processed and included in the API response.

The `responses` attribute contains an array of objects, where each object represents a response for a specific request. The number of response objects corresponds to the "total_requests" value, providing details or data associated with each individual request.

## Status Codes

The API returns either a 400 status code for a bad request or a 200 status code for a successful request, indicating the outcome of the API call.

| Status Code | Description |
| :--- | :--- |
| 200 | `OK` |
| 400 | `BAD REQUEST` |



## Actions: Sales Invoice

`CREATE`
```
{
  "requests": [
    {
      "action": "create_sales_invoice",
      "body": {
        "cusven_name": "",
        "invoice_number": "",
        "invoice_date": "",
        "paid_by_type": "",
        "cheque_number": "",
        "detail_lines": [
          {
            "item_number": "",
            "item_description": "",
            "quantity": "",
            "price": "",
            "tax_code": "",
            "line_amount": "",
            "ledger_account": "",
            "tax_lines": [
              {
                "tax_authority": "",
                "tax_amount": ""
              }
            ]
          }
        ],
        "tax_lines": [
          {
            "tax_authority": "",
            "tax_amount": ""
          }
        ]
      }
    }
  ]
}
```

`GET`
```
{
  "requests": [
    {
      "action": "lookup_sales_invoice",
      "body": {
        "find_cusven_name": "",
        "find_invoice_number": ""
      }
    }
  ]
}
```

`UPDATE`
```
{
  "requests": [
    {
      "action": "adjust_sales_invoice",
      "body": {
        "find_cusven_name": "",
        "find_invoice_number": "",
        "cusven_name": "",
        "invoice_number": "",
        "invoice_date": "",
        "paid_by_type": "",
        "cheque_number": "",
        "detail_lines": [
          {
            "item_number": "",
            "item_description": "",
            "quantity": "",
            "price": "",
            "tax_code": "",
            "line_amount": "",
            "ledger_account": "",
            "tax_lines": [
              {
                "tax_authority": "",
                "tax_amount": ""
              }
            ]
          }
        ],
        "tax_lines": [
          {
            "tax_authority": "",
            "tax_amount": ""
          }
        ]
      }
    }
  ]
}
```

`DELETE`
```
{
  "requests": [
    {
      "action": "void_sales_invoice",
      "body": {
        "find_cusven_name": "",
        "find_invoice_number": ""
      }
    }
  ]
}
```

## Actions: Purchase Invoice

`CREATE`
```
{
  "requests": [
    {
      "action": "create_purchase_invoice",
      "body": {
        "cusven_name": "",
        "invoice_number": "",
        "invoice_date": "",
        "paid_by_type": "",
        "cheque_number": "",
        "detail_lines": [
          {
            "item_number": "",
            "item_description": "",
            "quantity": "",
            "price": "",
            "tax_code": "",
            "line_amount": "",
            "ledger_account": "",
            "tax_lines": [
              {
                "tax_authority": "",
                "tax_amount": ""
              }
            ]
          }
        ],
        "tax_lines": [
          {
            "tax_authority": "",
            "tax_amount": ""
          }
        ]
      }
    }
  ]
}
```

`GET`
```
{
  "requests": [
    {
      "action": "lookup_purchase_invoice",
      "body": {
        "find_cusven_name": "",
        "find_invoice_number": ""
      }
    }
  ]
}
```

`UPDATE`
```
{
  "requests": [
    {
      "action": "adjust_purchase_invoice",
      "body": {
        "find_cusven_name": "",
        "find_invoice_number": "",
        "cusven_name": "",
        "invoice_number": "",
        "invoice_date": "",
        "paid_by_type": "",
        "cheque_number": "",
        "detail_lines": [
          {
            "item_number": "",
            "item_description": "",
            "quantity": "",
            "price": "",
            "tax_code": "",
            "line_amount": "",
            "ledger_account": "",
            "tax_lines": [
              {
                "tax_authority": "",
                "tax_amount": ""
              }
            ]
          }
        ],
        "tax_lines": [
          {
            "tax_authority": "",
            "tax_amount": ""
          }
        ]
      }
    }
  ]
}
```

`DELETE`
```
{
  "requests": [
    {
      "action": "void_purchase_invoice",
      "body": {
        "find_cusven_name": "",
        "find_invoice_number": ""
      }
    }
  ]
}
```

## Actions: General Journal

`CREATE`
```
{
  "requests": [
    {
      "action": "create_general_journal",
      "body": {
        "journal_date": "",
        "source": "",
        "comment": "",
        "detail_lines": [
          {
            "ledger_account": "",
            "debit_amount": "",
            "credit_amount": "",
            "comment": ""
          },
          {
            "ledger_account": "",
            "debit_amount": "",
            "credit_amount": "",
            "comment": ""
          }
        ]
      }
    }
  ]
}
```

`GET`
```
{
  "requests": [
    {
      "action": "lookup_general_journal",
      "body": {
        "find_journal_id": "",
        "find_journal_is_last_year": false
      }
    }
  ]
}
```

`UPDATE`
```
{
  "requests": [
    {
      "action": "adjust_general_journal",
      "body": {
        "find_journal_id": "",
        "journal_date": "",
        "source": "",
        "comment": "",
        "detail_lines": [
          {
            "ledger_account": "",
            "debit_amount": "",
            "credit_amount": "",
            "comment": ""
          },
          {
            "ledger_account": "",
            "debit_amount": "",
            "credit_amount": "",
            "comment": ""
          }
        ]
      }
    }
  ]
}
```

## Actions: Customer

`CREATE`
```
{
  "requests": [
    {
      "action": "create_customer",
      "body": {
        "name": "",
        "contact_name": "",
        "street1": "",
        "street2": "",
        "city": "",
        "province": "",
        "country": "",
        "postal_code": "",
        "phone1": "",
        "phone2": "",
        "fax": "",
        "email": "",
        "website": "",
        "currency_code": ""
      }
    }
  ]
}
```

`GET`
```
{
  "requests": [
    {
      "action": "lookup_customer",
      "body": {
        "find_name": ""
      }
    }
  ]
}
```

`UPDATE`
```
{
  "requests": [
    {
      "action": "adjust_customer",
      "body": {
        "find_name": "",
        "name": "",
        "contact_name": "",
        "street1": "",
        "street2": "",
        "city": "",
        "province": "",
        "country": "",
        "postal_code": "",
        "phone1": "",
        "phone2": "",
        "fax": "",
        "email": "",
        "website": "",
        "currency_code": ""
      }
    }
  ]
}
```

## Actions: Vendor

`CREATE`
```
{
  "requests": [
    {
      "action": "create_vendor",
      "body": {
        "name": "",
        "contact_name": "",
        "street1": "",
        "street2": "",
        "city": "",
        "province": "",
        "country": "",
        "postal_code": "",
        "phone1": "",
        "phone2": "",
        "fax": "",
        "email": "",
        "website": "",
        "currency_code": "",
        "tax_id": ""
      }
    }
  ]
}
```

`GET`
```
{
  "requests": [
    {
      "action": "lookup_vendor",
      "body": {
        "find_name": ""
      }
    }
  ]
}
```

`UPDATE`
```
{
  "requests": [
    {
      "action": "adjust_vendor",
      "body": {
        "find_name": "",
        "name": "",
        "contact_name": "",
        "street1": "",
        "street2": "",
        "city": "",
        "province": "",
        "country": "",
        "postal_code": "",
        "phone1": "",
        "phone2": "",
        "fax": "",
        "email": "",
        "website": "",
        "currency_code": "",
        "tax_id": ""
      }
    }
  ]
}
```

## Actions: Inventory

`CREATE`
```
{
  "requests": [
    {
      "action": "create_inventory",
      "body": {
        "name": "",
        "part_code": "",
        "name_alt": "",
        "is_service": false,
        "is_activity": false,
        "stocking_unit": "",
        "stocking_unit_alt": "",
        "price_regular": "",
        "price_preferred": "",
        "account_asset": "",
        "account_revenue": "",
        "account_expense": "",
        "account_variance": ""
      }
    }
  ]
}
```

`GET`
```
{
  "requests": [
    {
      "action": "lookup_inventory",
      "body": {
        "find_part_code": ""
      }
    }
  ]
}
```

`UPDATE`
```
{
  "requests": [
    {
      "action": "adjust_inventory",
      "body": {
        "find_part_code": "",
        "name": "",
        "part_code": "",
        "name_alt": "",
        "is_service": false,
        "is_activity": false,
        "price_regular": "",
        "price_preferred": "",
        "account_asset": "",
        "account_revenue": "",
        "account_expense": "",
        "account_variance": ""
      }
    }
  ]
}
```

## Actions: Employee

`CREATE`
```
{
  "requests": [
    {
      "action": "create_employee",
      "body": {
        "name": "",
        "street1": "",
        "street2": "",
        "city": "",
        "province": "",
        "postal_code": "",
        "phone1": "",
        "phone2": "",
        "sin": "",
        "birth_date": "",
        "hire_date": "",
        "tax_table_province": "",
        "pay_periods": ""
      }
    }
  ]
}
```

`GET`
```
{
  "requests": [
    {
      "action": "lookup_employee",
      "body": {
        "find_name": ""
      }
    }
  ]
}
```

`UPDATE`
```
{
  "requests": [
    {
      "action": "adjust_employee",
      "body": {
        "find_name": "",
        "name": "",
        "street1": "",
        "street2": "",
        "city": "",
        "province": "",
        "postal_code": "",
        "phone1": "",
        "phone2": "",
        "sin": "",
        "birth_date": "",
        "hire_date": "",
        "tax_table_province": "",
        "pay_periods": ""
      }
    }
  ]
}
```

## Actions: Project

`CREATE`
```
{
  "requests": [
    {
      "action": "create_project",
      "body": {
        "name": "",
        "name_alt": "",
        "start_date": ""
      }
    }
  ]
}
```

`GET`
```
{
  "requests": [
    {
      "action": "lookup_project",
      "body": {
        "find_name": ""
      }
    }
  ]
}
```

`UPDATE`
```
{
  "requests": [
    {
      "action": "adjust_project",
      "body": {
        "find_name": "",
        "name": "",
        "name_alt": "",
        "start_date": ""
      }
    }
  ]
}
```

## Actions: Chart of Accounts

`CREATE`
```
{
  "requests": [
    {
      "action": "create_account",
      "body": {
        "name": "",
        "name_alt": "",
        "account_number": "",
        "account_class": "",
        "account_type": ""
      }
    }
  ]
}
```

`GET`
```
{
  "requests": [
    {
      "action": "lookup_account",
      "body": {
        "find_account_number": ""
      }
    }
  ]
}
```

`UPDATE`
```
{
  "requests": [
    {
      "action": "adjust_account",
      "body": {
        "find_account_number": "",
        "name": "",
        "name_alt": "",
        "account_number": "",
        "account_class": "",
        "account_type": ""
      }
    }
  ]
}
```

## Actions: NoQuery SQL

`REQUEST`
```
{
  "requests": [
    {
      "action": "create_sql_non_query",
      "body": {
        "sql_non_query": ""
      }
    }
  ]
}
```

