using RabbitHole.Client;

internal class Program
{
    private static void OutputParamHelp(string? message = null)
    {
        if(message != null)
        {
            Console.WriteLine(message);
            Console.WriteLine();
        }

        Console.WriteLine("Usage: RabbitHole.CLI.exe -pz <PullZoneId> -p <Port> -lip <LocalIP> -t <Token>");
        Console.WriteLine("  -pz, -pullzone <PullZoneId>  The PullZoneId to use.");
        Console.WriteLine("  -lp, -localport <Port>       The port to listen on.");
        Console.WriteLine("  -lip, -localip <LocalIP>     The local IP to bind to.");
        Console.WriteLine("  -t, -token <Token>           The API token to use.");
        Console.WriteLine();
        Console.WriteLine();
    }

    private static void Main(string[] args)
    {
        var pullZoneId = ParamHelper.GetLong(args, 0, "-pz", "-pullzone");
        var localPort = (int)ParamHelper.GetLong(args, 0, "-lp", "-localport");
        var LocalIP = ParamHelper.GetString(args, "127.0.0.1", "-lip", "-localip");
        var authToken = ParamHelper.GetString(args, "", "-t", "-token");
        var tunnelEndpoint = ParamHelper.GetString(args, "inbound-tunnel.b-cdn.net", "-te", "-tunnelendpoint");
        var tunnelPort = (int)ParamHelper.GetLong(args, 4321, "-tp", "-tunnelport");
        var controlport = (int)ParamHelper.GetLong(args, 4322, "-cp", "-controlport");

        // Validate PullZoneId
        if (pullZoneId < 0)
        {
            OutputParamHelp("PullZoneId is required.");
            return;
        }

        // Validate Port
        if (localPort < 1 || localPort > 65535)
        {
            OutputParamHelp("Valid port number is required..");
            return;
        }

        // Validate LocalIP
        if (string.IsNullOrWhiteSpace(LocalIP))
        {
            OutputParamHelp("LocalIP is required.");
            return;
        }

        // Validate Token
        if (string.IsNullOrWhiteSpace(authToken))
        {
            OutputParamHelp("Token is required.");
            return;
        }

        Console.WriteLine($"Starting Rabbit Hole system with PullZoneId: {pullZoneId}, Port: {localPort}, LocalIP: {LocalIP}");
        var rabbitHoleClient = new RabbitHoleClient(
            pullZoneId: pullZoneId,
            apiKey: authToken,
            localIP: LocalIP,
            localPort: localPort,
            tunnelHostname: tunnelEndpoint,
            tunnelPort: tunnelPort,
            tunnelControlPort: controlport);
        rabbitHoleClient.Start();

        while (true)
        {
            Thread.Sleep(100);
        }
    }
}