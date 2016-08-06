using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Web.Script.Serialization;


namespace TTSTranslate
{
    partial class IvonaRequest
    {


        public IvonaRequest()
        {
            //try
            //{
            //    byte[] voice = IvonaCreateSpeech("Mary has a little lamb. Alice has a black cat.");
            //    var path = Guid.NewGuid().ToString("N") + ".wav";
            //    File.WriteAllBytes(path, voice);
            //    Console.WriteLine("Voice has been successfully created at {0}{1}", AppDomain.CurrentDomain.BaseDirectory, path);
            //}
            //catch (WebException ex)
            //{
            //    Console.WriteLine(ex.Message);
            //    foreach (string header in ex.Response.Headers)
            //    {
            //        Console.WriteLine("{0}: {1}", header, ex.Response.Headers[header]);
            //    }
            //    using (var responseStream = ex.Response.GetResponseStream())
            //    {
            //        if (responseStream != null)
            //        {
            //            using (var streamReader = new StreamReader(responseStream))
            //            {
            //                Console.WriteLine(streamReader.ReadToEnd());
            //            }
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine(ex.Message);
            //}
        }

        public static byte[] IvonaCreateSpeech(string text, Voice selectedVoice)
        {
            var date = DateTime.UtcNow;

            const string algorithm = "AWS4-HMAC-SHA256";
            const string regionName = "eu-west-1";
            const string serviceName = "tts";
            const string method = "POST";
            const string canonicalUri = "/CreateSpeech";
            const string canonicalQueryString = "";
            const string contentType = "application/json";


            const string host = serviceName + "." + regionName + ".ivonacloud.com";

            var obj = new
            {
                Input = new
                {
                    Data = text,
                    Type = "text/plain"
                },
                OutputFormat = new
                {
                    Codec = "MP3",
                    SampleRate = 22050
                },
                Parameters = new
                {
                    Rate = "medium",
                    Volume = "medium",
                    SentenceBreak = 500,
                    ParagraphBreak = 800
                },
                Voice = new
                {
                    Name = selectedVoice.Name,
                    Language = selectedVoice.Language,
                    Gender = selectedVoice.Gender
                }
            };
            var requestPayload = new JavaScriptSerializer().Serialize(obj);

            var hashedRequestPayload = HexEncode(Hash(ToBytes(requestPayload)));

            var dateStamp = date.ToString("yyyyMMdd");
            var requestDate = date.ToString("yyyyMMddTHHmmss") + "Z";
            var credentialScope = string.Format("{0}/{1}/{2}/aws4_request", dateStamp, regionName, serviceName);

            var headers = new SortedDictionary<string, string>
            {
                {"content-type", contentType},
                {"host", host},
                {"x-amz-date", requestDate}
            };

            string canonicalHeaders =
                string.Join("\n", headers.Select(x => x.Key.ToLowerInvariant() + ":" + x.Value.Trim())) + "\n";
            const string signedHeaders = "content-type;host;x-amz-date";

            // Task 1: Create a Canonical Request For Signature Version 4

            var canonicalRequest = method + '\n' + canonicalUri + '\n' + canonicalQueryString +
                                   '\n' + canonicalHeaders + '\n' + signedHeaders + '\n' + hashedRequestPayload;

            var hashedCanonicalRequest = HexEncode(Hash(ToBytes(canonicalRequest)));


            // Task 2: Create a String to Sign for Signature Version 4
            // StringToSign  = Algorithm + '\n' + RequestDate + '\n' + CredentialScope + '\n' + HashedCanonicalRequest

            var stringToSign = string.Format("{0}\n{1}\n{2}\n{3}", algorithm, requestDate, credentialScope,
                hashedCanonicalRequest);


            // Task 3: Calculate the AWS Signature Version 4

            // HMAC(HMAC(HMAC(HMAC("AWS4" + kSecret,"20130913"),"eu-west-1"),"tts"),"aws4_request")
            byte[] signingKey = GetSignatureKey(SecretKey, dateStamp, regionName, serviceName);

            // signature = HexEncode(HMAC(derived-signing-key, string-to-sign))
            var signature = HexEncode(HmacSha256(stringToSign, signingKey));


            // Task 4: Prepare a signed request
            // Authorization: algorithm Credential=access key ID/credential scope, SignedHeadaers=SignedHeaders, Signature=signature

            var authorization =
                string.Format("{0} Credential={1}/{2}/{3}/{4}/aws4_request, SignedHeaders={5}, Signature={6}",
                    algorithm, AccessKey, dateStamp, regionName, serviceName, signedHeaders, signature);

            // Send the request

            var webRequest = WebRequest.Create("https://" + host + canonicalUri);

            webRequest.Method = method;
            webRequest.Timeout = 20000;
            webRequest.ContentType = contentType;
            webRequest.Headers.Add("X-Amz-date", requestDate);
            webRequest.Headers.Add("Authorization", authorization);
            webRequest.Headers.Add("x-amz-content-sha256", hashedRequestPayload);
            webRequest.ContentLength = requestPayload.Length;

            using (Stream newStream = webRequest.GetRequestStream())
            {
                newStream.Write(ToBytes(requestPayload), 0, requestPayload.Length);
                newStream.Flush();
            }

            var response = (HttpWebResponse)webRequest.GetResponse();

            using (Stream responseStream = response.GetResponseStream())
            {
                if (responseStream != null)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        responseStream.CopyTo(memoryStream);
                        return memoryStream.ToArray();
                    }
                }
            }

