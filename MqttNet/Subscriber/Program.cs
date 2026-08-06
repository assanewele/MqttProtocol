using MQTTnet;
using MQTTnet.Formatter;
using MQTTnet.Protocol;

var factory = new MqttClientFactory();
using var client = factory.CreateMqttClient();

var options = new MqttClientOptionsBuilder()
    .WithTcpServer("localhost", 1883)
    .WithClientId("console-subscriber")             // fixed ID -> sessions become possible
    .WithProtocolVersion(MqttProtocolVersion.V500)
    //.WithCleanStart(false)                          // resume my session if the broker has one
    //.WithSessionExpiryInterval(3600)                // and keep it 1 hour after I disconnect
    .Build();

// 1. Declare the handler BEFORE connecting -> never miss an early message
client.ApplicationMessageReceivedAsync += e =>
{
    string topic = e.ApplicationMessage.Topic;
    string payload = e.ApplicationMessage.ConvertPayloadToString();
    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {topic} -> {payload}");
    return Task.CompletedTask;
};

// 2. Connect
await client.ConnectAsync(options, CancellationToken.None);
Console.WriteLine("Connected. Subscribing...");

// 3. Subscribe -> -t "hagerpilot/maison42/#" -q 1
var subscribeOptions = factory.CreateSubscribeOptionsBuilder()
    .WithTopicFilter(f => f
        .WithTopic("hagerpilot/maison42/#")
        .WithAtLeastOnceQoS()
        )
    .Build();
await client.SubscribeAsync(subscribeOptions, CancellationToken.None);
Console.WriteLine("Subscribed. Waiting for messages (press Enter to quit).");

// 4. Stay alive -> the handler does the work; the main thread just waits
Console.ReadLine();
await client.DisconnectAsync();