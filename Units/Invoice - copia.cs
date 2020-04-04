using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Globalization;
using System.Data.SqlClient;
using System.Data;
using System.IO;
using System.CodeDom.Compiler;
using System.Xml;
using System.Net.Http;
using System.Configuration;
using ServiceStack.Text;
using SAPbouiCOM;
using SAPbobsCOM;
using VisualD.GlobalVid;
using VisualD.SBOFunctions;
using VisualD.vkBaseForm;
using VisualD.MultiFunctions;
using VisualD.vkFormInterface;
using VisualD.untLog;
using Factura_Electronica_VK.Functions;
using FactRemota;
using pe.facturamovil;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using CrystalDecisions.ReportSource;
using Newtonsoft.Json;


namespace Factura_Electronica_VK.Invoice
{
    class TInvoice : TvkBaseForm, IvkFormInterface
    {
        private SAPbobsCOM.Recordset oRecordSet;
        private SAPbouiCOM.Form oForm;
        private String s;
        private Boolean Flag;
        private SAPbouiCOM.Matrix mtx;
        private SAPbouiCOM.StaticText oStatic;
        private SAPbouiCOM.EditText oEditText;
        private SAPbouiCOM.ComboBox oComboBox;
        //por Peru
        private String FacturadorPE;
        private String RUC;
        private Int32 LoginCount_FM;
        private String CCEmail_FM;
        private String Email_FM;
        private pe.facturamovil.Invoice oInvoice_FM;
        private pe.facturamovil.Ticket oTicket_FM;
        private pe.facturamovil.Note oNote_FM;
        private SAPbouiCOM.DataTable odt;
        private SAPbouiCOM.Grid ogrid;
        //private pe.facturamovil.User oUser_FM;
        private String JsonText;
        private CultureInfo _nf = new System.Globalization.CultureInfo("en-US");
        private SqlConnection ConexionADO = null;
        private String SerieAnterior = "";
        //
        private List<string> Lista;

        public static String DocSubType
        { get; set; }
        public static Boolean bFolderAdd
        { get; set; }
        public static String ObjType
        { get; set; }
        public static Boolean ReservaExp
        { get; set; }
        public VisualD.SBOFunctions.CSBOFunctions SBO_f;

        public new bool InitForm(string uid, string xmlPath, ref Application application, ref SAPbobsCOM.Company company, ref CSBOFunctions SBOFunctions, ref TGlobalVid _GlobalSettings)
        {
            SAPbouiCOM.Folder oFolder;
            SAPbouiCOM.Item oItem;
            SAPbouiCOM.Item oItemB;
            SAPbouiCOM.Matrix oMatrix;
            SAPbouiCOM.GridColumns oColumns;
            SAPbouiCOM.GridColumn oColumn;

            bool Result = base.InitForm(uid, xmlPath, ref application, ref company, ref SBOFunctions, ref _GlobalSettings);

            try
            {
                oRecordSet = (SAPbobsCOM.Recordset)(FCmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset));
                //FSBOf.LoadForm(xmlPath, 'VID_Entrega.srf', Uid);
                //var sPath : String := TMultiFunctions.ExtractFilePath(TMultiFunctions.ParamStr(0));
                //sPath := sPath + "\Forms\UpdDocuments.xml";
                //var _xml : XmlDocument := new XmlDocument();
                //_xml.Load(sPath);
                //var xmlstr : String := _xml.InnerXml;
                //xmlstr := xmlstr.Replace("F_11", uid);
                //FSBOApp.LoadBatchActions(var xmlstr);
                oForm = FSBOApp.Forms.Item(uid);
                Flag = false;
                oForm.Freeze(true);

                if (GlobalSettings.RunningUnderSQLServer)
                    s = "select ISNULL(U_FacturaP,'E') FacturadorPE from [@VID_FEPARAM] where Code = '1'";
                else
                    s = @"select IFNULL(""U_FacturaP"",'E') ""FacturadorPE"" from ""@VID_FEPARAM"" where ""Code"" = '1' ";

                oRecordSet.DoQuery(s);
                if (oRecordSet.RecordCount == 0)
                    throw new Exception("Debe parametrizar el Addon Factura Electronica");
                else
                    FacturadorPE = ((System.String)oRecordSet.Fields.Item("FacturadorPE").Value).Trim();


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
                oItemB = oForm.Items.Item("84");
                oItem = oForm.Items.Add("lblEstado", SAPbouiCOM.BoFormItemTypes.it_STATIC);
                oItem.Left = oItemB.Left;
                oItem.Width = oItemB.Width;
                oItem.Top = oItemB.Top + oItemB.Height + 5;
                oItem.Height = oItem.Height;
                oItem.LinkTo = "VID_FEEstado";
                oStatic = (StaticText)(oForm.Items.Item("lblEstado").Specific);
                oStatic.Caption = "Estado Doc. Electronico";

                oItemB = oForm.Items.Item("208");
                oItem = oForm.Items.Add("VID_Estado", SAPbouiCOM.BoFormItemTypes.it_COMBO_BOX);
                oItem.Left = oItemB.Left;
                oItem.Width = oItemB.Width + 30;
                oItem.Top = oItemB.Top + oItemB.Height + 5;
                oItem.Height = oItem.Height;
                oItem.DisplayDesc = true;
                oItem.Enabled = false;
                oComboBox = (ComboBox)(oForm.Items.Item("VID_Estado").Specific);
                if (ObjType == "13")
                    oComboBox.DataBind.SetBound(true, "OINV", "U_EstadoFE");
                else if (ObjType == "203")
                    oComboBox.DataBind.SetBound(true, "ODPI", "U_EstadoFE");

                if (FacturadorPE == "F")
                {
                    try
                    {
                        LoginCount_FM = 0;
                        //oUser_FM = new pe.facturamovil.User();
                        //if (oUser_FM.token == null)
                        if (GlobalSettings.oUser_FM.token == null)
                        {
                            //GlobalSettings.oUser_FM = new pe.facturamovil.User();
                            if (GlobalSettings.RunningUnderSQLServer)
                                oRecordSet.DoQuery("SELECT U_User,U_Pwd,U_CCEmail FROM [@VID_FEPARAM] WHERE Code = '1'");
                            else
                                oRecordSet.DoQuery(@"SELECT ""U_User"", ""U_Pwd"", ""U_CCEmail"" FROM ""@VID_FEPARAM"" WHERE ""Code"" = '1'");
                            var U_User = ((System.String)oRecordSet.Fields.Item("U_User").Value).Trim();
                            var U_Pwd = ((System.String)oRecordSet.Fields.Item("U_Pwd").Value).Trim();
                            GlobalSettings.oUser_FM = FacturaMovilGlobal.processor.Authenticate(U_User, U_Pwd);
                            FacturaMovilGlobal.userConnected = GlobalSettings.oUser_FM;
                            var ii = 0;
                            var bExistePE = false;

                            if (GlobalSettings.oUser_FM.companies.Find(c => c.code.Trim() == RUC.Trim()) != null)
                            {
                                FacturaMovilGlobal.selectedCompany = GlobalSettings.oUser_FM.companies.Single(c => c.code.Trim() == RUC.Trim());
                                bExistePE = true;
                                ii = GlobalSettings.oUser_FM.companies.Count;
                            }


                            if (!bExistePE)
                                throw new Exception("No se ha encontrado el RUC " + RUC + "en la conexion de Factura Movil");

                            CCEmail_FM = ((System.String)oRecordSet.Fields.Item("U_CCEmail").Value).Trim();
                        }
                    }
                    catch (Exception ex)
                    {
                        FSBOApp.StatusBar.SetText("No se pudo establecer conexion con el servidor Factura Movil : " + ex.Message, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                        OutLog("No se pudo establecer conexion con el servidor Factura Movil - User: " + ((System.String)oRecordSet.Fields.Item("U_User").Value).Trim() + " Pass: " + ((System.String)oRecordSet.Fields.Item("U_Pwd").Value).Trim() + " - " + ex.Message);
                    }
                }
                else if (FacturadorPE == "E")
                {
                    try
                    {
                        if (GlobalSettings.RunningUnderSQLServer)
                            s = "SELECT T0.U_Srvr 'Server', T0.U_Usr 'Usuario', T0.U_Pw 'Password' FROM [dbo].[@VID_MENUSU] T0";
                        else
                            s = @"SELECT T0.""U_Srvr"" ""Server"", T0.""U_Usr"" ""Usuario"", T0.""U_Pw"" ""Password"" FROM ""@VID_MENUSU"" T0";
                        oRecordSet.DoQuery(s);
                    }
                    catch //(Exception t)
                    {
                        FSBOApp.StatusBar.SetText("Los datos de acceso al servidor SQL no son validos (Gestion->Definiciones->Factura Electrónica->Configuración Conexión), guarde los datos", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                        FSBOApp.ActivateMenuItem("VID_RHSQL");
                        return false;
                    }

                    if (GlobalSettings.RunningUnderSQLServer)
                    {
                        ConexionADO = new SqlConnection("Data Source = " + FCmpny.Server + "; Initial Catalog = " + FCmpny.CompanyDB + "; User Id=" + ((System.String)oRecordSet.Fields.Item("Usuario").Value).Trim() + ";Password=" + ((System.String)oRecordSet.Fields.Item("Password").Value).Trim());

                        try
                        {
                            ConexionADO.Open();
                        }
                        catch //(Exception t)
                        {
                            FSBOApp.StatusBar.SetText("Los datos de acceso al servidor SQL no son validos (Gestion->Definiciones->Factura Electrónica->Configuración Conexión), guarde los datos", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                            FSBOApp.ActivateMenuItem("VID_RHSQL");
                            return false;
                        }
                        ConexionADO.Close();
                    }
                }

                //colocar folder con los campos necesarios en FE PERU
                oForm.DataSources.UserDataSources.Add("VID_FEDCTO", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1);
                oItem = oForm.Items.Add("VID_FEDCTO", SAPbouiCOM.BoFormItemTypes.it_FOLDER);

                if ((DocSubType == "--") || (DocSubType == "IB") || (DocSubType == "DN"))
                {
                    //para SAP 882 en adelante
                    oItemB = oForm.Items.Item("1320002137");

                    oItem.Left = oItemB.Left + 30;
                    oItem.Width = oItemB.Width;
                    oItem.Top = oItemB.Top;
                    oItem.Height = oItem.Height;
                    oFolder = (Folder)((oItem.Specific));
                    oFolder.Caption = "Factura Electrónica";
                    oFolder.Pane = 333;
                    oFolder.DataBind.SetBound(true, "", "VID_FEDCTO");
                    //para SAP 882 en adelante
                    oFolder.GroupWith("1320002137");

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
                    if (ObjType == "203")
                        oEditText.DataBind.SetBound(true, "ODPI", "U_BPP_MDTD");
                    else
                        oEditText.DataBind.SetBound(true, "OINV", "U_BPP_MDTD");

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
                    if (ObjType == "203")
                        oEditText.DataBind.SetBound(true, "ODPI", "U_BPP_MDSD");
                    else
                        oEditText.DataBind.SetBound(true, "OINV", "U_BPP_MDSD");

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
                    if (ObjType == "203")
                        oEditText.DataBind.SetBound(true, "ODPI", "U_BPP_MDCD");
                    else
                        oEditText.DataBind.SetBound(true, "OINV", "U_BPP_MDCD");


                    if (DocSubType == "DN")
                    {
                        //--
                        oItemB = oForm.Items.Item("lblMDCD");
                        oItem = oForm.Items.Add("lblMDTN", SAPbouiCOM.BoFormItemTypes.it_STATIC);
                        oItem.Left = oItemB.Left;
                        oItem.Width = oItemB.Width;
                        oItem.Top = oItemB.Top + oItemB.Height + 5;
                        oItem.Height = oItem.Height;
                        oItem.FromPane = 333;
                        oItem.ToPane = 333;
                        oItem.LinkTo = "VID_FEMDTN";
                        oStatic = (StaticText)(oForm.Items.Item("lblMDTN").Specific);
                        oStatic.Caption = "Tipo de operacion";

                        oItemB = oForm.Items.Item("lblMDTN");
                        oItem = oForm.Items.Add("VID_FEMDTN", SAPbouiCOM.BoFormItemTypes.it_COMBO_BOX);
                        oItem.Left = oItemB.Left + oItemB.Width + 5;
                        oItem.Width = 140; // oItemB.Width;
                        oItem.Top = oItemB.Top;
                        oItem.Height = oItem.Height;
                        oItem.FromPane = 333;
                        oItem.ToPane = 333;
                        oItem.DisplayDesc = true;
                        oComboBox = (ComboBox)(oForm.Items.Item("VID_FEMDTN").Specific);
                        oComboBox.DataBind.SetBound(true, "OINV", "U_BPP_MDTN");


                        if (GlobalSettings.RunningUnderSQLServer)
                        {
                            s = @"select Code 'Code', Name 'Name'
                                        from [@FM_NOTES] 
                                       ORDER BY Code ";
                        }
                        else
                        {
                            s = @"select ""Code"" ""Code"", ""Name"" ""Name""
                                        from ""@FM_NOTES""
                                       ORDER BY ""Code"" ";
                        }
                        oRecordSet.DoQuery(s);
                        FSBOf.FillCombo((ComboBox)(oForm.Items.Item("VID_FEMDTN").Specific), ref oRecordSet, false);

                        //--
                        oItemB = oForm.Items.Item("VID_FEMDTD");
                        oItem = oForm.Items.Add("lblFE", SAPbouiCOM.BoFormItemTypes.it_STATIC);
                        oItem.Left = oItemB.Left + oItemB.Width + 100;
                        oItem.Width = oItemB.Width + 60;
                        oItem.Top = oItemB.Top - oItemB.Height - 5;
                        oItem.Height = oItem.Height;
                        oItem.FromPane = 333;
                        oItem.ToPane = 333;
                        oItem.LinkTo = "lblMDCD";
                        oItem.Visible = false;
                        oStatic = (StaticText)(oForm.Items.Item("lblFE").Specific);
                        oStatic.Caption = "Datos documento origen";
                        oForm.Items.Item("lblFE").Visible = false;


                        //--
                        oItemB = oForm.Items.Item("lblFE");
                        oItem = oForm.Items.Add("lblMDTO", SAPbouiCOM.BoFormItemTypes.it_STATIC);
                        oItem.Left = oItemB.Left;
                        oItem.Width = oItemB.Width;
                        oItem.Top = oItemB.Top + oItemB.Height + 5;
                        oItem.Height = oItem.Height;
                        oItem.FromPane = 333;
                        oItem.ToPane = 333;
                        oItem.LinkTo = "VID_FEMDTO";
                        oStatic = (StaticText)(oForm.Items.Item("lblMDTO").Specific);
                        oStatic.Caption = "Tipo de Docto. origen";

                        oItemB = oForm.Items.Item("lblMDTO");
                        oItem = oForm.Items.Add("VID_FEMDTO", SAPbouiCOM.BoFormItemTypes.it_EDIT);
                        oItem.Left = oItemB.Left + oItemB.Width + 5;
                        oItem.Width = 90;
                        oItem.Top = oItemB.Top;
                        oItem.Height = oItem.Height;
                        oItem.FromPane = 333;
                        oItem.ToPane = 333;
                        oItem.RightJustified = true;
                        oEditText = (EditText)(oForm.Items.Item("VID_FEMDTO").Specific);
                        oEditText.DataBind.SetBound(true, "OINV", "U_BPP_MDTO");

                        //--
                        oItemB = oForm.Items.Item("lblMDTO");
                        oItem = oForm.Items.Add("lblMDSO", SAPbouiCOM.BoFormItemTypes.it_STATIC);
                        oItem.Left = oItemB.Left;
                        oItem.Width = oItemB.Width;
                        oItem.Top = oItemB.Top + oItemB.Height + 5;
                        oItem.Height = oItem.Height;
                        oItem.FromPane = 333;
                        oItem.ToPane = 333;
                        oItem.LinkTo = "VID_FEMDSO";
                        oStatic = (StaticText)(oForm.Items.Item("lblMDSO").Specific);
                        oStatic.Caption = "Serie documento origen";

                        oItemB = oForm.Items.Item("lblMDSO");
                        oItem = oForm.Items.Add("VID_FEMDSO", SAPbouiCOM.BoFormItemTypes.it_EDIT);
                        oItem.Left = oItemB.Left + oItemB.Width + 5;
                        oItem.Width = 90;
                        oItem.Top = oItemB.Top;
                        oItem.Height = oItem.Height;
                        oItem.FromPane = 333;
                        oItem.ToPane = 333;
                        oItem.RightJustified = true;
                        oEditText = (EditText)(oForm.Items.Item("VID_FEMDSO").Specific);
                        oEditText.DataBind.SetBound(true, "OINV", "U_BPP_MDSO");

                        //--
                        oItemB = oForm.Items.Item("lblMDSO");
                        oItem = oForm.Items.Add("lblMDCO", SAPbouiCOM.BoFormItemTypes.it_STATIC);
                        oItem.Left = oItemB.Left;
                        oItem.Width = oItemB.Width;
                        oItem.Top = oItemB.Top + oItemB.Height + 5;
                        oItem.Height = oItem.Height;
                        oItem.FromPane = 333;
                        oItem.ToPane = 333;
                        oItem.LinkTo = "VID_FEMDCO";
                        oStatic = (StaticText)(oForm.Items.Item("lblMDCO").Specific);
                        oStatic.Caption = "Correlativo docto. origen";

                        oItemB = oForm.Items.Item("lblMDCO");
                        oItem = oForm.Items.Add("VID_FEMDCO", SAPbouiCOM.BoFormItemTypes.it_EDIT);
                        oItem.Left = oItemB.Left + oItemB.Width + 5;
                        oItem.Width = 90;
                        oItem.Top = oItemB.Top;
                        oItem.Height = oItem.Height;
                        oItem.FromPane = 333;
                        oItem.ToPane = 333;
                        oItem.RightJustified = true;
                        oEditText = (EditText)(oForm.Items.Item("VID_FEMDCO").Specific);
                        oEditText.DataBind.SetBound(true, "OINV", "U_BPP_MDCO");
                    }
                }

            }
            catch (Exception e)
            {
                OutLog("InitForm: " + e.Message + " ** Trace: " + e.StackTrace);
                FSBOApp.MessageBox(e.Message + " ** Trace: " + e.StackTrace, 1, "Ok", "", "");
            }
            oForm.Visible = true;
            oForm.Freeze(false);
            return Result;
        }//fin InitForm


        public new void MenuEvent(ref MenuEvent pVal, ref Boolean BubbleEvent)
        {
            SAPbouiCOM.Conditions oConditions;
            SAPbouiCOM.Condition oCondition;
            String tabla;
            SAPbobsCOM.Recordset orsx = ((SAPbobsCOM.Recordset)FCmpny.GetBusinessObject(BoObjectTypes.BoRecordset));
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
                //1304 Actualizar

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
                        oComboBox = (ComboBox)(oForm.Items.Item("88").Specific);
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
                                if ((((System.String)orsx.Fields.Item("DocSubType").Value).Trim() == "IB") || (((System.String)orsx.Fields.Item("DocSubType").Value).Trim() == "EB"))
                                {
                                    oForm.Items.Item("VID_Estado").Visible = false;
                                    oForm.Items.Item("lblEstado").Visible = false;
                                }
                                else
                                {
                                    oForm.Items.Item("VID_Estado").Visible = true;
                                    oForm.Items.Item("lblEstado").Visible = true;
                                }
                            }
                            else
                            {
                                oForm.Items.Item("VID_Estado").Visible = false;
                                oForm.Items.Item("lblEstado").Visible = false;
                                //if (((System.String)orsx.Fields.Item("DocSubType").Value).Trim() != "--")
                                //{
                                //    oForm.Items.Item("VID_FEDCTO").Visible = true;
                                //    s = "112";
                                //    oForm.Items.Item(s).Click(BoCellClickType.ct_Regular);
                                //}
                            }

                        }
                        oForm.Freeze(false);
                    }

