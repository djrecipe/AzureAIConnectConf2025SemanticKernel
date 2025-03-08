using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Chroma;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel.Plugins.Memory;
using Microsoft.Extensions.Configuration;
using SemanticKernelExampleLib.AI;

var config = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build();

// SETUP AI
var azureEndpoint = config["AzureEndpoint"];
var apiKey = config["AzureApiKey"];
var chat_deployment = config["AzureChatDeployment"];
var embedding_deployment = config["AzureEmbeddingDeployment"];
var builder = Kernel.CreateBuilder()
                    .AddAzureOpenAITextEmbeddingGeneration(
                        embedding_deployment, azureEndpoint, apiKey)
                    .AddAzureOpenAIChatCompletion(
                        chat_deployment, azureEndpoint, apiKey);
var kernel = builder.Build();

// SETUP MEMORY
var memory_builder = new MemoryBuilder()
                        .WithMemoryStore(new VolatileMemoryStore())
                        .WithAzureOpenAITextEmbeddingGeneration(
                            embedding_deployment, azureEndpoint, apiKey);

var memory = memory_builder.Build();

// SETUP AI

AI.Initialize(kernel, memory);