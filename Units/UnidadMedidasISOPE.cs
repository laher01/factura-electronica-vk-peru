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

namespace Factura_Electronica_VK.UnidadMedidasISOPE
{
    class TUnidadMedidasISOPE : TvkBaseForm, IvkFormInterface
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
                FSBOf.LoadForm(xmlPath, "FM_UMISO.srf", uid);
                oForm = FSBOApp.Forms.Item(uid);
                oForm.AutoManaged = false;
                oForm.SupportedModes = -1;             // afm_All
                Flag = false;
                oForm.Freeze(true);

                if (GlobalSettings.RunningUnderSQLServer)
                {   s = @"select count(*) Cont from [@FM_UMISO]"; }
                else
                {   s = @"select count(*) ""Cont"" from ""@FM_UMISO"" "; }
                oRecordSet.DoQuery(s);
                if ((System.Int32)(oRecordSet.Fields.Item("Cont").Value) > 0)
                {   oForm.Mode = BoFormMode.fm_UPDATE_MODE; }
                else
                {   oForm.Mode = BoFormMode.fm_ADD_MODE; }


                oGrid = (Grid)(oForm.Items.Item("3").Specific);
                oDBDSHeader = oForm.DataSources.DBDataSources.Add("@FM_UMISO");

                oDataTable = oForm.DataSources.DataTables.Add("UMISO");
                if (GlobalSettings.RunningUnderSQLServer)
                {   s = @"select Code, Name, U_UMBASE, U_UMISO from [@FM_UMISO]
                          UNION ALL 
                          select CAST('' as varchar(20)), CAST('' as varchar(20)), CAST('' as varchar(20)), CAST('' as varchar(50))";
                }
                else
                {   s = @"select ""Code"", ""Name"", ""U_UMBASE"", ""U_UMISO"" from ""@FM_UMISO""
                          UNION ALL
                          select CAST('' as varchar(20)), CAST('' as varchar(20)), CAST('' as varchar(20)), CAST('' as varchar(50)) FROM DUMMY ";
                }
                oDataTable.ExecuteQuery(s);
                oGrid.DataTable = oDataTable;

                oGrid.Columns.Item("Code").Type = BoGridColumnType.gct_EditText;
                oColumn = (GridColumn)(oGrid.Columns.Item("Code"));
                var oEditCol = (EditTextColumn)(oColumn);
                oEditCol.Editable = true;
                oEditCol.TitleObject.Caption = "Codigo";

                oGrid.Columns.Item("Name").Type = BoGridColumnType.gct_EditText;
                oColumn = (GridColumn)(oGrid.Columns.Item("Name"));
                oEditCol = (EditTextColumn)(oColumn);
                oEditCol.Editable = true;
                oEditCol.Visible = false;
                oEditCol.TitleObject.Caption = "Descripción";


                oGrid.Columns.Item("U_UMBASE").Type = BoGridColumnType.gct_EditText;
                oColumn = (GridColumn)(oGrid.Columns.Item("U_UMBASE"));
                oEditCol = (EditTextColumn)(oColumn);
                oEditCol.Editable = true;
                oEditCol.TitleObject.Caption = "UM Base";

                oGrid.Columns.Item("U_UMISO").Type = BoGridColumnType.gct_EditText;
                oColumn = (GridColumn)(oGrid.Columns.Item("U_UMISO"));
                oEditCol = (EditTextColumn)(oColumn);
                oEditCol.Editable = true;
                oEditCol.TitleObject.Caption = "UM ISO";

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
                                oDataTable.SetValue("U_UMBASE", oDataTable.Rows.Count-1, "");
                                oDataTable.SetValue("U_UMISO", oDataTable.Rows.Count - 1, "");
                            }
                        }
                    }
                }

                if ((pVal.EventType == BoEventTypes.et_ITEM_PRESSED) && (pVal.BeforeAction))
                {
                    if ((pVal.ItemUID == "1") && ((oForm.Mode == BoFormMode.fm_ADD_MODE) || (oForm.Mode == BoFormMode.fm_UPDATE_MODE)))
                    {
                        if (LimpiarGrid()) BubbleEvent = CrearDatos();

                        if ((BubbleEvent) && (oForm.Mode != BoFormMode.fm_OK_MODE))
                            oForm.Mode = BoFormMode.fm_OK_MODE;
                    }
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
                    else if ((System.String)(oDataTable.GetValue("U_UMBASE", i)).ToString().Trim() == "") 
                    {
                        oDataTable.Rows.Remove(i);
                        i = i - 1;
                    }
                    else if ((System.String)(oDataTable.GetValue("U_UMISO", i)).ToString().Trim() == "")
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
                i = 0;
                oDBDSHeader.Clear();
                Functions = new TFunctions();
                Functions.SBO_f = FSBOf;
                if (GlobalSettings.RunningUnderSQLServer)
                {   s = "select Code, Name, U_UMBASE, U_UMISO from [@FM_UMISO]"; }
                else
                {   s = @"select ""Code"", ""Name"", ""U_UMBASE"", ""U_UMISO"" from ""@FM_UMISO"" "; }
                oRecordSet.DoQuery(s);
                if (oRecordSet.RecordCount > 0)
                {   Functions.PEUMISODel(ref oRecordSet); }

                while (i < oDataTable.Rows.Count)
                {
                    oDBDSHeader.InsertRecord(0);
                    oDBDSHeader.SetValue("Code", 0, (System.String)(oDataTable.GetValue("Code", i)).ToString().Trim());
                    oDBDSHeader.SetValue("Name", 0, (System.String)(oDataTable.GetValue("Name", i)).ToString().Trim());
                    oDBDSHeader.SetValue("U_UMBASE", 0, (System.String)(oDataTable.GetValue("U_UMBASE", i)).ToString().Trim());
                    oDBDSHeader.SetValue("U_UMISO", 0, (System.String)(oDataTable.GetValue("U_UMISO", i)).ToString().Trim());

                    _result = Functions.PEUMISOAdd(oDBDSHeader);

                    i++;
                }

                oDataTable.Rows.Add(1);
                oDataTable.SetValue("Code", oDataTable.Rows.Count -1, "");
                oDataTable.SetValue("Name", oDataTable.Rows.Count-1, "");
                oDataTable.SetValue("U_UMBASE", oDataTable.Rows.Count-1, "");
                oDataTable.SetValue("U_UMISO", oDataTable.Rows.Count-1, "");

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
