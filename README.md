# PayrollApp
Payroll system for the Technical Assistant team.
[![Build status](https://build.appcenter.ms/v0.1/apps/527735fc-290c-43fd-a595-bb7a581a38cd/branches/master/badge)](https://appcenter.ms)

## Building
You'll need to add a new ClientSecret.cs file with the following content before building the project:

```
namespace PayrollApp
{
    public class ClientSecret
    {
        public static string FaceApiEndpoint = "YOUR-ENDPOINT-HERE";
        public static string FaceApiKey = "YOUR-API-KEY-HERE";
        public static readonly string[] AcceptableEmails = new string[]
        {
            "ACCOUNT ONE",
            "ACCOUNT TWO"
        };
    }
}
```
