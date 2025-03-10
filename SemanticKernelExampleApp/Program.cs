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

// SETUP EXAMPLE CONTENT
var text = File.ReadAllText("Input.txt");

// SETUP AI
var azureEndpoint = config["AzureEndpoint"];
var apiKey = config["AzureApiKey"];
var chat_deployment = config["AzureChatDeployment"];
var embedding_deployment = config["AzureEmbeddingDeployment"];
var text2image_deployment = config["AzureTextToImageDeployment"];
var builder = Kernel.CreateBuilder()
                    .AddAzureOpenAITextEmbeddingGeneration(
                        embedding_deployment, azureEndpoint, apiKey)
                    .AddAzureOpenAIChatCompletion(
                        chat_deployment, azureEndpoint, apiKey)
                    .AddAzureOpenAITextToImage(text2image_deployment, azureEndpoint, apiKey);
var kernel = builder.Build();

// SETUP MEMORY
var memory_builder = new MemoryBuilder()
                        .WithMemoryStore(new VolatileMemoryStore())
                        .WithAzureOpenAITextEmbeddingGeneration(
                            embedding_deployment, azureEndpoint, apiKey);

var memory = memory_builder.Build();

// SETUP AI
var ai = new AI(kernel, memory);

// USE AI
//var summary = await ai.Summarize(text);
//Console.WriteLine($"Summary: {summary}");
//var id = await ai.Memorize(text);
//var result = await ai.Query(id, "What does Shelly sell?");
//Console.WriteLine($"Query: {result}");

// GENERATE TEXT TO IMAGE
var image = await ai.GenerateImage("an island");
File.WriteAllBytes("foo.bmp", image);