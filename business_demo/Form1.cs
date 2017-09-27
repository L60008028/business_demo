using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Linq;
namespace business_demo
{
    public partial class FmMain : Form
    {
        HttpServer httpListen = null;
        public FmMain()
        {


            InitializeComponent();
        }
        public string ApiUrl = "http://client.sms10000.com/api/webservice";
        HttpHelper http = new HttpHelper();
        private void btnSubmit_Click(object sender, EventArgs e)
        {
            string content = this.tbCon.Text.Trim();
            string mobiles = this.tbMobiles.Text.Trim();
            string eprId = this.tbEprId.Text.Trim();
            string userId = this.tbUserId.Text.Trim();
            string pwd = this.tbPwd.Text.Trim();
            content = http.UrlEncoder(content);

            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            long timestamp = Convert.ToInt64(ts.TotalMilliseconds);
            Random r = new Random();
            int msgid = r.Next();
            string format = "1";
            string key = eprId + userId + pwd + timestamp;
            key = http.GetMD5(key);
            string data = "cmd=send&eprId=" + eprId + "&userId=" + userId + "&key=" + key + "&timestamp=" + timestamp + "&format=" + format
             + "&mobile=" + mobiles + "&msgId=" + msgid + "&content=" + content;
            string re = http.Post(ApiUrl, data);
            WriteLog(re);
            Console.WriteLine("提交结果[" + msgid + "]：result=" + re);
            DateTime now = DateTime.Now;


        }

        public void WriteLog(string txt)
        {
            if (this.rtbLog.InvokeRequired)
            {
                this.rtbLog.Invoke(new Action<string>(x => WriteLog(x)), new object[] { txt });
            }
            else
            {
                if (rtbLog.TextLength == rtbLog.MaxLength)
                {
                    rtbLog.Clear();
                }
                this.rtbLog.AppendText(txt);
                this.rtbLog.AppendText("\n\r");
                this.rtbLog.ScrollToCaret();
            }
        }


        private void DataProcess(string data)
        {
            Console.WriteLine("DataProcess::" + data);
            try
            {
                XElement xe = XElement.Parse(data);
                if (xe != null && xe.Name.LocalName.Equals("returnForm"))
                {
                    var typeem = xe.Element("type");
                    if (typeem != null)
                    {
                        if (typeem.Value.Equals("1"))
                        {
                            var statusLst = from em in xe.Element("list").Elements("pushStatusForm") select em;
                            foreach (var statu in statusLst)
                            {
                                try
                                {
                                    var eprId = statu.Element("eprId").Value;
                                    var mobile = statu.Element("mobile").Value;
                                    var msgid = statu.Element("msgId").Value;
                                    var status = statu.Element("status").Value;
                                    var userId = statu.Element("userId").Value;
                                    string msg = string.Format("接收到状态回执：eprId={0},mobile={1},msgId={2},status={3},userId={4}", eprId, mobile, msgid, status, userId);
                                    this.WriteLog(msg);
                                }
                                catch (Exception ex)
                                {
                                    this.WriteLog("解析状态报告异常: " + ex.Message);
                                }
                            }
                        }
                        else if (typeem.Value.Equals("2"))
                        {
                            var smsLst = from em in xe.Element("list").Elements("pushSmsForm") select em;
                            foreach (var sms in smsLst)
                            {
                                try
                                {
                                    var eprId = sms.Element("eprId").Value;
                                    var mobile = sms.Element("mobile").Value;
                                    var msgid = sms.Element("msgId").Value;
                                    var content = sms.Element("content").Value;
                                    var userId = sms.Element("userId").Value;
                                    string msg = string.Format("接收到上行：eprId={0},mobile={1},msgId={2},content={3},userId={4}", eprId, mobile, msgid, content, userId);
                                    this.WriteLog(msg);

                                }
                                catch (Exception ex)
                                {
                                    this.WriteLog("解析上行异常: " + ex.Message);
                                }

                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this.WriteLog("异常: " + ex.Message);
            }
        }


        private void btnStart_Click(object sender, EventArgs e)
        {

            if (!string.IsNullOrEmpty(this.tbPort.Text.Trim()))
            {
                if (httpListen == null)
                {
                    httpListen = new HttpServer( int.Parse(this.tbPort.Text.Trim()));
                    httpListen.DateProcessEvent -= DataProcess;
                    httpListen.WriteMsgEvent -= WriteLog;
                    httpListen.DateProcessEvent += DataProcess;
                    httpListen.WriteMsgEvent += WriteLog;
                }
                else
                {
                    httpListen.StopListen();
                }
                httpListen.StartListen();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string content = "【比亚迪】[8月31日提车完成率简报]2015年第三季度(7.1-9.30)共计提车26923台,总完成率达37.18%,东北 49.84%,华南 44.44%,四川 39.78%,西南 39.00%,西北 37.01%,华东 35.87%,华中 32.30%,山东 30.69%,华北 30.39%.【比亚迪】";
           content = "3%";
           string data = "uid=71&auth=&mobile=13682488577&msg=" + http.UrlEncoderGBK(content)+ "&expid=8";
           data = "uid=71&auth=&mobile=13682488577&msg=【比亚迪】[8月31日提车完成率简报]2015年第三季度(7.1-9.30)共计提车26923台,总完成率达37.18%&expid=8&encode=utf-8";
          Console.WriteLine("提交结果data" + data);
          //string re = http.HttpPost("http://sms.10690221.com:9011/hy/", data);
          string re = http.Post("http://sms.10690221.com:9011/hy/", data);
         // string geturl = "http://sms.10690221.com:9011/hy/?" + data;
        // string re1 = http.Get(geturl);
         // Console.WriteLine("提交结果 ：result=" + re1);
        }





    }//end
}//end
