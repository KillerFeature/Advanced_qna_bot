using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Text;
using System.Configuration;

namespace Advanced_qna_bot
{
    public class qnahelper
    {
        static string QnaUrl = "https://qnademo03.azurewebsites.net/qnamaker";
        static string route = "/knowledgebases/" + ConfigurationSettings.AppSettings["KnowledgeBaseId"] + "/generateAnswer";
        static HttpClient client = new HttpClient();
        public async Task<IMessageActivity> getQnAresults(string input, IDialogContext context)
        {
            var responseMessage = context.MakeMessage();
            var request = new HttpRequestMessage();
                // POST method
                request.Method = HttpMethod.Post;

                // Add host + service to get full URI
                request.RequestUri = new Uri(QnaUrl + route);
                var body = @"{""question"": """ + input + @"""}";

                // set question
                request.Content = new StringContent( body, Encoding.UTF8, "application/json");

                // set authorization
                request.Headers.Add("Authorization", "EndpointKey "+ ConfigurationSettings.AppSettings["QnaApiKey"]);

                // Send request to Azure service, get response
                var response = client.SendAsync(request).Result;
                var jsonResponse = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            
                var textResponse = jsonResponse["answers"][0]["answer"].Value<string>();
                var textSplit = textResponse.Split(new string[] { "*" }, StringSplitOptions.None);



            if (textSplit.Length > 1 & textSplit.Length < 3)
            {
                try
                {
                    var action = JObject.Parse(textSplit.Last<string>());
                    switch (action["t"].Value<string>())
                    {
                        case "c": // Card
                            ThumbnailCard card = action["att"].ToObject<ThumbnailCard>();
//                            JsonConvert.PopulateObject(action["att"].ToString(),card);
                            responseMessage.Attachments.Add(card.ToAttachment());
                            break;
                        case "att":
                            foreach (JObject j in action["att"])
                            {
                                switch (j["contentType"].Value<string>())
                                {
                                    case "application/vnd.microsoft.card.hero":
                                        HeroCard hcard = j.ToObject<HeroCard>();
                                        responseMessage.Attachments.Add(hcard.ToAttachment());
                                        break;
                                    case "application/vnd.microsoft.card.thumbnail":
                                        ThumbnailCard tcard = j.ToObject<ThumbnailCard>();
                                        responseMessage.Attachments.Add(tcard.ToAttachment());
                                        break;
                                    default:

                                        break;
                                }
                            }
                            break;

                        default:
                            break;
                    }
                }
                catch (Exception)
                {
                    throw;         
                }

            }
            responseMessage.Text = textResponse.Split(new string[] { "*" }, StringSplitOptions.None).First<string>();
            return responseMessage;

        }
        public string testStr(string input)
        {

            return "";
        }
    }
    public class QnaResponse
    {
        public string text;
        public dynamic action;

    }
}