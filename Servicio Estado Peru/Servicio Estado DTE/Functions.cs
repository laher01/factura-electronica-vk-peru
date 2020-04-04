using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Newtonsoft.Json;
using SAPbobsCOM;

namespace Servicio_Estado_DTE.Functions
{
    public class TFunctions
    {
        
        //Funcion registra log
        public void AddLog(String Mensaje)
        {
            StreamWriter Arch;
            //Exe: String := 
            String sPath = Path.GetDirectoryName(this.GetType().Assembly.Location);
            String NomArch;
            String NomArchB;
            NomArch = "\\VDLog_" + String.Format("{0:yyyy-MM-dd}", DateTime.Now) + ".log";
            Arch = new StreamWriter(sPath + NomArch, true);
            NomArchB = sPath + "\\VDLog_" + String.Format("{0:yyyy-MM-dd}", DateTime.Now.AddDays(-1)) + ".log";
            //Elimina archivo del dia anterior
            //if (System.IO.File.Exists(NomArchB))
            //    System.IO.File.Delete(NomArchB);

            try
            {
                Arch.WriteLine(String.Format("{0:dd-MM-yyyy HH:mm:ss}", DateTime.Now) + " " + Mensaje);
            }
            finally
            {
                Arch.Flush();
                Arch.Close();
            }
        }

        public String DatosConfig(String Valor0, String Valor, XmlDocument xDoc)
        {
            XmlNodeList Configuracion;
            XmlNodeList lista;
            TFunctions Func;
            String _result = "";

            try
            {
                Configuracion = xDoc.GetElementsByTagName("Configuracion");
                lista = ((XmlElement)Configuracion[0]).GetElementsByTagName(Valor0);

                foreach (XmlElement nodo in lista)
                {
                    var nArchivos = nodo.GetElementsByTagName(Valor);
                    _result = (String)(nArchivos[0].InnerText);
                }

                return _result;
            }
            catch (Exception w)
            {
                Func = new TFunctions();
                Func.AddLog("DatosConfig: " + w.Message + " ** Trace: " + w.StackTrace);
                return "";
            }
        }

        public String UpLoadDocumentByUrl(XmlDocument xmlDOC, String json, String URL, String user, String pass)
        {
            string postData;
            //string url = “http://portalPE.asydoc.cl/SendDocument.ashx";
            try
            {
                WebRequest request = WebRequest.Create(URL);
                //**request.Credentials = new NetworkCredential(user, pass);
                request.Method = "POST";
                if (json == null)
                    postData = xmlDOC.InnerXml;
                else
                    postData = json;
                byte[] byteArray = Encoding.UTF8.GetBytes(postData);
                if (json == null)
                    request.ContentType = "text/xml";
                else
                    request.ContentType = "text/plain";
                request.ContentLength = byteArray.Length;
                Stream dataStream = request.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();
                WebResponse response = request.GetResponse();
                Console.WriteLine(((HttpWebResponse)(response)).StatusDescription);
                dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                string responseFromServer = reader.ReadToEnd();
                reader.Close();
                dataStream.Close();
                response.Close();

                return responseFromServer;
            }
            catch (Exception ex)
            {
                AddLog("Error UpLoadDocumentByUrl " + ex.Message);
                return "Error " + ex.Message;
            }
        }

        public String Encriptar(String _cadenaAencriptar)
        {
            System.String sresult;
            System.Byte[] encryted;

            sresult = System.String.Empty;
            encryted = System.Text.Encoding.Unicode.GetBytes(_cadenaAencriptar);
            sresult = Convert.ToBase64String(encryted);
            return sresult;
        }//fin Encriptar


        public String DesEncriptar(String _cadenaAdesencriptar)
        {
            String sresult;
            System.Byte[] decryted;

            sresult = System.String.Empty;
            decryted = Convert.FromBase64String(_cadenaAdesencriptar);
            //result = System       .Text.Encoding.Unicode.GetString(decryted, 0, decryted.ToArray().Length);
            sresult = System.Text.Encoding.Unicode.GetString(decryted);
            return sresult;
        }
        
    }
}
