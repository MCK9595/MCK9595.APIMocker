using ConsoleAppFramework;

var app = ConsoleApp.Create();
app.Add<Commands>();
app.Run(args);

public class Commands
{
    /// <summary>
    /// Start mock API server from OpenAPI definition
    /// </summary>
    /// <param name="input">-i, Path to OpenAPI definition file (yaml/json)</param>
    /// <param name="port">-p, Port number to listen on</param>
    public void Start(string input, int port = 8080)
    {
        Console.WriteLine($"Starting mock server from: {input} on port: {port}");
    }
}
