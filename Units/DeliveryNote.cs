using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SAPbouiCOM;
using SAPbobsCOM;
using System.Globalization;
using VisualD.GlobalVid;
using VisualD.SBOFunctions;
using System.Reflection;
using VisualD.vkBaseForm;
using VisualD.MultiFunctions;
using VisualD.vkFormInterface;
using System.IO;
using System.CodeDom.Compiler;
using System.Xml;
using System.Xml.Linq;
using Factura_Electronica_VK.Functions;
using VisualD.untLog;
using System.Data;
using FactRemota;
using ServiceStack.Text;
using System.Net.Http;
using System.Configuration;
using DLLparaXMLPE;
using Newtonsoft.Json;

namespace Factura_Electronica_VK.DeliveryNote
{
    public class TDeliveryNote : TvkBaseForm, IvkFormInterface
    {
        private SAPbobsCOM.Recordset oRecordSet;
        private SAPbouiCOM.Form oForm;
        private String s;
        private Boolean Flag;
        private SAPbouiCOM.Matrix mtx;
        private SAPbouiCOM.Item oItem = null;
        private SAPbouiCOM.Item oItemB = null;
        private SAPbouiCOM.Item oItemC = null;
        private SAPbouiCOM.StaticText oStatic;
        private SAPbouiCOM.EditText oEditText;
        private SAPbouiCOM.ComboBox oComboBox;
        private List<string> Lista;
        private CultureInfo _nf = new System.Globalization.CultureInfo("en-US");

        //por Peru
        private String RUC;

        public static String DocSubType
        { get; set; }
        public static Boolean bFolderAdd
        { get; set; }
        public static Boolean Transferencia
        { get; set; }
        public static Boolean Devolucion
        { get; set; }
        public static String ObjType
        { get; set; }

        public VisualD.SBOFunctions.CSBOFunctions SBO_f;

