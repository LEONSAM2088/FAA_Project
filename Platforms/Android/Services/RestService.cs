using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using GoogleGson;

namespace FAA_Project.Platforms.Android.Services
{
    public class RestService
    {
        HttpClient _client;
        JsonSerializerOptions _serializerOptions;

        public byte[] Items { get; private set; }

        public RestService()
        {
            _client = new HttpClient();
            _serializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
        }

        public async Task<string> SaveVideoAsync(byte[] item, bool isNewItem = false)
        {
           
            Uri uri = new Uri(string.Format(Constants.RestUrl));
            var asd = new Video();
            asd.file = item;
            asd.video_name = "kratos.mp4";
            try
            {
                using (var multipartFormContent = new MultipartFormDataContent())
                {
                    var byteArrayContent = new ByteArrayContent(asd.file);
                    var stringContent = new StringContent(asd.video_name);

                    //Add the file
                    multipartFormContent.Add(byteArrayContent, name: "file", fileName: "kratos.mp4");
                    multipartFormContent.Add(stringContent, name: "video_name");
                    //Send it
                    var response = await _client.PostAsync(uri, multipartFormContent);
                    response.EnsureSuccessStatusCode();
                    var jsonString = await response.Content.ReadAsStringAsync();

                    JObject parsedJson = JObject.Parse(jsonString);
                    string mainEmotion = (string)parsedJson["main_emotion"];
                    
                    return mainEmotion;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(@"\tERROR {0}", ex.Message);
                return "Перезапусти приложение, по-братски";
            }
            
        }

        public async Task Test()
        {
            
            try
            {
               

                HttpResponseMessage response = null;
              
                    response = await _client.GetAsync("https://e2f7-93-100-197-241.eu.ngrok.io/video/name/9bZkp7q19f0");

                if (response.IsSuccessStatusCode)
                    Debug.WriteLine(@"\tTodoItem successfully saved.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(@"\tERROR {0}", ex.Message);
            }
        }
    }
}
