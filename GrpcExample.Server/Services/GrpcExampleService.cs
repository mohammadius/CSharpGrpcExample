using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace GrpcExample.Server.Services;

public class GrpcExampleService : ExampleService.ExampleServiceBase
{
	public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
	{
		return Task.FromResult(new HelloReply {Message = "Hello " + request.Name});
	}

	public override async Task GetTimes(TimeRequest request, IServerStreamWriter<TimeResponse> responseStream, ServerCallContext context)
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

	public override async Task<AddResponse> AddNumbers(IAsyncStreamReader<AddRequest> requestStream, ServerCallContext context)
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
}