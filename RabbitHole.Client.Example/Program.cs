
using RabbitHole.Client;
using System.Net;

var test = new RabbitHoleClient(2909032, "zone_token", "192.168.68.62", 32400, "139.180.134.196", 4321, 4322);
test.Start();

while (true)
{
    Thread.Sleep(100);
}