using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.IO;
using System.Timers;
using SAPbobsCOM;
using System.Globalization;
using System.Net;
using System.Xml;
using System.Xml.Linq;
using ServiceStack.Text;
using System.Net.Http;
using System.Net.Mail;
//using System.Core;
using Microsoft.CSharp;
using Servicio_Estado_DTE.Functions;
using System.Net.NetworkInformation;
using System.Data.Sql;
using System.Data.SqlClient;
using Newtonsoft.Json;


namespace Servicio_Estado_DTE
{
    public partial class Service1 : ServiceBase
    {
        public Timer Tiempo;
        public String s;
        public SAPbobsCOM.CompanyClass oCompany;
        public SAPbobsCOM.Recordset oRecordSet = null;
        public CultureInfo _nf = new System.Globalization.CultureInfo("en-US");
        public TFunctions Func;
        public String sVersion = "1.0";
        public String TaxIdNum;
        public Boolean RunningSQLServer = false;
        public String OPGetEstado = "";
        public String Glob_Servidor;
        public String Glob_Licencia;
        public String Glob_UserSAP;
        public String Glob_PassSAP;
        public String Glob_SQL;
        public String Glob_UserSQL;
        public String Glob_PassSQL;
        public XmlDocument xDoc;
        public XmlNodeList Configuracion;
        public XmlNodeList lista;

        public Service1()
        {
            InitializeComponent();
            Tiempo = new Timer();
            Tiempo.Interval = 30000;
            Tiempo.Elapsed += new ElapsedEventHandler(tiempo_elapsed);
        }

        protected override void OnStart(string[] args)
        {
            Tiempo.Enabled = true;
        }

        protected override void OnStop()
        {
            Tiempo.Stop();
            Tiempo.Enabled = false;
        }

        public void tiempo_elapsed(object sender, EventArgs e)
        {
            Func = new TFunctions();
            Tiempo.Enabled = false;
            XmlNodeList BasesSAP;
            XmlNodeList BaseSAP;
            String sPath = Path.GetDirectoryName(this.GetType().Assembly.Location);
            String BaseName;
            String UserWS = "";
            String PassWS = "";
            String CompnyName = "";
            Boolean bEnviarMail;


            try
            {
                string contenido = String.Empty;
                contenido = File.ReadAllText(sPath + "\\Config.txt");
                contenido = Func.DesEncriptar(contenido);
                xDoc = new XmlDocument();
                xDoc.LoadXml(contenido);
                s = Func.DatosConfig("SistemaSAP", "SAP", xDoc);
                if (s == "")
                    throw new Exception("Debe parametrizar si SAP es SQL o HANA en xml de Configuración, en tag SistemaSAP -> SAP (SQL o HANA)");
                else if (s == "HANA")
                    RunningSQLServer = false;
                else
                    RunningSQLServer = true;

                //Consulta si envia Mail o no
                s = Func.DatosConfig("Mail", "EnviarMail", xDoc);
                if (s.Trim() == "Si")
                    bEnviarMail = true;
                else
                    bEnviarMail = false;

                //Consultar estado DTE enviado por el addon
                s = Func.DatosConfig("EasyDoc", "ProcesaGetEstado", xDoc).Replace("'", "");
                //Func.AddLog("Procesa GetEstado " + s);
                if (s.Trim() == "Si")
                {
                    s = Func.DatosConfig("EasyDoc", "OPGetEstado", xDoc).Replace("'", "");
                    if (s == "")
                        throw new Exception("Debe ingresar URL en xml de Configuración, en tag EasyDoc -> OPGetEstado");
                    else
                        OPGetEstado = s;
                }
                else
                    OPGetEstado = "";;

                try
                {
                    Func.AddLog("Inicio");
                    //Func.AddLog("Inicio1");

                    if (DatosConexion(xDoc))
                    {
                        BasesSAP = xDoc.GetElementsByTagName("BasesSAP");
                        BaseSAP = ((XmlElement)BasesSAP[0]).GetElementsByTagName("BaseSAP");
                        foreach (XmlElement nodo in BaseSAP)
                        {
                            try
                            {
                                BaseName = nodo.InnerText;
                                BaseName = BaseName.Trim();
                                if (oCompany == null)
                                    oCompany = new SAPbobsCOM.CompanyClass();
                                else
                                {
                                    oCompany.Disconnect();
                                    oCompany = null;
                                    oCompany = new SAPbobsCOM.CompanyClass();
                                }

                                if (ConectarBaseSAP(BaseName.Trim()))
                                {
                                    Func.AddLog("Conectado a SAP -> Base Datos " + BaseName.Trim());
                                    if (oRecordSet == null)
                                        oRecordSet = (SAPbobsCOM.Recordset)(oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset));
                                    //Func.AddLog("Inicio2");
                                    if (RunningSQLServer)
                                        s = @"SELECT ISNULL(TaxIdNum,'') TaxIdNum, CompnyName FROM OADM ";
                                    else
                                        s = @"SELECT IFNULL(""TaxIdNum"",'') ""TaxIdNum"", ""CompnyName"" FROM ""OADM"" ";
                                    oRecordSet.DoQuery(s);
                                    //Func.AddLog(s);
                                    if (oRecordSet.RecordCount == 0)
                                        throw new Exception("Debe ingresar RUC de Emisor, Gestión -> Inicialización Sistema -> Detalle Sociedad -> Datos de Contabilidad -> ID fiscal general 1");
                                    else
                                    {
                                        if (((System.String)oRecordSet.Fields.Item("TaxIdNum").Value).Trim() == "")
                                            throw new Exception("Debe ingresar RUC de Emisor, Gestión -> Inicialización Sistema -> Detalle Sociedad -> Datos de Contabilidad -> ID fiscal general 1");
                                        TaxIdNum = "20520967151"; //((System.String)oRecordSet.Fields.Item("TaxIdNum").Value).Trim();
                                        CompnyName = ((System.String)oRecordSet.Fields.Item("CompnyName").Value).Trim();
                                    }


                                    if (RunningSQLServer)
                                        s = @"SELECT ISNULL(U_UserED,'') 'UserED', ISNULL(U_PwdED,'') 'PassED' FROM [@VID_FEPARAM]";
                                    else
                                        s = @"SELECT IFNULL(""U_UserED"",'') ""UserED"", IFNULL(""U_PwdED"",'') ""PassED"" FROM ""@VID_FEPARAM"" ";
                                    oRecordSet.DoQuery(s);
                                    if (oRecordSet.RecordCount == 0)
                                        throw new Exception("No se encuentra parametrizado si es multisociedad, Gestión -> Definiciones -> Factura Electronica -> Parametros");
                                    else
                                    {
                                        if (((System.String)oRecordSet.Fields.Item("UserED").Value).Trim() != "")
                                            UserWS = Func.DesEncriptar(((System.String)oRecordSet.Fields.Item("UserED").Value).Trim());
                                        if (((System.String)oRecordSet.Fields.Item("PassED").Value).Trim() != "")
                                            PassWS = Func.DesEncriptar(((System.String)oRecordSet.Fields.Item("PassED").Value).Trim());
                                    }

                                    //Func.AddLog(OPGetEstado);
                                    if (OPGetEstado != "")//Consulta estado de documentos venta enviados al portal
                                        ConsultarOPGetEstado(UserWS, PassWS, CompnyName);
                                }
                            }
                            catch (Exception we)
                            {
                                Func.AddLog("Error foreach busca bases: version " + sVersion + " - " + we.Message + " ** Trace: " + we.StackTrace);
                            }
                            finally
                            {
                                if (oCompany != null)
                                    oCompany.Disconnect();
                                oCompany = null;
                                oRecordSet = null;
                            }
                        }
                    }
                    else
                        Func.AddLog("No se ha podido conectar a la Base SAP, revisar datos de conexion");//no se ha podido conectar
                }
                catch (Exception w)
                {
                    Func.AddLog("Error Time1: version " + sVersion + " - " + w.Message + ". ** Trace: " + w.StackTrace);
                }
            }
            catch (Exception we)
            {
                Func.AddLog("Error Time: version " + sVersion + " - " + we.Message + " ** Trace: " + we.StackTrace);
            }
            finally
            {
                Tiempo.Enabled = true;
                Func.AddLog("Fin");
            }
        }


