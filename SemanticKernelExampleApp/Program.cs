using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Chroma;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel.Plugins.Memory;
using Microsoft.Extensions.Configuration;

var config = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build();

// SETUP AI
var azureEndpoint = config["AzureEndpoint"];
var apiKey = config["AzureApiKey"];
var builder = Kernel.CreateBuilder()
                    .AddAzureOpenAITextEmbeddingGeneration(
                        "example-embedded-deployment-name", azureEndpoint, apiKey)
                    .AddAzureOpenAIChatCompletion(
                        "example-chat-deployment-name", azureEndpoint, apiKey);
var kernel = builder.Build();

// SETUP MEMORY
var memory_builder = new MemoryBuilder()
                        .WithMemoryStore(new VolatileMemoryStore())
                        .WithAzureOpenAITextEmbeddingGeneration(
                            "example-embedded-deployment-name", azureEndpoint, apiKey);

var memory = memory_builder.Build();