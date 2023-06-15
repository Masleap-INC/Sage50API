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

```json 
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