        private void Tratarcdrziptrama(string trama)
        {
            var returnByte = Convert.FromBase64String(trama);
            string respuesta;

            using (var memRespuesta = new MemoryStream(returnByte))
            {
                if (memRespuesta.Length <= 0)
                {
                    respuesta = "Respuesta SUNAT Vacío";
                }
                else
                {

                }
              }
         }


      
        //Consulta estado de documentos venta enviados al portal
        public void ConsultarOPGetEstado(String UserWS, String PassWS, String CompnyName)
        {
            String sObjType;
            String sDocSubType;
            String TipoDocElec;
            String sDocEntry;
            Boolean SeProceso = false;
            String Json, Id, Validation, Status; 
            String sMessage;
            String SerieP = "";
            String sFolioNum = "0";
            String sTabla;
            Int32 lRetCode;
            String sErrMsg;
            String EstadoDTE = "";
            String ExternalFolio;
            XDocument miXMLDoc;
            XmlDocument oXml;
            XmlNode oNode;
            String OPFinal;
            SAPbobsCOM.Recordset oRecordSetAux = (SAPbobsCOM.Recordset)(oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset));
            SAPbobsCOM.Documents oDocuments;
            SAPbobsCOM.StockTransfer oStockTransfer;
            SAPbobsCOM.Payments oPay;