        public new bool InitForm(string uid, string xmlPath, ref Application application, ref SAPbobsCOM.Company company, ref CSBOFunctions SBOFunctions, ref TGlobalVid _GlobalSettings)
        {
            SAPbouiCOM.Folder oFolder;
            String Tabla;

            bool Result = base.InitForm(uid, xmlPath, ref application, ref company, ref SBOFunctions, ref _GlobalSettings);
            try
            {
                oRecordSet = (SAPbobsCOM.Recordset)(FCmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset));
                //FSBOf.LoadForm(xmlPath, 'VID_Entrega.srf', Uid);
                oForm = FSBOApp.Forms.Item(uid);
                Flag = false;
                oForm.Freeze(true);

                if (GlobalSettings.RunningUnderSQLServer)
                    s = "select ISNULL(TaxIdNum,'') TaxIdNum from OADM ";
                else
                    s = @"select IFNULL(""TaxIdNum"",'') ""TaxIdNum"" from ""OADM"" ";

                oRecordSet.DoQuery(s);
                if (oRecordSet.RecordCount == 0)
                    throw new Exception("Debe ingresar RUC de Emisor, Gestión -> Inicialización Sistema -> Detalle Sociedad -> Datos de Contabilidad -> ID fiscal general 1");
                else
                    RUC = ((System.String)oRecordSet.Fields.Item("TaxIdNum").Value).Trim();

                if (ObjType == "21")
                    Tabla = "ORPD";
                else if (ObjType == "67")
                    Tabla = "OWTR";
                else
                    Tabla = "ODLN";

                
                if ( ObjType != "67")  // Entrega - Devolucion 
                {
                    //Campo con el estado de DTE
                    oItemC = oForm.Items.Item("84");
                    oItem = oForm.Items.Add("lblEstado", SAPbouiCOM.BoFormItemTypes.it_STATIC);
                    oItem.Left = oItemC.Left;
                    oItem.Width = oItemC.Width;
                    oItem.Top = oItemC.Top + oItemC.Height + 5;
                    oItem.Height = oItem.Height;
                    oItem.LinkTo = "VID_FEEstado";
                    oStatic = (StaticText)(oForm.Items.Item("lblEstado").Specific);
                    oStatic.Caption = "Estado Doc. Electronico";

                    oItemC = oForm.Items.Item("208");
                    oItem = oForm.Items.Add("VID_Estado", SAPbouiCOM.BoFormItemTypes.it_COMBO_BOX);
                    oItem.Left = oItemC.Left;
                    oItem.Width = oItemC.Width + 30;
                    oItem.Top = oItemC.Top + oItemC.Height + 5;
                    oItem.Height = oItem.Height;
                    oItem.DisplayDesc = true;
                    oItem.Enabled = false;
                    oComboBox = (ComboBox)(oForm.Items.Item("VID_Estado").Specific);
                    oComboBox.DataBind.SetBound(true, Tabla, "U_EstadoFE");
                }
                else // Trasferencia
                {
                    oItemC = oForm.Items.Item("37");
                    oItem = oForm.Items.Add("lblEstado", SAPbouiCOM.BoFormItemTypes.it_STATIC);
                    oItem.Left = oItemC.Left;
                    oItem.Width = oItemC.Width;
                    oItem.Top = oItemC.Top + oItemC.Height + 5;
                    oItem.Height = oItem.Height;
                    oItem.LinkTo = "VID_FEEstado";
                    oStatic = (StaticText)(oForm.Items.Item("lblEstado").Specific);
                    oStatic.Caption = "Estado Sunat";
                    
                    oItemC = oForm.Items.Item("36");
                    oItem = oForm.Items.Add("VID_Estado", SAPbouiCOM.BoFormItemTypes.it_COMBO_BOX);
                    oItem.Left = oItemC.Left;
                    oItem.Width = oItemC.Width;
                    oItem.Top = oItemC.Top + oItemC.Height + 5;
                    oItem.Height = oItem.Height;
                    oItem.DisplayDesc = true;
                    oItem.Enabled = false;
                    oComboBox = (ComboBox)(oForm.Items.Item("VID_Estado").Specific);
                    oComboBox.DataBind.SetBound(true, "OWTR", "U_EstadoFE");
                }

                //colocar folder con los campos necesarios en FE PERU
                oForm.DataSources.UserDataSources.Add("VID_FEDCTO", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1);
                oItem = oForm.Items.Add("VID_FEDCTO", SAPbouiCOM.BoFormItemTypes.it_FOLDER);

                //para SAP 882 en adelante
                if (ObjType != "67")  // Entrega - Devolucion 
                    oItemB = oForm.Items.Item("1320002137");
                else //tansferencia
                    oItemB = oForm.Items.Item("1320000082");
                
                oItem.Left = oItemB.Left + 30;
                oItem.Width = oItemB.Width;
                oItem.Top = oItemB.Top;
                oItem.Height = oItem.Height;
                oFolder = (Folder)((oItem.Specific));
                oFolder.Caption = "Factura Electrónica";
                oFolder.Pane = 333;
                oFolder.DataBind.SetBound(true, "", "VID_FEDCTO");
               
           
                //para SAP 882 en adelante
                if (ObjType != "67")  // Entrega - Devolucion 
                    oFolder.GroupWith("1320002137");
                else
                    oFolder.GroupWith("1320000082");


                //cargar campos de usuarios
                oItemB = oForm.Items.Item("40");
                oItem = oForm.Items.Add("lblMDTD", SAPbouiCOM.BoFormItemTypes.it_STATIC);
                oItem.Left = 50; //oItemB.Left;
                oItem.Width = 125;//;oItemB.Width;
                oItem.Top = oItemB.Top + 15;//195
                oItem.Height = oItem.Height;//14
                oItem.FromPane = 333;
                oItem.ToPane = 333;
                oItem.LinkTo = "VID_FEMDTD";
                oStatic = (StaticText)(oForm.Items.Item("lblMDTD").Specific);
                oStatic.Caption = "Tipo de Documento";

                oItemB = oForm.Items.Item("lblMDTD");
                oItem = oForm.Items.Add("VID_FEMDTD", SAPbouiCOM.BoFormItemTypes.it_EDIT);
                oItem.Left = oItemB.Left + oItemB.Width + 5;
                oItem.Width = 60;
                oItem.Top = oItemB.Top;
                oItem.Height = oItemB.Height;
                oItem.FromPane = 333;
                oItem.ToPane = 333;
                oItem.RightJustified = true;
                oEditText = (EditText)(oForm.Items.Item("VID_FEMDTD").Specific);
                oEditText.DataBind.SetBound(true, Tabla, "U_BPP_MDTD");

                //--
                oItemB = oForm.Items.Item("lblMDTD");
                oItem = oForm.Items.Add("lblMDSD", SAPbouiCOM.BoFormItemTypes.it_STATIC);
                oItem.Left = oItemB.Left;
                oItem.Width = oItemB.Width;
                oItem.Top = oItemB.Top + oItemB.Height + 5;
                oItem.Height = oItem.Height;
                oItem.FromPane = 333;
                oItem.ToPane = 333;
                oItem.LinkTo = "VID_FEMDSD";
                oStatic = (StaticText)(oForm.Items.Item("lblMDSD").Specific);
                oStatic.Caption = "Serie del documento";

                oItemB = oForm.Items.Item("lblMDSD");
                oItem = oForm.Items.Add("VID_FEMDSD", SAPbouiCOM.BoFormItemTypes.it_EDIT);
                oItem.Left = oItemB.Left + oItemB.Width + 5;
                oItem.Width = 90; // oItemB.Width;
                oItem.Top = oItemB.Top;
                oItem.Height = oItem.Height;
                oItem.FromPane = 333;
                oItem.ToPane = 333;
                oItem.RightJustified = true;
                oEditText = (EditText)(oForm.Items.Item("VID_FEMDSD").Specific);
                oEditText.DataBind.SetBound(true, Tabla, "U_BPP_MDSD");

                //--
                oItemB = oForm.Items.Item("lblMDSD");
                oItem = oForm.Items.Add("lblMDCD", SAPbouiCOM.BoFormItemTypes.it_STATIC);
                oItem.Left = oItemB.Left;
                oItem.Width = oItemB.Width;
                oItem.Top = oItemB.Top + oItemB.Height + 5;
                oItem.Height = oItem.Height;
                oItem.FromPane = 333;
                oItem.ToPane = 333;
                oItem.LinkTo = "VID_FEMDCD";
                oStatic = (StaticText)(oForm.Items.Item("lblMDCD").Specific);
                oStatic.Caption = "Correlativo del documento";

                oItemB = oForm.Items.Item("lblMDCD");
                oItem = oForm.Items.Add("VID_FEMDCD", SAPbouiCOM.BoFormItemTypes.it_EDIT);
                oItem.Left = oItemB.Left + oItemB.Width + 5;
                oItem.Width = 90; // oItemB.Width;
                oItem.Top = oItemB.Top;
                oItem.Height = oItem.Height;
                oItem.FromPane = 333;
                oItem.ToPane = 333;
                oItem.RightJustified = true;
                oEditText = (EditText)(oForm.Items.Item("VID_FEMDCD").Specific);
                oEditText.DataBind.SetBound(true, Tabla, "U_BPP_MDCD");

            }
            catch (Exception e)
            {
                OutLog("InitForm: " + e.Message + " ** Trace: " + e.StackTrace);
                FSBOApp.MessageBox(e.Message + " ** Trace: " + e.StackTrace, 1, "Ok", "", "");
            }
            return Result;

        }//fin InitForm


