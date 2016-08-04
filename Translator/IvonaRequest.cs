using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.Auth;
using System.Net;
using System.Security.Cryptography;
using Amazon.Runtime;

namespace TTSTranslate
{
    class IvonaRequest
    {
        public IvonaRequest()
        {
            /*
            Voice.Name:Nicole
            Input.Type:text/plain
            OutputFormat.Codec:MP3
            Voice.Language:en-AU
            Input.Data:Hello there, my name is Nicole. I am one of the IVONA voices. I will read anything you want. Enter any text here and click Play.
            OutputFormat.SampleRate:22050
            X-Amz-Algorithm:AWS4-HMAC-SHA256
            X-Amz-Date:20160804T024212Z
            X-Amz-SignedHeaders:host
            X-Amz-Expires:300
            X-Amz-Credential:GDNAI2EMTPXZH7ONBHZQ/20160804/global/tts/aws4_request 
            */
            BasicAWSCredentials c = new BasicAWSCredentials("GDNAIZ3FOT27P5YJ4UQA", "zfSCPa3rTjlK0hB0AeUB5w7Gbpo9LLhZkGF8tVTE");

            SHA256 SHA256Hash = SHA256.Create();

            var request = (HttpWebRequest)WebRequest.Create("https://tts.us-east-1.ivonacloud.com/CreateSpeech");

            var postData = @"{""Input"":{""Data"":""Hello world""}}";
            var data = Encoding.ASCII.GetBytes(postData);


            request.Method = "POST";
            request.ContentType = "application/json";
            request.Host = "tts.us-east-1.ivonacloud.com";
            request.Headers.Add("X-Amz-Date", DateTime.Now.ToString("YYYYMMDD'T'HHMMSS'Z'"));
            request.ContentLength = data.Length;
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.103 Safari/537.36";

            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            Console.WriteLine (request.ToString());
            //var response = (HttpWebResponse)request.GetResponse();

            //var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

        }
    }
}