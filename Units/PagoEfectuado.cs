using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Configuration;
using System.Threading;
using System.Data.SqlClient;
using System.Xml;
using System.IO;
using System.Data;
using System.Xml.Linq;
using VisualD.GlobalVid;
using VisualD.SBOFunctions;
using VisualD.vkBaseForm;
using VisualD.MultiFunctions;
using VisualD.vkFormInterface;
using VisualD.SBOObjectMg1;
using VisualD.Main;
using VisualD.MainObjBase;
using Newtonsoft.Json;
using SAPbouiCOM;
using SAPbobsCOM;
using VisualD.ADOSBOScriptExecute;
using Factura_Electronica_VK.Functions;
using DLLparaXMLPE;

namespace Factura_Electronica_VK.PagoEfectuado
{
    public class TPagoEfectuado : TvkBaseForm, IvkFormInterface
    {
        private List<string> Lista;
        private SAPbobsCOM.Recordset oRecordSet;
        private SAPbouiCOM.DBDataSource oDBDSHeader;
        private SAPbouiCOM.Form oForm;
        private SAPbouiCOM.StaticText oStatic;
        private SAPbouiCOM.EditText oEditText;
        private SAPbouiCOM.ComboBox oComboBox;
        private TFunctions Funciones = new TFunctions();
        private CultureInfo _nf = new System.Globalization.CultureInfo("en-US");
        private String s;
        private String RUC;
        private String SerieAnterior = "";
        public VisualD.SBOFunctions.CSBOFunctions SBO_f;

        public static Boolean bFolderAdd
        { get; set; }
        public static String ObjType
        { get; set; }

