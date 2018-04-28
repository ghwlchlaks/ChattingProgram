using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Choi_01
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading.Tasks;
    using System.Xml;


    namespace weatherApi
    {
        class weather
        {  
             public weather()
            { }
            public WeatherData Reading(string strCity)
            {
                try
                {
                    //http://api.openweathermap.org/data/2.5/forecast/daily?q=London&mode=xml&units=metric&cnt=7&appid=MYAPIKEYHERE
                    //"http://api.openweathermap.org/data/2.5/forecast/city?id=524901&APPID=92af1608b1025587e5f70b2f5f2a0870"
                    //http://api.openweathermap.org/data/2.5/weather?id=yourCityId&units=metric&APPID=yourAppidHere
                    WebClient wc = new WebClient();
                    //wc.Headers.Add("Default",
                    //    @"http://api.openweathermap.org/data/2.5/weather?id=524901&mode=xml&units=metric&APPID=92af1608b1025587e5f70b2f5f2a0870");
                    string buffer = wc.DownloadString("http://api.openweathermap.org/data/2.5/weather?q=" + strCity + "&mode=xml&units=metric&APPID=92af1608b1025587e5f70b2f5f2a0870");
                    //"http://api.openweathermap.org/data/2.5/weather?id=524901&mode=xml&units=metric&APPID=92af1608b1025587e5f70b2f5f2a0870"
                    wc.Dispose();

                    StringReader sr = new StringReader(buffer);
                    XmlDocument doc = new XmlDocument();
                    doc.Load(sr);
                    sr.Close();

                    XmlNode currentNode = doc.SelectSingleNode("current");

                    if (currentNode != null)
                    {
                        //string city = GetNodeValue(currentNode, "city");
                        string temp = GetNodeValue(currentNode, "temperature");                                    //화씨
                        string humdity = GetNodeValue(currentNode, "humidity");                                    //섭씨
                        string wind = GetNodeValue(currentNode.SelectSingleNode("wind"), "speed");                                //습도
                        string clouds = GetNodeValue(currentNode, "weather");
                        XmlAttribute attr = currentNode.SelectSingleNode("weather").Attributes["icon"];
                        string url;
                        if (attr != null)
                            url = attr.Value;
                        else
                            url = null;
                        return new WeatherData((Bitmap)LoadIcon(url),
                            strCity, temp, humdity, wind, clouds);
                    }
                    else
                        return null;
                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show(ex.ToString());
                    return null;
                }
            }

            private object LoadIcon(string url)
            {
                try
                {
                    if (string.IsNullOrEmpty(url))
                        return null;

                    WebClient wc = new WebClient();
                    url = "http://openweathermap.org/img/w/" + url + ".png";
                    Byte[] buffer = wc.DownloadData(url);
                    wc.Dispose();
                    MemoryStream ms = new MemoryStream(buffer);
                    Bitmap bmp = new Bitmap(ms);
                    ms.Dispose();
                    return bmp;
                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show(ex.ToString());
                    return null;
                }
            }

            private string GetNodeValue(XmlNode currentNode, string name)
            {
                try
                {
                    XmlAttribute attr = currentNode.SelectSingleNode(name).Attributes["value"];
                    if (attr != null)
                        return attr.Value;
                    return null;
                }
                catch
                {
                    return null;
                }

            }
        }

        class WeatherData
        {
            public string clouds;
            public string humdity;
            public string strCity;
            public string temp;
            public Bitmap icon;
            public string wind;

            public WeatherData(Bitmap icon, string strCity, string temp, string humdity, string wind, string clouds)
            {
                this.icon = icon;
                this.strCity = strCity;
                this.temp = temp;
                this.humdity = humdity;
                this.wind = wind;
                this.clouds = clouds;
            }
            public override string ToString()
            {
                return strCity + "날씨 정보..\n 온도: " + temp + "\n 습도:" + humdity + "\n 바람:" + wind + "\n 구름: " + clouds + "\n";
            }
        }
    }






}
