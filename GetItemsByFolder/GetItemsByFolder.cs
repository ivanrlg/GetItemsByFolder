using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using GetItemsByFolder.Models;

namespace GetItemsByFolder
{
    public static class GetItemsByFolder
    {
        [FunctionName("GetItemsByFolder")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            try
            {
                
                ConfigurationsValues configValues = readEnviornmentVariable();         
                GraphServiceClient _graphServiceClient = getGraphClient(configValues);
                List<FileModel> Items = new();

                // List all elements of the Business Centrals Folder
                IDriveItemChildrenCollectionRequest request = _graphServiceClient
                    .Users[configValues.UserID]
                    .Drive
                    .Items[configValues.FolderID]
                    .Children
                    .Request();

                IDriveItemChildrenCollectionPage results = await request.GetAsync();
                foreach (DriveItem file in results)
                {
                    bool IsFile = file.Folder == null;
                    if (IsFile)
                    {
                        byte[] FileArray = await Helper.FileDownloader.DownloadFile(_graphServiceClient, configValues.UserID, file.Id);
                        Items.Add(new FileModel
                        {
                            Id = file.Id,
                            Name = file.Name,
                            Size = file.Size.ToString(),
                            ExtensionType1 = file.File.MimeType.Split('/')[1],
                            ExtensionType2 = Path.GetExtension(file.Name),
                            Folder = !IsFile,
                            FileArray = FileArray
                        });
                    }

                    Console.WriteLine("File Id " + file.Id + "\n" +
                                      "File Name" + file.Name + "\n" +
                                      "File Size" + file.Size + "\n" +
                                      "File Folder" + IsFile + "\n");
                }

                return new OkObjectResult(Items);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new BadRequestObjectResult(ex.Message);
            }
        }

        public static ConfigurationsValues readEnviornmentVariable()
        {
            ConfigurationsValues configValues = new();

            configValues.Tenantid = System.Environment.GetEnvironmentVariable("tenantid", EnvironmentVariableTarget.Process);
            configValues.Clientid = System.Environment.GetEnvironmentVariable("clientid", EnvironmentVariableTarget.Process);
            configValues.ClientSecret = System.Environment.GetEnvironmentVariable("clientsecret", EnvironmentVariableTarget.Process);
            configValues.UserID = System.Environment.GetEnvironmentVariable("UserID", EnvironmentVariableTarget.Process);
            configValues.FolderID = System.Environment.GetEnvironmentVariable("FolderID", EnvironmentVariableTarget.Process);
            return configValues;
        }

        public static GraphServiceClient getGraphClient(ConfigurationsValues configValues)
        {
            IConfidentialClientApplication confidentialClientApplication = ConfidentialClientApplicationBuilder
           .Create(configValues.Clientid)
           .WithTenantId(configValues.Tenantid)
           .WithClientSecret(configValues.ClientSecret)
           .Build();
            ClientCredentialProvider authProvider = new(confidentialClientApplication);
            GraphServiceClient graphClient = new(authProvider);
            return graphClient;
        }
    }
}