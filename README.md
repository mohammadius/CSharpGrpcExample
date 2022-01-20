# C# gRPC Example
gRPC Server and Client example written in C#

## Download
you can download the win-x86 self-contained version of Server and Client from [here](https://github.com/mohammadius/CSharpGrpcExample/releases/download/release/grpc_example_win-x86.7z).

or download the project and build with `dotnet build` command. ([.NET 6 SDK](https://dotnet.microsoft.com/en-us/download) is required for building)

## Unary example
### protobuf contract
```protobuf
rpc SayHello (HelloRequest) returns (HelloReply);

message HelloRequest {
  string name = 1;
}
message HelloReply {
  string message = 1;
}
```
### Server code
```csharp
public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
{
    return Task.FromResult(new HelloReply {Message = "Hello " + request.Name});
}
```
### Client code
```csharp
Console.WriteLine("Write your name:");
var helloRequest = new HelloRequest {Name = Console.ReadLine()};
Console.WriteLine($"helloRequest: {{ Name: {helloRequest.Name} }}");
var helloReply = await client.SayHelloAsync(helloRequest);
Console.WriteLine($"HelloReply: {{ Message: {helloReply.Message} }}");
```

## Server Streaming example
### protobuf contract
```protobuf
rpc GetTimes (TimeRequest) returns (stream TimeResponse);

message TimeRequest {
  int32 count = 1;
}
message TimeResponse {
  int32 index = 1;
  google.protobuf.Timestamp time = 2;
}
```
### Server code
```csharp
public override async Task GetTimes(
    TimeRequest request,
    IServerStreamWriter<TimeResponse> responseStream,
    ServerCallContext context)
{
    for (var i = 0; i < request.Count; i++)
    {
        await responseStream.WriteAsync(new TimeResponse
        {
            Index = i,
            Time = Timestamp.FromDateTime(DateTime.UtcNow)
        });
        await Task.Delay(500);
    }
}
```
### Client code
```csharp
onsole.WriteLine("number of DateTimes to get (default is 5):");
if (!int.TryParse(Console.ReadLine(), out var count)) { count = 5; }
var timeRequest = new TimeRequest {Count = count};
Console.WriteLine($"timeRequest: {{ Count: {timeRequest.Count} }}");
using (var getTimes = client.GetTimes(timeRequest))
{
    while (await getTimes.ResponseStream.MoveNext())
    {
        Console.WriteLine($"timeReply: {{ Index: {getTimes.ResponseStream.Current.Index}, Time: {getTimes.ResponseStream.Current.Time} }}");
    }
}
```

## Client streaming example
### protobuf contract
```protobuf
rpc AddNumbers (stream AddRequest) returns (AddResponse);

message AddRequest {
  int32 number = 1;
}
message AddResponse {
  int32 sum = 1;
  repeated int32 numbers = 2;
}
```
### Server code
```csharp
public override async Task<AddResponse> AddNumbers(
    IAsyncStreamReader<AddRequest> requestStream,
    ServerCallContext context)
{
    var numbers = new LinkedList<int>();
    while (await requestStream.MoveNext() && !context.CancellationToken.IsCancellationRequested)
    {
        var message = requestStream.Current;
        numbers.AddLast(message.Number);
    }
    var response = new AddResponse {Sum = numbers.Sum()};
    response.Numbers.AddRange(numbers);
    return response;
}
```
### Client code
```csharp
using (var addNumbers = client.AddNumbers())
{
    while (true)
    {
        Console.Write("number to add (empty or NaN to complete): ");
        if (!int.TryParse(Console.ReadLine(), out var number)) { break; }
        await addNumbers.RequestStream.WriteAsync(new AddRequest {Number = number});
    }
    await addNumbers.RequestStream.CompleteAsync();
    var addResponse = await addNumbers.ResponseAsync;
    Console.WriteLine($"addResponse: {{ Sum: {addResponse.Sum}, Numbers: [{string.Join(", ", addResponse.Numbers)}] }}");
}
```

## Bi-directional streaming
not implementing for because it's hard to showcase properly
