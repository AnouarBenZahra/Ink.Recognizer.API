using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Ink.Recognizer.API
{
    public class Request
    {
        const string inkRecognitionUrl = "/inkrecognizer/v1.0-preview/recognize";

        public string GetResponse(string dataPath, string subscriptionKey, string endPoint = "https://api.cognitive.microsoft.com")
        {
            return recognizeInk(dataJson(dataPath).ToString(Newtonsoft.Json.Formatting.None), subscriptionKey, endPoint);
        }

        private async Task<string> requestAsync(string endpointUrl, string recognitionUrl, string subscriptionKey, string requestData)
        {

            using (HttpClient httpClient = new HttpClient { BaseAddress = new Uri(endpointUrl) })
            {
                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);

                var content = new StringContent(requestData, Encoding.UTF8, "application/json");
                var resAsync = await httpClient.PutAsync(recognitionUrl, content);
                if (resAsync.IsSuccessStatusCode)
                    return await resAsync.Content.ReadAsStringAsync();
                else
                    return $"Error: {resAsync.StatusCode}";
            }
        }
        private dynamic recognizeInk(string requestData, string subscriptionKey, string endPoint)
        {

            //construct the request
            var result = requestAsync(
                endPoint,
                inkRecognitionUrl,
                subscriptionKey,
                requestData).Result;

            return Newtonsoft.Json.JsonConvert.DeserializeObject(result);
        }

        private JObject dataJson(string fileLocation)
        {
            var jsonObj = new JObject();

            using (StreamReader file = File.OpenText(fileLocation))
            using (JsonTextReader reader = new JsonTextReader(file))
            {
                jsonObj = (JObject)JToken.ReadFrom(reader);
            }
            return jsonObj;
        }
    }
}
