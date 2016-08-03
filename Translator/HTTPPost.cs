using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TTSTranslate
{
    class HTTPPost
    {
        WebClient wc = new WebClient();

        public HTTPPost(string Phrase, String FileName, String Voice)
        {
            var request = (HttpWebRequest)WebRequest.Create("http://www.fromtexttospeech.com/");

            var postData = String.Format("input_text={0}&language=US+English&voice={1}&speed=0&action=process_text", Phrase, Voice);
            var data = Encoding.ASCII.GetBytes(postData);

            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = data.Length;
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.103 Safari/537.36";

            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            var response = (HttpWebResponse)request.GetResponse();

            var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

            Regex r = new Regex(@"flashvars\=\'file\=\/output\/([0-9]*\/[0-9]*\.mp3)\'");

            Match m = r.Match(responseString);

            wc.DownloadFile("http://www.fromtexttospeech.com/output/" + m.Groups[1].Value, FileName);

            Console.WriteLine(m.Value);
        }


    }
}