        public new void MenuEvent(ref MenuEvent pVal, ref Boolean BubbleEvent)
        {
            //Int32 Entry;
            base.MenuEvent(ref pVal, ref BubbleEvent);
            try
            {
                //1281 Buscar; 
                //1282 Crear
                //1284 cancelar; 
                //1285 Restablecer; 
                //1286 Cerrar; 
                //1288 Registro siguiente;
                //1289 Registro anterior; 
                //1290 Primer Registro; 
                //1291 Ultimo Registro; 
                //1287 Duplicar;

                if ((pVal.MenuUID != "") && (pVal.BeforeAction == false))
                {
                    if ((pVal.MenuUID == "1288") || (pVal.MenuUID == "1289") || (pVal.MenuUID == "1290") || (pVal.MenuUID == "1291"))
                    {
                        oForm.Freeze(true);
                        //oForm.Items.Item("VID_Estado").Enabled = false;

                        if (oForm.BusinessObject.Type != "67")
                            oComboBox = (ComboBox)(oForm.Items.Item("88").Specific);
                        else
                            oComboBox = (ComboBox)(oForm.Items.Item("40").Specific);
                        var sSeries = (System.String)(oComboBox.Value);

                        if (GlobalSettings.RunningUnderSQLServer)
                            s = @"select LEFT(ISNULL(UPPER(BeginStr),''),1) 'Valor' from NNM1 where Series = {0} ";
                        else
                            s = @"select LEFT(IFNULL(UPPER(""BeginStr""),''),1) ""Valor"" from ""NNM1"" where ""Series"" = {0} ";
                        s = String.Format(s, sSeries);
                        oRecordSet.DoQuery(s);
                        if (oRecordSet.RecordCount > 0)
                        {
                            if ((System.String)(oRecordSet.Fields.Item("Valor").Value) == "E")
                            {
                                oForm.Items.Item("VID_Estado").Visible = true;
                                oForm.Items.Item("VID_Estado").Enabled = false;
                                oForm.Items.Item("lblEstado").Visible = true;
                            }
                            else
                            {
                                oForm.Items.Item("VID_Estado").Visible = false;
                                oForm.Items.Item("lblEstado").Visible = false;
                            }
                        }
                        oForm.Freeze(false);
                    }

                    if ((pVal.MenuUID == "1282") || (pVal.MenuUID == "1281") || (pVal.MenuUID == "1287"))
                    {
                        oForm.Freeze(true);
                        //oForm.Items.Item("VID_Estado").Enabled = false;
                        if (oForm.BusinessObject.Type != "67")
                            oComboBox = (ComboBox)(oForm.Items.Item("88").Specific);
                        else
                            oComboBox = (ComboBox)(oForm.Items.Item("40").Specific);
                        var sSeries = (System.String)(oComboBox.Value);

                        if (GlobalSettings.RunningUnderSQLServer)
                            s = @"select LEFT(ISNULL(UPPER(BeginStr),''),1) 'Valor' from NNM1 where Series = {0}";
                        else
                            s = @"select LEFT(IFNULL(UPPER(""BeginStr""),''),1) ""Valor"" from ""NNM1"" where ""Series"" = {0} ";
                        s = String.Format(s, sSeries);
                        oRecordSet.DoQuery(s);
                        if (oRecordSet.RecordCount > 0)
                        {
                            if ((System.String)(oRecordSet.Fields.Item("Valor").Value) == "E")
                            {
                                oForm.Items.Item("VID_Estado").Visible = true;
                                oForm.Items.Item("VID_Estado").Enabled = false;
                                oForm.Items.Item("lblEstado").Visible = true;
                            }
                            else
                            {
                                oForm.Items.Item("VID_Estado").Visible = false;
                                oForm.Items.Item("lblEstado").Visible = false;
                            }

                            if ((pVal.MenuUID == "1282") || (pVal.MenuUID == "1287"))
                            {
                                ((ComboBox)oForm.Items.Item("VID_Estado").Specific).Select("N", BoSearchKey.psk_ByValue);
                            }
                        }
                        oForm.Freeze(false);
                    }
                }
            }
            catch (Exception e)
            {
                FSBOApp.MessageBox(e.Message + " ** Trace: " + e.StackTrace, 1, "Ok", "", "");
                OutLog("MenuEvent: " + e.Message + " ** Trace: " + e.StackTrace);
            }
        }//fin MenuEvent


