using System;
using Grpc.Core;
using Grpc.Net.Client;
using GrpcExample;

using var channel = GrpcChannel.ForAddress("https://localhost:7015");
var client = new ExampleService.ExampleServiceClient(channel);

Console.WriteLine("Unary Example.");
Console.WriteLine("Write your name:");
var helloRequest = new HelloRequest {Name = Console.ReadLine()};
Console.WriteLine($"helloRequest: {{ Name: {helloRequest.Name} }}");
var helloReply = await client.SayHelloAsync(helloRequest);
Console.WriteLine($"HelloReply: {{ Message: {helloReply.Message} }}");

Console.WriteLine("\nServer streaming Example.");
Console.WriteLine("number of DateTimes to get (default is 5):");
if (!int.TryParse(Console.ReadLine(), out var count))
{
	count = 5;
}

var timeRequest = new TimeRequest {Count = count};
Console.WriteLine($"timeRequest: {{ Count: {timeRequest.Count} }}");
using (var getTimes = client.GetTimes(timeRequest))
{
	while (await getTimes.ResponseStream.MoveNext())
	{
		Console.WriteLine($"timeReply: {{ Index: {getTimes.ResponseStream.Current.Index}, Time: {getTimes.ResponseStream.Current.Time} }}");
	}
}

Console.WriteLine("\nClient streaming Example.");
using (var addNumbers = client.AddNumbers())
{
	while (true)
	{
		Console.Write("number to add (empty or NaN to complete): ");
		if (!int.TryParse(Console.ReadLine(), out var number))
		{
			break;
		}

		await addNumbers.RequestStream.WriteAsync(new AddRequest {Number = number});
	}

	await addNumbers.RequestStream.CompleteAsync();
	var addResponse = await addNumbers.ResponseAsync;
	Console.WriteLine($"addResponse: {{ Sum: {addResponse.Sum}, Numbers: [{string.Join(", ", addResponse.Numbers)}] }}");
}