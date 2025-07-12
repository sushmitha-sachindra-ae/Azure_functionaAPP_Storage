using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Azure.Security.KeyVault.Secrets;
using Azure.Identity;
using Azure.Storage.Queues;


namespace My.Functions
{
    public static class HttpExample
    {
        [FunctionName("HttpExample")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.    Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            string responseMessage = string.IsNullOrEmpty(name)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"Hello, {name}. This HTTP triggered function executed successfully.";
                 log.LogInformation("response message "+responseMessage);
            try
            {
                string keyVaultName = "keyvaultaz204ss";
                string keyVaultUri = $"https://{keyVaultName}.vault.azure.net/";
                log.LogInformation("key vault uri " + keyVaultUri);
                var secretClient = new SecretClient(new Uri(keyVaultUri), new DefaultAzureCredential());
                string secretName = "storageQueueConString";
                log.LogInformation("secretname " + secretName);
                KeyVaultSecret secret = secretClient.GetSecret(secretName);
                Console.WriteLine($"Retrieved secret: {secret.Name} with value: {secret.Value}");
                log.LogInformation($"Retrieved secret: {secret.Name} with value: {secret.Value}");
                log.LogInformation(secret.Value);
                //string connectionString = secret.Value;
            }
            catch (Exception ex)
            {

                log.LogInformation(ex.Message);
                //var response = req.CreateResponse(HttpStatusCode.InternalServerError);
                // await response.WriteStringAsync("Something went wrong.");
                //  return OkObjectResult(ex.Message);
              

            }

              string queueName = "quickstartqueues" + Guid.NewGuid().ToString();
                string storageAccountName = "appstorageaz204ss";
                QueueClient queueClient = new QueueClient(new Uri($"https://{storageAccountName}.queue.core.windows.net/{queueName}"),
                    new DefaultAzureCredential());
                await queueClient.CreateAsync();
                await queueClient.SendMessageAsync("sent by default credential");
            log.LogInformation("Message is sent");
            return new OkObjectResult(responseMessage);
        }
    }
}
