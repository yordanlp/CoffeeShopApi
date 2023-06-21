using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

public class SqsPollingService : BackgroundService {
    private readonly IAmazonSQS _sqsClient;
    private readonly IConfiguration _configuration;

    public SqsPollingService(IAmazonSQS sqsClient, IConfiguration config)
    {
        _sqsClient = sqsClient;
        _configuration = config;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var receiveMessageRequest = new ReceiveMessageRequest
        {
            QueueUrl = _configuration.GetValue<string>("QueueUrl"), // replace with your queue URL
            WaitTimeSeconds = 20 // long polling
        };

        while (!stoppingToken.IsCancellationRequested)
        {
            var receiveMessageResponse = await _sqsClient.ReceiveMessageAsync(receiveMessageRequest);

            foreach (var message in receiveMessageResponse.Messages)
            {
                Console.WriteLine("Received message: " + message.Body);

                var deleteMessageRequest = new DeleteMessageRequest
                {
                    QueueUrl = receiveMessageRequest.QueueUrl,
                    ReceiptHandle = message.ReceiptHandle
                };
                await _sqsClient.DeleteMessageAsync(deleteMessageRequest);
            }
        }
    }
}