                    if ((pVal.MenuUID == "1282") || (pVal.MenuUID == "1281") || (pVal.MenuUID == "1287"))
                    {
                        oForm.Freeze(true);
                        //oForm.Items.Item("VID_Estado").Enabled = false;

                        oComboBox = (ComboBox)(oForm.Items.Item("88").Specific);
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
                                if ((((System.String)orsx.Fields.Item("DocSubType").Value).Trim() == "IB") || (((System.String)orsx.Fields.Item("DocSubType").Value).Trim() == "EB"))
                                {
                                    oForm.Items.Item("VID_Estado").Visible = false;
                                    oForm.Items.Item("lblEstado").Visible = false;
                                }
                                else
                                {
                                    oForm.Items.Item("VID_Estado").Visible = true;
                                    oForm.Items.Item("lblEstado").Visible = true;
                                }
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
                oForm.Freeze(false);
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
                        if (ObjType == "13")
                            s = (System.String)oForm.DataSources.DBDataSources.Item("OINV").GetValue("CANCELED", 0);
                        else if (ObjType == "203")
                            s = (System.String)oForm.DataSources.DBDataSources.Item("ODPI").GetValue("CANCELED", 0);

                        if (s == "N")
                            BubbleEvent = ValidarDatosFE_PE();
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
                    if (oForm.BusinessObject.Type == "203")
                        DocSubType = ((System.String)oForm.DataSources.DBDataSources.Item("ODPI").GetValue("DocSubType", 0)).Trim();
                    else
                        DocSubType = ((System.String)oForm.DataSources.DBDataSources.Item("OINV").GetValue("DocSubType", 0)).Trim();

                }

                if ((pVal.ItemUID == "88") && (pVal.EventType == BoEventTypes.et_COMBO_SELECT) && (pVal.BeforeAction))
                {
                    oComboBox = (ComboBox)(oForm.Items.Item("88").Specific);
                    SerieAnterior = (System.String)(oComboBox.Value);
                }

