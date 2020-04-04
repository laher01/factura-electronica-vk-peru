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
using VisualD.untLog;
using Factura_Electronica_VK.Functions;
using System.Data.SqlClient;
using Factura_Electronica_VK.CreditNotes;
using Factura_Electronica_VK.DeliveryNote;
using Factura_Electronica_VK.Invoice;
using Factura_Electronica_VK.PagoEfectuado;
using System.Data;

namespace Factura_Electronica_VK.ReImprimir
{
    class TReImprimir : TvkBaseForm, IvkFormInterface
    {
        private SAPbobsCOM.Recordset oRecordSet;
        private SAPbouiCOM.Form oForm;
        private CultureInfo _nf = new System.Globalization.CultureInfo("en-US");
        private String s;
        private SAPbouiCOM.EditText oEditText;
        private String URL_PE;
        //Para Peru
        private String RUC;

        public new bool InitForm(string uid, string xmlPath, ref Application application, ref SAPbobsCOM.Company company, ref CSBOFunctions SBOFunctions, ref TGlobalVid _GlobalSettings)
        {
            SAPbouiCOM.ComboBox oCombo;
            //return inherited InitForm(uid, xmlPath,var application,var company,var sboFunctions,var _GlobalSettings );
            bool Result = base.InitForm(uid, xmlPath, ref application, ref company, ref SBOFunctions, ref _GlobalSettings);
            try
            {
                oRecordSet = (SAPbobsCOM.Recordset)(FCmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset));

                //Lista    := New list<string>;

                FSBOf.LoadForm(xmlPath, "VID_ReImprimir.srf", uid);
                //EnableCrystal := true;
                oForm = FSBOApp.Forms.Item(uid);
                oForm.Freeze(true);
                oForm.AutoManaged = false;
                oForm.SupportedModes = -1;             // afm_All

                //oForm.DataBrowser.BrowseBy := "DocNum"; 

                // Ok Ad  Fnd Vw Rq Sec
                //Lista.Add('DocNum    , f,  f,  t,  f, n, 1');
                //Lista.Add('DocDate   , f,  t,  f,  f, r, 1');
                //Lista.Add('CardCode  , f,  t,  t,  f, r, 1');
                //FSBOf.SetAutoManaged(var oForm, Lista);

                if (GlobalSettings.RunningUnderSQLServer)
                    s = "select ISNULL(U_URLEasyDot,'') URL from [@VID_FEPARAM] where Code = '1'";
                else
                    s = @"select IFNULL(""U_URLEasyDot"",'') ""URL"" from ""@VID_FEPARAM"" where ""Code"" = '1' ";

                oRecordSet.DoQuery(s);
                if (oRecordSet.RecordCount == 0)
                    throw new Exception("Debe parametrizar el Addon Factura Electronica");
                else
                {
                    URL_PE = ((System.String)oRecordSet.Fields.Item("URL").Value).Trim();
                    if (URL_PE == "")
                        throw new Exception("Debe ingresar URL en parametros del Addon Factura Electronica");
                    
                }

                oCombo = (ComboBox)(oForm.Items.Item("TipDoc").Specific);
                oCombo.ValidValues.Add("01", "Factura");
                oCombo.ValidValues.Add("01A", "Factura de Anticipo");
                oCombo.ValidValues.Add("01X", "Factura de Exportación");
                oCombo.ValidValues.Add("03", "Boleta Venta");
                oCombo.ValidValues.Add("07", "Nota de Credito");
                oCombo.ValidValues.Add("08", "Nota de Debito");
                oCombo.ValidValues.Add("09T", "Guia Remisión x Transferencia");
                oCombo.ValidValues.Add("09D", "Guia Remisión x Devol. Compra");
                oCombo.ValidValues.Add("09", "Guia Remisión x Entrega");
                oCombo.ValidValues.Add("20", "Comprobante Retención");
                //oCombo.ValidValues.Add("09", "Guia de Remision Remitente");
                //oCombo.ValidValues.Add("12", "Ticket de Maquina Registradora");
                //oCombo.ValidValues.Add("31", "Guia Remision Transportista");

                oForm.Items.Item("Folio").Visible = true;
                oForm.Items.Item("FolioPref").Visible = true;

                oForm.DataSources.UserDataSources.Add("Folio", BoDataType.dt_SHORT_TEXT, 10);
                oEditText = (EditText)(oForm.Items.Item("Folio").Specific);
                oEditText.DataBind.SetBound(true, "", "Folio");


                oForm.DataSources.UserDataSources.Add("FolioPref", BoDataType.dt_SHORT_TEXT, 4);
                oEditText = (EditText)(oForm.Items.Item("FolioPref").Specific);
                oEditText.DataBind.SetBound(true, "", "FolioPref");

                ((SAPbouiCOM.ComboBox)oForm.Items.Item("TipDoc").Specific).Active = true;

                if (GlobalSettings.RunningUnderSQLServer)
                    s = "select ISNULL(TaxIdNum,'') TaxIdNum from OADM ";
                else
                    s = @"select IFNULL(""TaxIdNum"",'') ""TaxIdNum"" from ""OADM"" ";

                oRecordSet.DoQuery(s);
                if (oRecordSet.RecordCount == 0)
                    throw new Exception("Debe ingresar RUC de Emisor, Gestión -> Inicialización Sistema -> Detalle Sociedad -> Datos de Contabilidad -> ID fiscal general 1");
                else
                    RUC = ((System.String)oRecordSet.Fields.Item("TaxIdNum").Value).Trim();

                //s := '1';
                //oCombo.Select(s, BoSearchKey.psk_ByValue);

                //EditText(oForm.Items.Item('CardCode').Specific).Active := True;
                oForm.Mode = BoFormMode.fm_OK_MODE;

                oForm.DataSources.UserDataSources.Item("Folio").Value = "";
            }
            catch (Exception e)
            {
                OutLog("InitForm: " + e.Message + " ** Trace: " + e.StackTrace);
                FSBOApp.MessageBox(e.Message + " ** Trace: " + e.StackTrace, 1, "Ok", "", "");
            }
            oForm.Freeze(false);
            return Result;
        }//fin InitForm