        public new bool InitForm(string uid, string xmlPath, ref Application application, ref SAPbobsCOM.Company company, ref CSBOFunctions SBOFunctions, ref TGlobalVid _GlobalSettings)
        {
            Int32 CantRol;
            SAPbouiCOM.Folder oFolder;
            SAPbouiCOM.Item oItem;
            SAPbouiCOM.Item oItemB;
            Boolean Flag;


            bool Result = base.InitForm(uid, xmlPath, ref application, ref company, ref SBOFunctions, ref _GlobalSettings);

            oRecordSet = (SAPbobsCOM.Recordset)(FCmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset));
            Funciones.SBO_f = FSBOf;
            try
            {
                Lista = new List<string>();
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


                //Campo con el estado de DTE
                oItemB = oForm.Items.Item("53");
                oItem = oForm.Items.Add("lblEstado", SAPbouiCOM.BoFormItemTypes.it_STATIC);
                oItem.Left = oItemB.Left;
                oItem.Width = oItemB.Width;
                oItem.Top = oItemB.Top + oItemB.Height + 5;
                oItem.Height = oItem.Height;
                oItem.LinkTo = "VID_FEEstado";
                oStatic = (StaticText)(oForm.Items.Item("lblEstado").Specific);
                oStatic.Caption = "Estado Doc. Electronico";

                oItemB = oForm.Items.Item("52");
                oItem = oForm.Items.Add("VID_Estado", SAPbouiCOM.BoFormItemTypes.it_COMBO_BOX);
                oItem.Left = oItemB.Left;
                oItem.Width = oItemB.Width;
                oItem.Top = oItemB.Top + oItemB.Height + 5;
                oItem.Height = oItem.Height;
                oItem.DisplayDesc = true;
                oItem.Enabled = false;
                oComboBox = (ComboBox)(oForm.Items.Item("VID_Estado").Specific);

                //colocar folder con los campos necesarios en FE PERU
                //oForm.DataSources.UserDataSources.Add("VID_FEDCTO", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1);
                //oItem = oForm.Items.Add("VID_FEDCTO", SAPbouiCOM.BoFormItemTypes.it_FOLDER);

                //oItemB = oForm.Items.Item("1320002137");

                //oItem.Left = oItemB.Left + 30;
                //oItem.Width = oItemB.Width;
                //oItem.Top = oItemB.Top;
                //oItem.Height = oItem.Height;
                //oFolder = (Folder)((oItem.Specific));
                //oFolder.Caption = "Factura Electrónica";
                //oFolder.Pane = 333;
                //oFolder.DataBind.SetBound(true, "", "VID_FEDCTO");
                //para SAP 882 en adelante
                //oFolder.GroupWith("1320002137");

                //cargar campos de usuarios
                oItemB = oForm.Items.Item("lblEstado");
                oItem = oForm.Items.Add("lblPTSC", SAPbouiCOM.BoFormItemTypes.it_STATIC);
                oItem.Left = oItemB.Left;
                oItem.Width = oItemB.Width;
                oItem.Top = oItemB.Top + oItemB.Height + 5;
                oItem.Height = oItemB.Height;
                //oItem.FromPane = 333;
                //oItem.ToPane = 333;
                oItem.LinkTo = "VID_FEPTSC";
                oStatic = (StaticText)(oForm.Items.Item("lblPTSC").Specific);
                oStatic.Caption = "Serie del documento";

                oItemB = oForm.Items.Item("VID_Estado");
                oItem = oForm.Items.Add("VID_FEPTSC", SAPbouiCOM.BoFormItemTypes.it_EDIT);
                oItem.Left = oItemB.Left;
                oItem.Width = 90; // oItemB.Width;
                oItem.Top = oItemB.Top + oItemB.Height + 5;
                oItem.Height = oItemB.Height;
                //oItem.FromPane = 333;
                //oItem.ToPane = 333;
                oItem.RightJustified = true;
                oEditText = (EditText)(oForm.Items.Item("VID_FEPTSC").Specific);
                oEditText.DataBind.SetBound(true, "OVPM", "U_BPP_PTSC");

                //--
                oItemB = oForm.Items.Item("lblPTSC");
                oItem = oForm.Items.Add("lblPTCC", SAPbouiCOM.BoFormItemTypes.it_STATIC);
                oItem.Left = oItemB.Left;
                oItem.Width = oItemB.Width;
                oItem.Top = oItemB.Top + oItemB.Height + 5;
                oItem.Height = oItemB.Height;
                //oItem.FromPane = 333;
                //oItem.ToPane = 333;
                oItem.LinkTo = "VID_FEPTCC";
                oStatic = (StaticText)(oForm.Items.Item("lblPTCC").Specific);
                oStatic.Caption = "Correlativo del documento";

                oItemB = oForm.Items.Item("VID_FEPTSC");
                oItem = oForm.Items.Add("VID_FEPTCC", SAPbouiCOM.BoFormItemTypes.it_EDIT);
                oItem.Left = oItemB.Left;
                oItem.Width = 90; // oItemB.Width;
                oItem.Top = oItemB.Top + oItemB.Height + 5;
                oItem.Height = oItemB.Height;
                //oItem.FromPane = 333;
                //oItem.ToPane = 333;
                oItem.RightJustified = true;
                oEditText = (EditText)(oForm.Items.Item("VID_FEPTCC").Specific);
                oEditText.DataBind.SetBound(true, "OVPM", "U_BPP_PTCC");


            }
            catch (Exception e)
            {
                OutLog("InitForm: " + e.Message + " ** Trace: " + e.StackTrace);
                FSBOApp.MessageBox(e.Message + " ** Trace: " + e.StackTrace, 1, "Ok", "", "");
            }
            finally
            {
                if (oForm != null)
                {
                    oForm.Visible = true;
                    oForm.Freeze(false);
                }
            }


            return Result;
        }//fin InitForm