                if ((pVal.ItemUID == "88") && (pVal.EventType == BoEventTypes.et_COMBO_SELECT) && (!pVal.BeforeAction))
                {
                    oComboBox = (ComboBox)(oForm.Items.Item("88").Specific);
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
                        var Tab = (((System.String)oRecordSet.Fields.Item("ObjectCode").Value).Trim() == "203" ? "ODPI" : "OINV");
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
                FCmpny.GetLastError(out nErr, out sErr);
                FSBOApp.StatusBar.SetText("FormEvent: " + e.Message + " ** Trace: " + e.StackTrace, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                OutLog("FormEvent: " + e.Message + " ** Trace: " + e.StackTrace);
                if (oForm != null)
                    oForm.Freeze(false);
            }
            finally
            {
                ;
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
            Boolean bMultiSoc;
            String nMultiSoc;
            String TaxIdNum;
            String tabla = "";
            String Canceled = "";
            String GeneraT = "";
            String CAF = "";
            Int32 FolioNum;
            String TipoDocElect;
            String TTipoDoc = "";

            SAPbobsCOM.Documents oDocument;
            base.FormDataEvent(ref BusinessObjectInfo, ref BubbleEvent);

            try
            {
                ////pruebas
                //if ((BusinessObjectInfo.BeforeAction == false) && (BusinessObjectInfo.EventType == BoEventTypes.et_FORM_DATA_UPDATE) && (BusinessObjectInfo.ActionSuccess))
                //{

                //    if (oForm.BusinessObject.Type == "13") //And (Flag = true)) then
                //    {
                //        oForm.Items.Item("VID_Estado").Enabled = false;
                //    }
                //}

                if ((BusinessObjectInfo.BeforeAction == false) && (BusinessObjectInfo.EventType == BoEventTypes.et_FORM_DATA_ADD) && (BusinessObjectInfo.ActionSuccess))
                {
                    sDocEntry = FSBOf.GetDocEntryBusinessObjectInfo(BusinessObjectInfo.ObjectKey);
                    if (oForm.BusinessObject.Type == "203")
                    {
                        tabla = "ODPI";
                        TTipoDoc = "01A";
                    }
                    else
                    {
                        tabla = "OINV";
                        TTipoDoc = "01";
                    }

                    if (GlobalSettings.RunningUnderSQLServer)
                        s = @"select T0.DocSubType, SUBSTRING(UPPER(T2.BeginStr), 1, 1) 'Tipo', SUBSTRING(ISNULL(T2.BeginStr,''), 2, LEN(T2.BeginStr)) 'Inst'
                                                 ,ISNULL(T0.U_BPP_MDTD,'') BPP_MDTD, ISNULL(T0.U_BPP_MDSD,'') BPP_MDSD, ISNULL(T0.U_BPP_MDCD,'') BPP_MDCD, T0.CANCELED
                                             FROM {1} T0 WITH (NOLOCK)
                                                JOIN NNM1 T2 WITH (NOLOCK) ON T0.Series = T2.Series 
                                               WHERE T0.DocEntry = {0}";
                    else
                        s = @"select T0.""DocSubType"", SUBSTRING(UPPER(T2.""BeginStr""), 1, 1) ""Tipo"", SUBSTRING(IFNULL(T2.""BeginStr"",''), 2, LENGTH(T2.""BeginStr"")) ""Inst""
                                                 ,IFNULL(T0.""U_BPP_MDTD"",'') ""BPP_MDTD"", IFNULL(T0.""U_BPP_MDSD"",'') ""BPP_MDSD"", IFNULL(T0.""U_BPP_MDCD"",'') ""BPP_MDCD"", T0.""CANCELED""
                                             FROM ""{1}"" T0
                                             JOIN ""NNM1"" T2 ON T0.""Series"" = T2.""Series""
                                            WHERE T0.""DocEntry"" = {0} ";
                    s = String.Format(s, sDocEntry, tabla);
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
                            if (FacturadorPE == "F") //Factura Movil
                            {
                                if (sDocSubType == "--")
                                {
                                    TipoDocElec = "01";
                                    EnviarFE_PE(sDocEntry, GlobalSettings.RunningUnderSQLServer, ((System.String)oRecordSet.Fields.Item("BPP_MDSD").Value).Trim(), ((System.String)oRecordSet.Fields.Item("BPP_MDCD").Value).Trim(), TipoDocElec, oForm.BusinessObject.Type, sDocSubType, RUC, ref GlobalSettings.oUser_FM);
                                }
                                else if (sDocSubType == "DN")//Nota de Debito
                                {
                                    TipoDocElec = "08";
                                    EnviarDN_PE(sDocEntry, GlobalSettings.RunningUnderSQLServer, ((System.String)oRecordSet.Fields.Item("BPP_MDSD").Value).Trim(), ((System.String)oRecordSet.Fields.Item("BPP_MDCD").Value).Trim(), TipoDocElec, oForm.BusinessObject.Type, sDocSubType, RUC, ref GlobalSettings.oUser_FM);
                                }
                                else if (sDocSubType == "IB")//Boleta
                                {
                                    TipoDocElec = "03";
                                    EnviarBE_PE(sDocEntry, GlobalSettings.RunningUnderSQLServer, ((System.String)oRecordSet.Fields.Item("BPP_MDSD").Value).Trim(), ((System.String)oRecordSet.Fields.Item("BPP_MDCD").Value).Trim(), TipoDocElec, oForm.BusinessObject.Type, sDocSubType, RUC, ref GlobalSettings.oUser_FM);
                                }
                            }
                            else //FacturadoPE == E //EasyDot
                            {
                                if (sDocSubType == "--")
                                {
                                    TipoDocElec = "01";
                                    EnviarFE_PE_ED(sDocEntry, GlobalSettings.RunningUnderSQLServer, ((System.String)oRecordSet.Fields.Item("BPP_MDSD").Value).Trim(), ((System.String)oRecordSet.Fields.Item("BPP_MDCD").Value).Trim(), TipoDocElec, oForm.BusinessObject.Type, sDocSubType, RUC, ConexionADO, TTipoDoc);
                                }
                                else if (sDocSubType == "DN")//Nota de Debito
                                {
                                    TipoDocElec = "08";
                                    EnviarFE_PE_ED(sDocEntry, GlobalSettings.RunningUnderSQLServer, ((System.String)oRecordSet.Fields.Item("BPP_MDSD").Value).Trim(), ((System.String)oRecordSet.Fields.Item("BPP_MDCD").Value).Trim(), TipoDocElec, oForm.BusinessObject.Type, sDocSubType, RUC, ConexionADO, TipoDocElec);
                                }
                                else if (sDocSubType == "IB")//Boleta
                                {
                                    TipoDocElec = "03";
                                    EnviarFE_PE_ED(sDocEntry, GlobalSettings.RunningUnderSQLServer, ((System.String)oRecordSet.Fields.Item("BPP_MDSD").Value).Trim(), ((System.String)oRecordSet.Fields.Item("BPP_MDCD").Value).Trim(), TipoDocElec, oForm.BusinessObject.Type, sDocSubType, RUC, ConexionADO, TipoDocElec);
                                }
                            }
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


        public new void PrintEvent(ref SAPbouiCOM.PrintEventInfo eventInfo, ref Boolean BubbleEvent)
        {
            XmlDocument _xmlDocument;
            XmlNode N;

            base.PrintEvent(ref eventInfo, ref BubbleEvent);
            oForm = FSBOApp.Forms.Item(eventInfo.FormUID);
        }//fin PrintEvent


        public new void ReportDataEvent(ref SAPbouiCOM.ReportDataInfo eventInfo, ref Boolean BubbleEvent)
        {
            base.ReportDataEvent(ref eventInfo, ref BubbleEvent);
            oForm = FSBOApp.Forms.Item(eventInfo.FormUID);

            //OutLog("ReportData " + eventInfo.EventType.ToString);
            if (eventInfo.FormUID.Length > 0) //and (eventInfo.WithPrinterPreferences) then
            {
                ;
            }
        }//fin ReportDataEvent

        //Para Peru Factura Movil
        //Factura
        public new void EnviarFE_PE(String DocEntry, Boolean RunningUnderSQLServer, String SeriePE, String FolioNum, String TipoDocElec, String sObjType, String DocSubType, String lRUC, ref pe.facturamovil.User oUserFM)
        {
            SAPbobsCOM.Recordset orsLocal;
            SAPbobsCOM.Company Cmpny;
            SAPbobsCOM.Documents oDocumento;
            Boolean bImpresionOk;
            String Status;
            String sMessage;
            Int32 lRetCode;
            TFunctions Reg;
            String ProcNomE;
            String ProcNomD;
            String ProcNomR;
            String externalFolio;
            String Email;
            String Id = "0";
            String Validation = "";
            String IncPay;
            String DocDate;

            try
            {
                bImpresionOk = true;
                Cmpny = SBO_f.Cmpny;
                JsonText = "";
                orsLocal = (SAPbobsCOM.Recordset)(Cmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset));
                if (sObjType == "203")
                    oDocumento = (SAPbobsCOM.Documents)(Cmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oDownPayments));
                else
                    oDocumento = (SAPbobsCOM.Documents)(Cmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oInvoices));

                sMessage = "";

                if (RunningUnderSQLServer)
                    s = "select ISNULL(U_IncPay,'N') IncPay from [@VID_FEPARAM] where Code = '1'";
                else
                    s = @"select IFNULL(""U_IncPay"",'N') ""IncPay"" from ""@VID_FEPARAM"" where ""Code"" = '1' ";
                orsLocal.DoQuery(s);
                IncPay = ((System.String)orsLocal.Fields.Item("IncPay").Value).Trim();

                //validar que exista procedimentos para tipo documento
                if (RunningUnderSQLServer)
                { s = "select ISNULL(U_ProcNomE,'') 'ProcNomE', ISNULL(U_ProcNomD,'') 'ProcNomD', ISNULL(U_ProcNomR,'') 'ProcNomR' from [@VID_FEPROCED] where ISNULL(U_Habili,'N') = 'Y' and U_TipoDocPE = '{0}'"; }
                else
                { s = @"select IFNULL(""U_ProcNomE"",'') ""ProcNomE"", IFNULL(""U_ProcNomD"",'') ""ProcNomD"", IFNULL(""U_ProcNomR"",'') ""ProcNomR"" from ""@VID_FEPROCED"" where IFNULL(""U_Habili"",'N') = 'Y' and ""U_TipoDocPE"" = '{0}'"; }

                s = String.Format(s, TipoDocElec);
                orsLocal.DoQuery(s);
                if (orsLocal.RecordCount == 0)
                {
                    //SBO_f.SBOApp.StatusBar.SetText("No se encuentra procedimientos para Documento electronico " + TipoDocElec, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                    throw new Exception("No se encuentra procedimientos para Documento electronico " + TipoDocElec);
                }
                else
                {
                    if ((System.String)(orsLocal.Fields.Item("ProcNomE").Value).ToString().Trim() == "")
                        //SBO_f.SBOApp.StatusBar.SetText("No se encuentra procedimiento de encabezado para Documento electronico " + TipoDocElec, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                        throw new Exception("No se encuentra procedimiento de encabezado para Documento electronico " + TipoDocElec);
                    else if ((System.String)(orsLocal.Fields.Item("ProcNomD").Value).ToString().Trim() == "")
                        //SBO_f.SBOApp.StatusBar.SetText("No se encuentra procedimiento de detalle para Documento electronico " + TipoDocElec, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                        throw new Exception("No se encuentra procedimiento de detalle para Documento electronico " + TipoDocElec);
                    else if ((System.String)(orsLocal.Fields.Item("ProcNomR").Value).ToString().Trim() == "")
                        //SBO_f.SBOApp.StatusBar.SetText("No se encuentra procedimiento de referencia para Documento electronico " + TipoDocElec, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                        throw new Exception("No se encuentra procedimiento de referencia para Documento electronico " + TipoDocElec);

                    ProcNomE = (System.String)(orsLocal.Fields.Item("ProcNomE").Value).ToString().Trim();
                    ProcNomD = (System.String)(orsLocal.Fields.Item("ProcNomD").Value).ToString().Trim();
                    ProcNomR = (System.String)(orsLocal.Fields.Item("ProcNomR").Value).ToString().Trim();
                }


                if ((oDocumento.GetByKey(Convert.ToInt32(DocEntry))) && (bImpresionOk))
                {
                    if (RunningUnderSQLServer)
                        s = "exec " + ProcNomE + "  " + oDocumento.DocEntry + ", '" + TipoDocElec + "', '" + sObjType + "'";
                    else
                        s = "CALL " + ProcNomE + "  (" + oDocumento.DocEntry + ", '" + TipoDocElec + "', '" + sObjType + "')";

                    //consulta por encabezado
                    orsLocal.DoQuery(s);
                    if (orsLocal.RecordCount > 0)
                    {
                        oInvoice_FM = new pe.facturamovil.Invoice();
                        oInvoice_FM.currency = ((System.String)orsLocal.Fields.Item("currency").Value).Trim();
                        oInvoice_FM.date = ((System.DateTime)orsLocal.Fields.Item("date").Value);
                        oInvoice_FM.series = ((System.String)orsLocal.Fields.Item("series").Value).Trim();
                        externalFolio = ((System.String)orsLocal.Fields.Item("externalFolio").Value).Trim();
                        oInvoice_FM.externalFolio = externalFolio;
                        if (((System.String)orsLocal.Fields.Item("sellerCode").Value).Trim() != "-1")
                            oInvoice_FM.sellerCode = ((System.String)orsLocal.Fields.Item("sellerCode").Value).Trim();

                        var oClient = new pe.facturamovil.Client();
                        oClient.code = ((System.String)orsLocal.Fields.Item("code").Value).Trim();
                        oClient.name = ((System.String)orsLocal.Fields.Item("name").Value).Trim();
                        oClient.address = ((System.String)orsLocal.Fields.Item("address").Value).Trim();

                        var oDistrict = new pe.facturamovil.Municipality();
                        oDistrict.code = ((System.String)orsLocal.Fields.Item("municipality").Value).Trim();
                        oClient.municipality = oDistrict;
                        oClient.contact = ((System.String)orsLocal.Fields.Item("contact").Value).Trim();
                        oClient.phone = ((System.String)orsLocal.Fields.Item("phone").Value).Trim();

                        if (((System.String)orsLocal.Fields.Item("identityDocumentType").Value).Trim() != "")
                        {
                            var oid = new pe.facturamovil.IdentityDocumentType();
                            oid.code = ((System.String)orsLocal.Fields.Item("identityDocumentType").Value);
                            oClient.identityDocumentType = oid;
                        }

                        Email = ((System.String)orsLocal.Fields.Item("email").Value).Trim();
                        oClient.email = Email;
                        oInvoice_FM.client = oClient;
                        oInvoice_FM.expirationDate = ((System.DateTime)orsLocal.Fields.Item("expirationDate").Value);

                        try
                        {
                            var oAditional = new pe.facturamovil.AdditionalPrintInformation();
                            if (((System.String)orsLocal.Fields.Item("certificateNumber").Value).Trim() == "")
                                oAditional.certificateNumber = null;
                            else
                                oAditional.certificateNumber = ((System.String)orsLocal.Fields.Item("certificateNumber").Value).Trim();

                            if (((System.String)orsLocal.Fields.Item("contactP").Value).Trim() == "")
                                oAditional.contact = null;
                            else
                                oAditional.contact = ((System.String)orsLocal.Fields.Item("contactP").Value).Trim();

                            if (((System.String)orsLocal.Fields.Item("gloss").Value).Trim() == "")
                                oAditional.gloss = null;
                            else
                                oAditional.gloss = ((System.String)orsLocal.Fields.Item("gloss").Value).Trim();

                            if (((System.String)orsLocal.Fields.Item("project").Value).Trim() == "")
                                oAditional.project = null;
                            else
                                oAditional.project = ((System.String)orsLocal.Fields.Item("project").Value).Trim();

                            if (((System.String)orsLocal.Fields.Item("reference").Value).Trim() == "")
                                oAditional.reference = null;
                            else
                                oAditional.reference = ((System.String)orsLocal.Fields.Item("reference").Value).Trim();

                            if (((System.String)orsLocal.Fields.Item("account").Value).Trim() == "")
                                oAditional.account = null;
                            else
                                oAditional.account = ((System.String)orsLocal.Fields.Item("account").Value).Trim();

                            if (((System.String)orsLocal.Fields.Item("estimateNumber").Value).Trim() == "")
                                oAditional.estimateNumber = null;
                            else
                                oAditional.estimateNumber = ((System.String)orsLocal.Fields.Item("estimateNumber").Value).Trim();

                            oInvoice_FM.additionalPrintInformation = oAditional;
                        }
                        catch (Exception er)
                        {
                            SBO_f.oLog.OutLog("Error additionalPrintInformation - " + er.Message);
                        }

                        if (IncPay == "Y")
                        {
                            //PAYMENT
                            oInvoice_FM.payments = new List<pe.facturamovil.Payment>();
                            var oPayment = new pe.facturamovil.Payment();
                            oPayment.position = 1;
                            oPayment.date = ((System.DateTime)orsLocal.Fields.Item("datePayment").Value);
                            oPayment.amount = ((System.Double)orsLocal.Fields.Item("amountPayment").Value);
                            oPayment.description = ((System.String)orsLocal.Fields.Item("descriptionPayment").Value).Trim();

                            oInvoice_FM.payments.Add(oPayment);
                        }


                        //DETALLE
                        if (RunningUnderSQLServer)
                            s = "exec " + ProcNomD + "  " + oDocumento.DocEntry + ", '" + TipoDocElec + "', '" + sObjType + "'";
                        else
                            s = "CALL " + ProcNomD + "  (" + oDocumento.DocEntry + ", '" + TipoDocElec + "', '" + sObjType + "')";
                        //consulta por detalle
                        orsLocal.DoQuery(s);
                        if (orsLocal.RecordCount > 0)
                        {
                            oInvoice_FM.details = new List<pe.facturamovil.Detail>();
                            while (!orsLocal.EoF)
                            {
                                var oProduct = new pe.facturamovil.Product();
                                var oService = new pe.facturamovil.Service();

                                if (((System.String)orsLocal.Fields.Item("DocType").Value).Trim() == "I")
                                {

                                    oProduct.code = ((System.String)orsLocal.Fields.Item("code").Value).Trim();
                                    oProduct.name = ((System.String)orsLocal.Fields.Item("name").Value).Trim();

                                    var oUM = new pe.facturamovil.Unit();
                                    oUM.code = ((System.String)orsLocal.Fields.Item("unit").Value).Trim();
                                    oProduct.unit = oUM;
                                    oProduct.price = ((System.Double)orsLocal.Fields.Item("price").Value);

                                    var oIGV = new pe.facturamovil.ExemptType();
                                    oIGV.code = ((System.String)orsLocal.Fields.Item("exemptType").Value).Trim();
                                    oProduct.exemptType = oIGV;
                                }
                                else if (((System.String)orsLocal.Fields.Item("DocType").Value).Trim() == "S")
                                {
                                    oService.name = ((System.String)orsLocal.Fields.Item("name").Value).Trim();

                                    var oUM = new pe.facturamovil.Unit();
                                    oUM.code = ((System.String)orsLocal.Fields.Item("unit").Value).Trim();
                                    oService.unit = oUM;
                                    oService.price = ((System.Double)orsLocal.Fields.Item("price").Value);
                                    var oIGV = new pe.facturamovil.ExemptType();
                                    oIGV.code = ((System.String)orsLocal.Fields.Item("exemptType").Value).Trim();
                                    oService.exemptType = oIGV;
                                }

                                var oDetail = new pe.facturamovil.Detail();

                                oDetail.position = ((System.Int32)orsLocal.Fields.Item("idLine").Value);

                                if (((System.String)orsLocal.Fields.Item("DocType").Value).Trim() == "I")
                                {
                                    oDetail.product = oProduct;
                                    oDetail.quantity = float.Parse(((System.Double)orsLocal.Fields.Item("quantity").Value).ToString().Trim());
                                    oDetail.description = ((System.String)orsLocal.Fields.Item("description").Value).Trim();
                                }
                                else if (((System.String)orsLocal.Fields.Item("DocType").Value).Trim() == "S")
                                {
                                    oDetail.service = oService;
                                    oDetail.description = ((System.String)orsLocal.Fields.Item("description").Value).Trim();
                                    oDetail.quantity = float.Parse(((System.Double)orsLocal.Fields.Item("quantity").Value).ToString().Trim());
                                }

                                oDetail.longDescription = ((System.String)orsLocal.Fields.Item("longDescription").Value).Trim();

                                oInvoice_FM.details.Add(oDetail);

                                orsLocal.MoveNext();
                            }//fin agregar detalle a la cabecera


                            //REFERENCIAS
                            if (RunningUnderSQLServer)
                                s = "exec " + ProcNomR + "  " + oDocumento.DocEntry + ", '" + TipoDocElec + "', '" + sObjType + "'";
                            else
                                s = "CALL " + ProcNomR + "  (" + oDocumento.DocEntry + ", '" + TipoDocElec + "','" + sObjType + "')";
                            //consulta por referencia
                            orsLocal.DoQuery(s);
                            if (orsLocal.RecordCount > 0)
                            {
                                oInvoice_FM.references = new List<pe.facturamovil.Reference>();
                                while (!orsLocal.EoF)
                                {
                                    var oReference = new pe.facturamovil.Reference();
                                    oReference.position = ((System.Int32)orsLocal.Fields.Item("position").Value);
                                    var oDocType = new pe.facturamovil.DocumentType();
                                    oDocType.code = ((System.String)orsLocal.Fields.Item("documentType").Value).Trim();
                                    oReference.documentType = oDocType;
                                    oReference.referencedFolio = ((System.String)orsLocal.Fields.Item("referencedFolio").Value).Trim();
                                    oReference.date = ((System.DateTime)orsLocal.Fields.Item("date").Value);
                                    oReference.description = ((System.String)orsLocal.Fields.Item("description").Value).Trim();

                                    oInvoice_FM.references.Add(oReference);
                                    orsLocal.MoveNext();
                                }
                            }

                            //termina de cargar documento
                            JsonText = FacturaMovilGlobal.processor.getInvoiceJson(oInvoice_FM);
                            //oRecordSet.DoQuery("UPDATE [@OFMP] SET U_JSON='" + JsonText + "' WHERE DOCENTRY=1");

                            if (FacturaMovilGlobal.userConnected == null)
                            {
                                try
                                {
                                    LoginCount_FM = 0;
                                    //oUser_FM = new pe.facturamovil.User();
                                    if (oUserFM.token == null)
                                    {
                                        //GlobalSettings.oUser_FM = new pe.facturamovil.User();
                                        if (RunningUnderSQLServer)
                                            orsLocal.DoQuery("SELECT U_User,U_Pwd,U_CCEmail FROM [@VID_FEPARAM] WHERE Code = '1'");
                                        else
                                            orsLocal.DoQuery(@"SELECT ""U_User"", ""U_Pwd"", ""U_CCEmail"" FROM ""@VID_FEPARAM"" WHERE ""Code"" = '1'");
                                        //SBO_f.oLog.OutLog("usuario : '" + ((System.String)orsLocal.Fields.Item("U_User").Value).Trim() + "'");
                                        //SBO_f.oLog.OutLog("password: '" + ((System.String)orsLocal.Fields.Item("U_Pwd").Value).Trim() + "'");
                                        oUserFM = FacturaMovilGlobal.processor.Authenticate(((System.String)orsLocal.Fields.Item("U_User").Value).Trim(), ((System.String)orsLocal.Fields.Item("U_Pwd").Value).Trim());
                                        FacturaMovilGlobal.userConnected = oUserFM;

                                        var ii = 0;
                                        var bExistePE = false;

                                        if (oUserFM.companies.Find(c => c.code.Trim() == lRUC.Trim()) != null)
                                        {
                                            FacturaMovilGlobal.selectedCompany = oUserFM.companies.Single(c => c.code.Trim() == lRUC.Trim());
                                            bExistePE = true;
                                            ii = oUserFM.companies.Count;
                                        }

                                        if (!bExistePE)
                                            throw new Exception("No se ha encontrado el RUC " + lRUC + "en la conexion de Factura Movil");

                                        CCEmail_FM = ((System.String)orsLocal.Fields.Item("U_CCEmail").Value).Trim();
                                    }
                                }
                                catch (Exception ex)
                                {
                                    //throw new Exception("Motivos de error en conexion : " + ex.Message);
                                    bImpresionOk = false;
                                    sMessage = "Motivos de error en conexion : " + ex.Message;
                                }
                            }

                            try
                            {
                                if (bImpresionOk)
                                {
                                    FacturaMovilGlobal.processor.sendInvoice(FacturaMovilGlobal.selectedCompany, oInvoice_FM, FacturaMovilGlobal.userConnected.token);
                                    Id = oInvoice_FM.id.ToString();
                                    Validation = oInvoice_FM.validation;

                                    //orsLocal.DoQuery("UPDATE OINV SET U_FM_MDFE='Y' WHERE NUMATCARD='" + NumAtCard + "' AND DOCSUBTYPE='--'")
                                    SBO_f.SBOApp.StatusBar.SetText("Factura emitida con exito.", SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                    FacturaMovilGlobal.processor.showDocument(oInvoice_FM);

                                    if (Email != "")
                                    {
                                        SBO_f.SBOApp.StatusBar.SetText("Enviando documento via email. Porfavor Espere...", SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                        FacturaMovilGlobal.processor.sendEmail(FacturaMovilGlobal.selectedCompany, oInvoice_FM, Email, CCEmail_FM, FacturaMovilGlobal.userConnected.token);
                                        SBO_f.SBOApp.StatusBar.SetText("Factura emitida y enviada al cliente electronicamente con exito. Numero SUNAT : " + externalFolio, SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                    }
                                    else
                                        SBO_f.SBOApp.StatusBar.SetText("Factura emitida electronicamente con exito. Asegurese de enviar el documento al cliente. Numero SUNAT : " + externalFolio, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                                }
                            }
                            catch (Exception ex)
                            {
                                SBO_f.SBOApp.StatusBar.SetText(ex.Message, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                SBO_f.oLog.OutLog("EnviarFE_PE " + ex.Message + " ** Trace: " + ex.StackTrace);
                                bImpresionOk = false;
                                sMessage = ex.Message;
                            }
                        }
                        else
                        {
                            SBO_f.SBOApp.StatusBar.SetText("No se encuentra Datos en detalle", SAPbouiCOM.BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                            bImpresionOk = false;
                        }

                    }
                    else
                    {
                        SBO_f.SBOApp.StatusBar.SetText("No se encuentra Datos en encabezado", SAPbouiCOM.BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                        bImpresionOk = false;
                    }
                }
                else
                {
                    SBO_f.SBOApp.StatusBar.SetText("Error - No se ha encontrado el documento", SAPbouiCOM.BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                    bImpresionOk = false;
                }

                DocDate = SBO_f.DateToStr(oDocumento.DocDate);

                if (!bImpresionOk)
                {
                    //SBO_f.SBOApp.MessageBox("Error envio documento electronico ");
                    SBO_f.SBOApp.StatusBar.SetText("Error envio documento electrónico (1) - " + sMessage, SAPbouiCOM.BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                    //sObjType = "13";
                    Status = "EE";
                    if (sMessage == "")
                        sMessage = "Error envio documento electronico a Factura Movil";
                }
                else
                {
                    Status = "EC";
                    //sObjType = "13";
                    sMessage = "Enviado satisfactoriamente";
                    SBO_f.SBOApp.StatusBar.SetText("Se ha enviado satisfactoriamente el documento electronico a Factura Movil", SAPbouiCOM.BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success);
                    //oDocumento.UserFields.Fields.Item("U_EstadoFE").Value = "P";
                    //actualizo campo de impresion para que no aparezca formulario solicitando folio
                    oDocumento.Printed = PrintStatusEnum.psYes;
                    lRetCode = oDocumento.Update();
                    if (lRetCode != 0)
                    {
                        s = SBO_f.Cmpny.GetLastErrorDescription();
                        SBO_f.SBOApp.StatusBar.SetText("Error actualizar documento - " + s, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                        SBO_f.oLog.OutLog("Error actualizar Factura - " + s);
                    }
                }

                if (RunningUnderSQLServer)
                { s = "SELECT DocEntry, U_Status FROM [@VID_FELOG] WITH (NOLOCK) WHERE U_DocEntry = {0} AND U_ObjType = '{1}' AND U_SubType = '{2}'"; }
                else
                { s = @"SELECT ""DocEntry"", ""U_Status"" FROM ""@VID_FELOG"" WHERE ""U_DocEntry"" = {0} AND ""U_ObjType"" = '{1}' AND ""U_SubType"" = '{2}' "; }
                s = String.Format(s, DocEntry, sObjType, DocSubType);
                orsLocal.DoQuery(s);
                Reg = new TFunctions();
                Reg.SBO_f = SBO_f;

                if (sMessage.Length > 254)
                    sMessage = sMessage.Substring(0, 253);

                if (orsLocal.RecordCount == 0)
                    Reg.FELOGAdd(Int32.Parse(DocEntry), sObjType, DocSubType, SeriePE, Int32.Parse(FolioNum), Status, sMessage, TipoDocElec, SBO_f.SBOApp.Company.UserName, JsonText, Id, Validation, DocDate);
                else
                {
                    if ((System.String)(orsLocal.Fields.Item("U_Status").Value) != "RR")
                        Reg.FELOGUptM((System.Int32)(orsLocal.Fields.Item("DocEntry").Value), Int32.Parse(DocEntry), sObjType, DocSubType, SeriePE, Int32.Parse(FolioNum), Status, sMessage, TipoDocElec, SBO_f.SBOApp.Company.UserName, JsonText, Id, Validation, DocDate);
                    else
                        SBO_f.SBOApp.StatusBar.SetText("Documento ya se encuentra en Factura Movil y en SUNAT", SAPbouiCOM.BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success);
                }

            }
            catch (Exception e)
            {
                SBO_f.SBOApp.StatusBar.SetText(e.Message, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                SBO_f.oLog.OutLog("EnviarFE_PE " + e.Message + " ** Trace: " + e.StackTrace);
            }
        }
        //Boleta PERU FM
        public new void EnviarBE_PE(String DocEntry, Boolean RunningUnderSQLServer, String SeriePE, String FolioNum, String TipoDocElec, String sObjType, String DocSubType, String lRUC, ref pe.facturamovil.User oUserFM)
        {
            SAPbobsCOM.Recordset orsLocal;
            SAPbobsCOM.Company Cmpny;
            SAPbobsCOM.Documents oDocumento;
            Boolean bImpresionOk;
            String Status;
            String sMessage;
            Int32 lRetCode;
            TFunctions Reg;
            String ProcNomE;
            String ProcNomD;
            String ProcNomR;
            String externalFolio;
            String Email;
            String Id = "0";
            String Validation = "";
            String DocDate;

            try
            {
                bImpresionOk = true;
                Cmpny = SBO_f.Cmpny;
                JsonText = "";
                orsLocal = (SAPbobsCOM.Recordset)(Cmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset));

                if (sObjType == "203")
                    oDocumento = (SAPbobsCOM.Documents)(Cmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oDownPayments));
                else
                    oDocumento = (SAPbobsCOM.Documents)(Cmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oInvoices));
                sMessage = "";


                //validar que exista procedimentos para tipo documento
                if (RunningUnderSQLServer)
                { s = "select ISNULL(U_ProcNomE,'') 'ProcNomE', ISNULL(U_ProcNomD,'') 'ProcNomD', ISNULL(U_ProcNomR,'') 'ProcNomR' from [@VID_FEPROCED] where ISNULL(U_Habili,'N') = 'Y' and U_TipoDocPE = '{0}'"; }
                else
                { s = @"select IFNULL(""U_ProcNomE"",'') ""ProcNomE"", IFNULL(""U_ProcNomD"",'') ""ProcNomD"", IFNULL(""U_ProcNomR"",'') ""ProcNomR"" from ""@VID_FEPROCED"" where IFNULL(""U_Habili"",'N') = 'Y' and ""U_TipoDocPE"" = '{0}'"; }

                s = String.Format(s, TipoDocElec);
                orsLocal.DoQuery(s);
                if (orsLocal.RecordCount == 0)
                {
                    //SBO_f.SBOApp.StatusBar.SetText("No se encuentra procedimientos para Documento electronico " + TipoDocElec, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                    throw new Exception("No se encuentra procedimientos para Documento electronico " + TipoDocElec);
                }
                else
                {
                    if ((System.String)(orsLocal.Fields.Item("ProcNomE").Value).ToString().Trim() == "")
                        //SBO_f.SBOApp.StatusBar.SetText("No se encuentra procedimiento de encabezado para Documento electronico " + TipoDocElec, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                        throw new Exception("No se encuentra procedimiento de encabezado para Documento electronico " + TipoDocElec);
                    else if ((System.String)(orsLocal.Fields.Item("ProcNomD").Value).ToString().Trim() == "")
                        //SBO_f.SBOApp.StatusBar.SetText("No se encuentra procedimiento de detalle para Documento electronico " + TipoDocElec, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                        throw new Exception("No se encuentra procedimiento de detalle para Documento electronico " + TipoDocElec);
                    else if ((System.String)(orsLocal.Fields.Item("ProcNomR").Value).ToString().Trim() == "")
                        //SBO_f.SBOApp.StatusBar.SetText("No se encuentra procedimiento de referencia para Documento electronico " + TipoDocElec, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                        throw new Exception("No se encuentra procedimiento de referencia para Documento electronico " + TipoDocElec);

                    ProcNomE = (System.String)(orsLocal.Fields.Item("ProcNomE").Value).ToString().Trim();
                    ProcNomD = (System.String)(orsLocal.Fields.Item("ProcNomD").Value).ToString().Trim();
                    ProcNomR = (System.String)(orsLocal.Fields.Item("ProcNomR").Value).ToString().Trim();
                }


                if ((oDocumento.GetByKey(Convert.ToInt32(DocEntry))) && (bImpresionOk))
                {
                    if (RunningUnderSQLServer)
                        s = "exec " + ProcNomE + "  " + oDocumento.DocEntry + ", '" + TipoDocElec + "'";
                    else
                        s = "CALL " + ProcNomE + "  (" + oDocumento.DocEntry + ", '" + TipoDocElec + "')";
                    //consulta por encabezado
                    orsLocal.DoQuery(s);
                    if (orsLocal.RecordCount > 0)
                    {
                        oTicket_FM = new pe.facturamovil.Ticket();
                        oTicket_FM.currency = ((System.String)orsLocal.Fields.Item("currency").Value).Trim();
                        oTicket_FM.date = ((System.DateTime)orsLocal.Fields.Item("date").Value);
                        oTicket_FM.series = ((System.String)orsLocal.Fields.Item("series").Value).Trim();
                        externalFolio = ((System.String)orsLocal.Fields.Item("externalFolio").Value).Trim();
                        oTicket_FM.externalFolio = externalFolio;

                        var oClient = new pe.facturamovil.Client();
                        oClient.code = ((System.String)orsLocal.Fields.Item("code").Value).Trim();
                        oClient.name = ((System.String)orsLocal.Fields.Item("name").Value).Trim();
                        oClient.address = ((System.String)orsLocal.Fields.Item("address").Value).Trim();
                        var oDistrict = new pe.facturamovil.Municipality();
                        oDistrict.code = ((System.String)orsLocal.Fields.Item("municipality").Value).Trim();
                        oClient.municipality = oDistrict;
                        oClient.contact = ((System.String)orsLocal.Fields.Item("contact").Value).Trim();
                        oClient.phone = ((System.String)orsLocal.Fields.Item("phone").Value).Trim();

                        if (((System.String)orsLocal.Fields.Item("identityDocumentType").Value).Trim() != "") //agragado 20161006
                        {
                            var oid = new pe.facturamovil.IdentityDocumentType();
                            oid.code = ((System.String)orsLocal.Fields.Item("identityDocumentType").Value);
                            oClient.identityDocumentType = oid;
                        }

                        Email = ((System.String)orsLocal.Fields.Item("email").Value).Trim();
                        oClient.email = Email;
                        oTicket_FM.client = oClient;
                        oTicket_FM.expirationDate = ((System.DateTime)orsLocal.Fields.Item("expirationDate").Value);



                        try
                        {
                            var oAditional = new pe.facturamovil.AdditionalPrintInformation();
                            if (((System.String)orsLocal.Fields.Item("certificateNumber").Value).Trim() == "")
                                oAditional.certificateNumber = null;
                            else
                                oAditional.certificateNumber = ((System.String)orsLocal.Fields.Item("certificateNumber").Value).Trim();

                            if (((System.String)orsLocal.Fields.Item("contactP").Value).Trim() == "")
                                oAditional.contact = null;
                            else
                                oAditional.contact = ((System.String)orsLocal.Fields.Item("contactP").Value).Trim();

                            if (((System.String)orsLocal.Fields.Item("gloss").Value).Trim() == "")
                                oAditional.gloss = null;
                            else
                                oAditional.gloss = ((System.String)orsLocal.Fields.Item("gloss").Value).Trim();

                            if (((System.String)orsLocal.Fields.Item("project").Value).Trim() == "")
                                oAditional.project = null;
                            else
                                oAditional.project = ((System.String)orsLocal.Fields.Item("project").Value).Trim();

                            if (((System.String)orsLocal.Fields.Item("reference").Value).Trim() == "")
                                oAditional.reference = null;
                            else
                                oAditional.reference = ((System.String)orsLocal.Fields.Item("reference").Value).Trim();

                            if (((System.String)orsLocal.Fields.Item("account").Value).Trim() == "")
                                oAditional.account = null;
                            else
                                oAditional.account = ((System.String)orsLocal.Fields.Item("account").Value).Trim();

                            if (((System.String)orsLocal.Fields.Item("estimateNumber").Value).Trim() == "")
                                oAditional.estimateNumber = null;
                            else
                                oAditional.estimateNumber = ((System.String)orsLocal.Fields.Item("estimateNumber").Value).Trim();

                            oTicket_FM.additionalPrintInformation = oAditional;
                        }
                        catch (Exception er)
                        {
                            SBO_f.oLog.OutLog("Error additionalPrintInformation - " + er.Message);
                        }


                        //DETALLE
                        if (RunningUnderSQLServer)
                            s = "exec " + ProcNomD + "  " + oDocumento.DocEntry + ", '" + TipoDocElec + "'";
                        else
                            s = "CALL " + ProcNomD + "  (" + oDocumento.DocEntry + ", '" + TipoDocElec + "')";
                        //consulta por detalle
                        orsLocal.DoQuery(s);
                        if (orsLocal.RecordCount > 0)
                        {
                            oTicket_FM.details = new List<pe.facturamovil.Detail>();
                            while (!orsLocal.EoF)
                            {
                                var oProduct = new pe.facturamovil.Product();
                                var oService = new pe.facturamovil.Service();

                                if (((System.String)orsLocal.Fields.Item("DocType").Value).Trim() == "I")
                                {

                                    oProduct.code = ((System.String)orsLocal.Fields.Item("code").Value).Trim();
                                    oProduct.name = ((System.String)orsLocal.Fields.Item("name").Value).Trim();

                                    var oUM = new pe.facturamovil.Unit();
                                    oUM.code = ((System.String)orsLocal.Fields.Item("unit").Value).Trim();
                                    oProduct.unit = oUM;
                                    oProduct.price = ((System.Double)orsLocal.Fields.Item("price").Value);

                                    var oIGV = new pe.facturamovil.ExemptType();
                                    oIGV.code = ((System.String)orsLocal.Fields.Item("exemptType").Value).Trim();
                                    oProduct.exemptType = oIGV;
                                }
                                else if (((System.String)orsLocal.Fields.Item("DocType").Value).Trim() == "S")
                                {
                                    oService.name = ((System.String)orsLocal.Fields.Item("name").Value).Trim();

                                    var oUM = new pe.facturamovil.Unit();
                                    oUM.code = ((System.String)orsLocal.Fields.Item("unit").Value).Trim();
                                    oService.unit = oUM;
                                    oService.price = ((System.Double)orsLocal.Fields.Item("price").Value);
                                    var oIGV = new pe.facturamovil.ExemptType();
                                    oIGV.code = ((System.String)orsLocal.Fields.Item("exemptType").Value).Trim();
                                    oService.exemptType = oIGV;
                                }

                                var oDetail = new pe.facturamovil.Detail();

                                oDetail.position = ((System.Int32)orsLocal.Fields.Item("idLine").Value);

                                if (((System.String)orsLocal.Fields.Item("DocType").Value).Trim() == "I")
                                {
                                    oDetail.product = oProduct;
                                    oDetail.quantity = float.Parse(((System.Double)orsLocal.Fields.Item("quantity").Value).ToString().Trim());
                                    oDetail.description = ((System.String)orsLocal.Fields.Item("description").Value).Trim();
                                }
                                else if (((System.String)orsLocal.Fields.Item("DocType").Value).Trim() == "S")
                                {
                                    oDetail.service = oService;
                                    oDetail.description = ((System.String)orsLocal.Fields.Item("description").Value).Trim();
                                    oDetail.quantity = float.Parse(((System.Double)orsLocal.Fields.Item("quantity").Value).ToString().Trim());
                                }

                                oDetail.longDescription = ((System.String)orsLocal.Fields.Item("longDescription").Value).Trim();

                                oTicket_FM.details.Add(oDetail);

                                orsLocal.MoveNext();
                            }//fin agregar detalle a la cabecera


                            //REFERENCIAS
                            if (RunningUnderSQLServer)
                                s = "exec " + ProcNomR + "  " + oDocumento.DocEntry + ", '" + TipoDocElec + "'";
                            else
                                s = "CALL " + ProcNomR + "  (" + oDocumento.DocEntry + ", '" + TipoDocElec + "')";
                            //consulta por referencia
                            orsLocal.DoQuery(s);
                            if (orsLocal.RecordCount > 0)
                            {
                                oTicket_FM.references = new List<pe.facturamovil.Reference>();
                                while (!orsLocal.EoF)
                                {
                                    var oReference = new pe.facturamovil.Reference();
                                    oReference.position = ((System.Int32)orsLocal.Fields.Item("position").Value);
                                    var oDocType = new pe.facturamovil.DocumentType();
                                    oDocType.code = ((System.String)orsLocal.Fields.Item("documentType").Value).Trim();
                                    oReference.documentType = oDocType;
                                    oReference.referencedFolio = ((System.String)orsLocal.Fields.Item("referencedFolio").Value).Trim();
                                    oReference.date = ((System.DateTime)orsLocal.Fields.Item("date").Value);
                                    oReference.description = ((System.String)orsLocal.Fields.Item("description").Value).Trim();

                                    oTicket_FM.references.Add(oReference);
                                    orsLocal.MoveNext();
                                }
                            }

                            //termina de cargar documento
                            JsonText = FacturaMovilGlobal.processor.getTicketJson(oTicket_FM);
                            //oRecordSet.DoQuery("UPDATE [@OFMP] SET U_JSON='" + JsonText + "' WHERE DOCENTRY=1");

                            if (FacturaMovilGlobal.userConnected == null)
                            {
                                try
                                {
                                    LoginCount_FM = 0;
                                    //oUser_FM = new pe.facturamovil.User();
                                    if (oUserFM.token == null)
                                    {
                                        //GlobalSettings.oUser_FM = new pe.facturamovil.User();
                                        if (RunningUnderSQLServer)
                                            orsLocal.DoQuery("SELECT U_User,U_Pwd,U_CCEmail FROM [@VID_FEPARAM] WHERE Code = '1'");
                                        else
                                            orsLocal.DoQuery(@"SELECT ""U_User"", ""U_Pwd"", ""U_CCEmail"" FROM ""@VID_FEPARAM"" WHERE ""Code"" = '1'");
                                        oUserFM = FacturaMovilGlobal.processor.Authenticate(((System.String)orsLocal.Fields.Item("U_User").Value).Trim(), ((System.String)orsLocal.Fields.Item("U_Pwd").Value).Trim());
                                        FacturaMovilGlobal.userConnected = oUserFM;
                                        var ii = 0;
                                        var bExistePE = false;

                                        if (oUserFM.companies.Find(c => c.code.Trim() == lRUC.Trim()) != null)
                                        {
                                            FacturaMovilGlobal.selectedCompany = GlobalSettings.oUser_FM.companies.Single(c => c.code.Trim() == lRUC.Trim());
                                            bExistePE = true;
                                            ii = oUserFM.companies.Count;
                                        }

                                        if (!bExistePE)
                                            throw new Exception("No se ha encontrado el RUC " + lRUC + "en la conexion de Factura Movil");
                                        CCEmail_FM = ((System.String)orsLocal.Fields.Item("U_CCEmail").Value).Trim();
                                    }
                                }
                                catch (Exception ex)
                                {
                                    //if (SBO_f.SBOApp.MessageBox("No se pudo establecer conexion con el servidor. Desea Continuar?", 2, "Si", "No") == 2)
                                    //{
                                    //throw new Exception("Motivos de error en conexion : " + ex.Message);
                                    bImpresionOk = false;
                                    sMessage = "Motivos de error en conexion : " + ex.Message;
                                    //}
                                }
                            }

                            try
                            {
                                if (bImpresionOk)
                                {
                                    FacturaMovilGlobal.processor.sendTicket(FacturaMovilGlobal.selectedCompany, oTicket_FM, FacturaMovilGlobal.userConnected.token);
                                    Id = oTicket_FM.id.ToString();
                                    Validation = oTicket_FM.validation;
                                    //orsLocal.DoQuery("UPDATE OINV SET U_FM_MDFE='Y' WHERE NUMATCARD='" + NumAtCard + "' AND DOCSUBTYPE='--'")
                                    SBO_f.SBOApp.StatusBar.SetText("Boleta emitida con exito.", SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                    FacturaMovilGlobal.processor.showDocument(oTicket_FM);

                                    if (Email != "")
                                    {
                                        SBO_f.SBOApp.StatusBar.SetText("Enviando documento via email. Porfavor Espere...", SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                        FacturaMovilGlobal.processor.sendEmail(FacturaMovilGlobal.selectedCompany, oTicket_FM, Email, CCEmail_FM, FacturaMovilGlobal.userConnected.token);
                                        SBO_f.SBOApp.StatusBar.SetText("Boleta emitida y enviada al cliente electronicamente con exito. Numero SUNAT : " + externalFolio, SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                    }
                                    else
                                        SBO_f.SBOApp.StatusBar.SetText("Boleta emitida electronicamente con exito. Asegurese de enviar el documento al cliente. Numero SUNAT : " + externalFolio, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                                }
                            }
                            catch (Exception ex)
                            {
                                SBO_f.SBOApp.StatusBar.SetText(ex.Message, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                SBO_f.oLog.OutLog("EnviarBE_PE " + ex.Message + " ** Trace: " + ex.StackTrace);
                                bImpresionOk = false;
                                sMessage = ex.Message;
                            }
                        }
                        else
                        {
                            SBO_f.SBOApp.StatusBar.SetText("No se encuentra Datos en detalle", SAPbouiCOM.BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                            bImpresionOk = false;
                        }

                    }
                    else
                    {
                        SBO_f.SBOApp.StatusBar.SetText("No se encuentra Datos en encabezado", SAPbouiCOM.BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                        bImpresionOk = false;
                    }
                }
                else
                {
                    SBO_f.SBOApp.StatusBar.SetText("Error - No se ha encontrado el documento", SAPbouiCOM.BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                    bImpresionOk = false;
                }

                DocDate = SBO_f.DateToStr(oDocumento.DocDate);

                if (!bImpresionOk)
                {
                    //SBO_f.SBOApp.MessageBox("Error envio documento electronico ");
                    SBO_f.SBOApp.StatusBar.SetText("Error envio documento electrónico (1)", SAPbouiCOM.BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                    sObjType = "13";
                    Status = "EE";
                    if (sMessage == "")
                        sMessage = "Error envio documento electronico a Factura Movil";
                }
                else
                {
                    Status = "EC";
                    sObjType = "13";
                    sMessage = "Enviado satisfactoriamente";
                    SBO_f.SBOApp.StatusBar.SetText("Se ha enviado satisfactoriamente el documento electronico a Factura Movil", SAPbouiCOM.BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success);
                    //oDocumento.UserFields.Fields.Item("U_EstadoFE").Value = "P";
                    //actualizo campo de impresion para que no aparezca formulario solicitando folio
                    oDocumento.Printed = PrintStatusEnum.psYes;
                    lRetCode = oDocumento.Update();
                    if (lRetCode != 0)
                    {
                        s = SBO_f.Cmpny.GetLastErrorDescription();
                        SBO_f.SBOApp.StatusBar.SetText("Error actualizar documento - " + s, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                        SBO_f.oLog.OutLog("Error actualizar Boleta - " + s);
                    }
                }

                if (RunningUnderSQLServer)
                { s = "SELECT DocEntry, U_Status FROM [@VID_FELOG] WITH (NOLOCK) WHERE U_DocEntry = {0} AND U_ObjType = '{1}' AND U_SubType = '{2}'"; }
                else
                { s = @"SELECT ""DocEntry"", ""U_Status"" FROM ""@VID_FELOG"" WHERE ""U_DocEntry"" = {0} AND ""U_ObjType"" = '{1}' AND ""U_SubType"" = '{2}' "; }
                s = String.Format(s, DocEntry, sObjType, DocSubType);
                orsLocal.DoQuery(s);
                Reg = new TFunctions();
                Reg.SBO_f = SBO_f;

                if (sMessage.Length > 254)
                    sMessage = sMessage.Substring(0, 253);

                if (orsLocal.RecordCount == 0)
                    Reg.FELOGAdd(Int32.Parse(DocEntry), sObjType, DocSubType, SeriePE, Int32.Parse(FolioNum), Status, sMessage, TipoDocElec, SBO_f.SBOApp.Company.UserName, JsonText, Id, Validation, DocDate);
                else
                {
                    if ((System.String)(orsLocal.Fields.Item("U_Status").Value) != "RR")
                        Reg.FELOGUptM((System.Int32)(orsLocal.Fields.Item("DocEntry").Value), Int32.Parse(DocEntry), sObjType, DocSubType, SeriePE, Int32.Parse(FolioNum), Status, sMessage, TipoDocElec, SBO_f.SBOApp.Company.UserName, JsonText, Id, Validation, DocDate);
                    else
                        SBO_f.SBOApp.StatusBar.SetText("Documento ya se encuentra en Factura Movil y en SUNAT", SAPbouiCOM.BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success);
                }

            }
            catch (Exception e)
            {
                SBO_f.SBOApp.StatusBar.SetText(e.Message, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                SBO_f.oLog.OutLog("EnviarBE_PE " + e.Message + " ** Trace: " + e.StackTrace);
            }
        }
        //Nota de Debito
        public new void EnviarDN_PE(String DocEntry, Boolean RunningUnderSQLServer, String SeriePE, String FolioNum, String TipoDocElec, String sObjType, String DocSubType, String lRUC, ref pe.facturamovil.User oUserFM)
        {
            SAPbobsCOM.Recordset orsLocal;
            SAPbobsCOM.Company Cmpny;
            SAPbobsCOM.Documents oDocumento;
            Boolean bImpresionOk;
            String Status;
            String sMessage;
            Int32 lRetCode;
            TFunctions Reg;
            String ProcNomE;
            String ProcNomD;
            String ProcNomR;
            String externalFolio;
            String Email;
            String Id = "0";
            String Validation = "";
            String DocDate;

            try
            {
                bImpresionOk = true;
                Cmpny = SBO_f.Cmpny;
                JsonText = "";
                orsLocal = (SAPbobsCOM.Recordset)(Cmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset));
                oDocumento = (SAPbobsCOM.Documents)(Cmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oInvoices));
                sMessage = "";

                //validar que exista procedimentos para tipo documento
                if (RunningUnderSQLServer)
                { s = "select ISNULL(U_ProcNomE,'') 'ProcNomE', ISNULL(U_ProcNomD,'') 'ProcNomD', ISNULL(U_ProcNomR,'') 'ProcNomR' from [@VID_FEPROCED] where ISNULL(U_Habili,'N') = 'Y' and U_TipoDocPE = '{0}'"; }
                else
                { s = @"select IFNULL(""U_ProcNomE"",'') ""ProcNomE"", IFNULL(""U_ProcNomD"",'') ""ProcNomD"", IFNULL(""U_ProcNomR"",'') ""ProcNomR"" from ""@VID_FEPROCED"" where IFNULL(""U_Habili"",'N') = 'Y' and ""U_TipoDocPE"" = '{0}'"; }

                s = String.Format(s, TipoDocElec);
                orsLocal.DoQuery(s);
                if (orsLocal.RecordCount == 0)
                {
                    //SBO_f.SBOApp.StatusBar.SetText("No se encuentra procedimientos para Documento electronico " + TipoDocElec, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                    throw new Exception("No se encuentra procedimientos para Documento electronico " + TipoDocElec);
                }
                else
                {
                    if ((System.String)(orsLocal.Fields.Item("ProcNomE").Value).ToString().Trim() == "")
                        //SBO_f.SBOApp.StatusBar.SetText("No se encuentra procedimiento de encabezado para Documento electronico " + TipoDocElec, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                        throw new Exception("No se encuentra procedimiento de encabezado para Documento electronico " + TipoDocElec);
                    else if ((System.String)(orsLocal.Fields.Item("ProcNomD").Value).ToString().Trim() == "")
                        //SBO_f.SBOApp.StatusBar.SetText("No se encuentra procedimiento de detalle para Documento electronico " + TipoDocElec, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                        throw new Exception("No se encuentra procedimiento de detalle para Documento electronico " + TipoDocElec);
                    else if ((System.String)(orsLocal.Fields.Item("ProcNomR").Value).ToString().Trim() == "")
                        //SBO_f.SBOApp.StatusBar.SetText("No se encuentra procedimiento de referencia para Documento electronico " + TipoDocElec, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                        throw new Exception("No se encuentra procedimiento de referencia para Documento electronico " + TipoDocElec);

                    ProcNomE = (System.String)(orsLocal.Fields.Item("ProcNomE").Value).ToString().Trim();
                    ProcNomD = (System.String)(orsLocal.Fields.Item("ProcNomD").Value).ToString().Trim();
                    ProcNomR = (System.String)(orsLocal.Fields.Item("ProcNomR").Value).ToString().Trim();
                }


                if ((oDocumento.GetByKey(Convert.ToInt32(DocEntry))) && (bImpresionOk))
                {
                    if (RunningUnderSQLServer)
                        s = "exec " + ProcNomE + "  " + oDocumento.DocEntry + ", '" + TipoDocElec + "'";
                    else
                        s = "CALL " + ProcNomE + "  (" + oDocumento.DocEntry + ", '" + TipoDocElec + "')";
                    //consulta por encabezado
                    orsLocal.DoQuery(s);
                    if (orsLocal.RecordCount > 0)
                    {
                        oNote_FM = new pe.facturamovil.Note();
                        oNote_FM.currency = ((System.String)orsLocal.Fields.Item("currency").Value).Trim();
                        oNote_FM.date = ((System.DateTime)orsLocal.Fields.Item("date").Value);
                        oNote_FM.series = ((System.String)orsLocal.Fields.Item("series").Value).Trim();
                        externalFolio = ((System.String)orsLocal.Fields.Item("externalFolio").Value).Trim();
                        oNote_FM.externalFolio = externalFolio;

                        var oNoteType = new pe.facturamovil.NoteType();
                        oNoteType.code = ((System.String)orsLocal.Fields.Item("noteType").Value).Trim();
                        oNoteType.isCredit = false;
                        oNote_FM.noteType = oNoteType;

                        var oClient = new pe.facturamovil.Client();
                        oClient.code = ((System.String)orsLocal.Fields.Item("code").Value).Trim();
                        oClient.name = ((System.String)orsLocal.Fields.Item("name").Value).Trim();
                        oClient.address = ((System.String)orsLocal.Fields.Item("address").Value).Trim();
                        var oDistrict = new pe.facturamovil.Municipality();
                        oDistrict.code = ((System.String)orsLocal.Fields.Item("municipality").Value).Trim();
                        oClient.municipality = oDistrict;
                        oClient.contact = ((System.String)orsLocal.Fields.Item("contact").Value).Trim();
                        oClient.phone = ((System.String)orsLocal.Fields.Item("phone").Value).Trim();

                        if (((System.String)orsLocal.Fields.Item("identityDocumentType").Value).Trim() != "")
                        {
                            var oid = new pe.facturamovil.IdentityDocumentType();
                            oid.code = ((System.String)orsLocal.Fields.Item("identityDocumentType").Value);
                            oClient.identityDocumentType = oid;
                        }

                        Email = ((System.String)orsLocal.Fields.Item("email").Value).Trim();
                        oClient.email = Email;
                        oNote_FM.client = oClient;
                        oNote_FM.expirationDate = ((System.DateTime)orsLocal.Fields.Item("expirationDate").Value);

                        try
                        {
                            var oAditional = new pe.facturamovil.AdditionalPrintInformation();
                            if (((System.String)orsLocal.Fields.Item("certificateNumber").Value).Trim() == "")
                                oAditional.certificateNumber = null;
                            else
                                oAditional.certificateNumber = ((System.String)orsLocal.Fields.Item("certificateNumber").Value).Trim();

                            if (((System.String)orsLocal.Fields.Item("contactP").Value).Trim() == "")
                                oAditional.contact = null;
                            else
                                oAditional.contact = ((System.String)orsLocal.Fields.Item("contactP").Value).Trim();

                            if (((System.String)orsLocal.Fields.Item("gloss").Value).Trim() == "")
                                oAditional.gloss = null;
                            else
                                oAditional.gloss = ((System.String)orsLocal.Fields.Item("gloss").Value).Trim();

                            if (((System.String)orsLocal.Fields.Item("project").Value).Trim() == "")
                                oAditional.project = null;
                            else
                                oAditional.project = ((System.String)orsLocal.Fields.Item("project").Value).Trim();

                            if (((System.String)orsLocal.Fields.Item("reference").Value).Trim() == "")
                                oAditional.reference = null;
                            else
                                oAditional.reference = ((System.String)orsLocal.Fields.Item("reference").Value).Trim();

                            if (((System.String)orsLocal.Fields.Item("account").Value).Trim() == "")
                                oAditional.account = null;
                            else
                                oAditional.account = ((System.String)orsLocal.Fields.Item("account").Value).Trim();

                            if (((System.String)orsLocal.Fields.Item("estimateNumber").Value).Trim() == "")
                                oAditional.estimateNumber = null;
                            else
                                oAditional.estimateNumber = ((System.String)orsLocal.Fields.Item("estimateNumber").Value).Trim();

                            oNote_FM.additionalPrintInformation = oAditional;
                        }
                        catch (Exception er)
                        {
                            SBO_f.oLog.OutLog("Error additionalPrintInformation - " + er.Message);
                        }

                        //DETALLE
                        if (RunningUnderSQLServer)
                            s = "exec " + ProcNomD + "  " + oDocumento.DocEntry + ", '" + TipoDocElec + "'";
                        else
                            s = "CALL " + ProcNomD + "  (" + oDocumento.DocEntry + ", '" + TipoDocElec + "')";
                        //consulta por detalle
                        orsLocal.DoQuery(s);
                        if (orsLocal.RecordCount > 0)
                        {
                            oNote_FM.details = new List<pe.facturamovil.Detail>();
                            while (!orsLocal.EoF)
                            {
                                var oProduct = new pe.facturamovil.Product();
                                var oService = new pe.facturamovil.Service();

                                if (((System.String)orsLocal.Fields.Item("DocType").Value).Trim() == "I")
                                {

                                    oProduct.code = ((System.String)orsLocal.Fields.Item("code").Value).Trim();
                                    oProduct.name = ((System.String)orsLocal.Fields.Item("name").Value).Trim();

                                    var oUM = new pe.facturamovil.Unit();
                                    oUM.code = ((System.String)orsLocal.Fields.Item("unit").Value).Trim();
                                    oProduct.unit = oUM;
                                    oProduct.price = ((System.Double)orsLocal.Fields.Item("price").Value);

                                    var oIGV = new pe.facturamovil.ExemptType();
                                    oIGV.code = ((System.String)orsLocal.Fields.Item("exemptType").Value).Trim();
                                    oProduct.exemptType = oIGV;
                                }
                                else if (((System.String)orsLocal.Fields.Item("DocType").Value).Trim() == "S")
                                {
                                    oService.name = ((System.String)orsLocal.Fields.Item("name").Value).Trim();

                                    var oUM = new pe.facturamovil.Unit();
                                    oUM.code = ((System.String)orsLocal.Fields.Item("unit").Value).Trim();
                                    oService.unit = oUM;
                                    oService.price = ((System.Double)orsLocal.Fields.Item("price").Value);
                                    var oIGV = new pe.facturamovil.ExemptType();
                                    oIGV.code = ((System.String)orsLocal.Fields.Item("exemptType").Value).Trim();
                                    oService.exemptType = oIGV;
                                }

                                var oDetail = new pe.facturamovil.Detail();

                                oDetail.position = ((System.Int32)orsLocal.Fields.Item("idLine").Value);

                                if (((System.String)orsLocal.Fields.Item("DocType").Value).Trim() == "I")
                                {
                                    oDetail.product = oProduct;
                                    oDetail.quantity = float.Parse(((System.Double)orsLocal.Fields.Item("quantity").Value).ToString().Trim());
                                    oDetail.description = ((System.String)orsLocal.Fields.Item("description").Value).Trim();
                                }
                                else if (((System.String)orsLocal.Fields.Item("DocType").Value).Trim() == "S")
                                {
                                    oDetail.service = oService;
                                    oDetail.description = ((System.String)orsLocal.Fields.Item("description").Value).Trim();
                                    oDetail.quantity = float.Parse(((System.Double)orsLocal.Fields.Item("quantity").Value).ToString().Trim());
                                }

                                oDetail.longDescription = ((System.String)orsLocal.Fields.Item("longDescription").Value).Trim();

                                oNote_FM.details.Add(oDetail);

                                orsLocal.MoveNext();
                            }//fin agregar detalle a la cabecera


                            //REFERENCIAS
                            if (RunningUnderSQLServer)
                                s = "exec " + ProcNomR + "  " + oDocumento.DocEntry + ", '" + TipoDocElec + "'";
                            else
                                s = "CALL " + ProcNomR + "  (" + oDocumento.DocEntry + ", '" + TipoDocElec + "')";
                            //consulta por referencia
                            orsLocal.DoQuery(s);
                            if (orsLocal.RecordCount > 0)
                            {
                                oNote_FM.references = new List<pe.facturamovil.Reference>();
                                while (!orsLocal.EoF)
                                {
                                    var oReference = new pe.facturamovil.Reference();
                                    oReference.position = ((System.Int32)orsLocal.Fields.Item("position").Value);
                                    var oDocType = new pe.facturamovil.DocumentType();
                                    oDocType.code = ((System.String)orsLocal.Fields.Item("documentType").Value).Trim();
                                    oReference.documentType = oDocType;
                                    oReference.referencedFolio = ((System.String)orsLocal.Fields.Item("referencedFolio").Value).Trim();
                                    oReference.date = ((System.DateTime)orsLocal.Fields.Item("date").Value);
                                    oReference.description = ((System.String)orsLocal.Fields.Item("description").Value).Trim();

                                    oNote_FM.references.Add(oReference);
                                    orsLocal.MoveNext();
                                }
                            }

                            //termina de cargar documento
                            JsonText = FacturaMovilGlobal.processor.getNoteJson(oNote_FM);
                            //oRecordSet.DoQuery("UPDATE [@OFMP] SET U_JSON='" + JsonText + "' WHERE DOCENTRY=1");

                            if (FacturaMovilGlobal.userConnected == null)
                            {
                                try
                                {

                                    LoginCount_FM = 0;
                                    //oUser_FM = new pe.facturamovil.User();
                                    if (oUserFM.token == null)
                                    {
                                        //oUserFM = new pe.facturamovil.User();
                                        if (RunningUnderSQLServer)
                                            orsLocal.DoQuery("SELECT U_User,U_Pwd,U_CCEmail FROM [@VID_FEPARAM] WHERE Code = '1'");
                                        else
                                            orsLocal.DoQuery(@"SELECT ""U_User"", ""U_Pwd"", ""U_CCEmail"" FROM ""@VID_FEPARAM"" WHERE ""Code"" = '1'");
                                        oUserFM = FacturaMovilGlobal.processor.Authenticate(((System.String)orsLocal.Fields.Item("U_User").Value).Trim(), ((System.String)orsLocal.Fields.Item("U_Pwd").Value).Trim());
                                        FacturaMovilGlobal.userConnected = oUserFM;

                                        var ii = 0;
                                        var bExistePE = false;

                                        if (oUserFM.companies.Find(c => c.code.Trim() == lRUC.Trim()) != null)
                                        {
                                            FacturaMovilGlobal.selectedCompany = oUserFM.companies.Single(c => c.code.Trim() == lRUC.Trim());
                                            bExistePE = true;
                                            ii = oUserFM.companies.Count;
                                        }

                                        if (!bExistePE)
                                            throw new Exception("No se ha encontrado el RUC " + lRUC + "en la conexion de Factura Movil");

                                        CCEmail_FM = ((System.String)orsLocal.Fields.Item("U_CCEmail").Value).Trim();
                                    }
                                }
                                catch (Exception ex)
                                {
                                    //if (SBO_f.SBOApp.MessageBox("No se pudo establecer conexion con el servidor. Desea Continuar?", 2, "Si", "No") == 2)
                                    //{
                                    //    throw new Exception("Motivos de error en conexion : " + ex.Message);
                                    //}
                                    bImpresionOk = false;
                                    sMessage = "Motivos de error en conexion : " + ex.Message;
                                }
                            }

                            try
                            {
                                if (bImpresionOk)
                                {
                                    FacturaMovilGlobal.processor.sendNote(FacturaMovilGlobal.selectedCompany, oNote_FM, FacturaMovilGlobal.userConnected.token);
                                    Id = oNote_FM.id.ToString();
                                    Validation = oNote_FM.validation;
                                    //orsLocal.DoQuery("UPDATE OINV SET U_FM_MDFE='Y' WHERE NUMATCARD='" + NumAtCard + "' AND DOCSUBTYPE='--'")
                                    SBO_f.SBOApp.StatusBar.SetText("Nota de Debito emitida con exito.", SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                    FacturaMovilGlobal.processor.showDocument(oNote_FM);


                                    if (Email != "")
                                    {
                                        SBO_f.SBOApp.StatusBar.SetText("Enviando documento via email. Porfavor Espere...", SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                        FacturaMovilGlobal.processor.sendEmail(FacturaMovilGlobal.selectedCompany, oNote_FM, Email, CCEmail_FM, FacturaMovilGlobal.userConnected.token);
                                        SBO_f.SBOApp.StatusBar.SetText("Nota de Debito emitida y enviada al cliente electronicamente con exito. Numero SUNAT : " + externalFolio, SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                                    }
                                    else
                                        SBO_f.SBOApp.StatusBar.SetText("Factura emitida electronicamente con exito. Asegurese de enviar el documento al cliente. Numero SUNAT : " + externalFolio, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                                }
                            }
                            catch (Exception ex)
                            {
                                SBO_f.SBOApp.StatusBar.SetText(ex.Message, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                SBO_f.oLog.OutLog("EnviarND_PE " + ex.Message + " ** Trace: " + ex.StackTrace);
                                bImpresionOk = false;
                                sMessage = ex.Message;
                            }
                        }
                        else
                        {
                            SBO_f.SBOApp.StatusBar.SetText("No se encuentra Datos en detalle", SAPbouiCOM.BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                            bImpresionOk = false;
                        }

                    }
                    else
                    {
                        SBO_f.SBOApp.StatusBar.SetText("No se encuentra Datos en encabezado", SAPbouiCOM.BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                        bImpresionOk = false;
                    }
                }
                else
                {
                    SBO_f.SBOApp.StatusBar.SetText("Error - No se ha encontrado el documento", SAPbouiCOM.BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                    bImpresionOk = false;
                }

                DocDate = SBO_f.DateToStr(oDocumento.DocDate);

                if (!bImpresionOk)
                {
                    //SBO_f.SBOApp.MessageBox("Error envio documento electronico ");
                    SBO_f.SBOApp.StatusBar.SetText("Error envio documento electrónico (1)", SAPbouiCOM.BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                    sObjType = "13";
                    Status = "EE";
                    if (sMessage == "")
                        sMessage = "Error envio documento electronico a Factura Movil";
                }
                else
                {
                    Status = "EC";
                    sObjType = "13";
                    sMessage = "Enviado satisfactoriamente a Factura Movil";
                    SBO_f.SBOApp.StatusBar.SetText("Se ha enviado satisfactoriamente el documento electronico", SAPbouiCOM.BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success);
                    //oDocumento.UserFields.Fields.Item("U_EstadoFE").Value = "P";
                    //actualizo campo de impresion para que no aparezca formulario solicitando folio
                    oDocumento.Printed = PrintStatusEnum.psYes;
                    lRetCode = oDocumento.Update();
                    if (lRetCode != 0)
                    {
                        s = SBO_f.Cmpny.GetLastErrorDescription();
                        SBO_f.SBOApp.StatusBar.SetText("Error actualizar documento - " + s, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                        //SBO_f.oLog.OutLog("Error actualizar Nota debito - " + s);
                    }
                }

                if (RunningUnderSQLServer)
                    s = "SELECT DocEntry, U_Status FROM [@VID_FELOG] WITH (NOLOCK) WHERE U_DocEntry = {0} AND U_ObjType = '{1}' AND U_SubType = '{2}'";
                else
                    s = @"SELECT ""DocEntry"", ""U_Status"" FROM ""@VID_FELOG"" WHERE ""U_DocEntry"" = {0} AND ""U_ObjType"" = '{1}' AND ""U_SubType"" = '{2}' ";
                s = String.Format(s, DocEntry, sObjType, DocSubType);
                orsLocal.DoQuery(s);
                Reg = new TFunctions();
                Reg.SBO_f = SBO_f;

                if (sMessage.Length > 254)
                    sMessage = sMessage.Substring(0, 253);

                if (orsLocal.RecordCount == 0)
                    Reg.FELOGAdd(Int32.Parse(DocEntry), sObjType, DocSubType, SeriePE, Int32.Parse(FolioNum), Status, sMessage, TipoDocElec, SBO_f.SBOApp.Company.UserName, JsonText, Id, Validation, DocDate);
                else
                {
                    if ((System.String)(orsLocal.Fields.Item("U_Status").Value) != "RR")
                        Reg.FELOGUptM((System.Int32)(orsLocal.Fields.Item("DocEntry").Value), Int32.Parse(DocEntry), sObjType, DocSubType, SeriePE, Int32.Parse(FolioNum), Status, sMessage, TipoDocElec, SBO_f.SBOApp.Company.UserName, JsonText, Id, Validation, DocDate);
                    else
                        SBO_f.SBOApp.StatusBar.SetText("Documento ya se encuentra en Factura Movil y en SUNAT", SAPbouiCOM.BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success);
                }

            }
            catch (Exception e)
            {
                SBO_f.SBOApp.StatusBar.SetText(e.Message, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                SBO_f.oLog.OutLog("EnviarDN_PE " + e.Message + " ** Trace: " + e.StackTrace);
            }
        }


        //Para PEru EasyDot
        public new void EnviarFE_PE_ED(String DocEntry, Boolean RunningUnderSQLServer, String SeriePE, String FolioNum, String TipoDocElec, String sObjType, String DocSubType, String lRUC, SqlConnection MConexionADO, String TipoDocElecAddon)
        {
            String URL;
            String Procedimiento;
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
            SqlCommand comando1 = new SqlCommand();
            System.Data.DataTable rTable;
            SqlDataAdapter adapter;
            SAPbobsCOM.Recordset ors = ((SAPbobsCOM.Recordset)Cmpny.GetBusinessObject(BoObjectTypes.BoRecordset));
            try
            {

                if (ConexionADO == null)
                    ConexionADO = MConexionADO;

                if (RunningUnderSQLServer)
                    s = @"SELECT U_URLEasyDot 'URL', ISNULL(U_UserED,'') 'User', ISNULL(U_PwdED,'') 'Pass' FROM [@VID_FEPARAM]";
                else
                    s = @"SELECT ""U_URLEasyDot"" ""URL"", IFNULL(""U_UserED"",'') ""User"", IFNULL(""U_PwdED"",'') ""Pass"" FROM ""@VID_FEPARAM"" ";

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

                    URL = ((System.String)ors.Fields.Item("URL").Value).Trim() + "/SendDocument.ashx";
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
                        Procedimiento = ((System.String)ors.Fields.Item("ProcNomE").Value).Trim();
                        if (RunningUnderSQLServer)
                            s = @"exec {0} {1}, '{2}', '{3}'";//Factura
                        else
                            s = @"call {0} ({1}, '{2}', '{3}')";//Factura
                        s = String.Format(s, Procedimiento, DocEntry, TipoDocElec, sObjType);
                        if (ConexionADO.State == ConnectionState.Closed)
                            ConexionADO.Open();

                        comando1.Connection = ConexionADO;
                        comando1.CommandText = s;
                        rTable = new System.Data.DataTable();
                        adapter = new SqlDataAdapter(comando1);
                        adapter.Fill(rTable);
                        if (rTable.Rows.Count > 0)
                        {
                            var i = 0;
                            foreach (DataRow row in rTable.Rows)
                            {
                                if (i == 0)
                                    s = row[0].ToString().Trim();
                                else
                                    s += row[0].ToString().Trim();
                                i++;
                            }
                        }

                        if (ConexionADO.State == ConnectionState.Open)
                            ConexionADO.Close();

                        if (s == "")
                            throw new Exception("No se encuentra datos para Documento electronico " + TipoDocElec);
                        else
                        {
                            var bImpresion = false;
                            oXml = new XmlDocument();
                            oXml.LoadXml(s);
                            //obtiene string de pdf

                            s = Reg.PDFenString(TipoDocElecAddon, DocEntry, sObjType, SeriePE, FolioNum, RunningUnderSQLServer);

                            if (s == "")
                                throw new Exception("No se ha creado PDF");

                            //s = Reg.Base64Encode(s);

                            //Agrega el PDF al xml
                            XmlNode node;
                            if (oXml.SelectSingleNode("//CamposExtras") == null)
                                node = oXml.CreateNode(XmlNodeType.Element, "CamposExtras", null);
                            else
                                node = oXml.SelectSingleNode("//CamposExtras");
                            
                            XmlNode nodePDF = oXml.CreateElement("PDF");
                            nodePDF.InnerText = s;
                            node.AppendChild(nodePDF);
                            oXml.DocumentElement.AppendChild(node);

                            s = Reg.UpLoadDocumentByUrl(oXml, RunningUnderSQLServer, URL, userED, passED);

                            //SBO_f.SBOApp.StatusBar.SetText(s, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);

                            oXml.LoadXml(s);
                            //var Configuracion = oXml.GetElementsByTagName("Error");
                            var lista = ((XmlElement)oXml.GetElementsByTagName("Error")[0]).GetElementsByTagName("ErrorText");
                            var ErrorText = lista[0].InnerText;
                            lista = ((XmlElement)oXml.GetElementsByTagName("Error")[0]).GetElementsByTagName("ErrorCode");
                            var ErrorCode = lista[0].InnerText;

                            if (ErrorCode != "0")
                            {
                                SBO_f.SBOApp.StatusBar.SetText("Error envio documento electrónico (1)", SAPbouiCOM.BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                //sObjType = "13";
                                if (ErrorCode == "-103")
                                    Status = "RR";
                                else
                                    Status = "EE";
                                sMessage = ErrorText;
                                if (sMessage == "")
                                    sMessage = "Error envio documento electronico a EasyDot";
                            }
                            else
                            {
                                Status = "RR";
                                //sObjType = "13";
                                sMessage = "Enviado satisfactoriamente a EasyDot y Aceptado";
                                SBO_f.SBOApp.StatusBar.SetText("Se ha enviado satisfactoriamente el documento electronico", SAPbouiCOM.BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success);
                                var oDocumento = (SAPbobsCOM.Documents)(Cmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oInvoices));
                                if (oDocumento.GetByKey(Convert.ToInt32(DocEntry)))
                                {
                                    DocDate = SBO_f.DateToStr(oDocumento.DocDate);
                                    oDocumento.Printed = PrintStatusEnum.psYes;
                                    lRetCode = oDocumento.Update();
                                    if (lRetCode != 0)
                                    {
                                        s = SBO_f.Cmpny.GetLastErrorDescription();
                                        SBO_f.SBOApp.StatusBar.SetText("Error actualizar documento - " + s, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                                        sMessage = "Error actualizar documento - " + s;
                                        //SBO_f.oLog.OutLog("Error actualizar Nota debito - " + s);
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
                                s = "SELECT DocEntry, U_Status FROM [@VID_FELOG] WITH (NOLOCK) WHERE U_DocEntry = {0} AND U_ObjType = '{1}' AND U_SubType = '{2}'";
                            else
                                s = @"SELECT ""DocEntry"", ""U_Status"" FROM ""@VID_FELOG"" WHERE ""U_DocEntry"" = {0} AND ""U_ObjType"" = '{1}' AND ""U_SubType"" = '{2}' ";
                            s = String.Format(s, DocEntry, sObjType, DocSubType);
                            ors.DoQuery(s);
                            if (ors.RecordCount == 0)
                                Reg.FELOGAdd(Int32.Parse(DocEntry), sObjType, DocSubType, SeriePE, Int32.Parse(FolioNum), Status, sMessage, TipoDocElec, SBO_f.SBOApp.Company.UserName, "", ErrorCode, ErrorText, DocDate);
                            else
                            {
                                if ((System.String)(ors.Fields.Item("U_Status").Value) != "RR")
                                {
                                    SBO_f.SBOApp.StatusBar.SetText("Documento se ha enviado a EasyDot", SAPbouiCOM.BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success);
                                    Reg.FELOGUptM((System.Int32)(ors.Fields.Item("DocEntry").Value), Int32.Parse(DocEntry), sObjType, DocSubType, SeriePE, Int32.Parse(FolioNum), Status, sMessage, TipoDocElec, SBO_f.SBOApp.Company.UserName, "", ErrorCode, ErrorText, DocDate);
                                }
                                else
                                    SBO_f.SBOApp.StatusBar.SetText("Documento ya se ha enviado anteriormente a EasyDot y se encuentra en Sunat", SAPbouiCOM.BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success);
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
                if (ConexionADO.State == ConnectionState.Open)
                    ConexionADO.Close();
            }
        }

        private Boolean ValidarDatosFE_PE()
        {
            Boolean _result;
            SAPbouiCOM.DBDataSource oDBDSDir;
            SAPbouiCOM.DBDataSource oDBDSH;
            TFunctions Param;
            Boolean DocElec;
            String Tabla;
            Int32 i;
            SAPbouiCOM.EditText oEditText;
            SAPbouiCOM.ComboBox oComboBox;
            String TipoLinea = "";
            String TipoDoc = "";
            String TipoDocElec = "";
            String[] CaracteresInvalidos = { "Ñ", "°", "|", "!", @"""", "#", "$", "=", "?", "\\", "¿", "¡", "~", "´", "+", "{", "}", "[", "]", "-", ":", "%" };
            String s1;
            Int32 CantLineas;
            Boolean bExtranjero = false;
            String VatStatus = "Y";
            String BPP_BPTP = "";

            try
            {
                _result = true;
                if (ObjType == "203")
                {
                    oDBDSDir = oForm.DataSources.DBDataSources.Item("DPI12");
                    oDBDSH = oForm.DataSources.DBDataSources.Item("ODPI");
                }
                else
                {
                    oDBDSDir = oForm.DataSources.DBDataSources.Item("INV12");
                    oDBDSH = oForm.DataSources.DBDataSources.Item("OINV");
                }

                var sDocSubType = (System.String)(oDBDSH.GetValue("DocSubType", 0)).Trim();

                if (sDocSubType == "--") //Factura
                    TipoDocElec = "01";
                else if (sDocSubType == "DN") //Nota Debito
                    TipoDocElec = "08";
                else if (sDocSubType == "IB") //Boleta
                    TipoDocElec = "03";


                if ((TipoDocElec == "01") || (TipoDocElec == "03") || (TipoDocElec == "08"))
                {
                    if (GlobalSettings.RunningUnderSQLServer)
                        s = @"select VatStatus, ISNULL(U_BPP_BPTP,'') BPP_BPTP from OCRD where CardCode = '{0}'";
                    else
                        s = @"select ""VatStatus"", IFNULL(""U_BPP_BPTP"",'') ""BPP_BPTP"" from ""OCRD"" where ""CardCode"" = '{0}'";
                    s = String.Format(s, (System.String)(oDBDSH.GetValue("CardCode", 0)).Trim());
                    oRecordSet.DoQuery(s);
                    if (oRecordSet.RecordCount > 0)
                    {
                        VatStatus = ((System.String)oRecordSet.Fields.Item("VatStatus").Value).Trim();
                        BPP_BPTP = ((System.String)oRecordSet.Fields.Item("BPP_BPTP").Value).Trim();
                    }

                    if ((TipoDocElec != "03") && (((VatStatus != "N") || (BPP_BPTP != "SND")) && ((TipoDocElec == "01") || (TipoDocElec == "08"))))
                    {
                        if ((System.String)(oDBDSDir.GetValue("CityB", 0)).Trim() == "")
                        {
                            FSBOApp.StatusBar.SetText("Debe ingresar ciudad en Destinatario de Factura", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                            _result = false;
                        }

                        //if (((System.String)(oDBDSDir.GetValue("CityS", 0)).Trim() == "") && (_result))
                        //{
                        //    FSBOApp.StatusBar.SetText("Debe ingresar ciudad en Destinatario de Despacho", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                        //    _result = false;
                        //}

                        if (((System.String)(oDBDSDir.GetValue("BlockB", 0)).Trim() == "") && (_result))
                        {
                            FSBOApp.StatusBar.SetText("Debe ingresar comuna en Destinatario de Factura", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                            _result = false;
                        }

                        //if (((System.String)(oDBDSDir.GetValue("CountyS", 0)).Trim() == "") && (_result))
                        //{
                        //    FSBOApp.StatusBar.SetText("Debe ingresar comuna en Destinatario de Despacho", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                        //    _result = false;
                        //}

                        if (((System.String)(oDBDSDir.GetValue("StreetB", 0)).Trim() == "") && (_result))
                        {
                            FSBOApp.StatusBar.SetText("Debe ingresar calle en Destinatario de Factura", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                            _result = false;
                        }

                        //if (((System.String)(oDBDSDir.GetValue("StreetS", 0)).Trim() == "") && (_result))
                        //{
                        //    FSBOApp.StatusBar.SetText("Debe ingresar calle en Destinatario de Despacho", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                        //    _result = false;
                        //}

                    }

                    s = (System.String)(oDBDSH.GetValue("CardName", 0)).Trim();
                    if ((s == "") && (_result))
                    {
                        FSBOApp.StatusBar.SetText("Debe ingresar Nombre Cliente", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                        _result = false;
                    }

                    ////valida RUC
                    //se deja comentado, por problemas en la validacion de un cliente, Jimmy colocara una validacion en el TN 20151204
                    //se dejo valida para no tener errores al enviar el documento a easydoc 20170719
                    if ((_result) && ((TipoDocElec != "03") && (((VatStatus != "N") || (BPP_BPTP != "SND")) && ((TipoDocElec == "01") || (TipoDocElec == "08")))))
                    {
                        Param = new TFunctions();
                        Param.SBO_f = FSBOf;
                        s = Param.ValidarRuc((System.String)(oDBDSH.GetValue("LicTradNum", 0)));
                        if (s != "OK")
                        {
                            FSBOApp.StatusBar.SetText(s, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                            _result = false;
                        }
                    }

                    if (_result)
                    {

                        if (GlobalSettings.RunningUnderSQLServer)
                        {
                            //s = "select ISNULL(U_ValDescL,'Y') 'ValDescL' from [@VID_FEPARAM]";
                            s1 = "select ISNULL(U_CantLineas,0) CantLineas from [@VID_FEPROCED] where U_TipoDocPE = '" + TipoDocElec + "' and U_Habili = 'Y'";
                        }

                        else
                        {
                            //s = @"select IFNULL(""U_ValDescL"",'Y') ""ValDescL"" from ""@VID_FEPARAM"" ";
                            s1 = @"select IFNULL(""U_CantLineas"",0) ""CantLineas"" from ""@VID_FEPROCED"" where ""U_TipoDocPE"" = '" + TipoDocElec + @"' and ""U_Habili"" = 'Y'";
                        }

                        oRecordSet.DoQuery(s1);
                        if (oRecordSet.RecordCount > 0)
                            CantLineas = (System.Int32)(oRecordSet.Fields.Item("CantLineas").Value);
                        else
                        {
                            FSBOApp.StatusBar.SetText("Debe parametrizar el maximo de lineas para documento " + TipoDocElec, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                            return false;
                        }

                        oComboBox = (ComboBox)(oForm.Items.Item("3").Specific);
                        TipoDoc = oComboBox.Selected.Value.Trim();
                        if (TipoDoc == "S")
                            mtx = (Matrix)(oForm.Items.Item("39").Specific);
                        else
                            mtx = (Matrix)(oForm.Items.Item("38").Specific);


                        if ((mtx.RowCount - 1 > CantLineas) && (((System.String)oDBDSH.GetValue("SummryType", 0)).Trim() == "N")) //valida total de lineas solo cuando no es resumen
                        {
                            FSBOApp.StatusBar.SetText("Cantidad de lineas supera lo permitido, parametrización FE", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                            return false;
                        }

                        i = 1;
                        while (i < mtx.RowCount)
                        {
                            if (TipoDoc == "S") //System.String(oDBDSH.GetValue("DocType",0)).Trim()
                            {
                                TipoLinea = "";
                            }
                            else
                            {
                                oComboBox = (ComboBox)(mtx.Columns.Item("257").Cells.Item(i).Specific);
                                TipoLinea = (System.String)(oComboBox.Selected.Value);
                            }

                            //if ((System.String)(oRecordSet.Fields.Item("ValDescL").Value) == "Y")
                            //{
                            //    if (TipoDoc == "S") //System.String(oDBDSH.GetValue("DocType",0)).Trim()
                            //    {
                            //        oEditText = (EditText)(mtx.Columns.Item("6").Cells.Item(i).Specific);
                            //    }
                            //    else
                            //    {
                            //        oEditText = (EditText)(mtx.Columns.Item("15").Cells.Item(i).Specific);
                            //    }

                            //    if ((Convert.ToDouble((System.String)(oEditText.Value)) < 0) && (TipoLinea == ""))
                            //    {
                            //        s = "Descuento negativo en la linea " + Convert.ToString(i);
                            //        FSBOApp.StatusBar.SetText(s, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                            //        _result = false;
                            //        i = mtx.RowCount;
                            //    }
                            //}

                            if (_result)
                            {
                                if (TipoDoc == "S")
                                    oEditText = (EditText)(mtx.Columns.Item("1").Cells.Item(i).Specific);
                                else
                                    oEditText = (EditText)(mtx.Columns.Item("3").Cells.Item(i).Specific);

                                s = oEditText.Value;
                                if ((s == "") && (TipoLinea == ""))
                                {
                                    FSBOApp.StatusBar.SetText("Debe ingresar descripción en la linea " + Convert.ToString(i), BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                    _result = false;
                                    i = mtx.RowCount;
                                }

                            }

                            i++;
                        }


                        //validacion solo para nota de debito
                        if (TipoDocElec == "08")
                        {
                            //Validacion tipo de operacion
                            if ((System.String)(oDBDSH.GetValue("U_BPP_MDTN", 0)).Trim() == "")
                            {
                                FSBOApp.StatusBar.SetText("Debe ingresar tipo de operacion", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                return false;
                            }
                            //else if ((System.String)(oDBDSH.GetValue("U_BPP_MDTD", 0)).Trim() != "08")
                            //{
                            //    FSBOApp.StatusBar.SetText("El documento es una Nota de Debito y debe tener Tipo documento 08", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                            //    return false; 
                            //}
                            else if (((System.String)(oDBDSH.GetValue("U_BPP_MDTO", 0)).Trim() != "01") && ((System.String)(oDBDSH.GetValue("U_BPP_MDTO", 0)).Trim() != "03"))
                            {
                                FSBOApp.StatusBar.SetText("Solo puede tener como referencia una factura o boleta", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                return false;
                            }
                            else
                            {
                                if (GlobalSettings.RunningUnderSQLServer)
                                {
                                    s = @"select U_TypeCode
                                            from [@FM_NOTES] 
                                           where Code = '{0}' ";
                                }
                                else
                                {
                                    s = @"select ""U_TypeCode""
                                            from ""@FM_NOTES""
                                           where ""Code"" = '{0}' ";
                                }
                                s = String.Format(s, (System.String)(oDBDSH.GetValue("U_BPP_MDTN", 0)).Trim());
                                oRecordSet.DoQuery(s);
                                if (oRecordSet.RecordCount == 0)
                                {
                                    FSBOApp.StatusBar.SetText("No se encuentra tipo de operacion", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                    return false;
                                }
                                //else if (((System.String)(oRecordSet.Fields.Item("Distribuido").Value)).Trim() != "02")
                                else if ((System.String)(oDBDSH.GetValue("U_BPP_MDTN", 0)).Trim() != "11")
                                {
                                    FSBOApp.StatusBar.SetText("Debe seleccionar tipo de operacion valida por FM", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                    return false;
                                }
                            }
                        }
                    }
                }

                return _result;
            }
            catch (Exception e)
            {
                FSBOApp.StatusBar.SetText(e.Message, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                OutLog("ValidarDatosFE_PE " + e.Message + " ** Trace: " + e.StackTrace);
                return false;
            }

        }


    }//fin Class
}
