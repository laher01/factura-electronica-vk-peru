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

namespace Factura_Electronica_VK.ConfiguracionImpuestoPE
{
    class TConfiguracionImpuestoPE : TvkBaseForm, IvkFormInterface
    {
        private SAPbobsCOM.Recordset oRecordSet;
        private SAPbouiCOM.Form oForm;
        private String s;
        private Boolean Flag;
        private SAPbouiCOM.Matrix mtx;
        private SAPbouiCOM.Grid oGrid;
        private SAPbouiCOM.DataTable oDataTable;
        private SAPbouiCOM.GridColumn oColumn;
        private SAPbouiCOM.DBDataSource oDBDSHeader;

        public new bool InitForm(string uid, string xmlPath, ref Application application, ref SAPbobsCOM.Company company, ref CSBOFunctions SBOFunctions, ref TGlobalVid _GlobalSettings)
        {
            bool Result = base.InitForm(uid, xmlPath, ref application, ref company, ref SBOFunctions, ref _GlobalSettings);
            try
            {
                oRecordSet = (SAPbobsCOM.Recordset)(FCmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset));
                FSBOf.LoadForm(xmlPath, "FM_IVA.srf", uid);
                oForm = FSBOApp.Forms.Item(uid);
                oForm.AutoManaged = false;
                oForm.SupportedModes = -1;             // afm_All
                Flag = false;
                oForm.Freeze(true);

                if (GlobalSettings.RunningUnderSQLServer)
                    s = @"select count(*) Cont from [@FM_IVA]";
                else
                    s = @"select count(*) ""Cont"" from ""@FM_IVA"" ";
                oRecordSet.DoQuery(s);
                if ((System.Int32)(oRecordSet.Fields.Item("Cont").Value) > 0)
                    oForm.Mode = BoFormMode.fm_UPDATE_MODE;
                else
                    oForm.Mode = BoFormMode.fm_ADD_MODE;


                oGrid = (Grid)(oForm.Items.Item("3").Specific);
                oDBDSHeader = oForm.DataSources.DBDataSources.Add("@FM_IVA");

                oDataTable = oForm.DataSources.DataTables.Add("Tax");
                if (GlobalSettings.RunningUnderSQLServer)
                {   s = @"select Code, Name from [@FM_IVA]
                          UNION ALL 
                          select CAST('' as varchar(20)), CAST('' as varchar(20))"; }
                else
                {   s = @"SELECT ""Code"", ""Name"" from ""@FM_IVA""
                          UNION ALL
                          SELECT CAST('' AS VARCHAR(20)), CAST('' AS VARCHAR(20)) FROM DUMMY "; }
                oDataTable.ExecuteQuery(s);
                oGrid.DataTable = oDataTable;

                oGrid.Columns.Item("Code").Type = BoGridColumnType.gct_ComboBox;
                oColumn = (GridColumn)(oGrid.Columns.Item("Code"));
                var oComboCol = (ComboBoxColumn)(oColumn);
                oComboCol.Editable = true;
                oComboCol.TitleObject.Caption = "Impuesto SAP";

                if (GlobalSettings.RunningUnderSQLServer)
                {   s = @"select Code, Name from OSTA"; }
                else
                {   s = @"select ""Code"", ""Name"" from ""OSTA"" "; }
                oRecordSet.DoQuery(s);
                FSBOf.FillComboGrid((GridColumn)(oGrid.Columns.Item("Code")), ref oRecordSet, true);

                oGrid.Columns.Item("Name").Type = BoGridColumnType.gct_EditText;
                oColumn = (GridColumn)(oGrid.Columns.Item("Name"));
                var oEditCol = (EditTextColumn)(oColumn);
                oEditCol.Editable = true;
                oEditCol.TitleObject.Caption = "Código Impto. SUNAT";

                oGrid.AutoResizeColumns();
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
            Int32 nErr;
            String sErr;
            base.FormEvent(FormUID, ref pVal, ref BubbleEvent);

            try
            {
                if (pVal.ItemUID == "3")
                {
                    if ((pVal.ColUID == "Code") && (!pVal.BeforeAction) && (pVal.EventType == BoEventTypes.et_COMBO_SELECT))
                    {
                        if (pVal.Row == oDataTable.Rows.Count -1)
                        {
                            if ((System.String)(oDataTable.GetValue("Code", oDataTable.Rows.Count -1)) != "")
                            {
                                oDataTable.Rows.Add(1);
                                oDataTable.SetValue("Code", oDataTable.Rows.Count -1, "");
                                oDataTable.SetValue("Name", oDataTable.Rows.Count-1, "");
                            }
                        }
                    }
                }

                if ((pVal.EventType == BoEventTypes.et_ITEM_PRESSED) && (pVal.BeforeAction))
                {
                    var paso = false;
                    if ((pVal.ItemUID == "1") && ((oForm.Mode == BoFormMode.fm_ADD_MODE) || (oForm.Mode == BoFormMode.fm_UPDATE_MODE)))
                    {
                        BubbleEvent = false;
                        if (LimpiarGrid()) paso = CrearDatos();

                        if ((paso) && (oForm.Mode != BoFormMode.fm_OK_MODE))
                            oForm.Mode = BoFormMode.fm_OK_MODE;
                    }
                    else if ((pVal.ItemUID == "1") && (oForm.Mode == BoFormMode.fm_OK_MODE))
                        BubbleEvent = true;
                }
            }
            catch (Exception e)
            {
                FCmpny.GetLastError(out nErr, out sErr);
                FSBOApp.StatusBar.SetText("FormEvent: " + e.Message + " ** Trace: " + e.StackTrace, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                OutLog("FormEvent: " + e.Message + " ** Trace: " + e.StackTrace);
            }
        }//fin FormEvent



        private Boolean LimpiarGrid()
        {
            Boolean _result;
            Int32 i;

            try
            {
                _result = true;
                i = 0;
                while (i < oDataTable.Rows.Count)
                {
                    if ((System.String)(oDataTable.GetValue("Code", i)).ToString().Trim() == "")
                    {
                        oDataTable.Rows.Remove(i);
                        i = i - 1;
                    }
                    else if ((System.String)(oDataTable.GetValue("Name", i)).ToString().Trim() == "") 
                    {
                        oDataTable.Rows.Remove(i);
                        i = i - 1;
                    }
                    i++;
                }

                return _result;
            }
            catch (Exception e)
            {
                FSBOApp.StatusBar.SetText(e.Message, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                OutLog("LimpiarGrid " + e.Message + " ** Trace: " + e.StackTrace);
                return false;
            }
        }//fin LimpiarGrid



        private Boolean CrearDatos()
        {
            Boolean _result;
            Int32 i;
            TFunctions Functions;

            try
            {
                _result = true;
                oDBDSHeader.Clear();
                Functions = new TFunctions();
                Functions.SBO_f = FSBOf;
                if (GlobalSettings.RunningUnderSQLServer)
                    s = "SELECT Code, Name FROM [@FM_IVA]";
                else
                    s = @"SELECT ""Code"", ""Name"" FROM ""@FM_IVA"" ";
                oRecordSet.DoQuery(s);
                
                if (oRecordSet.RecordCount > 0)
                    Functions.PEImpDel(ref oRecordSet);

                i = 0;
                while (i < oDataTable.Rows.Count)
                {
                    oDBDSHeader.Clear();
                    oDBDSHeader.InsertRecord(0);
                    oDBDSHeader.SetValue("Code", 0, ((System.String)oDataTable.GetValue("Code", i)).Trim());
                    oDBDSHeader.SetValue("Name", 0, ((System.String)oDataTable.GetValue("Name", i)).Trim());
                    _result = Functions.PEImpAdd(oDBDSHeader); 
                    i++;
                }

                oDataTable.Rows.Add(1);
                oDataTable.SetValue("Code", oDataTable.Rows.Count -1, "");
                oDataTable.SetValue("Name", oDataTable.Rows.Count-1, "");

                return _result;
            }
            catch (Exception e)
            {
                FSBOApp.StatusBar.SetText(e.Message, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                OutLog("CrearDatos " + e.Message + " ** Trace: " + e.StackTrace);
                return false;
            }
        }//fin CrearDatos


    }//fin Class
}
