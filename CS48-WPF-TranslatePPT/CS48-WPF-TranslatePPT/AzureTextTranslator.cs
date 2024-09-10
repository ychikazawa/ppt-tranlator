using Azure.AI.Translation.Text;
using Azure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CS48_WPF_TranslatePPT
{
    internal class AzureTextTranslator
    {
        public static dynamic TranslateText(string subscriptionKey, string region, string text, string fromLanguage, string toLanguage)
        {
            string endpoint = "https://api.cognitive.microsofttranslator.com/";
            var client = new TextTranslationClient(new AzureKeyCredential(subscriptionKey), new Uri(endpoint), region);
            var inputText = new List<string> { text };
            var response = client.Translate(toLanguage, inputText, fromLanguage);

            return response.Value.FirstOrDefault().Translations.FirstOrDefault().Text;
        }
    }
}
