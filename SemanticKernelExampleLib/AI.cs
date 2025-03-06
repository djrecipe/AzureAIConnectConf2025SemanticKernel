using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel.Plugins.Memory;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace SemanticKernelExampleLib.AI
{

    public class AI
    {
        internal static AI Instance { get; private set; } = null;
        private readonly Kernel kernel = null;
        private readonly ISemanticTextMemory memory = null;
        private readonly KernelFunction functionSummarize = null;
        private readonly KernelFunction functionChat = null;
        private string history = "";
        public static void Initialize(Kernel Kernel, ISemanticTextMemory Memory)
        {
            Instance = new AI(Kernel, Memory);
        }
        public AI(Kernel Kernel, ISemanticTextMemory Memory)
        {
            if (Kernel is null)
                throw new ArgumentNullException(nameof(Kernel), "Invalid (null) semantic kernel");
            if (Memory is null)
                throw new ArgumentNullException(nameof(Memory), "Invalid (null) semantic kernel memory");

            kernel = Kernel;
            memory = Memory;

            // initialize summary skill
            string skPrompt = """
            {{$input}}

            
            Summarize the content above.
            """;
            functionSummarize = kernel.CreateFunctionFromPrompt(skPrompt,
                new OpenAIPromptExecutionSettings { MaxTokens = 200, Temperature = 0.8 });

            // initialize chat-with-document skill
            skPrompt = @"
ChatBot can respond with facts or data pulled from user input, history of user input, semantic memory, or text embeddings.
ChatBot should only respond with facts or data that it has been provided via user input, history of user input, semantic memory, or text embeddings.
ChatBot can say 'I don't know' if it does not have an answer.
ChatBot should keep your answer as short and concise as possible.




Result when querying the user's input against text embeddings for the stored document:
{{$embeddingsresult}}




The confidence level of the result of the text embedding query that I just mentioned:
{{$embeddingsconfidence}}




Chat:
{{$history}}
User: {{$userInput}}
ChatBot: ";
            functionChat = kernel.CreateFunctionFromPrompt(skPrompt, new OpenAIPromptExecutionSettings { MaxTokens = 200, Temperature = 0.8 });

            // set-up memory
            kernel.ImportPluginFromObject(new TextMemoryPlugin(memory));

            // set singleton instance
            Instance ??= this;
        }

        public async Task<Guid> Memorize(string Text, IEnumerable<int> pages = null)
        {
            var new_name = Guid.NewGuid();
            Dictionary<int, string> page_texts = new Dictionary<int, string>() { {0, Text } };
            foreach(var pair in page_texts)
            {
                var sanitize = pair.Value.Replace("\n", " ").Replace("\r", " ").Trim();
                if (!string.IsNullOrWhiteSpace(sanitize))
                {
                    await memory.SaveInformationAsync(new_name.ToString(), description: $"Text for page {pair.Key} of the document",
                        text: $"Text for page {pair.Key} of the document is everything that follows:\n\n\n {sanitize} \n\n\n",
                        id: $"page{pair.Key}");
                }
            };
            return new_name;
        }

        public async Task<string> Query(Guid ID, string Query, IEnumerable<int> Indices = null)
        {
            // check text embeddings
            var embeddings_result = memory.SearchAsync(ID.ToString(), Query, limit: 3, minRelevanceScore: 0.4);
            if(embeddings_result.CountAsync().Result == 0)
            {
                return "No information (with relevance 0.6 or higher) was found for your query.";
            }

            var embeddings_result_list = await embeddings_result.ToListAsync();
            var embeddings_result_str = string.Join(",", embeddings_result_list.Select(e => e.Metadata.Text));
            var embeddings_confidence = embeddings_result_list.Select(e => e.Relevance).Max();

            //await foreach (var memory in embeddings_result)
            //{
            //    var embd = memory.Embedding;
            //    Console.WriteLine($"Embedding: {embd.Value}");
            //}

            // query embeddings
            var args = new KernelArguments();
            args["history"] = history;
            args["userInput"] = Query;
            args["embeddingsresult"] = embeddings_result_str;
            args["embeddingsconfidence"] = embeddings_confidence;

            // perform query
            var result = await functionChat.InvokeAsync(this.kernel, args);

            // return result
            return result.ToString();
        }

        public async Task<string> Query(string Query)
        {
            // query embeddings
            var args = new KernelArguments();
            args["history"] = history;
            args["userInput"] = Query;

            // perform query
            var result = await functionChat.InvokeAsync(this.kernel, args);

            // return result
            return result.ToString();
        }

        public async Task<string> Summarize(string Text, IEnumerable<int> pages = null)
        {
            // perform summarization
            var args = new KernelArguments();
            args["input"] = Text;
            var result = await functionSummarize.InvokeAsync(this.kernel, args);

            // return result
            return result.ToString();
        }
    }
}
