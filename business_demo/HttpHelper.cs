using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using System.Net;

namespace business_demo
{
   public  class HttpHelper
    {

        /// <summary>
        /// GET提交
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public  string Get(string url)
        {
            string re = "-1";
            try
            {

                WebRequest request = HttpWebRequest.Create(url);//请求对像
                request.Timeout = 20000;
                request.Method = "GET";
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;//响应对像
                Stream stream = response.GetResponseStream();//流对像
                StreamReader reader = new StreamReader(stream, Encoding.GetEncoding("GBK"));//读流对像
                re = reader.ReadToEnd();//读流
                reader.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("SendSms,HttpSms.Submit=>Exception:" + ex.Message + "url=" + url);
            }
            return re;
        }



        public string HttpPost(string Url, string postDataStr)
        {

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);

            request.Method = "POST";

            request.ContentType = "application/x-www-form-urlencoded";

            request.ContentLength = Encoding.UTF8.GetByteCount(postDataStr);

            //request.CookieContainer = cookie;

            Stream myRequestStream = request.GetRequestStream();

            StreamWriter myStreamWriter = new StreamWriter(myRequestStream, Encoding.GetEncoding("gb2312"));

            myStreamWriter.Write(postDataStr);

            myStreamWriter.Close();



            HttpWebResponse response = (HttpWebResponse)request.GetResponse();



            //response.Cookies = cookie.GetCookies(response.ResponseUri);

            Stream myResponseStream = response.GetResponseStream();

            StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));

            string retString = myStreamReader.ReadToEnd();

            myStreamReader.Close();

            myResponseStream.Close();



            return retString;

        }



        /// <summary>
        /// post提交
        /// </summary>
        /// <param name="url"></param>
        /// <param name="postvalues"></param>
        /// <returns></returns>
        public string Post(string url, string postvalues)
        {
            string stringResponse = string.Empty;
            try
            {
                string _method = "post";

                string sUserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; MS Web Services Client Protocol 2.0.50727.4223)";
                string sContentType = "application/x-www-form-urlencoded;charset=UTF-8";
                sContentType = "text/html;charset=UTF-8";
                //string sRequestEncoding = "ascii";
                //string sRequestEncoding = "GBK";

                byte[] data = Encoding.GetEncoding("UTF-8").GetBytes(postvalues);


                WebRequest webRequest = WebRequest.Create(url);
                HttpWebRequest httpRequest = webRequest as HttpWebRequest;
                if (httpRequest == null)
                {
                    return "-1";
                }

              
                if (_method.ToLower() == "post")
                {
                    httpRequest.UserAgent = sUserAgent;
                    httpRequest.ContentType = sContentType;
                    //httpRequest.Accept = "*/*";
                    httpRequest.Method = "POST";
                    httpRequest.ContentLength = data.Length;
                    httpRequest.KeepAlive = false;
                    // httpRequest.Timeout = 2000;
                    httpRequest.ServicePoint.Expect100Continue = false;//关闭Expect:100-Continue握手
                    System.Net.ServicePointManager.DefaultConnectionLimit = 200;
                    Stream requestStream = httpRequest.GetRequestStream();
                    requestStream.Write(data, 0, data.Length);
                     
                    requestStream.Flush();
                    requestStream.Close();

                }
                else
                {
                    httpRequest.Method = _method;
                }
                Stream responseStream;
                try
                {
                    HttpWebResponse response = (HttpWebResponse)httpRequest.GetResponse();
                    Console.WriteLine("status:" + response.StatusCode);
                    responseStream = response.GetResponseStream();
                }
                catch (WebException e)
                {
                    //   throw new ApplicationException(string.Format("POST操作发生异常: {0}", e.Message));
                    return "-1";
                }



                using (StreamReader responseReader = new StreamReader(responseStream, Encoding.GetEncoding("UTF-8")))
                {
                    stringResponse = responseReader.ReadToEnd();
                }

                responseStream.Close();


            }
            catch (Exception ex)
            {
                stringResponse = "-1";
            }
            finally
            {
                GC.Collect();
            }

            return stringResponse;

        }

        /// <summary>
        /// MD5加密
        /// </summary>
        /// <param name="str">要加密的字符串</param>
        /// <returns></returns>
        public string GetMD5(string str)
        {
            MD5 md5 = MD5.Create();
            byte[] data = md5.ComputeHash(Encoding.Default.GetBytes(str));
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sb.Append(data[i].ToString("x2").ToUpper());
            }
            return sb.ToString(); ;
        }


        /// <summary>
        /// 中文编码utf-8
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public string UrlEncoder(string content)
        {
            string re = "";
            if (!string.IsNullOrEmpty(content))
            {
                re = System.Web.HttpUtility.UrlEncode(content, Encoding.GetEncoding("UTF-8"));
            }
            return re;

        }
        /// <summary>
        /// 中文编码gbk
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public string UrlEncoderGBK(string content)
        {
            string re = "";
            if (!string.IsNullOrEmpty(content))
            {
                re = System.Web.HttpUtility.UrlEncode(content, Encoding.GetEncoding("GBK"));
            }
            return re;

        }

    }//end
}//end