            return new byte[0];
        }

        public static SupportedVoices IvonaListVoices()
        {
            var date = DateTime.UtcNow;

            const string algorithm = "AWS4-HMAC-SHA256";
            const string regionName = "eu-west-1";
            const string serviceName = "tts";
            const string method = "POST";
            const string canonicalUri = "/ListVoices";
            const string canonicalQueryString = "";
            const string contentType = "application/json";


            const string host = serviceName + "." + regionName + ".ivonacloud.com";

            var obj = new
            {
                Voice = new
                {
                }
            };
            var requestPayload = new JavaScriptSerializer().Serialize(obj);

            var hashedRequestPayload = HexEncode(Hash(ToBytes(requestPayload)));

            var dateStamp = date.ToString("yyyyMMdd");
            var requestDate = date.ToString("yyyyMMddTHHmmss") + "Z";
            var credentialScope = string.Format("{0}/{1}/{2}/aws4_request", dateStamp, regionName, serviceName);

            var headers = new SortedDictionary<string, string>
            {
                {"content-type", contentType},
                {"host", host},
                {"x-amz-date", requestDate}
            };

            string canonicalHeaders =
                string.Join("\n", headers.Select(x => x.Key.ToLowerInvariant() + ":" + x.Value.Trim())) + "\n";
            const string signedHeaders = "content-type;host;x-amz-date";

            // Task 1: Create a Canonical Request For Signature Version 4

            var canonicalRequest = method + '\n' + canonicalUri + '\n' + canonicalQueryString +
                                   '\n' + canonicalHeaders + '\n' + signedHeaders + '\n' + hashedRequestPayload;

            var hashedCanonicalRequest = HexEncode(Hash(ToBytes(canonicalRequest)));


            // Task 2: Create a String to Sign for Signature Version 4
            // StringToSign  = Algorithm + '\n' + RequestDate + '\n' + CredentialScope + '\n' + HashedCanonicalRequest

            var stringToSign = string.Format("{0}\n{1}\n{2}\n{3}", algorithm, requestDate, credentialScope,
                hashedCanonicalRequest);


            // Task 3: Calculate the AWS Signature Version 4

            // HMAC(HMAC(HMAC(HMAC("AWS4" + kSecret,"20130913"),"eu-west-1"),"tts"),"aws4_request")
            byte[] signingKey = GetSignatureKey(SecretKey, dateStamp, regionName, serviceName);

            // signature = HexEncode(HMAC(derived-signing-key, string-to-sign))
            var signature = HexEncode(HmacSha256(stringToSign, signingKey));


            // Task 4: Prepare a signed request
            // Authorization: algorithm Credential=access key ID/credential scope, SignedHeadaers=SignedHeaders, Signature=signature

            var authorization =
                string.Format("{0} Credential={1}/{2}/{3}/{4}/aws4_request, SignedHeaders={5}, Signature={6}",
                    algorithm, AccessKey, dateStamp, regionName, serviceName, signedHeaders, signature);

            // Send the request

            var webRequest = WebRequest.Create("https://" + host + canonicalUri);

            webRequest.Method = method;
            webRequest.Timeout = 20000;
            webRequest.ContentType = contentType;
            webRequest.Headers.Add("X-Amz-date", requestDate);
            webRequest.Headers.Add("Authorization", authorization);
            webRequest.Headers.Add("x-amz-content-sha256", hashedRequestPayload);
            webRequest.ContentLength = requestPayload.Length;

            using (Stream newStream = webRequest.GetRequestStream())
            {
                newStream.Write(ToBytes(requestPayload), 0, requestPayload.Length);
                newStream.Flush();
            }

            var response = (HttpWebResponse)webRequest.GetResponse();

            using (Stream responseStream = response.GetResponseStream())
            {
                if (responseStream != null)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        //responseStream.CopyTo(memoryStream);
                        String responseString = new StreamReader(responseStream).ReadToEnd();

                        return new JavaScriptSerializer().Deserialize<SupportedVoices>(responseString);
                    }
                }
            }

            return new SupportedVoices();// new List<Voice>());
        }

        private static byte[] GetSignatureKey(String key, String dateStamp, String regionName, String serviceName)
        {
            byte[] kDate = HmacSha256(dateStamp, ToBytes("AWS4" + key));
            byte[] kRegion = HmacSha256(regionName, kDate);
            byte[] kService = HmacSha256(serviceName, kRegion);
            return HmacSha256("aws4_request", kService);
        }

        private static byte[] ToBytes(string str)
        {
            return Encoding.UTF8.GetBytes(str.ToCharArray());
        }

        private static string HexEncode(byte[] bytes)
        {
            return BitConverter.ToString(bytes).Replace("-", string.Empty).ToLowerInvariant();
        }

        private static byte[] Hash(byte[] bytes)
        {
            return SHA256.Create().ComputeHash(bytes);
        }

        private static byte[] HmacSha256(String data, byte[] key)
        {
            return new HMACSHA256(key).ComputeHash(ToBytes(data));
        }
    }

    public class SupportedVoices
    {
        public List<Voice> Voices { get; set; }

        //public Voices(List<Voice> voices)
        //{
        //    SupportedVoices = voices;

        //}
    }

    public class Voice
    {
        public String Name { get; set; }
        public String Language { get; set; }
        public String Gender { get; set; }

        public override string ToString()
        {
            return String.Format("{0}, {1}, {2}", Name, Language, Gender);
        }
    }
}