        public new void FormEvent(String FormUID, ref SAPbouiCOM.ItemEvent pVal, ref Boolean BubbleEvent)
        {
            Int32 nErr;
            String sErr;

            base.FormEvent(FormUID, ref pVal, ref BubbleEvent);
            try
            {
                if ((pVal.EventType == BoEventTypes.et_ITEM_PRESSED) && (pVal.BeforeAction))
                {
                    if ((pVal.ItemUID == "1") && (oForm.Mode == BoFormMode.fm_ADD_MODE))
                    {
                        ;// BubbleEvent = ValidarDatosFE();
                    }
                }

                if ((pVal.EventType == BoEventTypes.et_ITEM_PRESSED) && (!pVal.BeforeAction))
                {
                    if (pVal.ItemUID == "VID_FEDCTO")
                    {
                        oForm.PaneLevel = 333;
                    }
                }

                if ((pVal.EventType == BoEventTypes.et_FORM_ACTIVATE) && (!pVal.BeforeAction))
                {
                    GlobalSettings.PrevFormUID = oForm.UniqueID;
                }

                if (((pVal.ItemUID == "40") || (pVal.ItemUID == "88")) && (pVal.EventType == BoEventTypes.et_COMBO_SELECT) && (!pVal.BeforeAction))
                {
                    if (oForm.BusinessObject.Type != "67")
                        oComboBox = (ComboBox)(oForm.Items.Item("88").Specific);
                    else
                        oComboBox = (ComboBox)(oForm.Items.Item("40").Specific);
                    var sSeries = (System.String)(oComboBox.Value);

                    if (GlobalSettings.RunningUnderSQLServer)
                        s = @"select LEFT(ISNULL(UPPER(BeginStr),''),1) 'Valor' from NNM1 where Series = {0} ";
                    else
                        s = @"select LEFT(IFNULL(UPPER(""BeginStr""),''),1) ""Valor"" from ""NNM1"" where ""Series"" = {0} ";
                    s = String.Format(s, sSeries);
                    oRecordSet.DoQuery(s);
                    if (oRecordSet.RecordCount > 0)
                    {
                        if ((System.String)(oRecordSet.Fields.Item("Valor").Value) == "E")
                        {
                            oForm.Items.Item("VID_Estado").Visible = true;
                            oForm.Items.Item("lblEstado").Visible = true;
                        }
                        else
                        {
                            oForm.Items.Item("VID_Estado").Visible = false;
                            oForm.Items.Item("lblEstado").Visible = false;
                        }
                    }
                }

            }
            catch (Exception e)
            {
                FCmpny.GetLastError(out nErr, out sErr);
                FSBOApp.StatusBar.SetText("FormEvent: " + e.Message + " ** Trace: " + e.StackTrace, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                OutLog("FormEvent: " + e.Message + " ** Trace: " + e.StackTrace);
                if (oForm != null)
                    oForm.Freeze(false);
            }
        }//fin FormEvent


        public new void FormDataEvent(ref BusinessObjectInfo BusinessObjectInfo, ref Boolean BubbleEvent)
        {
            String sDocEntry;
            String sDocSubType;
            String TipoDocElec = "";
            Int32 lRetCode;
            String Tipo;
            TFunctions Reg;
            String[] FE09 = { "15", "67", "21" };
            XmlDocument _xmlDocument;
            XmlNode N;
            SAPbobsCOM.Documents oDocument = null;
            SAPbobsCOM.StockTransfer oTransfer = null;
            String TaxIdNum;
            String Canceled = "";
            Int32 FolioNum;
            Int32 FDocEntry = 0;
            Int32 FLineId = -1;
            String Tabla;
            String TTipoDoc;

            base.FormDataEvent(ref BusinessObjectInfo, ref BubbleEvent);

            try
            {
                if ((BusinessObjectInfo.BeforeAction == false) && (BusinessObjectInfo.EventType == BoEventTypes.et_FORM_DATA_ADD) && (BusinessObjectInfo.ActionSuccess))
                {
                    if (FE09.Contains(oForm.BusinessObject.Type))
                    {
                        if (oForm.BusinessObject.Type == "67")
                        {
                            _xmlDocument = new XmlDocument();
                            _xmlDocument.LoadXml(BusinessObjectInfo.ObjectKey);
                            N = _xmlDocument.SelectSingleNode("StockTransferParams");
                            sDocEntry = ((System.String)N.InnerText).Trim();
                        }
                        else
                            sDocEntry = FSBOf.GetDocEntryBusinessObjectInfo(BusinessObjectInfo.ObjectKey);
                        if (oForm.BusinessObject.Type == "21")
                        {
                            Tabla = "ORPD";
                            TTipoDoc = "09D";
                        }
                        else if (oForm.BusinessObject.Type == "67")
                        {
                            Tabla = "OWTR";
                            TTipoDoc = "09T";
                        }
                        else //if (oForm.BusinessObject.Type == "67")
                        {
                            Tabla = "ODLN";
                            TTipoDoc = "09";
                        }

                        if (GlobalSettings.RunningUnderSQLServer)
                            s = @"select T0.DocSubType, SUBSTRING(UPPER(T2.BeginStr), 1, 1) 'Tipo'
                                                 ,ISNULL(T0.U_BPP_MDTD,'') BPP_MDTD, ISNULL(T0.U_BPP_MDSD,'') BPP_MDSD, ISNULL(T0.U_BPP_MDCD,'') BPP_MDCD, T0.CANCELED
                                             FROM {1} T0 WITH (NOLOCK)
                                                JOIN NNM1 T2 WITH (NOLOCK) ON T0.Series = T2.Series 
                                               WHERE T0.DocEntry = {0}";
                        else
                            s = @"select T0.""DocSubType"", SUBSTRING(UPPER(T2.""BeginStr""), 1, 1) ""Tipo""
                                                 ,IFNULL(T0.""U_BPP_MDTD"",'') ""BPP_MDTD"", IFNULL(T0.""U_BPP_MDSD"",'') ""BPP_MDSD"", IFNULL(T0.""U_BPP_MDCD"",'') ""BPP_MDCD"", T0.""CANCELED""
                                             FROM ""{1}"" T0
                                             JOIN ""NNM1"" T2 ON T0.""Series"" = T2.""Series""
                                            WHERE T0.""DocEntry"" = {0} ";
                        s = String.Format(s, sDocEntry, Tabla);
                        oRecordSet.DoQuery(s);
                        sDocSubType = (System.String)(oRecordSet.Fields.Item("DocSubType").Value);
                        Canceled = (System.String)(oRecordSet.Fields.Item("CANCELED").Value);

                        if (Canceled == "N")
                        {
                            if (((System.String)oRecordSet.Fields.Item("BPP_MDTD").Value).Trim() == "")
                                FSBOApp.StatusBar.SetText("No se encuentra ingresado tipo de documento", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                            else if (((System.String)oRecordSet.Fields.Item("BPP_MDSD").Value).Trim() == "")
                                FSBOApp.StatusBar.SetText("No se encuentra ingresado serie del documento", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                            else if (((System.String)oRecordSet.Fields.Item("BPP_MDCD").Value).Trim() == "")
                                FSBOApp.StatusBar.SetText("No se encuentra ingresado correlativo del documento", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                            else
                            {
                                SBO_f = FSBOf;
                                TipoDocElec = "09";
                                EnviarFE_PE_ED(sDocEntry, GlobalSettings.RunningUnderSQLServer, ((System.String)oRecordSet.Fields.Item("BPP_MDSD").Value).Trim(), ((System.String)oRecordSet.Fields.Item("BPP_MDCD").Value).Trim(), TipoDocElec, oForm.BusinessObject.Type, sDocSubType, RUC, TTipoDoc);
                            }
                        }
                        //--
                    }
                }
            }
            catch (Exception e)
            {
                OutLog("FormDataEvent - " + e.Message + " ** Trace: " + e.StackTrace);
                FSBOApp.StatusBar.SetText(e.Message + " ** Trace: " + e.StackTrace, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
            }

        }//fin FormDataEvent


        public new void PrintEvent(ref SAPbouiCOM.PrintEventInfo eventInfo, ref Boolean BubbleEvent)
        {
            //XmlDocument _xmlDocument;
            //XmlNode N;

            base.PrintEvent(ref eventInfo, ref BubbleEvent);

            //oForm = FSBOApp.Forms.Item(eventInfo.FormUID);
        }//fin PrintEvent

        public new void ReportDataEvent(ref SAPbouiCOM.ReportDataInfo eventInfo, ref Boolean BubbleEvent)
        {
            base.ReportDataEvent(ref eventInfo, ref BubbleEvent);

            //oForm = FSBOApp.Forms.Item(eventInfo.FormUID);
        }//fin ReportDataEvent

        //Para PEru EasyDot
        public new void EnviarFE_PE_ED(String DocEntry, Boolean RunningUnderSQLServer, String SeriePE, String FolioNum, String TipoDocElec, String sObjType, String DocSubType, String lRUC, String TipoDocElecAddon)
        {
            String URL;
            String URL_PDF;
            String ProcedimientoE;
            String ProcedimientoD;
            String ProcedimientoR;
            XmlDocument oXml = null;
            String sXML = "";
            String userED;
            String passED;
            TFunctions Reg = new TFunctions();
            SAPbobsCOM.Company Cmpny = SBO_f.Cmpny;
            Reg.SBO_f = SBO_f;
            String Status;
            String sMessage = "";
            Int32 lRetCode;
            String DocDate = "";
            XDocument miXML = null;
            XElement xNodo = null;
            String ExternalFolio;
            TDLLparaXMLPE Dll = new TDLLparaXMLPE();
            Dll.SBO_f = SBO_f;
            String MostrarXML = "N";
            SAPbobsCOM.Recordset ors = ((SAPbobsCOM.Recordset)Cmpny.GetBusinessObject(BoObjectTypes.BoRecordset));

            try
            {
                if (RunningUnderSQLServer)
                    s = @"SELECT U_URLEasyDot 'URL', ISNULL(U_UserED,'') 'User', ISNULL(U_PwdED,'') 'Pass', ISNULL(U_MostrarXML,'N') 'MostrarXML' FROM [@VID_FEPARAM]";
                else
                    s = @"SELECT ""U_URLEasyDot"" ""URL"", IFNULL(""U_UserED"",'') ""User"", IFNULL(""U_PwdED"",'') ""Pass"", IFNULL(""U_MostrarXML"",'N') ""MostrarXML"" FROM ""@VID_FEPARAM"" ";

                ors.DoQuery(s);
                if (ors.RecordCount == 0)
                    SBO_f.SBOApp.StatusBar.SetText("No se ha ingresado URL", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                else if (((System.String)ors.Fields.Item("URL").Value).Trim() == "")
                    SBO_f.SBOApp.StatusBar.SetText("No se ha ingresado URL", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                else if (((System.String)ors.Fields.Item("User").Value).Trim() == "")
                    throw new Exception("No se encuentra usuario en Parametros");
                else if (((System.String)ors.Fields.Item("Pass").Value).Trim() == "")
                    throw new Exception("No se encuentra password en Parametros");
                else
                {
                    userED = Reg.DesEncriptar((System.String)(ors.Fields.Item("User").Value).ToString().Trim());
                    passED = Reg.DesEncriptar((System.String)(ors.Fields.Item("Pass").Value).ToString().Trim());
                    MostrarXML = ((System.String)ors.Fields.Item("MostrarXML").Value).Trim();

                    URL = ((System.String)ors.Fields.Item("URL").Value).Trim() + "/SendGuiaRemision.ashx";
                    URL_PDF = ((System.String)ors.Fields.Item("URL").Value).Trim() + "/SendPdf.ashx";
                    //validar que exista procedimentos para tipo documento
                    if (RunningUnderSQLServer)
                        s = "select ISNULL(U_ProcNomE,'') 'ProcNomE', ISNULL(U_ProcNomD,'') 'ProcNomD', ISNULL(U_ProcNomR,'') 'ProcNomR' FROM [@VID_FEPROCED] where ISNULL(U_Habili,'N') = 'Y' and U_TipoDocPE = '{0}'";
                    else
                        s = @"select IFNULL(""U_ProcNomE"",'') ""ProcNomE"", IFNULL(""U_ProcNomD"",'') ""ProcNomD"", IFNULL(""U_ProcNomR"",'') ""ProcNomR"" FROM ""@VID_FEPROCED"" where IFNULL(""U_Habili"",'N') = 'Y' and ""U_TipoDocPE"" = '{0}'";

                    s = String.Format(s, TipoDocElec);
                    ors.DoQuery(s);
                    if (ors.RecordCount == 0)
                        throw new Exception("No se encuentra procedimientos para Documento electronico " + TipoDocElec);
                    else
                    {
                        ProcedimientoE = ((System.String)ors.Fields.Item("ProcNomE").Value).Trim();
                        ProcedimientoD = ((System.String)ors.Fields.Item("ProcNomD").Value).Trim();
                        ProcedimientoR = ((System.String)ors.Fields.Item("ProcNomR").Value).Trim();

                        if (ProcedimientoE == "")
                            throw new Exception("No se encuentra procedimiento de encabezado para Documento electronico " + TipoDocElec);
                        else if (ProcedimientoD == "")
                            throw new Exception("No se encuentra procedimiento de detalle para Documento electronico " + TipoDocElec);
                        //else if ((ProcedimientoR == "") && (TipoDocElec == "08"))
                        //    throw new Exception("No se encuentra procedimiento de referencia para Documento electronico " + TipoDocElec);

                        if (RunningUnderSQLServer)
                            s = @"exec {0} {1}, '{2}', '{3}'";//Encabezado
                        else
                            s = @"CALL {0} ({1}, '{2}', '{3}')";
                        s = String.Format(s, ProcedimientoE, DocEntry, TipoDocElec, sObjType);
                        ors.DoQuery(s);

                        if (ors.RecordCount == 0)
                            throw new Exception("No se encuentra datos para Documento electronico " + TipoDocElec);
                        else
                        {
                            var bImpresion = false;
                            ExternalFolio = ((System.String)ors.Fields.Item("IdDocumento").Value).Trim();
                            miXML = new XDocument(
                             new XDeclaration("1.0", "utf-8", "yes")
                                    , new XElement("DocumentoElectronico"));

                            sXML = Dll.GenerarXMLStringDelivery(ref ors, TipoDocElec, ref miXML, "E");
                            if (sXML == "")
                                throw new Exception("Problema para generar xml Documento electronico " + TipoDocElec);

                            //para REFERENCIA
                            if (RunningUnderSQLServer)
                                s = @"exec {0} {1}, '{2}', '{3}'";
                            else
                                s = @"CALL {0} ({1}, '{2}', '{3}')";
                            s = String.Format(s, ProcedimientoR, DocEntry, TipoDocElec, sObjType);
                            ors.DoQuery(s);

                            if (ors.RecordCount > 0)
                            {
                                sXML = Dll.GenerarXMLStringDelivery(ref ors, TipoDocElec, ref miXML, "R");
                                if (sXML == "")
                                    throw new Exception("Problema para generar xml Documento electronico " + TipoDocElec);
                            }

                            //DETALLE
                            if (RunningUnderSQLServer)
                                s = @"exec {0} {1}, '{2}', '{3}'";
                            else
                                s = @"CALL {0} ({1}, '{2}', '{3}')";
                            s = String.Format(s, ProcedimientoD, DocEntry, TipoDocElec, sObjType);
                            ors.DoQuery(s);

                            if (ors.RecordCount == 0)
                                throw new Exception("No se encuentra datos de detalle para Documento electronico " + TipoDocElec);
                            else
                            {
                                sXML = Dll.GenerarXMLStringDelivery(ref ors, TipoDocElec, ref miXML, "D");
                                if (sXML == "")
                                    throw new Exception("Problema para generar xml Documento electronico " + TipoDocElec);

                                oXml = new XmlDocument();
                                using (var xmlReader = miXML.CreateReader())
                                {
                                    oXml.Load(xmlReader);
                                }

                                //Agrega el PDF al xml
                                XmlNode node;
                                if (oXml.SelectSingleNode("//CamposExtras") == null)
                                    node = oXml.CreateNode(XmlNodeType.Element, "CamposExtras", null);
                                else
                                    node = oXml.SelectSingleNode("//CamposExtras");

                                if (MostrarXML == "Y")
                                    SBO_f.oLog.OutLog(oXml.InnerXml);
                                //ENVIO AL PORTAL
                                s = Reg.UpLoadDocumentByUrl2(oXml, null, RunningUnderSQLServer, URL, userED, passED, TipoDocElec + "_" + ExternalFolio);

                                oXml.LoadXml(s);
                                //var Configuracion = oXml.GetElementsByTagName("Error");
                                var lista = ((XmlElement)oXml.GetElementsByTagName("Error")[0]).GetElementsByTagName("ErrorText");
                                var ErrorText = lista[0].InnerText;
                                if (ErrorText.Length > 250)
                                    ErrorText = ErrorText.Substring(0, 250);
                                lista = ((XmlElement)oXml.GetElementsByTagName("Error")[0]).GetElementsByTagName("ErrorCode");
                                var ErrorCode = lista[0].InnerText;
                                lista = ((XmlElement)oXml.GetElementsByTagName("Error")[0]).GetElementsByTagName("PDF417");
                                var PDF417 = lista[0].InnerText;
                                lista = ((XmlElement)oXml.GetElementsByTagName("Error")[0]).GetElementsByTagName("IdDocument");
                                var IdDocument = lista[0].InnerText;

                                if (ErrorCode != "0")
                                {
                                    SBO_f.SBOApp.StatusBar.SetText("Error envio documento electrónico (1)", SAPbouiCOM.BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                    SBO_f.oLog.OutLog("Error en envio documento electronico al portal (1) Codigo Error Portal: " + ErrorCode + " Mensaje Portal: " + ErrorText);
                                    //sObjType = "13";
                                    if (ErrorCode == "-103")
                                        Status = "RR";
                                    else
                                        Status = "EE";
                                    sMessage = ErrorText;
                                    var emsg = sMessage;
                                    if (sMessage == "")
                                        sMessage = "Error envio documento electronico a EasyDot";

                                    if (TipoDocElecAddon == "09T")
                                    {
                                        var oStockTransfer = (SAPbobsCOM.StockTransfer)(Cmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oStockTransfer));
                                        if (oStockTransfer.GetByKey(Convert.ToInt32(DocEntry)))
                                        {
                                            DocDate = SBO_f.DateToStr(oStockTransfer.DocDate);
                                            if (emsg == "")
                                                oStockTransfer.UserFields.Fields.Item("U_EstadoFE").Value = "N";
                                            else if (Status == "EE")
                                                oStockTransfer.UserFields.Fields.Item("U_EstadoFE").Value = "P";
                                            else if (Status == "RR")
                                                oStockTransfer.UserFields.Fields.Item("U_EstadoFE").Value = "A";
                                            else
                                                oStockTransfer.UserFields.Fields.Item("U_EstadoFE").Value = "P";

                                            lRetCode = oStockTransfer.Update();
                                            if (lRetCode != 0)
                                            {
                                                s = SBO_f.Cmpny.GetLastErrorDescription();
                                                SBO_f.SBOApp.StatusBar.SetText("Error actualizar documento - " + s, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                                                sMessage = "Error actualizar documento - " + s;
                                                SBO_f.oLog.OutLog(sMessage);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var oDocumento1 = (SAPbobsCOM.Documents)(Cmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oInvoices));
                                        if (oDocumento1.GetByKey(Convert.ToInt32(DocEntry)))
                                        {
                                            DocDate = SBO_f.DateToStr(oDocumento1.DocDate);
                                            if (emsg == "")
                                                oDocumento1.UserFields.Fields.Item("U_EstadoFE").Value = "N";
                                            else if (Status == "EE")
                                                oDocumento1.UserFields.Fields.Item("U_EstadoFE").Value = "P";
                                            else if (Status == "RR")
                                                oDocumento1.UserFields.Fields.Item("U_EstadoFE").Value = "A";
                                            else
                                                oDocumento1.UserFields.Fields.Item("U_EstadoFE").Value = "P";

                                            lRetCode = oDocumento1.Update();
                                            if (lRetCode != 0)
                                            {
                                                s = SBO_f.Cmpny.GetLastErrorDescription();
                                                SBO_f.SBOApp.StatusBar.SetText("Error actualizar documento - " + s, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                                                sMessage = "Error actualizar documento - " + s;
                                                SBO_f.oLog.OutLog(sMessage);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    if (PDF417 == "")
                                    {
                                        SBO_f.oLog.OutLog("No se ha recibido PDF417 -> " + ExternalFolio);
                                        SBO_f.SBOApp.StatusBar.SetText("No se ha recibido PDF417", SAPbouiCOM.BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                    }
                                    Status = "RR";
                                    //sObjType = "13";
                                    sMessage = "Enviado satisfactoriamente a EasyDot y Aceptado";
                                    SBO_f.SBOApp.StatusBar.SetText("Se ha enviado satisfactoriamente el documento electronico", SAPbouiCOM.BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success);

                                    if (TipoDocElecAddon == "09T")
                                    {
                                        var oStockTransfer = (SAPbobsCOM.StockTransfer)(Cmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oStockTransfer));
                                        if (oStockTransfer.GetByKey(Convert.ToInt32(DocEntry)))
                                        {
                                            DocDate = SBO_f.DateToStr(oStockTransfer.DocDate);
                                            oStockTransfer.UserFields.Fields.Item("U_EstadoFE").Value = "A";
                                            oStockTransfer.UserFields.Fields.Item("U_PDF417").Value = PDF417.Trim();
                                            lRetCode = oStockTransfer.Update();
                                            if (lRetCode != 0)
                                            {
                                                s = SBO_f.Cmpny.GetLastErrorDescription();
                                                SBO_f.SBOApp.StatusBar.SetText("Error actualizar documento - " + s, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                                                sMessage = "Error actualizar documento - " + s;
                                                SBO_f.oLog.OutLog(sMessage);
                                            }
                                            else
                                                bImpresion = true;
                                        }
                                        else
                                        {
                                            sMessage = "No se ha encontrado documento al actualizar Impresion";
                                            bImpresion = false;
                                        }
                                    }
                                    else
                                    {
                                        SAPbobsCOM.Documents oDocumento;
                                        if (TipoDocElecAddon == "09D")
                                            oDocumento = (SAPbobsCOM.Documents)(Cmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oPurchaseReturns));
                                        else
                                            oDocumento = (SAPbobsCOM.Documents)(Cmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oDeliveryNotes));
                                        if (oDocumento.GetByKey(Convert.ToInt32(DocEntry)))
                                        {
                                            DocDate = SBO_f.DateToStr(oDocumento.DocDate);
                                            oDocumento.Printed = PrintStatusEnum.psYes;
                                            oDocumento.UserFields.Fields.Item("U_EstadoFE").Value = "A";
                                            oDocumento.UserFields.Fields.Item("U_PDF417").Value = PDF417.Trim();
                                            lRetCode = oDocumento.Update();
                                            if (lRetCode != 0)
                                            {
                                                s = SBO_f.Cmpny.GetLastErrorDescription();
                                                SBO_f.SBOApp.StatusBar.SetText("Error actualizar documento - " + s, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                                                sMessage = "Error actualizar documento - " + s;
                                                SBO_f.oLog.OutLog(sMessage);
                                            }
                                            else
                                                bImpresion = true;
                                        }
                                        else
                                        {
                                            sMessage = "No se ha encontrado documento al actualizar Impresion";
                                            bImpresion = false;
                                        }
                                    }
                                }
                                oXml = null;

                                if (RunningUnderSQLServer)
                                    s = "SELECT DocEntry, U_Status, U_Id FROM [@VID_FELOG] WITH (NOLOCK) WHERE U_DocEntry = {0} AND U_ObjType = '{1}' AND U_SubType = '{2}'";
                                else
                                    s = @"SELECT ""DocEntry"", ""U_Status"", ""U_Id"" FROM ""@VID_FELOG"" WHERE ""U_DocEntry"" = {0} AND ""U_ObjType"" = '{1}' AND ""U_SubType"" = '{2}' ";
                                s = String.Format(s, DocEntry, sObjType, DocSubType);
                                ors.DoQuery(s);
                                if (ors.RecordCount == 0)
                                    Reg.FELOGAdd(Int32.Parse(DocEntry), sObjType, DocSubType, SeriePE, Int32.Parse(FolioNum), Status, sMessage, TipoDocElec, SBO_f.SBOApp.Company.UserName, "", IdDocument, ErrorText, DocDate, ExternalFolio);
                                else
                                {
                                    if ((System.String)(ors.Fields.Item("U_Status").Value) != "RR")
                                    {
                                        SBO_f.SBOApp.StatusBar.SetText("Documento se ha enviado a EasyDot", SAPbouiCOM.BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success);
                                        Reg.FELOGUptM((System.Int32)(ors.Fields.Item("DocEntry").Value), Int32.Parse(DocEntry), sObjType, DocSubType, SeriePE, Int32.Parse(FolioNum), Status, sMessage, TipoDocElec, SBO_f.SBOApp.Company.UserName, "", (IdDocument == "0" ? ((System.String)ors.Fields.Item("U_Id").Value).Trim() : IdDocument), ErrorText, DocDate, ExternalFolio);
                                    }
                                    else
                                        SBO_f.SBOApp.StatusBar.SetText("Documento ya se ha enviado anteriormente a EasyDot y se encuentra en Sunat", SAPbouiCOM.BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success);
                                }

                                if ((bImpresion) && (PDF417 != ""))
                                {
                                    //obtiene string de pdf
                                    var sPDF = Reg.PDFenString(TipoDocElecAddon, DocEntry, sObjType, SeriePE, FolioNum, RunningUnderSQLServer);
                                    /*var sjson = @"<root><DocType>{0}</DocType><DocNum>{1}</DocNum><RUC>{2}</RUC><PDF>{3}</PDF></root>";
                                    sjson = String.Format(sjson, TipoDocElec, ExternalFolio, RUC, sPDF);
                                    XmlDocument xm = new XmlDocument();
                                    xm.LoadXml(sjson);
                                    String json = JsonConvert.SerializeXmlNode(xm);*/
                                    var sjson = @"""DocType"":""{0}"", " + Environment.NewLine + @"""DocNum"":""{1}"", " + Environment.NewLine + @"""RUC"":""{2}""," + Environment.NewLine + @"""PDF"":""{3}""";
                                    sjson = String.Format(sjson, TipoDocElec, ExternalFolio, lRUC, sPDF);
                                    sjson = "{" + Environment.NewLine + sjson + Environment.NewLine + "}";
                                    s = Reg.UpLoadDocumentByUrl(null, sjson, RunningUnderSQLServer, URL_PDF, userED, passED, TipoDocElec + "_" + ExternalFolio);
                                    var results = JsonConvert.DeserializeObject<dynamic>(s);
                                    var jStatus = results.Status;
                                    var jDescripcion = results.Descripcion;

                                    if (jStatus.Value == "OK")
                                        SBO_f.SBOApp.StatusBar.SetText("PDF enviado al portal", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success);
                                    else
                                    {
                                        SBO_f.SBOApp.StatusBar.SetText("PDF no se ha enviado al portal, " + ((System.String)jDescripcion.Value).Trim(), BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                        SBO_f.oLog.OutLog("PDF no se ha enviado al portal, Tipo Doc " + TipoDocElec + ", Folio " + ExternalFolio + " -> " + ((System.String)jDescripcion.Value).Trim());
                                    }
                                }
                            }
                        }
                    }
                }

            }
            catch (Exception x)
            {
                SBO_f.SBOApp.StatusBar.SetText("EnviarFE_PE_ED: " + x.Message + " ** Trace: " + x.StackTrace, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                SBO_f.oLog.OutLog("EnviarFE_PE_ED: " + x.Message + " ** Trace: " + x.StackTrace);
            }
            finally
            {
                SBO_f._ReleaseCOMObject(ors);
                SBO_f._ReleaseCOMObject(oXml);
                SBO_f._ReleaseCOMObject(miXML);
                SBO_f._ReleaseCOMObject(xNodo);
            }
        }

    }//fin Class
}