        public new void FormEvent(String FormUID, ref SAPbouiCOM.ItemEvent pVal, ref Boolean BubbleEvent)
        {
            base.FormEvent(FormUID, ref pVal, ref BubbleEvent);
            try
            {
                if ((pVal.EventType == BoEventTypes.et_ITEM_PRESSED) && (pVal.BeforeAction == false) && (pVal.ItemUID == "btn1"))
                {
                    BubbleEvent = false;
                    if (ValidarPE())
                        ImprimirPE();

                }
            }
            catch (Exception e)
            {
                FSBOApp.StatusBar.SetText(e.Message + " ** Trace: " + e.StackTrace, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                OutLog("FormEvent: " + e.Message + " ** Trace: " + e.StackTrace);
            }
        }//fin FormEvent

        private Boolean ValidarPE()
        {
            Boolean _result = true;
            String sFolio = "";
            String sSerie = "";
            String sTipo;
            String Tabla = "";
            String TablaDir = "";
            String TablaDetalle = "";
            Int32 i32 = 0;
            Boolean canConvert;
            String sDocSubType = "";
            String sDocEntry;
            String ObjType;
            String TipoDocElect = "";
            SAPbouiCOM.EditText oEditText;
            SAPbouiCOM.ComboBox oComboBox;

            try
            {
                oEditText = (EditText)(oForm.Items.Item("Folio").Specific);
                sFolio = oEditText.Value;
                oEditText = (EditText)(oForm.Items.Item("FolioPref").Specific);
                sSerie = oEditText.Value;
                oComboBox = (ComboBox)(oForm.Items.Item("TipDoc").Specific);
                sTipo = oComboBox.Value;

                if (sTipo == "01") //Factura venta
                {
                    Tabla = "OINV";
                    TablaDetalle = "INV1";
                    TablaDir = "INV12";
                    sDocSubType = "--";
                    TipoDocElect = "01";
                }
                else if (sTipo == "01A") //Factura anticipo
                {
                    Tabla = "ODPI";
                    TablaDetalle = "DPI1";
                    TablaDir = "DPI12";
                    sDocSubType = "--";
                    TipoDocElect = "01";
                }
                else if (sTipo == "01X") //Factura Exportacion
                {
                    Tabla = "OINV";
                    TablaDetalle = "INV1";
                    TablaDir = "INV12";
                    sDocSubType = "IX";
                    TipoDocElect = "01";
                }
                else if (sTipo == "03") //Boleta
                {
                    Tabla = "OINV";
                    TablaDetalle = "INV1";
                    TablaDir = "INV12";
                    sDocSubType = "IB";
                    TipoDocElect = "03";
                }
                else if (sTipo == "08") //nota debito
                {
                    Tabla = "OINV";
                    TablaDetalle = "INV1";
                    TablaDir = "INV12";
                    sDocSubType = "DN";
                    TipoDocElect = "08";
                }
                else if (sTipo == "07") //nota de credito
                {
                    Tabla = "ORIN";
                    TablaDetalle = "RIN1";
                    TablaDir = "RIN12";
                    sDocSubType = "--";
                    TipoDocElect = "07";
                }
                else if (sTipo == "09") //guia remision Entrega
                {
                    Tabla = "ODLN";
                    TablaDetalle = "DLN1";
                    TablaDir = "DLN12";
                    sDocSubType = "--";
                    TipoDocElect = "09";
                }
                else if (sTipo == "09D") //guia remision Devolucion compra
                {
                    Tabla = "ORPD";
                    TablaDetalle = "RPD1";
                    TablaDir = "RPD12";
                    sDocSubType = "--";
                    TipoDocElect = "09";
                }
                else if (sTipo == "09T") //guia remision Transferencia
                {
                    Tabla = "OWTR";
                    TablaDetalle = "WTR1";
                    TablaDir = "WTR12";
                    sDocSubType = "--";
                    TipoDocElect = "09";
                }
                else if (sTipo == "20")
                {
                    Tabla = "OVPM";
                    sDocSubType = "--";
                    TipoDocElect = "20";
                }

                if (sTipo == "")
                {
                    _result = false;
                    FSBOApp.StatusBar.SetText("Debe seleccionar Tipo Documento Electronico", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                }
                else if (Tabla == "")
                {
                    _result = false;
                    FSBOApp.StatusBar.SetText("No se reconoce Tipo Documento Electronico", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                }
                else if (sFolio == "")
                {
                    _result = false;
                    FSBOApp.StatusBar.SetText("Debe ingresar Numero de Folio", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                }
                else if (sSerie == "")
                {
                    _result = false;
                    FSBOApp.StatusBar.SetText("Debe ingresar la Serie", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                }
                else
                {
                    canConvert = System.Int32.TryParse(sFolio, out i32);
                    if (!canConvert)
                    {
                        _result = false;
                        FSBOApp.StatusBar.SetText("Numero de Folio debe ser numerico", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                    }
                }

                if (_result)
                {
                    if (GlobalSettings.RunningUnderSQLServer)
                        s = @"SELECT U_Status FROM [@VID_FELOG] WHERE U_SeriePE = '{0}' AND U_FolioNum = {1}";
                    else
                        s = @"SELECT ""U_Status"" FROM ""@VID_FELOG"" WHERE ""U_SeriePE"" = '{0}' AND ""U_FolioNum"" = {1}";
                    s = String.Format(s, sSerie, i32);
                    oRecordSet.DoQuery(s);
                    if (oRecordSet.RecordCount != 0)
                    {
                        if ((((System.String)oRecordSet.Fields.Item("U_Status").Value).Trim() == "EC") || (((System.String)oRecordSet.Fields.Item("U_Status").Value).Trim() == "RR")
                            || (((System.String)oRecordSet.Fields.Item("U_Status").Value).Trim() == "RZ") || (((System.String)oRecordSet.Fields.Item("U_Status").Value).Trim() == "DB"))
                        {
                            FSBOApp.StatusBar.SetText("Numero de Folio a sido enviado anteriormente", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                            return false;
                        }
                    }
                    
                    if (sTipo != "20")
                    {
                        if (GlobalSettings.RunningUnderSQLServer)
                        {
                            s = @"SELECT SUBSTRING(UPPER(T2.BeginStr), 1, 1) 'DocElec', T0.DocEntry, T0.ObjType
                                    FROM {0} T0 
                                    JOIN NNM1 T2 ON T0.Series = T2.Series 
                                   WHERE 1 = 1
                                     AND T0.U_BPP_MDSD = '{1}'
                                     AND T0.U_BPP_MDCD = '{2}'
                                     AND T0.U_BPP_MDTD = '{3}'
                                     AND T0.DocSubType = '{4}'
                                   ORDER BY T0.DocEntry DESC";
                        }//Confirmar si solo busca documentos abietos
                        else
                        {
                            s = @"SELECT SUBSTRING(UPPER(T2.""BeginStr""), 1, 1) ""DocElec"", T0.""DocEntry"", T0.""ObjType""
                                    FROM ""{0}"" T0 
                                    JOIN ""NNM1"" T2 ON T0.""Series"" = T2.""Series"" 
                                   WHERE 1 = 1
                                     AND T0.""U_BPP_MDSD"" = '{1}'
                                     AND T0.""U_BPP_MDCD"" = '{2}'
                                     AND T0.""U_BPP_MDTD"" = '{3}'
                                     AND T0.""DocSubType"" = '{4}'
                                   ORDER BY T0.""DocEntry"" DESC";
                        }
                        s = String.Format(s, Tabla, sSerie, sFolio, TipoDocElect, sDocSubType);
                    }
                    else
                    {
                        if (GlobalSettings.RunningUnderSQLServer)
                        {
                            s = @"SELECT SUBSTRING(UPPER(T2.BeginStr), 1, 1) 'DocElec', T0.DocEntry, T0.ObjType
                                    FROM {0} T0 
                                    JOIN NNM1 T2 ON T0.Series = T2.Series 
                                   WHERE 1 = 1
                                     AND T0.U_BPP_PTSC = '{1}'
                                     AND T0.U_BPP_PTCC = '{2}'
                                   ORDER BY T0.DocEntry DESC";
                        }//Confirmar si solo busca documentos abietos
                        else
                        {
                            s = @"SELECT SUBSTRING(UPPER(T2.""BeginStr""), 1, 1) ""DocElec"", T0.""DocEntry"", T0.""ObjType""
                                    FROM ""{0}"" T0 
                                    JOIN ""NNM1"" T2 ON T0.""Series"" = T2.""Series"" 
                                   WHERE 1 = 1
                                     AND T0.""U_BPP_PTSC"" = '{1}'
                                     AND T0.""U_BPP_PTCC"" = '{2}'
                                   ORDER BY T0.""DocEntry"" DESC";
                        }
                        s = String.Format(s, Tabla, sSerie, sFolio);
                    }

                    oRecordSet.DoQuery(s);
                    if (oRecordSet.RecordCount == 0)
                    {
                        _result = false;
                        FSBOApp.StatusBar.SetText("Numero de Serie y Folio no se ha encontrado", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                    }
                    else
                    {
                        sDocEntry = oRecordSet.Fields.Item("DocEntry").Value.ToString();
                        ObjType = oRecordSet.Fields.Item("ObjType").Value.ToString();
                        if (sTipo == "20")
                            _result = true;
                        else
                            _result = ValidarDatos_PE(TipoDocElect, Tabla, sDocSubType, sDocEntry, ObjType, TablaDir, TablaDetalle);
                    }
                }

                return _result;
            }
            catch (Exception e)
            {
                FSBOApp.StatusBar.SetText(e.Message + " ** Trace: " + e.StackTrace, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                OutLog("ValidarPE: " + e.Message + " ** Trace: " + e.StackTrace);
                return false;
            }
        }


        private Boolean ValidarDatos_PE(String TipoDocElec, String Tabla, String DocSubType, String DocEntry, String ObjType, String TablaDir, String TablaDetalle)
        {
            Boolean _result = true;
            String[] CaracteresInvalidos = { "Ñ", "°", "|", "!", @"""", "#", "$", "=", "?", "\\", "¿", "¡", "~", "´", "+", "{", "}", "[", "]", "-", ":", "%" };
            TFunctions Param;
            int i;
            int c;
            String VatStatus = "Y";
            String BPP_BPTP = "";
            //SAPbouiCOM.DBDataSource oDBDSDir;
            //SAPbouiCOM.DBDataSource oDBDSH;

            try
            {
                if (TipoDocElec == "20")
                    return true;

                var oDBDSH = oForm.DataSources.DBDataSources.Add(Tabla);
                var oDBDSDir = oForm.DataSources.DBDataSources.Add(TablaDir);
                var oDBDSDet = oForm.DataSources.DBDataSources.Add(TablaDetalle);

                SAPbouiCOM.Conditions oConditions;
                SAPbouiCOM.Condition oCondition;

                oConditions = new SAPbouiCOM.Conditions();
                oCondition = oConditions.Add();
                oCondition.Alias = "DocEntry";
                oCondition.Operation = BoConditionOperation.co_EQUAL;
                oCondition.CondVal = DocEntry;

                oDBDSH.Query(oConditions);
                oDBDSDet.Query(oConditions);
                oDBDSDir.Query(oConditions);


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


                if (ObjType == "14")
                {
                    //valida para nota credito
                    if (_result)
                    {
                        if (oDBDSH.GetValue("U_BPP_MDTN", 0) == "")
                        {
                            FSBOApp.StatusBar.SetText("Debe seleccionar Tipo de operacion", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                            _result = false;
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
                            else if (((System.String)(oDBDSH.GetValue("U_BPP_MDTN", 0)).Trim() == "11") || ((System.String)(oDBDSH.GetValue("U_BPP_MDTN", 0)).Trim() == "10") || ((System.String)(oDBDSH.GetValue("U_BPP_MDTN", 0)).Trim() == "04"))
                            {
                                FSBOApp.StatusBar.SetText("Debe seleccionar tipo de operacion valida por Factura Movil", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                return false;
                            }

                            if (_result)
                            {
                                c = 0;
                                var BaseEntry = (System.String)(oDBDSDet.GetValue("BaseEntry", 0));
                                var PedirRefCab = false;

                                i = 0;
                                while (i < oDBDSDet.Size)
                                {
                                    var BaseEntry2 = (System.String)(oDBDSDet.GetValue("BaseEntry", i));//BaseEntry
                                    var BaseType2 = (System.String)(oDBDSDet.GetValue("BaseType", i)); //basetype

                                    if (BaseEntry != BaseEntry2)
                                    { c = c + 1; }

                                    if ((BaseEntry2 == "") || (BaseType2 != "13"))
                                    { PedirRefCab = true; }
                                    i++;
                                }

                                if (PedirRefCab)
                                {
                                    if (oDBDSH.GetValue("U_BPP_MDTO", 0).Trim() == "")
                                    {
                                        FSBOApp.StatusBar.SetText("Debe seleccionar Tipo de operacion de documento origen", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                        _result = false;
                                    }
                                    else if (oDBDSH.GetValue("U_BPP_MDSO", 0).Trim() == "")
                                    {
                                        FSBOApp.StatusBar.SetText("Debe seleccionar Serie documento origen", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                        _result = false;
                                    }
                                    else if (oDBDSH.GetValue("U_BPP_MDCO", 0).Trim() == "")
                                    {
                                        FSBOApp.StatusBar.SetText("Debe seleccionar Correlativo documento origen", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                        _result = false;
                                    }
                                    else
                                    {
                                        var TablaAux = "OINV";
                                        if (GlobalSettings.RunningUnderSQLServer)
                                        {
                                            s = @"SELECT COUNT(*) 'Cont'
                                                        FROM {0} T1 WITH (NOLOCK)
                                                        JOIN NNM1 T2 WITH (NOLOCK) ON T1.Series = T2.Series
                                                       WHERE ISNULL(T1.U_BPP_MDTD, '') = '{1}'
                                                         AND ISNULL(T1.U_BPP_MDSD, '') = '{2}'
                                                         AND ISNULL(T1.U_BPP_MDCD, '') = '{3}'
                                                         AND CASE 
                                                               WHEN '{1}' = '01' THEN '--'
                            	                               WHEN '{1}' = '03' THEN 'IB'
                            	                               WHEN '{1}' = '08' THEN 'DN'
                            	                               Else '-1'
                                                             END = T1.DocSubType";
                                        }
                                        else
                                        {
                                            s = @"SELECT COUNT(*) ""Cont""
                                                    FROM ""{0}"" T1
                                                    JOIN ""NNM1"" T2 ON T1.""Series"" = T2.""Series""
                                                   WHERE IFNULL(T1.""U_BPP_MDTD"", '') = '{1}'
                                                         AND IFNULL(T1.""U_BPP_MDSD"", '') = '{2}'
                                                         AND IFNULL(T1.""U_BPP_MDCD"", '') = '{3}'
                                                         AND CASE
                                                               WHEN '{1}' = '01' THEN '--'
                            	                               WHEN '{1}' = '03' THEN 'IB'
                            	                               WHEN '{1}' = '08' THEN 'DN'
                            	                               Else '-1'
                                                             END = T1.""DocSubType"" ";
                                        }
                                        s = String.Format(s, TablaAux, (System.String)(oDBDSH.GetValue("U_BPP_MDTO", 0)).Trim(), (System.String)(oDBDSH.GetValue("U_BPP_MDSO", 0)).Trim(), (System.String)(oDBDSH.GetValue("U_BPP_MDCO", 0)).Trim());
                                        oRecordSet.DoQuery(s);
                                        if ((System.Int32)(oRecordSet.Fields.Item("Cont").Value) > 0)
                                            _result = true;
                                        else
                                        {
                                            FSBOApp.StatusBar.SetText("No se ha encontrado documento de referencia,", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                            _result = false;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else if ((ObjType == "13") && (TipoDocElec == "08"))//Nota de debito
                {
                    //valida para nota debito
                    if ((_result) && ((System.String)(oDBDSH.GetValue("DocSubType", 0)).Trim() == "DN"))
                    {
                        if (oDBDSH.GetValue("U_BPP_MDTO", 0).Trim() == "")
                        {
                            FSBOApp.StatusBar.SetText("Debe seleccionar Tipo de operacion de documento origen", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                            _result = false;
                        }
                        else if (oDBDSH.GetValue("U_BPP_MDSO", 0).Trim() == "")
                        {
                            FSBOApp.StatusBar.SetText("Debe seleccionar Serie documento origen", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                            _result = false;
                        }
                        else if (oDBDSH.GetValue("U_BPP_MDCO", 0).Trim() == "")
                        {
                            FSBOApp.StatusBar.SetText("Debe seleccionar Correlativo documento origen", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                            _result = false;
                        }
                        else if ((System.String)(oDBDSH.GetValue("U_BPP_MDTN", 0)).Trim() == "")
                        {
                            FSBOApp.StatusBar.SetText("Debe ingresar tipo de operacion", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                            _result = false;
                        }
                        else if ((System.String)(oDBDSH.GetValue("U_BPP_MDTN", 0)).Trim() != "11")
                        {
                            FSBOApp.StatusBar.SetText("Debe seleccionar tipo de operacion valida por FM", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                            return false;
                        }
                        else if ((System.String)(oDBDSH.GetValue("U_BPP_MDTD", 0)).Trim() != "08")
                        {
                            FSBOApp.StatusBar.SetText("El documento es una Nota de Debito y debe tener Tipo documento 08", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                            return false;
                        }
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
                                _result = false;
                            }


                            Tabla = "OINV";

                            if (GlobalSettings.RunningUnderSQLServer)
                            {
                                s = @"SELECT COUNT(*) 'Cont'
                                            FROM {0} T1 WITH (NOLOCK)
                                            JOIN NNM1 T2 WITH (NOLOCK) ON T1.Series = T2.Series
                                           WHERE ISNULL(T1.U_BPP_MDTD, '') = '{1}'
                                                         AND ISNULL(T1.U_BPP_MDSD, '') = '{2}'
                                                         AND ISNULL(T1.U_BPP_MDCD, '') = '{3}'
                                                         AND CASE 
                                                               WHEN '{1}' = '01' THEN '--'
                            	                               WHEN '{1}' = '03' THEN 'IB'
                            	                               WHEN '{1}' = '08' THEN 'DN'
                            	                               Else '-1'
                                                             END = T1.DocSubType";
                            }
                            else
                            {
                                s = @"SELECT COUNT(*) ""Cont""
                                            FROM ""{0}"" T1
                                            JOIN ""NNM1"" T2 ON T1.""Series"" = T2.""Series""
                                           WHERE IFNULL(T1.""U_BPP_MDTD"", '') = '{1}'
                                                         AND IFNULL(T1.""U_BPP_MDSD"", '') = '{2}'
                                                         AND IFNULL(T1.""U_BPP_MDCD"", '') = '{3}'
                                                         AND CASE
                                                               WHEN '{1}' = '01' THEN '--'
                            	                               WHEN '{1}' = '03' THEN 'IB'
                            	                               WHEN '{1}' = '08' THEN 'DN'
                            	                               Else '-1'
                                                             END = T1.""DocSubType"" ";
                            }
                            s = String.Format(s, Tabla, ((System.String)oDBDSH.GetValue("U_BPP_MDTO", 0)).Trim(), ((System.String)oDBDSH.GetValue("U_BPP_MDSO", 0)).Trim(), ((System.String)oDBDSH.GetValue("U_BPP_MDCO", 0)).Trim());//, DocSubType);
                            oRecordSet.DoQuery(s);

                            if ((System.Int32)(oRecordSet.Fields.Item("Cont").Value) > 0)
                                _result = true;
                            else
                            {
                                FSBOApp.StatusBar.SetText("No se ha encontrado documento de referencia", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                _result = false;
                            }
                        }
                    }
                }
                else
                {
                    if ((TipoDocElec != "03") && (((VatStatus != "N") || (BPP_BPTP != "SND")) && ((TipoDocElec == "01") || (TipoDocElec == "08"))))
                    {
                        if ((System.String)(oDBDSDir.GetValue("CityB", 0)).Trim() == "")
                        {
                            FSBOApp.StatusBar.SetText("Debe ingresar ciudad en Destinatario de Factura", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                            _result = false;
                        }

                        if (((System.String)(oDBDSDir.GetValue("BlockB", 0)).Trim() == "") && (_result))
                        {
                            FSBOApp.StatusBar.SetText("Debe ingresar comuna en Destinatario de Factura", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                            _result = false;
                        }

                        if (((System.String)(oDBDSDir.GetValue("StreetB", 0)).Trim() == "") && (_result))
                        {
                            FSBOApp.StatusBar.SetText("Debe ingresar calle en Destinatario de Factura", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                            _result = false;
                        }
                    }
                }


                //Validar que ingresaron Nombre Cliente
                s = (System.String)(oDBDSH.GetValue("CardName", 0)).Trim();
                if ((s == "") && (_result))
                {
                    FSBOApp.StatusBar.SetText("Debe ingresar Nombre Cliente", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                    _result = false;
                }

                //validar caracteres invalidos en el nombre del cliente
                //se comenta segun reunion de viernes 20150320, se creo una funcion que limpia lo caracteres invalidos al momento de enviar al portal
                //if (_result)
                //{
                //    foreach (String cara in CaracteresInvalidos)
                //    {
                //        if (s.IndexOf(cara) > 0)
                //        {
                //            FSBOApp.StatusBar.SetText(@"Nombre Cliente tiene caracteres prohibidos (" + cara + ")", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                //            _result = false;
                //            break;
                //        }
                //    }
                //}

                //se deja comentado, por problemas en la validacion de un cliente, Jimmy colocara una validacion en el TN 20151204
                //valida rut
                //if ((_result) && ((TipoDocElec != "03") && (((VatStatus != "N") || (BPP_BPTP != "SND")) && ((TipoDocElec == "01") || (TipoDocElec == "08")))))
                //{
                //    Param = new TFunctions();
                //    Param.SBO_f = FSBOf;
                //    s = Param.ValidarRuc((System.String)(oDBDSH.GetValue("LicTradNum", 0)));

                //    if (s != "OK")
                //    {
                //        FSBOApp.StatusBar.SetText(s, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                //        _result = false;
                //    }
                //}

                //valida descuentos negativos en el detalle del documento, caracteres especiales y descripcion de articulo
                if (_result)
                {
                    i = 0;
                    while (i < oDBDSDet.Size)
                    {
                        if (_result)
                        {
                            s = (System.String)(oDBDSDet.GetValue("Dscription", i));
                            if (s == "")
                            {
                                FSBOApp.StatusBar.SetText("Debe ingresar descripción en la linea " + Convert.ToString(i), BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                _result = false;
                                i = oDBDSDet.Size;
                            }
                        }
                        i++;
                    }
                }

                return _result;
            }
            catch (Exception e)
            {
                FSBOApp.StatusBar.SetText(e.Message + " ** Trace: " + e.StackTrace, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                OutLog("ValidarDatos_PE: " + e.Message + " ** Trace: " + e.StackTrace);
                return false;
            }
        }

        private Boolean ImprimirPE()
        {
            String sDocEntry = "";
            String sFolio = "";
            String sFolioPref = "";
            String Tabla = "";
            String sTipo = "";
            String sDocSubType = "";
            String sObjType = "";
            String GLOB_EncryptSQL;
            String TipoDocElect = "";
            SAPbouiCOM.EditText oEditText;
            SAPbouiCOM.ComboBox oComboBox;
            //String[] FE52 = { "52", "52T", "52D" };
            //String[] FEOt = { "01", "01A", "08", "03" };

            try
            {
                oForm.Freeze(true);
                GLOB_EncryptSQL = GlobalSettings.GLOB_EncryptSQL;
                oEditText = (EditText)(oForm.Items.Item("Folio").Specific);
                sFolio = oEditText.Value;
                oEditText = (EditText)(oForm.Items.Item("FolioPref").Specific);
                sFolioPref = oEditText.Value;
                oComboBox = (ComboBox)(oForm.Items.Item("TipDoc").Specific);
                sTipo = oComboBox.Value;
                sDocSubType = "";
                if (sTipo == "01") //Factura venta
                {
                    Tabla = "OINV";
                    sDocSubType = "'--'";
                    TipoDocElect = "01";
                }
                else if (sTipo == "01A") //Factura anticipo
                {
                    Tabla = "ODPI";
                    sDocSubType = "'--'";
                    TipoDocElect = "01";
                }
                else if (sTipo == "01X") //Factura Exportacion
                {
                    Tabla = "OINV";
                    sDocSubType = "'IX'";
                    TipoDocElect = "01";
                }
                else if (sTipo == "08") //Nota de Debito
                {
                    Tabla = "OINV";
                    sDocSubType = "'DN'";
                    TipoDocElect = "08";
                }
                else if (sTipo == "03") //Boleta
                {
                    Tabla = "OINV";
                    sDocSubType = "'IB','--'";
                    TipoDocElect = "03";
                }
                else if (sTipo == "07") //nota de credito
                {
                    Tabla = "ORIN";
                    sDocSubType = "'--'";
                    TipoDocElect = "07";
                }
                else if (sTipo == "09") //guia remision Entrega
                {
                    Tabla = "ODLN";
                    sDocSubType = "'--'";
                    TipoDocElect = "09";
                    sObjType = "15";
                }
                else if (sTipo == "09T") //guia remision Transferencia
                {
                    Tabla = "OWTR";
                    sDocSubType = "'--'";
                    TipoDocElect = "09";
                    sObjType = "67";
                }
                else if (sTipo == "09D") //guia remision Devolucion
                {
                    Tabla = "ORPD";
                    sDocSubType = "'--'";
                    TipoDocElect = "09";
                    sObjType = "21";
                }
                else if (sTipo == "20") //Comprobante Retencion
                {
                    Tabla = "OVPM";
                    sDocSubType = "'--'";
                    TipoDocElect = "20";
                }


                if (TipoDocElect == "20")
                {
                    if (GlobalSettings.RunningUnderSQLServer)
                        s = @"SELECT CAST(T0.DocEntry AS VARCHAR(20)) 'DocEntry', '--' 'DocSubType'
                            FROM {0} T0 
                            JOIN NNM1 T2 ON T0.Series = T2.Series 
                           WHERE (T0.U_BPP_PTCC = '{1}')
                             AND T0.U_BPP_PTSC = '{2}'
                             --AND SUBSTRING(UPPER(T2.BeginStr), 1, 1) = 'E'
                           ORDER BY T0.DocEntry DESC";
                    else
                        s = @"SELECT CAST(T0.""DocEntry"" AS VARCHAR(20)) ""DocEntry"", '--' ""DocSubType""
                            FROM ""{0}"" T0 
                            JOIN ""NNM1"" T2 ON T0.""Series"" = T2.""Series"" 
                           WHERE (T0.""U_BPP_PTCC"" = '{1}')
                             AND T0.""U_BPP_PTSC"" = '{2}'
                             --AND SUBSTRING(UPPER(T2.""BeginStr""), 1, 1) = 'E'
                           ORDER BY T0.""DocEntry"" DESC";
                    s = String.Format(s, Tabla, sFolio, sFolioPref);
                }
                else
                {
                    if (GlobalSettings.RunningUnderSQLServer)
                        s = @"SELECT CAST(T0.DocEntry AS VARCHAR(20)) 'DocEntry', T0.DocSubType
                            FROM {0} T0 
                            JOIN NNM1 T2 ON T0.Series = T2.Series 
                           WHERE (T0.U_BPP_MDCD = '{1}')
                             AND T0.U_BPP_MDSD = '{3}'
                             --AND SUBSTRING(UPPER(T2.BeginStr), 1, 1) = 'E'
                             AND T0.DocSubType IN ({2})
                           ORDER BY T0.DocEntry DESC";
                    else
                        s = @"SELECT CAST(T0.""DocEntry"" AS VARCHAR(20)) ""DocEntry"", T0.""DocSubType""
                            FROM ""{0}"" T0 
                            JOIN ""NNM1"" T2 ON T0.""Series"" = T2.""Series"" 
                           WHERE (T0.""U_BPP_MDCD"" = '{1}')
                             AND T0.""U_BPP_MDSD"" = '{3}'
                             --AND SUBSTRING(UPPER(T2.""BeginStr""), 1, 1) = 'E'
                             AND T0.""DocSubType"" IN ({2})
                           ORDER BY T0.""DocEntry"" DESC";
                    s = String.Format(s, Tabla, sFolio, sDocSubType, sFolioPref);
                }

                oRecordSet.DoQuery(s);

                if (oRecordSet.RecordCount > 0)
                {
                    sDocEntry = (System.String)(oRecordSet.Fields.Item("DocEntry").Value);
                    sDocSubType = ((System.String)oRecordSet.Fields.Item("DocSubType").Value).Trim();

                    //if (sTipo in ['33','34','39','41','56'])
                    if ((sTipo == "01") || (sTipo == "01X"))
                    {
                        var oInvoice_FM = new TInvoice();
                        oInvoice_FM.SBO_f = FSBOf;
                        oInvoice_FM.EnviarFE_PE_ED(sDocEntry, GlobalSettings.RunningUnderSQLServer, sFolioPref, sFolio, TipoDocElect, "13", sDocSubType, RUC, sTipo);
                    }
                    else if (sTipo == "01A")
                    {
                        var oInvoice_FM = new TInvoice();
                        oInvoice_FM.SBO_f = FSBOf;
                        oInvoice_FM.EnviarFE_PE_ED(sDocEntry, GlobalSettings.RunningUnderSQLServer, sFolioPref, sFolio, TipoDocElect, "203", sDocSubType, RUC, sTipo);
                    }
                    else if (sTipo == "03")
                    {
                        var oInvoice_FM = new TInvoice();
                        oInvoice_FM.SBO_f = FSBOf;
                        oInvoice_FM.EnviarFE_PE_ED(sDocEntry, GlobalSettings.RunningUnderSQLServer, sFolioPref, sFolio, TipoDocElect, "13", sDocSubType, RUC, sTipo);
                    }
                    if (sTipo == "08")
                    {
                        var oInvoice_FM = new TInvoice();
                        oInvoice_FM.SBO_f = FSBOf;
                        oInvoice_FM.EnviarFE_PE_ED(sDocEntry, GlobalSettings.RunningUnderSQLServer, sFolioPref, sFolio, TipoDocElect, "13", sDocSubType, RUC, sTipo);
                    }
                    else if (sTipo == "07")
                    {
                        var oCreditNotes_FM = new TCreditNotes();
                        oCreditNotes_FM.SBO_f = FSBOf;
                        oCreditNotes_FM.EnviarCN_PE_ED(sDocEntry, GlobalSettings.RunningUnderSQLServer, sFolioPref, sFolio, TipoDocElect, "14", sDocSubType, RUC, sTipo);
                    }
                    else if (sTipo == "20")
                    {
                        var oPagoEfectuado = new TPagoEfectuado();
                        oPagoEfectuado.SBO_f = FSBOf;
                        oPagoEfectuado.EnviarFE_PE_ED(sDocEntry, GlobalSettings.RunningUnderSQLServer, sFolioPref, sFolio, TipoDocElect, "46", RUC);
                    }
                    else if ((sTipo == "09") || (sTipo == "09T") || (sTipo == "09D"))
                    {
                        var oDelivery = new TDeliveryNote();
                        oDelivery.SBO_f = FSBOf;
                        oDelivery.EnviarFE_PE_ED(sDocEntry, GlobalSettings.RunningUnderSQLServer, sFolioPref, sFolio, TipoDocElect, sObjType, sDocSubType, RUC, sTipo);
                    }
                }
                else
                    FSBOApp.StatusBar.SetText("No se ha encontrado el documento " + sFolioPref + "-" + sFolio, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);

                oForm.DataSources.UserDataSources.Item("Folio").Value = "";
                oForm.Freeze(false);
                return true;
            }
            catch (Exception e)
            {
                FSBOApp.StatusBar.SetText(e.Message + " ** Trace: " + e.StackTrace, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                OutLog("ImprimirPE: " + e.Message + " ** Trace: " + e.StackTrace);
                return false;
            }
            oForm.Freeze(false);
        }//fin ImprimirPE


    }//fin Class
}
