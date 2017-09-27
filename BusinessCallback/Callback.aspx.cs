using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using System.Xml.Linq;

namespace BusinessCallback
{
    /// <summary>
    /// 商务短信回调
    /// </summary>
    public partial class Callback : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            
            if (Request.RequestType.ToUpper() == "POST")
            {
                using (var reader = new StreamReader(Request.InputStream))
                {
                    string xmlData = reader.ReadToEnd();//接收到xml数据
                    try
                    {
                        FileStream fs = new FileStream(@"E:\vs2010express\business_demo\business_demo\bin\Debug\log.txt", FileMode.OpenOrCreate);
                        StreamWriter sw = new StreamWriter(fs);
                        sw.Write(xmlData);
                        sw.Close();
                        fs.Close();
                    }
                    catch (Exception)
                    {
    
                    }
               
                    /////////////解析xml///////////////
                    try
                    {

                        XElement xe = XElement.Parse(xmlData);
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
                                         
                                        }
                                        catch (Exception ex)
                                        {
                                            
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
                                            

                                        }
                                        catch (Exception ex)
                                        {
                                          
                                        }

                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                         
                    }
                }
                 
                Response.Write("100");
            }
            else
            {
                Response.Write("hello,this is callback");
            }
        }
    }//end
}