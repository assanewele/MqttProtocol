using System.Text.Json;
using MQTTnet;
using MQTTnet.Formatter;
using MQTTnet.Protocol;

// 1. Create the client (the "factory" pattern is just MQTTnet's way to build clients)
var factory = new MqttClientFactory();
using var client = factory.CreateMqttClient();

// 2. Connection options -> everything you used to pass as CLI flags
var options = new MqttClientOptionsBuilder()
    .WithTcpServer("localhost", 1883)               // where the broker lives
    .WithClientId("console-publisher")              // -i "console-publisher"
    .WithProtocolVersion(MqttProtocolVersion.V500)  // we decided: MQTT 5
    .Build();

// 3. Connect -> this is the moment "New client connected" appears in window A
await client.ConnectAsync(options, CancellationToken.None);
Console.WriteLine("Connected to broker.");

// 4. Build the payload -> a real object serialized to JSON, not a hand-written string
var etat = new { etat = "on", horodatage = DateTime.UtcNow };
string payload = JsonSerializer.Serialize(etat);

// 5. Build the message -> topic + payload + options, like -t / -m / -q / -r
var message = new MqttApplicationMessageBuilder()
    .WithTopic("hagerpilot/maison42/equipements/lampe1/etat")
    .WithPayload(payload)
    .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce) // -q 1
    //.WithRetainFlag(false)                                            // add -r? set true
    .Build();

// 6. Publish, then leave politely (clean disconnect -> no Last Will triggered)
await client.PublishAsync(message, CancellationToken.None);
Console.WriteLine($"Published on {message.Topic}: {payload}");

await client.DisconnectAsync();