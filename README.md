# PayrollApp
Payroll system for the Technical Assistant team.

## Building
You'll need to add a new ClientSecret.cs file with the following content before building the project:

```
namespace PayrollApp
{
    public class ClientSecret
    {
        public string FaceApiEndpoint = "YOUR-ENDPOINT-HERE";
        public string FaceApiKey = "YOUR-API-KEY-HERE";
    }
}
```