        public new void FormEvent(String FormUID, ref SAPbouiCOM.ItemEvent pVal, ref Boolean BubbleEvent)
        {
            SAPbouiCOM.DataTable oDataTable;
            SAPbouiCOM.IChooseFromListEvent oCFLEvento = null;
            base.FormEvent(FormUID, ref pVal, ref BubbleEvent);

            try
            {
                if ((pVal.ItemUID == "87") && (pVal.EventType == BoEventTypes.et_COMBO_SELECT) && (pVal.BeforeAction))
                {
                    oComboBox = (ComboBox)(oForm.Items.Item("87").Specific);
                    SerieAnterior = (System.String)(oComboBox.Value);
                }

                if ((pVal.ItemUID == "87") && (pVal.EventType == BoEventTypes.et_COMBO_SELECT) && (!pVal.BeforeAction))
                {
                    oComboBox = (ComboBox)(oForm.Items.Item("87").Specific);
                    var sSeries = (System.String)(oComboBox.Value);

                    if (GlobalSettings.RunningUnderSQLServer)
                        s = @"select LEFT(ISNULL(UPPER(BeginStr),''),1) 'Valor', DocSubType, SUBSTRING(ISNULL(UPPER(BeginStr),''),2,LEN(ISNULL(UPPER(BeginStr),''))) 'Doc', ObjectCode, SeriesName
                                from NNM1 where Series = {0} --AND ObjectCode = '{1}' ";
                    else
                        s = @"select LEFT(IFNULL(UPPER(""BeginStr""),''),1) ""Valor"", ""DocSubType"", SUBSTRING(IFNULL(UPPER(""BeginStr""),''),2,LENGTH(IFNULL(UPPER(""BeginStr""),''))) ""Doc"", ""ObjectCode"", ""SeriesName""
                                from ""NNM1"" where ""Series"" = {0} --AND ""ObjectCode"" = '{1}' ";
                    s = String.Format(s, sSeries, oForm.BusinessObject.Type);
                    oRecordSet.DoQuery(s);
                    if (oRecordSet.RecordCount > 0)
                    {
                        if ((System.String)(oRecordSet.Fields.Item("Valor").Value) == "E")
                        {
                            oForm.Items.Item("VID_Estado").Visible = true;
                            oForm.Items.Item("lblEstado").Visible = true;
                            oForm.Items.Item("VID_FEDCTO").Visible = true;
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
                if (FCmpny.InTransaction)
                    FCmpny.EndTransaction(BoWfTransOpt.wf_RollBack);
                FSBOApp.StatusBar.SetText(e.Message + " ** Trace: " + e.StackTrace, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                OutLog("FormEvent: " + e.Message + " ** Trace: " + e.StackTrace);
            }

        }//fin FormEvent

        public new void FormDataEvent(ref BusinessObjectInfo BusinessObjectInfo, ref Boolean BubbleEvent)
        {
            base.FormDataEvent(ref BusinessObjectInfo, ref BubbleEvent);
            String TipoDocElec;
            String sDocEntry;
            String Canceled;
            try
            {
                if ((BusinessObjectInfo.BeforeAction == false) && (BusinessObjectInfo.EventType == BoEventTypes.et_FORM_DATA_ADD) && (BusinessObjectInfo.ActionSuccess))
                {
                    //sDocEntry = FSBOf.GetDocEntryBusinessObjectInfo(BusinessObjectInfo.ObjectKey);
                    var _xmlDocument = new XmlDocument();
                    _xmlDocument.LoadXml(BusinessObjectInfo.ObjectKey);
                    var N = _xmlDocument.SelectSingleNode("PaymentParams");
                    sDocEntry = ((System.String)N.InnerText).Trim();

                    if (GlobalSettings.RunningUnderSQLServer)
                        s = @" SELECT SUBSTRING(UPPER(T2.BeginStr), 1, 1) 'Tipo'
                                 , ISNULL(T0.U_BPP_PTSC,'') BPP_PTSC
                                 , ISNULL(T0.U_BPP_PTCC,'') BPP_PTCC
                                 , T0.CANCELED
                                 , ISNULL(T0.U_BPP_MDTD,'') BPP_MDTD
                                 , ISNULL(T0.U_BPP_MDSD,'') BPP_MDSD
                                 , ISNULL(T0.U_BPP_MDCD,'') BPP_MDCD
                                 FROM OVPM T0 WITH (NOLOCK)
                                 JOIN NNM1 T2 WITH (NOLOCK) ON T0.Series = T2.Series 
                                 WHERE T0.DocEntry = {0}";
                    else
                        s = @" SELECT SUBSTRING(UPPER(T2.""BeginStr""), 1, 1) ""Tipo""
                                , IFNULL(T0.""U_BPP_PTSC"",'') ""BPP_PTSC""
                                , IFNULL(T0.""U_BPP_PTCC"",'') ""BPP_PTCC""
                                , T0.""CANCELED""
                                , IFNULL(T0.""U_BPP_MDTD"",'') ""BPP_MDTD""
                                , IFNULL(T0.""U_BPP_MDSD"",'') ""BPP_MDSD""
                                , IFNULL(T0.""U_BPP_MDCD"",'') ""BPP_MDCD""
                                FROM ""OVPM"" T0
                                JOIN ""NNM1"" T2 ON T0.""Series"" = T2.""Series""
                                WHERE T0.""DocEntry"" = {0} ";
                    s = String.Format(s, sDocEntry);
                    oRecordSet.DoQuery(s);
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
                            TipoDocElec = "20";
                            EnviarFE_PE_ED(sDocEntry, GlobalSettings.RunningUnderSQLServer, ((System.String)oRecordSet.Fields.Item("BPP_PTSC").Value).Trim(), ((System.String)oRecordSet.Fields.Item("BPP_PTCC").Value).Trim(), TipoDocElec, oForm.BusinessObject.Type, RUC);
                        }
                    }
                    //--
                }

            }
            catch (Exception e)
            {
                FSBOApp.StatusBar.SetText("FormDataEvent: " + e.Message + " ** Trace: " + e.StackTrace, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                OutLog("FormDataEvent: " + e.Message + " ** Trace: " + e.StackTrace);
            }
        }//fin FormDataEvent

        public new void MenuEvent(ref MenuEvent pVal, ref Boolean BubbleEvent)
        {
            //Int32 Entry;
            base.MenuEvent(ref pVal, ref BubbleEvent);
            SAPbobsCOM.Recordset orsx = ((SAPbobsCOM.Recordset)FCmpny.GetBusinessObject(BoObjectTypes.BoRecordset));
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
                if ((pVal.MenuUID != "") && (pVal.BeforeAction == false))
                {
                    //if (ObjType == "203")
                    //    DocSubType = ((System.String)oForm.DataSources.DBDataSources.Item("ODPI").GetValue("DocSubType", 0)).Trim();
                    //else
                    //    DocSubType = ((System.String)oForm.DataSources.DBDataSources.Item("OINV").GetValue("DocSubType", 0)).Trim();

                    if ((pVal.MenuUID == "1288") || (pVal.MenuUID == "1289") || (pVal.MenuUID == "1290") || (pVal.MenuUID == "1291") || (pVal.MenuUID == "1304"))
                    {
                        oForm.Freeze(true);
                        //oForm.Items.Item("VID_Estado").Enabled = false;
                        oComboBox = (ComboBox)(oForm.Items.Item("87").Specific);
                        var sSeries = (System.String)(oComboBox.Value);

                        if (GlobalSettings.RunningUnderSQLServer)
                            s = @"select LEFT(ISNULL(UPPER(BeginStr),''),1) 'Valor', DocSubType, SUBSTRING(ISNULL(UPPER(BeginStr),''),2,LEN(ISNULL(UPPER(BeginStr),''))) 'Doc', ObjectCode
                                        from NNM1 where Series = {0} --AND ObjectCode = '{1}' ";
                        else
                            s = @"select LEFT(IFNULL(UPPER(""BeginStr""),''),1) ""Valor"", ""DocSubType"", SUBSTRING(IFNULL(UPPER(""BeginStr""),''),2,LENGTH(IFNULL(UPPER(""BeginStr""),''))) ""Doc"", ""ObjectCode""
                                        from ""NNM1"" where ""Series"" = {0} --AND ""ObjectCode"" = '{1}' ";
                        s = String.Format(s, sSeries, oForm.BusinessObject.Type);
                        orsx.DoQuery(s);
                        if (orsx.RecordCount > 0)
                        {
                            if (((System.String)orsx.Fields.Item("Valor").Value).Trim() == "E")
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

                        oComboBox = (ComboBox)(oForm.Items.Item("87").Specific);
                        var sSeries = (System.String)(oComboBox.Value);

                        if (GlobalSettings.RunningUnderSQLServer)
                            s = @"select LEFT(ISNULL(UPPER(BeginStr),''),1) 'Valor', DocSubType, SUBSTRING(ISNULL(UPPER(BeginStr),''),2,LEN(ISNULL(UPPER(BeginStr),''))) 'Doc', ObjectCode
                                        from NNM1 where Series = {0} --AND ObjectCode = '{1}' ";
                        else
                            s = @"select LEFT(IFNULL(UPPER(""BeginStr""),''),1) ""Valor"", ""DocSubType"", SUBSTRING(IFNULL(UPPER(""BeginStr""),''),2,LENGTH(IFNULL(UPPER(""BeginStr""),''))) ""Doc"", ""ObjectCode""
                                        from ""NNM1"" where ""Series"" = {0} --AND ""ObjectCode"" = '{1}' ";
                        s = String.Format(s, sSeries, oForm.BusinessObject.Type);
                        orsx.DoQuery(s);
                        if (orsx.RecordCount > 0)
                        {
                            if ((System.String)(orsx.Fields.Item("Valor").Value) == "E")
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
                }
            }
            catch (Exception e)
            {
                FSBOApp.MessageBox(e.Message + " ** Trace: " + e.StackTrace, 1, "Ok", "", "");
                OutLog("MenuEvent: " + e.Message + " ** Trace: " + e.StackTrace);
            }
        }//fin MenuEvent


        //Para PEru EasyDot
        public new void EnviarFE_PE_ED(String DocEntry, Boolean RunningUnderSQLServer, String SeriePE, String FolioNum, String TipoDocElec, String sObjType, String lRUC)
        {
            String URL;
            String URL_PDF;
            String ProcedimientoE;
            String ProcedimientoD;
            XmlDocument oXml = null;
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
            SAPbobsCOM.Recordset ors = ((SAPbobsCOM.Recordset)Cmpny.GetBusinessObject(BoObjectTypes.BoRecordset));
            String sXML;
            TDLLparaXMLPE Dll = new TDLLparaXMLPE();
            Dll.SBO_f = SBO_f;
            String MostrarXML = "N";

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

                    URL = ((System.String)ors.Fields.Item("URL").Value).Trim() + "/SendRetencion.ashx";
                    URL_PDF = ((System.String)ors.Fields.Item("URL").Value).Trim() + "/SendPdf.ashx";
                    //validar que exista procedimentos para tipo documento
                    if (RunningUnderSQLServer)
                        s = "select ISNULL(U_ProcNomE,'') 'ProcNomE', ISNULL(U_ProcNomD,'') 'ProcNomD' FROM [@VID_FEPROCED] where ISNULL(U_Habili,'N') = 'Y' and U_TipoDocPE = '{0}'";
                    else
                        s = @"select IFNULL(""U_ProcNomE"",'') ""ProcNomE"", IFNULL(""U_ProcNomD"",'') ""ProcNomD"" FROM ""@VID_FEPROCED"" where IFNULL(""U_Habili"",'N') = 'Y' and ""U_TipoDocPE"" = '{0}'";

                    s = String.Format(s, TipoDocElec);
                    ors.DoQuery(s);
                    if (ors.RecordCount == 0)
                        throw new Exception("No se encuentra procedimientos para Documento electronico " + TipoDocElec);
                    else
                    {
                        ProcedimientoE = ((System.String)ors.Fields.Item("ProcNomE").Value).Trim();
                        ProcedimientoD = ((System.String)ors.Fields.Item("ProcNomD").Value).Trim();

                        if (ProcedimientoE == "")
                            throw new Exception("No se encuentra procedimiento de encabezado para Documento electronico " + TipoDocElec);
                        else if (ProcedimientoD == "")
                            throw new Exception("No se encuentra procedimiento de detalle para Documento electronico " + TipoDocElec);

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
                             new XDeclaration("1.0", "utf-8", "yes"),
                                    new XElement("DocumentoElectronico"));

                            sXML = Dll.GenerarXMLStringPayment(ref ors, TipoDocElec, ref miXML, "E");
                            if (sXML == "")
                                throw new Exception("Problema para generar xml Documento electronico " + TipoDocElec);

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
                                sXML = Dll.GenerarXMLStringPayment(ref ors, TipoDocElec, ref miXML, "D");
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

                                    var oPayments1 = (SAPbobsCOM.Documents)(Cmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oVendorPayments));
                                    if (oPayments1.GetByKey(Convert.ToInt32(DocEntry)))
                                    {
                                        DocDate = SBO_f.DateToStr(oPayments1.DocDate);
                                        if (emsg == "")
                                            oPayments1.UserFields.Fields.Item("U_EstadoFE").Value = "N";
                                        else if (Status == "EE")
                                            oPayments1.UserFields.Fields.Item("U_EstadoFE").Value = "P";
                                        else if (Status == "RR")
                                            oPayments1.UserFields.Fields.Item("U_EstadoFE").Value = "A";
                                        else
                                            oPayments1.UserFields.Fields.Item("U_EstadoFE").Value = "P";

                                        lRetCode = oPayments1.Update();
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
                                    if (PDF417 == "")
                                    {
                                        SBO_f.oLog.OutLog("No se ha recibido PDF417 -> " + ExternalFolio);
                                        SBO_f.SBOApp.StatusBar.SetText("No se ha recibido PDF417", SAPbouiCOM.BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                    }
                                    Status = "RR";
                                    //sObjType = "13";
                                    sMessage = "Enviado satisfactoriamente a EasyDot y Aceptado";
                                    SBO_f.SBOApp.StatusBar.SetText("Se ha enviado satisfactoriamente el documento electronico", SAPbouiCOM.BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success);
                                    var oPayments = ((SAPbobsCOM.Payments)Cmpny.GetBusinessObject(BoObjectTypes.oVendorPayments));
                                    if (oPayments.GetByKey(Convert.ToInt32(DocEntry)))
                                    {
                                        DocDate = SBO_f.DateToStr(oPayments.DocDate);
                                        //oPayments.Printed = PrintStatusEnum.psYes;
                                        oPayments.UserFields.Fields.Item("U_EstadoFE").Value = "A";
                                        oPayments.UserFields.Fields.Item("U_PDF417").Value = PDF417.Trim();
                                        lRetCode = oPayments.Update();
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
                                oXml = null;

                                if (RunningUnderSQLServer)
                                    s = "SELECT DocEntry, U_Status, U_Id FROM [@VID_FELOG] WITH (NOLOCK) WHERE U_DocEntry = {0} AND U_ObjType = '{1}'";
                                else
                                    s = @"SELECT ""DocEntry"", ""U_Status"", ""U_Id"" FROM ""@VID_FELOG"" WHERE ""U_DocEntry"" = {0} AND ""U_ObjType"" = '{1}' ";
                                s = String.Format(s, DocEntry, sObjType);
                                ors.DoQuery(s);
                                if (ors.RecordCount == 0)
                                    Reg.FELOGAdd(Int32.Parse(DocEntry), sObjType, "--", SeriePE, Int32.Parse(FolioNum), Status, sMessage, TipoDocElec, SBO_f.SBOApp.Company.UserName, "", IdDocument, ErrorText, DocDate, ExternalFolio);
                                else
                                {
                                    if ((System.String)(ors.Fields.Item("U_Status").Value) != "RR")
                                    {
                                        SBO_f.SBOApp.StatusBar.SetText("Documento se ha enviado a EasyDot", SAPbouiCOM.BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success);
                                        Reg.FELOGUptM((System.Int32)(ors.Fields.Item("DocEntry").Value), Int32.Parse(DocEntry), sObjType, "--", SeriePE, Int32.Parse(FolioNum), Status, sMessage, TipoDocElec, SBO_f.SBOApp.Company.UserName, "", (IdDocument == "0" ? ((System.String)ors.Fields.Item("U_Id").Value).Trim() : IdDocument), ErrorText, DocDate, ExternalFolio);
                                    }
                                    else
                                        SBO_f.SBOApp.StatusBar.SetText("Documento ya se ha enviado anteriormente a EasyDot y se encuentra en Sunat", SAPbouiCOM.BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success);
                                }

                                if ((bImpresion) && (PDF417 != ""))
                                {
                                    //obtiene string de pdf
                                    var sPDF = Reg.PDFenString(TipoDocElec, DocEntry, sObjType, SeriePE, FolioNum, RunningUnderSQLServer);
                                    /*var sjson = @"<root><DocType>{0}</DocType><DocNum>{1}</DocNum><RUC>{2}</RUC><PDF>{3}</PDF></root>";
                                    sjson = String.Format(sjson, TipoDocElec, ExternalFolio, RUC, sPDF);
                                    XmlDocument xm = new XmlDocument();
                                    xm.LoadXml(sjson);
                                    String json = JsonConvert.SerializeXmlNode(xm);*/
                                    var sjson = @"""DocType"":""{0}"",""DocNum"":""{1}"",""RUC"":""{2}"",""PDF"":""{3}""";
                                    sjson = String.Format(sjson, TipoDocElec, ExternalFolio, lRUC, sPDF);
                                    sjson = "{" + sjson + "}";
                                    s = Reg.UpLoadDocumentByUrl(null, sjson, RunningUnderSQLServer, URL_PDF, userED, passED, TipoDocElec + "_" + ExternalFolio);


                                    SBO_f.SBOApp.StatusBar.SetText("PDF enviado al portal", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success);
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

    }//fin class
}