            try
            {
                if (RunningSQLServer)
                    s = @"SELECT T0.DocEntry
                                      ,T0.U_DocEntry
                                      ,T0.U_SubType
                                      ,T0.U_FolioNum
                                      ,T0.U_ObjType
                                      ,T0.U_TipoDoc
                                      ,T0.U_Status
                                      ,T0.U_UserCode
                                      ,T0.U_Json
									  ,T0.U_SeriePE
									  ,T0.U_Id
									  ,T0.U_DocDate
                                      ,T0.U_Validation
                                      ,T0.U_ExtFolio
                                  FROM [@VID_FELOG] T0 WITH (nolock)
                                 WHERE T0.U_Status IN ('EC')
                                   AND ISNULL(T0.U_ExtFolio,'') <> '' 
                                   ";//saque opcion 'EE' que estaba en el monitor
                else
                    s = @"SELECT T0.""DocEntry""
                                      ,T0.""U_DocEntry""
                                      ,T0.""U_SubType""
                                      ,T0.""U_FolioNum""
                                      ,T0.""U_ObjType""
                                      ,T0.""U_TipoDoc""
                                      ,T0.""U_Status""
                                      ,T0.""U_UserCode""
                                      ,T0.""U_Json""
									  ,T0.""U_SeriePE""
									  ,T0.""U_Id""
									  ,T0.""U_DocDate""
                                      ,T0.""U_Validation""
                                      ,T0.""U_ExtFolio""
                                  FROM ""@VID_FELOG"" T0
                                 WHERE T0.""U_Status"" IN ('EC')
                                   AND IFNULL(T0.""U_ExtFolio"",'') <> '' ";//saque opcion 'EE' que estaba en el monitor

                oRecordSet.DoQuery(s);

                if (oRecordSet.RecordCount > 0)
                {
                    while (!oRecordSet.EoF)
                    {
                        try
                        {
                            ExternalFolio = (System.String)(oRecordSet.Fields.Item("U_ExtFolio").Value);
                            s = (System.String)(oRecordSet.Fields.Item("U_ObjType").Value);
                            if (s == "15")
                                sTabla = "ODLN";
                            else if (s == "14")
                                sTabla = "ORIN";
                            else if (s == "67")
                                sTabla = "OWTR";
                            else if (s == "21")
                                sTabla = "ORPD";
                            else if (s == "203")
                                sTabla = "ODPI";
                            else if (s == "46")
                                sTabla = "OVPM";
                            else
                                sTabla = "OINV";

                            sObjType = s;
                            sDocEntry = Convert.ToString((System.Double)(oRecordSet.Fields.Item("U_DocEntry").Value));

                            //Consulta estado al portal
                            OPFinal = OPGetEstado.Replace("&amp;", "&");

                            miXMLDoc = new XDocument(
                            new XDeclaration("1.0", "utf-8", "yes")
                                   , new XElement("documentoelectronico",
                                       new XElement("DocNum", TaxIdNum),
                                       new XElement("DocType", ((System.String)oRecordSet.Fields.Item("U_TipoDoc").Value).Trim()),
                                       new XElement("IdDocumento", ((System.String)oRecordSet.Fields.Item("U_ExtFolio").Value).Trim()
                                           ))
                                   );
                            oXml = new XmlDocument();
                            using (var xmlReader = miXMLDoc.CreateReader())
                            {
                                oXml.Load(xmlReader);
                            }

                            oXml.LoadXml(@"<documentoelectronico><DocNum>20520967151</DocNum><DocType>01</DocType><IdDocumento>F001-190</IdDocumento></documentoelectronico>"); //PRUEBAS INTERNAS
                            s = Func.UpLoadDocumentByUrl(oXml, null, OPFinal, UserWS, PassWS);

                            oXml.LoadXml(s);

                            oNode = oXml.DocumentElement.SelectSingleNode("/Error/ErrorCode");
                            
                            string errorCode = oNode.InnerText;
                            
                            oNode = oXml.DocumentElement.SelectSingleNode("/Error/ErrorText");
                            string errorText = oNode.InnerText;

                            oNode = oXml.DocumentElement.SelectSingleNode("/Error/CdrZipTrama");
                            string strcdr = oNode.InnerText;

                            if (strcdr != null)
                            {
                                Tratarcdrziptrama(strcdr);
                            }



                            if (errorCode == "0")
                            {
                                Func.AddLog("Base " + CompnyName + " - Tipo Documento " + ((System.String)oRecordSet.Fields.Item("U_TipoDoc").Value).Trim() + " - Folio " + ((System.String)oRecordSet.Fields.Item("U_ExtFolio").Value).Trim() + " - " + errorText);
                                sMessage = errorText;
                                EstadoDTE = "RR";
                            }
                            else
                            {
                                Func.AddLog("Base " + CompnyName + " - Tipo Documento " + ((System.String)oRecordSet.Fields.Item("U_TipoDoc").Value).Trim() + " - Folio " + ((System.String)oRecordSet.Fields.Item("U_ExtFolio").Value).Trim() + " -> " + errorCode + ": " + errorText);
                                sMessage = errorText;
                                if (errorCode == "-106")//error sunat
                                    EstadoDTE = "RZ";
                                else if (errorCode == "-200")  //
                                    EstadoDTE = "EC";
                                else
                                    EstadoDTE = "RZ";
                            }

                            if (errorText == "")
                            {
                                Func.AddLog("WebService devolvio en blanco -> base " + CompnyName + " - Tipo Documento " + ((System.String)oRecordSet.Fields.Item("U_TipoDoc").Value).Trim() + " - Folio " + ((System.String)oRecordSet.Fields.Item("U_ExtFolio").Value));
                                sMessage = "WebService devolvio en blanco";
                                EstadoDTE = "EC";
                            }

                            GC.Collect();
                            GC.WaitForPendingFinalizers();

                            lRetCode = FELOGUptM(((System.Int32)oRecordSet.Fields.Item("DocEntry").Value), ((System.Double)oRecordSet.Fields.Item("U_DocEntry").Value), sObjType, ((System.String)oRecordSet.Fields.Item("U_SubType").Value).Trim(), ((System.String)oRecordSet.Fields.Item("U_SeriePE").Value).Trim(), ((System.Double)oRecordSet.Fields.Item("U_FolioNum").Value), EstadoDTE, sMessage, ((System.String)oRecordSet.Fields.Item("U_TipoDoc").Value).Trim(), ((System.String)oRecordSet.Fields.Item("U_UserCode").Value).Trim(), ((System.String)oRecordSet.Fields.Item("U_Json").Value).Trim(), ((System.String)oRecordSet.Fields.Item("U_Id").Value).Trim(), ((System.String)oRecordSet.Fields.Item("U_Validation").Value).Trim(), ((System.DateTime)oRecordSet.Fields.Item("U_DocDate").Value), ExternalFolio);
                            if (lRetCode == 0)
                                Func.AddLog("Error al actualizar Log de Documento Electronico, base " + CompnyName + " -> TipoDoc " + (System.String)(oRecordSet.Fields.Item("U_TipoDoc").Value) + " " + Convert.ToString((System.Double)(oRecordSet.Fields.Item("U_FolioNum").Value)));
                            else
                            {
                                //actualizar campo crear en cabecera de documento con el estado
                                if (sObjType == "67")
                                {
                                    oStockTransfer = (SAPbobsCOM.StockTransfer)(oCompany.GetBusinessObject(BoObjectTypes.oStockTransfer));
                                    if (oStockTransfer.GetByKey(Convert.ToInt32(sDocEntry)))
                                    {
                                        if (EstadoDTE == "RR")
                                            oStockTransfer.UserFields.Fields.Item("U_EstadoFE").Value = "A";
                                        else if (EstadoDTE == "RZ")
                                            oStockTransfer.UserFields.Fields.Item("U_EstadoFE").Value = "R";
                                        else if (EstadoDTE == "EC")
                                            oStockTransfer.UserFields.Fields.Item("U_EstadoFE").Value = "P";
                                        else if (EstadoDTE == "EE")
                                            oStockTransfer.UserFields.Fields.Item("U_EstadoFE").Value = "P";
                                        else
                                            oStockTransfer.UserFields.Fields.Item("U_EstadoFE").Value = "N";

                                        lRetCode = oStockTransfer.Update();
                                        if (lRetCode != 0)
                                        {
                                            sErrMsg = oCompany.GetLastErrorDescription();
                                            Func.AddLog("No se actualizado estado de documento, base " + CompnyName + " TipoDoc -> " + (System.String)(oRecordSet.Fields.Item("U_TipoDoc").Value) + " folio " + ((System.String)oRecordSet.Fields.Item("U_ExtFolio").Value) + " - " + sErrMsg);
                                            if (RunningSQLServer)
                                                s = @"UPDATE [@VID_FELOG] SET U_Message = ISNULL(U_Message,'') + '- No se actualizo Doc' WHERE DocEntry = {0}";
                                            else
                                                s = @"UPDATE ""@VID_FELOG"" SET ""U_Message"" = IFNULL(""U_Message"",'') || '- No se actualizo Doc' WHERE ""DocEntry"" = {0}";
                                            s = String.Format(s, ((System.Double)oRecordSet.Fields.Item("DocEntry").Value));
                                            oRecordSetAux.DoQuery(s);
                                        }
                                    }
                                    oStockTransfer = null;
                                }
                                else if (sObjType == "46")
                                {
                                    oPay = ((SAPbobsCOM.Payments)oCompany.GetBusinessObject(BoObjectTypes.oVendorPayments));
                                    if (oPay.GetByKey(Convert.ToInt32(sDocEntry)))
                                    {
                                        if (EstadoDTE == "RR")
                                            oPay.UserFields.Fields.Item("U_EstadoFE").Value = "A";
                                        else if (EstadoDTE == "RZ")
                                            oPay.UserFields.Fields.Item("U_EstadoFE").Value = "R";
                                        else if (EstadoDTE == "EC")
                                            oPay.UserFields.Fields.Item("U_EstadoFE").Value = "P";
                                        else if (EstadoDTE == "EE")
                                            oPay.UserFields.Fields.Item("U_EstadoFE").Value = "P";
                                        else
                                            oPay.UserFields.Fields.Item("U_EstadoFE").Value = "N";

                                        lRetCode = oPay.Update();
                                        if (lRetCode != 0)
                                        {
                                            sErrMsg = oCompany.GetLastErrorDescription();
                                            Func.AddLog("No se actualizado estado de documento, base " + CompnyName + " TipoDoc -> " + (System.String)(oRecordSet.Fields.Item("U_TipoDoc").Value) + " folio " + ((System.String)oRecordSet.Fields.Item("U_ExtFolio").Value) + " - " + sErrMsg);
                                            if (RunningSQLServer)
                                                s = @"UPDATE [@VID_FELOG] SET U_Message = ISNULL(U_Message,'') + '- No se actualizo Doc' WHERE DocEntry = {0}";
                                            else
                                                s = @"UPDATE ""@VID_FELOG"" SET ""U_Message"" = IFNULL(""U_Message"",'') || '- No se actualizo Doc' WHERE ""DocEntry"" = {0}";
                                            s = String.Format(s, ((System.Double)oRecordSet.Fields.Item("DocEntry").Value));
                                            oRecordSetAux.DoQuery(s);
                                        }
                                    }
                                    oStockTransfer = null;
                                }
                                else
                                {
                                    if (sObjType == "15")
                                        oDocuments = (SAPbobsCOM.Documents)(oCompany.GetBusinessObject(BoObjectTypes.oDeliveryNotes));
                                    else if (sObjType == "14")
                                        oDocuments = (SAPbobsCOM.Documents)(oCompany.GetBusinessObject(BoObjectTypes.oCreditNotes));
                                    else if (sObjType == "21")
                                        oDocuments = (SAPbobsCOM.Documents)(oCompany.GetBusinessObject(BoObjectTypes.oPurchaseReturns));
                                    else if (sObjType == "203")
                                        oDocuments = (SAPbobsCOM.Documents)(oCompany.GetBusinessObject(BoObjectTypes.oDownPayments));
                                    else
                                        oDocuments = (SAPbobsCOM.Documents)(oCompany.GetBusinessObject(BoObjectTypes.oInvoices));

                                    if (oDocuments.GetByKey(Convert.ToInt32(sDocEntry)))
                                    {
                                        if (EstadoDTE == "RR")
                                            oDocuments.UserFields.Fields.Item("U_EstadoFE").Value = "A";
                                        else if (EstadoDTE == "RZ")
                                            oDocuments.UserFields.Fields.Item("U_EstadoFE").Value = "R";
                                        else if (EstadoDTE == "EC")
                                            oDocuments.UserFields.Fields.Item("U_EstadoFE").Value = "P";
                                        else if (EstadoDTE == "EE")
                                            oDocuments.UserFields.Fields.Item("U_EstadoFE").Value = "P";
                                        else
                                            oDocuments.UserFields.Fields.Item("U_EstadoFE").Value = "N";

                                        lRetCode = oDocuments.Update();
                                        if (lRetCode != 0)
                                        {
                                            sErrMsg = oCompany.GetLastErrorDescription();
                                            Func.AddLog("No se actualizado estado de documento, base " + CompnyName + " TipoDoc -> " + (System.String)(oRecordSet.Fields.Item("U_TipoDoc").Value) + " Folio " + ((System.String)oRecordSet.Fields.Item("U_ExtFolio").Value) + " - " + sErrMsg);
                                            if (RunningSQLServer)
                                                s = @"UPDATE [@VID_FELOG] SET U_Message = ISNULL(U_Message,'') + '- No se actualizo Doc' WHERE DocEntry = {0}";
                                            else
                                                s = @"UPDATE ""@VID_FELOG"" SET ""U_Message"" = IFNULL(""U_Message"",'') || '- No se actualizo Doc' WHERE ""DocEntry"" = {0}";
                                            s = String.Format(s, ((System.Int32)oRecordSet.Fields.Item("DocEntry").Value));
                                            oRecordSetAux.DoQuery(s);
                                        }
                                    }
                                    oDocuments = null;

                                }
                            }
                        }
                        catch (Exception x)
                        {
                            Func.AddLog("Err base " + CompnyName + " -> documento " + (System.String)(oRecordSet.Fields.Item("U_TipoDoc").Value) + " folio " + (System.Double)(oRecordSet.Fields.Item("U_FolioNum").Value) + " - " + x.Message + ", StackTrace " + x.StackTrace);
                        }
                        oRecordSet.MoveNext();
                    }//fin while

                }

            }
            catch (Exception o)
            {
                Func.AddLog("**Error GetEstado, base " + CompnyName + ": version " + sVersion + " - " + o.Message + " ** Trace: " + o.StackTrace);
            }
        }

        public Boolean ConectarBaseSAP(String BaseName)
        {
            Int32 lRetCode;
            TFunctions Func;
            String sErrMsg;

            Func = new TFunctions();
            try
            {
                oCompany.Server = Glob_Servidor;
                oCompany.LicenseServer = Glob_Licencia;
                oCompany.DbUserName = Glob_UserSQL;
                oCompany.DbPassword = Glob_PassSQL;

                if (Glob_SQL == "2008")
                    oCompany.DbServerType = SAPbobsCOM.BoDataServerTypes.dst_MSSQL2008;
                else if (Glob_SQL == "2012")
                    oCompany.DbServerType = SAPbobsCOM.BoDataServerTypes.dst_MSSQL2012;
                else if (Glob_SQL == "2014")
                    oCompany.DbServerType = SAPbobsCOM.BoDataServerTypes.dst_MSSQL2014;
                else if (Glob_SQL == "2016")
                    oCompany.DbServerType = SAPbobsCOM.BoDataServerTypes.dst_MSSQL2016;
                else if (Glob_SQL == "HANA")
                    oCompany.DbServerType = SAPbobsCOM.BoDataServerTypes.dst_HANADB;

                oCompany.UseTrusted = false;
                oCompany.CompanyDB = BaseName;
                oCompany.UserName = Glob_UserSAP;
                oCompany.Password = Glob_PassSAP;

                //            Func.AddLog(oCompany.Server);
                //            Func.AddLog(oCompany.LicenseServer);
                //            Func.AddLog(oCompany.DbUserName);
                //            Func.AddLog(oCompany.DbPassword);
                //            Func.AddLog(oCompany.CompanyDB);
                //            Func.AddLog(oCompany.UserName);
                //            Func.AddLog(oCompany.Password);
                lRetCode = oCompany.Connect();
                if (lRetCode != 0)
                {
                    sErrMsg = oCompany.GetLastErrorDescription();
                    //Func := new TFunciones;
                    Func.AddLog("Error de conexión base SAP, " + sErrMsg);
                    return false;
                }
                else
                    return true;
            }
            catch (Exception w)
            {
                //Func := new TFunciones;
                Func.AddLog("ConectarBase: " + w.Message + " ** Trace: " + w.StackTrace);
                return false;
            }
        }

        private Boolean DatosConexion(XmlDocument xDoc)
        {
            XmlNodeList Configuracion;
            XmlNodeList lista;
            Int32 lRetCode;
            TFunctions Func;
            String sErrMsg;
            String sPath = Path.GetDirectoryName(this.GetType().Assembly.Location);
            Boolean _return = false;

            Func = new TFunctions();
            try
            {
       

                Configuracion = xDoc.GetElementsByTagName("Configuracion");
                lista = ((XmlElement)Configuracion[0]).GetElementsByTagName("ServidorSAP");

                foreach (XmlElement nodo in lista)
                {
                    var i = 0;
                    var nServidor = nodo.GetElementsByTagName("Servidor");
                    var nLicencia = nodo.GetElementsByTagName("ServLicencia");
                    var nUserSAP = nodo.GetElementsByTagName("UsuarioSAP");
                    var nPassSAP = nodo.GetElementsByTagName("PasswordSAP");
                    var nSQL = nodo.GetElementsByTagName("SQL");
                    var nUserSQL = nodo.GetElementsByTagName("UsuarioSQL");
                    var nPassSQL = nodo.GetElementsByTagName("PasswordSQL");
                    var nBaseSAP = nodo.GetElementsByTagName("BaseSAP");

                    Glob_Servidor = (System.String)(nServidor[i].InnerText);
                    Glob_Licencia = (System.String)(nLicencia[i].InnerText);
                    Glob_UserSAP = (System.String)(nUserSAP[i].InnerText);
                    Glob_PassSAP = (System.String)(nPassSAP[i].InnerText);
                    Glob_SQL = (System.String)(nSQL[i].InnerText);
                    Glob_UserSQL = (System.String)(nUserSQL[i].InnerText);
                    Glob_PassSQL = (System.String)(nPassSQL[i].InnerText);
                    return true;
                }
                return false;
            }
            catch (Exception w)
            {
                //Func := new TFunciones;
                Func.AddLog("DatosConexion: " + w.Message + " ** Trace: " + w.StackTrace);
                return false;
            }

        }

        //no esta operativo 
        private void EnviarMail(String CompnyName)
        {
            List<string> Lista = new List<string>();
            String mensaje = "";
            var msg = new MailMessage();
            String Mails = "";
            String MailFrom;
            String MailSmtpHost;
            String MailUser;
            String MailPass;
            String Hora1;
            String Hora2;
            String Puerto;

            try
            {
                Hora1 = Func.DatosConfig("Mail", "HoraEnvio1", xDoc);
                Hora2 = Func.DatosConfig("Mail", "HoraEnvio2", xDoc);

                if ((Hora1 == "") && (Hora2 == ""))
                    throw new Exception("No se ha definido hora de envio para el mail en xml de Configuración, debe ingresar al menos una hora");

                s = DateTime.Now.ToString("HHmm");
                Hora1 = Hora1.Replace(":", "").Replace(".", "");
                Hora2 = Hora2.Replace(":", "").Replace(".", "");

                if ((Hora1 == s) || (Hora2 == s))
                {
                    //buscar mail en tabla VID_FEPARAM
                    if (RunningSQLServer)
                        s = @"SELECT ISNULL(U_Mails_CL,'') 'Mail' FROM [@VID_FEPARAM]";
                    else
                        s = @"SELECT IFNULL(""U_Mails_CL"",'') ""Mail"" FROM ""@VID_FEPARAM"" ";
                    oRecordSet.DoQuery(s);
                    if (oRecordSet.RecordCount == 0)
                        throw new Exception("No se ha encontrado mail donde enviar los DTE con problemas");
                    else
                        Mails = ((System.String)oRecordSet.Fields.Item("Mail").Value).Trim();

                    MailFrom = Func.DatosConfig("Mail", "MailFrom", xDoc);
                    MailSmtpHost = Func.DatosConfig("Mail", "MailSmtpHost", xDoc);
                    MailUser = Func.DatosConfig("Mail", "MailUser", xDoc);
                    MailPass = Func.DatosConfig("Mail", "MailPass", xDoc);
                    Puerto = Func.DatosConfig("Mail", "Puerto", xDoc);

                    if (MailFrom == "")
                        throw new Exception("Debe Ingresar direccion de mail de donde se enviara el mail");

                    if (MailUser == "")
                        throw new Exception("Debe Ingresar usuario para enviar el mail");

                    if (MailPass == "")
                        throw new Exception("Debe Ingresar password para enviar el mail");

                    if (MailPass == "")
                        Puerto = "587";


                    if (RunningSQLServer)
                        s = @"SELECT T0.DocEntry
                                      ,T0.U_DocEntry
                                      ,T0.U_SubType
                                      ,T0.U_FolioNum
                                      ,T0.U_ObjType
                                      ,T0.U_TipoDoc
                                      ,T0.U_Status
                                      ,ISNULL((SELECT D1.Descr
									             FROM CUFD D0
												 JOIN UFD1 D1 ON D1.TableID = D0.TableID
												             AND D1.FieldID = D0.FieldID
											    WHERE D0.TableID = '@VID_FELOG'
												  AND D0.AliasID = 'Status'
												  AND D1.FldValue = T0.U_Status),'') 'Estado'
                                      ,T0.U_UserCode
									  ,T0.U_DocDate
                                  FROM [@VID_FELOG] T0 WITH (nolock)
                                 WHERE T0.U_Status IN ('EE', 'RZ')";
                    else
                        s = @"SELECT T0.""DocEntry""
                                      ,T0.""U_DocEntry""
                                      ,T0.""U_SubType""
                                      ,T0.""U_FolioNum""
                                      ,T0.""U_ObjType""
                                      ,T0.""U_TipoDoc""
                                      ,T0.""U_Status""
                                      ,IFNULL((SELECT D1.""Descr""
									             FROM ""CUFD"" D0
												 JOIN ""UFD1"" D1 ON D1.""TableID"" = D0.""TableID""
												             AND D1.""FieldID"" = D0.""FieldID""
											    WHERE D0.""TableID"" = '@VID_FELOG'
												  AND D0.""AliasID"" = 'Status'
												  AND D1.""FldValue"" = T0.""U_Status""),'') ""Estado""
                                      ,T0.""U_UserCode""
									  ,T0.""U_DocDate""
                                  FROM ""@VID_FELOG"" T0
                                 WHERE T0.""U_Status"" IN ('EE', 'RZ')";

                    oRecordSet.DoQuery(s);
                    while (!oRecordSet.EoF)
                    {
                        s = ((System.String)oRecordSet.Fields.Item("U_TipoDoc").Value).Trim() + "," + ((System.Double)oRecordSet.Fields.Item("U_FolioNum").Value).ToString().Trim() + ","
                            + ((System.String)oRecordSet.Fields.Item("Estado").Value).Trim() + "," + ((System.String)oRecordSet.Fields.Item("U_UserCode").Value).Trim();
                        Lista.Add(s);
                        oRecordSet.MoveNext();
                    }
                    var i = 0;
                    foreach (String Valor in Lista)
                    {
                        var valores = Valor.Split(',');
                        if (i == 0)
                        {
                            mensaje = "Detalle de documento que ha quedado en estado Rechazado en SII o han tenido errores y no han llegado a SII." + Environment.NewLine;
                            mensaje = mensaje + "En caso de estar rechazado debe revisar el mail enviado por SII al mail que tenga registrado para ver el problema del rechazo." + Environment.NewLine;
                            mensaje = mensaje + Environment.NewLine;
                        }
                        mensaje = mensaje + "Tipo DTE:" + valores[0] + "    Folio:" + valores[1] + "    Estado:" + valores[2] + "   Usuario:" + valores[3] + Environment.NewLine;
                        i++;
                    }

                    if (mensaje != "")
                    {
                        mensaje = mensaje + Environment.NewLine;
                        mensaje = mensaje + Environment.NewLine;
                        mensaje = mensaje + "mail enviado automatico por Servicio Estado FE";
                    }

                    foreach (String enviar_a in Mails.Split(';'))
                    {
                        msg.To.Add(new MailAddress(enviar_a));
                    }

                    msg.From = new MailAddress(MailFrom.Trim());
                    msg.Subject = "DTE con Rechazados o con Errrores, " + CompnyName;
                    msg.SubjectEncoding = System.Text.Encoding.UTF8;
                    msg.Body = mensaje;
                    msg.BodyEncoding = System.Text.Encoding.UTF8;
                    msg.Priority = MailPriority.High;

                    //Generar lista de Archivos a Adjuntar
                    //var archivo = Adjuntar(Directory.GetFiles(tomarde), tomarde);
                    //if (archivo.Count == 0) Application.Exit();
                    //Adjuntando los Archivos al Correo
                    //foreach (string arch in archivo)
                    //{
                    //    msg.Attachments.Add(new Attachment(arch));
                    //}

                    var smtpClient = new SmtpClient();
                    if (MailSmtpHost != "")
                    {
                        smtpClient.Host = MailSmtpHost.Trim();
                        smtpClient.EnableSsl = true;
                        smtpClient.Port = Convert.ToInt32(Puerto);

                    }
                    else
                        smtpClient.EnableSsl = false;
                    smtpClient.Credentials = new System.Net.NetworkCredential(MailUser.Trim(), MailPass.Trim());
                    smtpClient.Send(msg);
                    Func.AddLog("Mail enviado para " + CompnyName);
                }
            }
            catch (Exception we)
            {
                Func.AddLog("EnviarMail: version " + sVersion + " - " + we.Message + " ** Trace: " + we.StackTrace);
            }
        }

        //Inserta registro LOG no esta operativo
        public Int32 FELOGAdd(Int32 DocEntry, String ObjType, String SubType, String SeriePE, Int32 FolioNum, String Status, String sMessage, String TipoDoc, String UserCode, String JsonText, String Id, String Validation, String ExternalFolio)//, ref SAPbobsCOM.Company oCompany)
        {
            SAPbobsCOM.GeneralService oFELOG = null;
            SAPbobsCOM.GeneralData oFELOGData = null;
            SAPbobsCOM.GeneralDataCollection oFELOGLines = null;
            SAPbobsCOM.GeneralDataParams oFELOGParameter = null;
            SAPbobsCOM.CompanyService CmpnyService;

            CmpnyService = oCompany.GetCompanyService();

            try
            {
                //Get GeneralService (oCmpSrv is the CompanyService)
                oFELOG = (SAPbobsCOM.GeneralService)(CmpnyService.GetGeneralService("VID_FELOG"));

                //Create data for new row in main UDO
                oFELOGData = (SAPbobsCOM.GeneralData)(oFELOG.GetDataInterface(SAPbobsCOM.GeneralServiceDataInterfaces.gsGeneralData));
                oFELOGData.SetProperty("U_DocEntry", DocEntry);
                oFELOGData.SetProperty("U_ObjType", ObjType);
                oFELOGData.SetProperty("U_FolioNum", FolioNum);
                oFELOGData.SetProperty("U_SubType", SubType);
                oFELOGData.SetProperty("U_Status", Status);
                oFELOGData.SetProperty("U_Message", sMessage);
                oFELOGData.SetProperty("U_TipoDoc", TipoDoc);
                oFELOGData.SetProperty("U_UserCode", UserCode);
                oFELOGData.SetProperty("U_Json", JsonText);
                oFELOGData.SetProperty("U_SeriePE", SeriePE);
                oFELOGData.SetProperty("U_Id", Id);
                oFELOGData.SetProperty("U_Validation", Validation);
                oFELOGData.SetProperty("U_ExtFolio", ExternalFolio);

                //Add the new row, including children, to database
                //oGeneralParams := oGeneralService.Add(oGeneralData);
                //Cmpny.StartTransaction();
                oFELOGParameter = oFELOG.Add(oFELOGData);
                return (System.Int32)(oFELOGParameter.GetProperty("DocEntry"));

                //Cmpny.EndTransaction(BoWfTransOpt.wf_Commit);
            }
            catch (Exception e)
            {
                Func.AddLog("DocEntry: " + DocEntry.ToString() + " ObjType: " + ObjType + " SubType: " + SubType + "Error insertar datos en FELOG: " + e.Message + " ** Trace: " + e.StackTrace);
                return 0;
            }
            finally
            {
                oFELOG = null;
                oFELOGData = null;
                oFELOGLines = null;
                oFELOGParameter = null;
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }

        }//fin FELOGAdd

        //Actualiza registro en el LOG
        public Int32 FELOGUptM(Int32 DocEntry, Double DocEntryDoc, String ObjType, String SubType, String SeriePE, Double FolioNum, String Status, String sMessage, String TipoDoc, String UserCode, String JsonText, String Id, String Validation, DateTime DocDate, String ExternalFolio)//, ref SAPbobsCOM.Company oCompany)
        {
            SAPbobsCOM.GeneralService oFELOG = null;
            SAPbobsCOM.GeneralData oFELOGData = null;
            SAPbobsCOM.GeneralDataCollection oFELOGLines = null;
            SAPbobsCOM.GeneralDataParams oFELOGParameter = null;
            String StrDummy;
            SAPbobsCOM.CompanyService CmpnyService;

            CmpnyService = oCompany.GetCompanyService();

            try
            {
                oFELOG = (SAPbobsCOM.GeneralService)(CmpnyService.GetGeneralService("VID_FELOG"));
                oFELOGParameter = (SAPbobsCOM.GeneralDataParams)(oFELOG.GetDataInterface(SAPbobsCOM.GeneralServiceDataInterfaces.gsGeneralDataParams));
                StrDummy = Convert.ToString(DocEntry);
                oFELOGParameter.SetProperty("DocEntry", StrDummy);
                oFELOGData = oFELOG.GetByParams(oFELOGParameter);
                oFELOGData.SetProperty("U_DocEntry", Convert.ToString(DocEntryDoc));
                oFELOGData.SetProperty("U_FolioNum", Convert.ToString(FolioNum));
                oFELOGData.SetProperty("U_Status", Status);
                oFELOGData.SetProperty("U_Message", sMessage);
                oFELOGData.SetProperty("U_TipoDoc", TipoDoc);
                oFELOGData.SetProperty("U_UserCode", UserCode);
                oFELOGData.SetProperty("U_ExtFolio", ExternalFolio);
                if (JsonText != null)
                    oFELOGData.SetProperty("U_Json", JsonText);

                oFELOGData.SetProperty("U_SeriePE", SeriePE);

                if (Id != null)
                    oFELOGData.SetProperty("U_Id", Id);

                if (Validation != null)
                    oFELOGData.SetProperty("U_Validation", Validation);

                if (DocDate != null)
                    oFELOGData.SetProperty("U_DocDate", DocDate);

                //oGen.SetNewDataSourceHeader_InUDO(oDBDSHeader, oFELOGData);

                //Cmpny.StartTransaction();
                oFELOG.Update(oFELOGData);
                //Result :=Convert.ToInt32(TMultiFunctions.Trim(System.String(oFELOGData.GetProperty('DocEntry'))));
                return (System.Int32)(oFELOGData.GetProperty("DocEntry"));
                //Cmpny.EndTransaction(BoWfTransOpt.wf_Commit);
            }
            catch (Exception e)
            {
                Func.AddLog("Actualizar tabla FELOG: " + e.Message + " ** Trace: " + e.StackTrace);
                //if (Cmpny.InTransaction) then
                //Cmpny.EndTransaction(BoWfTransOpt.wf_RollBack);
                return 0;
            }
            finally
            {
                oFELOG = null;
                oFELOGData = null;
                oFELOGLines = null;
                oFELOGParameter = null;
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }
    }
}
