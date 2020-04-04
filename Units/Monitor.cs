using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
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
using System.Data;
using System.Diagnostics;
using ServiceStack.Text;
using System.Net.Http;
using System.Configuration;

namespace Factura_Electronica_VK.Monitor
{
    class TMonitor : TvkBaseForm, IvkFormInterface
    {
        private List<string> Lista;
        private SAPbobsCOM.Recordset oRecordSet;
        private SAPbouiCOM.Grid oGrid;
        private SAPbouiCOM.Form oForm;
        private SAPbouiCOM.Item oItem;
        private SAPbouiCOM.EditText oEditText;
        private SAPbouiCOM.CheckBox oCheckBox;
        private SAPbouiCOM.GridColumn oColumn;
        private SAPbouiCOM.DataTable oDataTable;
        private SAPbouiCOM.DBDataSource oDBDSHeader;
        private SAPbouiCOM.DBDataSource oDBDSD;
        private CultureInfo _nf = new System.Globalization.CultureInfo("en-US");
        private String s;
        private String RUC;

        public new bool InitForm(string uid, string xmlPath, ref Application application, ref SAPbobsCOM.Company company, ref CSBOFunctions SBOFunctions, ref TGlobalVid _GlobalSettings)
        {
            bool Result = base.InitForm(uid, xmlPath, ref application, ref company, ref SBOFunctions, ref _GlobalSettings);
            try
            {
                oRecordSet = (SAPbobsCOM.Recordset)(FCmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset));

                //Lista    := New list<string>;

                FSBOf.LoadForm(xmlPath, "VID_Monitor.srf", uid);
                //EnableCrystal := true;
                oForm = FSBOApp.Forms.Item(uid);
                oForm.Freeze(true);
                oForm.AutoManaged = false;
                oForm.SupportedModes = -1;             // afm_All
                oForm.EnableMenu("1282", false); //Crear
                oForm.EnableMenu("1281", false); //Actualizar

                // Ok Ad  Fnd Vw Rq Sec
                //Lista.Add('DocNum    , f,  f,  t,  f, n, 1');
                //Lista.Add('DocDate   , f,  t,  f,  f, r, 1');
                //Lista.Add('CardCode  , f,  t,  t,  f, r, 1');
                //FSBOf.SetAutoManaged(var oForm, Lista);

                oDBDSHeader = oForm.DataSources.DBDataSources.Add("@VID_FELOG");
                oDBDSD = oForm.DataSources.DBDataSources.Add("@VID_FELOGD");

                oForm.DataSources.UserDataSources.Add("FechaD", BoDataType.dt_DATE, 10);
                oEditText = (EditText)(oForm.Items.Item("FechaD").Specific);
                oEditText.DataBind.SetBound(true, "", "FechaD");
                oEditText.Value = DateTime.Now.ToString("yyyyMMdd");

                oForm.DataSources.UserDataSources.Add("FechaH", BoDataType.dt_DATE, 10);
                oEditText = (EditText)(oForm.Items.Item("FechaH").Specific);
                oEditText.DataBind.SetBound(true, "", "FechaH");
                oEditText.Value = DateTime.Now.ToString("yyyyMMdd");

                oForm.DataSources.UserDataSources.Add("chk_Todo", BoDataType.dt_SHORT_TEXT, 1);
                oCheckBox = (CheckBox)(oForm.Items.Item("chk_Todo").Specific);
                oCheckBox.DataBind.SetBound(true, "", "chk_Todo");
                oCheckBox.ValOn = "Y";
                oCheckBox.ValOff = "N";
                oCheckBox.Checked = false;

                oForm.DataSources.UserDataSources.Add("Rechazados", BoDataType.dt_SHORT_TEXT, 1);
                oCheckBox = (CheckBox)(oForm.Items.Item("Rechazados").Specific);
                oCheckBox.DataBind.SetBound(true, "", "Rechazados");
                oCheckBox.ValOn = "Y";
                oCheckBox.ValOff = "N";
                oCheckBox.Checked = true;

                oForm.DataSources.UserDataSources.Add("Pendientes", BoDataType.dt_SHORT_TEXT, 1);
                oCheckBox = (CheckBox)(oForm.Items.Item("Pendientes").Specific);
                oCheckBox.DataBind.SetBound(true, "", "Pendientes");
                oCheckBox.ValOn = "Y";
                oCheckBox.ValOff = "N";
                oCheckBox.Checked = true;

                oForm.DataSources.UserDataSources.Add("Aceptados", BoDataType.dt_SHORT_TEXT, 1);
                oCheckBox = (CheckBox)(oForm.Items.Item("Aceptados").Specific);
                oCheckBox.DataBind.SetBound(true, "", "Aceptados");
                oCheckBox.ValOn = "Y";
                oCheckBox.ValOff = "N";
                oCheckBox.Checked = false;

                oForm.DataSources.UserDataSources.Add("DadoBaja", BoDataType.dt_SHORT_TEXT, 1);
                oCheckBox = (CheckBox)(oForm.Items.Item("DadoBaja").Specific);
                oCheckBox.DataBind.SetBound(true, "", "DadoBaja");
                oCheckBox.ValOn = "Y";
                oCheckBox.ValOff = "N";
                oCheckBox.Checked = false;

                oForm.DataSources.UserDataSources.Add("Errores", BoDataType.dt_SHORT_TEXT, 1);
                oCheckBox = (CheckBox)(oForm.Items.Item("Errores").Specific);
                oCheckBox.DataBind.SetBound(true, "", "Errores");
                oCheckBox.ValOn = "Y";
                oCheckBox.ValOff = "N";
                oCheckBox.Checked = true;

                oDataTable = oForm.DataSources.DataTables.Add("dt");
                oGrid = (Grid)(oForm.Items.Item("grid").Specific);
                oGrid.DataTable = oDataTable;

                if (GlobalSettings.RunningUnderSQLServer)
                    s = "select ISNULL(TaxIdNum,'') TaxIdNum from OADM ";
                else
                    s = @"select IFNULL(""TaxIdNum"",'') ""TaxIdNum"" from ""OADM"" ";

                oRecordSet.DoQuery(s);
                if (oRecordSet.RecordCount == 0)
                    throw new Exception("Debe ingresar RUC de Emisor, Gestión -> Inicialización Sistema -> Detalle Sociedad -> Datos de Contabilidad -> ID fiscal general 1");
                else
                    RUC = ((System.String)oRecordSet.Fields.Item("TaxIdNum").Value).Trim();

                CargarDatosPE();
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
            IvkFormInterface oFormVk;
            String oUid;
            String prmKey;
            SAPbouiCOM.EditTextColumn oEditColumn;

            base.FormEvent(FormUID, ref pVal, ref BubbleEvent);
            try
            {
                if ((pVal.EventType == BoEventTypes.et_MATRIX_LINK_PRESSED) && (pVal.BeforeAction) && (pVal.ItemUID == "grid"))
                {
                    s = (System.String)(oDataTable.GetValue("ObjType", pVal.Row));
                    oColumn = (GridColumn)(oGrid.Columns.Item("DocEntry"));
                    oEditColumn = (EditTextColumn)(oColumn);
                    oEditColumn.LinkedObjectType = s;
                }

                if ((pVal.EventType == BoEventTypes.et_ITEM_PRESSED) && (!pVal.BeforeAction))
                {
                    if (pVal.ItemUID == "ActGrilla")
                        CargarDatosPE();

                    if (pVal.ItemUID == "chk_Todo")
                        CargarDatosPE();

                    if (pVal.ItemUID == "Rechazados")
                        CargarDatosPE();

                    if (pVal.ItemUID == "Pendientes")
                        CargarDatosPE();

                    if (pVal.ItemUID == "Aceptados")
                        CargarDatosPE();

                    if (pVal.ItemUID == "DadoBaja")
                        CargarDatosPE();

                    if (pVal.ItemUID == "Errores")
                        CargarDatosPE();
                }

                if ((pVal.EventType == BoEventTypes.et_VALIDATE) && (pVal.BeforeAction) && (pVal.ItemUID == "FechaD"))
                {
                    oEditText = (EditText)(oForm.Items.Item("FechaD").Specific);
                    if ((System.String)(oEditText.Value) == "")
                    {
                        FSBOApp.StatusBar.SetText("Debe ingresar una fecha desde", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                        BubbleEvent = false;
                    }
                }

                if ((pVal.EventType == BoEventTypes.et_VALIDATE) && (pVal.BeforeAction) && (pVal.ItemUID == "FechaH"))
                {
                    oEditText = (EditText)(oForm.Items.Item("FechaH").Specific);
                    if ((System.String)(oEditText.Value) == "")
                    {
                        FSBOApp.StatusBar.SetText("Debe ingresar una fecha hasta", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                        BubbleEvent = false;
                    }
                }
            }
            catch (Exception e)
            {
                if (FCmpny.InTransaction) FCmpny.EndTransaction(BoWfTransOpt.wf_RollBack);

                FSBOApp.StatusBar.SetText(e.Message + " ** Trace: " + e.StackTrace, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                OutLog("FormEvent: " + e.Message + " ** Trace: " + e.StackTrace);
            }
        }//fin FormEvent


        private void CargarDatosPE()
        {
            SAPbouiCOM.EditTextColumn oEditColumn;
            SAPbouiCOM.CheckBox oChkRechazados;
            SAPbouiCOM.CheckBox oChkPendientes;
            SAPbouiCOM.CheckBox oChkAceptados;
            SAPbouiCOM.CheckBox oChkDadoBaja;
            SAPbouiCOM.CheckBox oChkErrores;
            String FechaD, FechaH, Status;
            try
            {
                oForm.Freeze(true);
                oChkRechazados = (CheckBox)(oForm.Items.Item("Rechazados").Specific);
                oChkPendientes = (CheckBox)(oForm.Items.Item("Pendientes").Specific);
                oChkAceptados = (CheckBox)(oForm.Items.Item("Aceptados").Specific);
                oChkDadoBaja = (CheckBox)(oForm.Items.Item("DadoBaja").Specific);
                oChkErrores = (CheckBox)(oForm.Items.Item("Errores").Specific);

                if ((oChkRechazados.Checked) || (oChkPendientes.Checked) || (oChkAceptados.Checked) || (oChkErrores.Checked))
                {
                    if (GlobalSettings.RunningUnderSQLServer)
                    {
                        Status = "and T0.U_Status in (";
                        Status = Status + (oChkRechazados.Checked ? "'RZ'," : "");
                        Status = Status + (oChkPendientes.Checked ? "'EC'," : "");
                        Status = Status + (oChkAceptados.Checked ? "'RR'," : "");
                        Status = Status + (oChkDadoBaja.Checked ? "'DB'," : "");
                        Status = Status + (oChkErrores.Checked ? "'EE'," : "");
                        Status = Status.Substring(0, Status.Length - 1);
                        Status = Status + ")";
                    }
                    else
                    {
                        Status = @"and T0.""U_Status"" in (";
                        Status = Status + (oChkRechazados.Checked ? "'RZ'," : "");
                        Status = Status + (oChkPendientes.Checked ? "'EC'," : "");
                        Status = Status + (oChkAceptados.Checked ? "'RR'," : "");
                        Status = Status + (oChkDadoBaja.Checked ? "'DB'," : "");
                        Status = Status + (oChkErrores.Checked ? "'EE'," : "");
                        Status = Status.Substring(0, Status.Length - 1);
                        Status = Status + ")";
                    }
                }
                else if ((!oChkRechazados.Checked) && (!oChkPendientes.Checked) && (!oChkAceptados.Checked) && (!oChkDadoBaja.Checked) && (!oChkErrores.Checked))
                {
                    //en caso de no encontrar ninguno marcado sale de la funcion
                    oForm.Freeze(false);
                    return;
                }
                else
                    Status = "";



                if (GlobalSettings.RunningUnderSQLServer)
                    s = @"SELECT 
                               LTRIM(STR(T0.U_DocEntry,18,0))	'DocEntry'
	                          ,CASE U_ObjType
		                        WHEN '13' THEN (SELECT DocNum FROM OINV WHERE DocEntry = T0.U_DocEntry)
		                        WHEN '14' THEN (SELECT DocNum FROM ORIN WHERE DocEntry = T0.U_DocEntry)
		                        WHEN '15' THEN (SELECT DocNum FROM ODLN WHERE DocEntry = T0.U_DocEntry)
		                        WHEN '67' THEN (SELECT DocNum FROM OWTR WHERE DocEntry = T0.U_DocEntry)
                                WHEN '21' THEN (SELECT DocNum FROM ORPD WHERE DocEntry = T0.U_DocEntry)
	                           END				'DocNum'
                              ,T0.U_TipoDoc		'TipoDoc'
                              ,ISNULL(T0.U_SeriePE,'')  'SeriePE'
                              ,LTRIM(STR(T0.U_FolioNum,18,0))	'Folio'
                              ,(SELECT C1.Descr FROM CUFD C0 JOIN UFD1 C1 ON C1.TableID=C0.TableID AND C1.FieldID=C0.FieldID WHERE C0.TableID = '@VID_FELOG' AND C0.AliasID='Status' AND C1.FldValue= T0.U_Status)	'Estado'
                              ,T0.U_Status
                              ,T0.U_Message		'Mensaje'
                              ,T0.U_ObjType     'ObjType'
                              ,T0.U_Path		'Path'
                              ,T0.U_ExtFolio	'ExtFolio'
                              ,T0.DocEntry		'Key'
                              ,ISNULL(T0.U_Id,'0') 'Id'
                              ,ISNULL(T0.U_Validation,'') 'Validation'
                          FROM [@vid_felog] T0 WITH (NOLOCK)
                          JOIN OUSR T2 ON T2.USER_CODE = T0.U_UserCode
                         WHERE {0}
                           {3}
                           AND ISNULL(T0.U_DocDate, T0.CreateDate) BETWEEN '{1}' AND '{2}'
                           
                         ORDER BY T0.DocEntry DESC";
                else
                    s = @"SELECT  
                               LTRIM(TO_ALPHANUM(T0.""U_DocEntry""))	""DocEntry""
	                          ,CASE ""U_ObjType""
		                        WHEN '13' THEN (SELECT ""DocNum"" FROM ""OINV"" WHERE ""DocEntry"" = T0.""U_DocEntry"")
		                        WHEN '14' THEN (SELECT ""DocNum"" FROM ""ORIN"" WHERE ""DocEntry"" = T0.""U_DocEntry"")
		                        WHEN '15' THEN (SELECT ""DocNum"" FROM ""ODLN"" WHERE ""DocEntry"" = T0.""U_DocEntry"")
		                        WHEN '67' THEN (SELECT ""DocNum"" FROM ""OWTR"" WHERE ""DocEntry"" = T0.""U_DocEntry"")
                                WHEN '21' THEN (SELECT ""DocNum"" FROM ""ORPD"" WHERE ""DocEntry"" = T0.""U_DocEntry"")
	                           END				""DocNum""
                              ,T0.""U_TipoDoc""		""TipoDoc""
                              ,IFNULL(T0.""U_SeriePE"",'')  ""SeriePE""
                              ,LTRIM(TO_ALPHANUM(T0.""U_FolioNum""))	""Folio""
                              ,(SELECT C1.""Descr"" FROM ""CUFD"" C0 JOIN ""UFD1"" C1 ON C1.""TableID""=C0.""TableID"" AND C1.""FieldID""=C0.""FieldID"" WHERE C0.""TableID"" = '@VID_FELOG' AND C0.""AliasID""='Status' AND C1.""FldValue""= T0.""U_Status"")	""Estado""
                              ,T0.""U_Status""
                              ,T0.""U_Message""		""Mensaje"" 
                              ,T0.""U_ObjType""     ""ObjType""
                              ,T0.""U_Path""		""Path""
                              ,T0.""U_ExtFolio""	""ExtFolio""
                              ,T0.""DocEntry""		""Key""
                              ,IFNULL(T0.""U_Id"",'0') ""Id""
                              ,IFNULL(T0.""U_Validation"",'') ""Validation""
                          FROM ""@VID_FELOG"" T0 
                          JOIN ""OUSR"" T2 on T2.""USER_CODE"" = T0.""U_UserCode""
                         WHERE {0}
                           {3}
                           AND IFNULL(T0.""U_DocDate"", T0.""CreateDate"") BETWEEN '{1}' AND '{2}'
                         ORDER BY T0.""DocEntry"" DESC ";

                oCheckBox = (CheckBox)(oForm.Items.Item("chk_Todo").Specific);
                oEditText = (EditText)(oForm.Items.Item("FechaD").Specific);
                FechaD = (System.String)(oEditText.Value).Trim();

                oEditText = (EditText)(oForm.Items.Item("FechaH").Specific);
                FechaH = (System.String)(oEditText.Value).Trim();

                if (GlobalSettings.RunningUnderSQLServer)
                    s = String.Format(s, !oCheckBox.Checked ? "T0.U_UserCode = '" + FSBOApp.Company.UserName + "'" : "1=1", FechaD, FechaH, Status);
                else
                    s = String.Format(s, !oCheckBox.Checked ? @"T0.""U_UserCode"" = '" + FSBOApp.Company.UserName + "'" : "1=1", FechaD, FechaH, Status);
                oDataTable.ExecuteQuery(s);

                oGrid.Columns.Item("TipoDoc").Type = BoGridColumnType.gct_EditText;
                oColumn = (GridColumn)(oGrid.Columns.Item("TipoDoc"));
                oEditColumn = (EditTextColumn)(oColumn);
                oEditColumn.Editable = false;
                oEditColumn.TitleObject.Caption = "Tipo Documento";

                oGrid.Columns.Item("DocEntry").Type = BoGridColumnType.gct_EditText;
                oColumn = (GridColumn)(oGrid.Columns.Item("DocEntry"));
                oEditColumn = (EditTextColumn)(oColumn);
                oEditColumn.Editable = false;
                oEditColumn.TitleObject.Caption = "Llave SAP";
                oEditColumn.LinkedObjectType = "13";
                oEditColumn.RightJustified = true;

                oGrid.Columns.Item("DocNum").Type = BoGridColumnType.gct_EditText;
                oColumn = (GridColumn)(oGrid.Columns.Item("DocNum"));
                oEditColumn = (EditTextColumn)(oColumn);
                oEditColumn.Editable = false;
                oEditColumn.TitleObject.Caption = "Número SAP";
                oEditColumn.RightJustified = true;

                oGrid.Columns.Item("SeriePE").Type = BoGridColumnType.gct_EditText;
                oColumn = (GridColumn)(oGrid.Columns.Item("SeriePE"));
                oEditColumn = (EditTextColumn)(oColumn);
                oEditColumn.Editable = false;
                oEditColumn.TitleObject.Caption = "Número Serie";
                oEditColumn.RightJustified = true;

                oGrid.Columns.Item("Folio").Type = BoGridColumnType.gct_EditText;
                oColumn = (GridColumn)(oGrid.Columns.Item("Folio"));
                oEditColumn = (EditTextColumn)(oColumn);
                oEditColumn.Editable = false;
                oEditColumn.TitleObject.Caption = "Número Correlativo";
                oEditColumn.RightJustified = true;

                oGrid.Columns.Item("Estado").Type = BoGridColumnType.gct_EditText;
                oColumn = (GridColumn)(oGrid.Columns.Item("Estado"));
                oEditColumn = (EditTextColumn)(oColumn);
                oEditColumn.Editable = false;
                oEditColumn.TitleObject.Caption = "Estado";

                oGrid.Columns.Item("U_Status").Type = BoGridColumnType.gct_EditText;
                oColumn = (GridColumn)(oGrid.Columns.Item("U_Status"));
                oEditColumn = (EditTextColumn)(oColumn);
                oEditColumn.Editable = false;
                oEditColumn.TitleObject.Caption = "U_Status";
                oEditColumn.Visible = false;

                oGrid.Columns.Item("Mensaje").Type = BoGridColumnType.gct_EditText;
                oColumn = (GridColumn)(oGrid.Columns.Item("Mensaje"));
                oEditColumn = (EditTextColumn)(oColumn);
                oEditColumn.Editable = false;
                oEditColumn.TitleObject.Caption = "Comentario";

                oGrid.Columns.Item("ObjType").Type = BoGridColumnType.gct_EditText;
                oColumn = (GridColumn)(oGrid.Columns.Item("ObjType"));
                oEditColumn = (EditTextColumn)(oColumn);
                oEditColumn.Editable = false;
                oEditColumn.TitleObject.Caption = "ObjType";
                oEditColumn.Visible = false;

                oGrid.Columns.Item("Path").Type = BoGridColumnType.gct_EditText;
                oColumn = (GridColumn)(oGrid.Columns.Item("Path"));
                oEditColumn = (EditTextColumn)(oColumn);
                oEditColumn.Editable = false;
                oEditColumn.TitleObject.Caption = "Documento";
                oEditColumn.Visible = false;

                oGrid.Columns.Item("ExtFolio").Type = BoGridColumnType.gct_EditText;
                oColumn = (GridColumn)(oGrid.Columns.Item("ExtFolio"));
                oEditColumn = (EditTextColumn)(oColumn);
                oEditColumn.Editable = false;
                oEditColumn.TitleObject.Caption = "IdDocumento";
                oEditColumn.Visible = true;

                oGrid.Columns.Item("Key").Type = BoGridColumnType.gct_EditText;
                oColumn = (GridColumn)(oGrid.Columns.Item("Key"));
                oEditColumn = (EditTextColumn)(oColumn);
                oEditColumn.Editable = false;
                oEditColumn.TitleObject.Caption = "Key";
                oEditColumn.Visible = false;

                oGrid.Columns.Item("Id").Type = BoGridColumnType.gct_EditText;
                oColumn = (GridColumn)(oGrid.Columns.Item("Id"));
                oEditColumn = (EditTextColumn)(oColumn);
                oEditColumn.Editable = false;
                oEditColumn.TitleObject.Caption = "Id";
                oEditColumn.Visible = false;

                oGrid.Columns.Item("Validation").Type = BoGridColumnType.gct_EditText;
                oColumn = (GridColumn)(oGrid.Columns.Item("Validation"));
                oEditColumn = (EditTextColumn)(oColumn);
                oEditColumn.Editable = false;
                oEditColumn.TitleObject.Caption = "Validation";
                oEditColumn.Visible = false;

                oGrid.AutoResizeColumns();
            }
            catch (Exception e)
            {
                OutLog("CargarDatosPE : " + e.Message + " ** Trace: " + e.StackTrace);
                FSBOApp.StatusBar.SetText("CargarDatosPE : " + e.Message + " ** Trace: " + e.StackTrace, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
            }
            oForm.Freeze(false);
        }//fin CargarDatosPE

    }//fin class
}
