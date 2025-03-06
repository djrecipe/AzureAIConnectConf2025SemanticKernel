using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Chroma;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel.Plugins.Memory;

// SETUP AI
var azureEndpoint = "https://EXAMPLE.openai.azure.com/";
var apiKey = "abc123";
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