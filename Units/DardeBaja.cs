using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Configuration;
using System.Xml;
using System.Xml.Linq;
using VisualD.GlobalVid;
using VisualD.SBOFunctions;
using VisualD.vkBaseForm;
using VisualD.MultiFunctions;
using VisualD.vkFormInterface;
using VisualD.SBOObjectMg1;
using VisualD.Main;
using VisualD.MainObjBase;
using System.Threading;
using System.Data.SqlClient;
using SAPbouiCOM;
using SAPbobsCOM;
using System.IO;
using System.Data;
using VisualD.ADOSBOScriptExecute;
using Newtonsoft.Json;
using Factura_Electronica_VK.Functions;

namespace Factura_Electronica_VK.DardeBaja
{
    public class TDardeBaja : TvkBaseForm, IvkFormInterface
    {
        private List<string> Lista;
        private SAPbobsCOM.Recordset oRecordSet;
        private SAPbouiCOM.DBDataSource oDBDSHeader;
        private SAPbouiCOM.Form oForm;
        private TFunctions Funciones = new TFunctions();
        private CultureInfo _nf = new System.Globalization.CultureInfo("en-US");
        private String s;

        public new bool InitForm(string uid, string xmlPath, ref Application application, ref SAPbobsCOM.Company company, ref CSBOFunctions SBOFunctions, ref TGlobalVid _GlobalSettings)
        {
            Int32 CantRol;
            SAPbouiCOM.ComboBox oCombo;
            SAPbouiCOM.EditText oEditText;
            bool Result = base.InitForm(uid, xmlPath, ref application, ref company, ref SBOFunctions, ref _GlobalSettings);

            oRecordSet = (SAPbobsCOM.Recordset)(FCmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset));
            Funciones.SBO_f = FSBOf;
            try
            {
                Lista = new List<string>();
                FSBOf.LoadForm(xmlPath, "VID_DarBaja.srf", uid);
                //EnableCrystal := true;
                oForm = FSBOApp.Forms.Item(uid);
                oForm.Freeze(true);
                oForm.AutoManaged = true;
                oForm.SupportedModes = -1;             // afm_All

                oCombo = (ComboBox)(oForm.Items.Item("TipoDoc").Specific);
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
                oCombo.Select("01", BoSearchKey.psk_ByValue);
                oForm.Items.Item("TipoDoc").DisplayDesc = true;

                oEditText = (EditText)(oForm.Items.Item("FDesde").Specific);
                //oEditText.Value = DateTime.Now.ToString("yyyyMMdd");

                oEditText = (EditText)(oForm.Items.Item("FHasta").Specific);
                //oEditText.Value = DateTime.Now.ToString("yyyyMMdd");

                var oGrid = ((SAPbouiCOM.Grid)oForm.Items.Item("grid").Specific);
                oGrid.DataTable = oForm.DataSources.DataTables.Add("dt");


                // Ok Ad  Fnd Vw Rq Sec
                Lista.Add("TipoDoc  , f,  t,  t,  f, r, 1");
                Lista.Add("FDesde   , f,  t,  f,  f, r, 1");
                Lista.Add("FHasta   , f,  t,  f,  f, r, 1");
                Lista.Add("Razon    , t,  t,  f,  f, r, 1");
                Lista.Add("DocEntry , f,  f,  t,  f, r, 1");
                FSBOf.SetAutoManaged(ref oForm, Lista);
                oForm.Mode = BoFormMode.fm_ADD_MODE;

            }
            catch (Exception e)
            {
                OutLog("InitForm: " + e.Message + " ** Trace: " + e.StackTrace);
                FSBOApp.MessageBox(e.Message + " ** Trace: " + e.StackTrace, 1, "Ok", "", "");
            }
            finally
            {
                if (oForm != null)
                    oForm.Freeze(false);
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
                if (pVal.EventType == BoEventTypes.et_ITEM_PRESSED && pVal.BeforeAction)
                {
                    if (pVal.ItemUID == "1" && oForm.Mode == BoFormMode.fm_ADD_MODE)
                    {
                        BubbleEvent = false;
                        if (Validar(true))
                            GuardarRegistros();
                    }
                }

                if ((pVal.EventType == BoEventTypes.et_ITEM_PRESSED) && (!pVal.BeforeAction))
                {
                    if (pVal.ItemUID == "btnBuscar" && oForm.Mode == BoFormMode.fm_ADD_MODE)
                    {
                        if (Validar(false))
                            BuscarRegistros();
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

            try
            {

            }
            catch (Exception e)
            {
                FSBOApp.StatusBar.SetText("FormDataEvent: " + e.Message + " ** Trace: " + e.StackTrace, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                OutLog("FormDataEvent: " + e.Message + " ** Trace: " + e.StackTrace);
            }
        }//fin FormDataEvent


        public new void MenuEvent(ref MenuEvent pVal, ref Boolean BubbleEvent)
        {
            SAPbouiCOM.ComboBox oCombo;
            SAPbouiCOM.EditText oEditText;
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


                if ((pVal.MenuUID != "") && (pVal.BeforeAction == false))
                {
                    if (pVal.MenuUID == "1282")
                    {
                        oCombo = (ComboBox)(oForm.Items.Item("TipoDoc").Specific);
                        oCombo.Select("01", BoSearchKey.psk_ByValue);

                        oEditText = (EditText)(oForm.Items.Item("FDesde").Specific);
                        oEditText.Value = "";// DateTime.Now.ToString("yyyyMMdd");

                        oEditText = (EditText)(oForm.Items.Item("FHasta").Specific);
                        oEditText.Value = ""; // DateTime.Now.ToString("yyyyMMdd");

                        oEditText = (EditText)(oForm.Items.Item("Serie").Specific);
                        oEditText.Value = "";

                        oEditText = (EditText)(oForm.Items.Item("CDesde").Specific);
                        oEditText.Value = "";

                        oEditText = (EditText)(oForm.Items.Item("CHasta").Specific);
                        oEditText.Value = "";

                        oForm.Items.Item("btnBuscar").Enabled = true;
                    }

                    if ((pVal.MenuUID == "1281") || (pVal.MenuUID == "1284") || (pVal.MenuUID == "1285") || (pVal.MenuUID == "1286") || (pVal.MenuUID == "1288")
                        || (pVal.MenuUID == "1289") || (pVal.MenuUID == "1290") || (pVal.MenuUID == "1291"))
                        oForm.Items.Item("btnBuscar").Enabled = false;
                }
            }
            catch (Exception e)
            {
                FSBOApp.MessageBox(e.Message + " ** Trace: " + e.StackTrace, 1, "Ok", "", "");
                OutLog("MenuEvent: " + e.Message + " ** Trace: " + e.StackTrace);
            }
        }//fin MenuEvent

        private Boolean Validar(Boolean bGuardar)
        {
            try
            {
                if ((System.String)(oForm.DataSources.DBDataSources.Item("@VID_FEDARBAJA").GetValue("U_TipoDoc", 0)) == "")
                {
                    FSBOApp.StatusBar.SetText("Debe ingresar Tipo Documento electronico", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                    return false;
                }
                else if (((System.String)(oForm.DataSources.DBDataSources.Item("@VID_FEDARBAJA").GetValue("U_FDesde", 0)) == "")
                    && ((System.String)(oForm.DataSources.DBDataSources.Item("@VID_FEDARBAJA").GetValue("U_FHasta", 0)) == "")
                     && ((System.String)(oForm.DataSources.DBDataSources.Item("@VID_FEDARBAJA").GetValue("U_Serie", 0)) == "")
                    && ((System.String)(oForm.DataSources.DBDataSources.Item("@VID_FEDARBAJA").GetValue("U_CDesde", 0)) == "")
                    && ((System.String)(oForm.DataSources.DBDataSources.Item("@VID_FEDARBAJA").GetValue("U_CHasta", 0)) == "")
                    && (!bGuardar))
                {
                    FSBOApp.StatusBar.SetText("Debe ingresar un parametro para buscar (Fechas o Serie y Correlativo)", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                    return false;
                }

                if (bGuardar)
                {
                    if ((System.String)(oForm.DataSources.DBDataSources.Item("@VID_FEDARBAJA").GetValue("U_Razon", 0)) == "")
                    {
                        FSBOApp.StatusBar.SetText("Debe ingresar razón", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                        return false;
                    }
                }

                return true;
            }
            catch (Exception xx)
            {
                FSBOApp.StatusBar.SetText("Error Validar - " + xx.Message, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                OutLog("Error Validar - " + xx.Message + ", TRACE " + xx.StackTrace);
                return false;
            }
        }

        private void BuscarRegistros()
        {
            SAPbouiCOM.EditText oEditText;
            SAPbouiCOM.ComboBox oComboBox;
            String Tipo, sDocSubType, FDesde, FHasta, CDesde, CHasta, Serie;
            String Tabla = "";
            String TipoDocElect = "";
            String ObjType = "";
            SAPbouiCOM.Grid oGrid;
            try
            {
                oForm.Freeze(true);
                oEditText = (EditText)(oForm.Items.Item("FDesde").Specific);
                FDesde = oEditText.Value;
                oEditText = (EditText)(oForm.Items.Item("FHasta").Specific);
                FHasta = oEditText.Value;
                oEditText = (EditText)(oForm.Items.Item("Serie").Specific);
                Serie = oEditText.Value;
                oEditText = (EditText)(oForm.Items.Item("CDesde").Specific);
                CDesde = oEditText.Value;
                oEditText = (EditText)(oForm.Items.Item("CHasta").Specific);
                CHasta = oEditText.Value;
                oComboBox = (ComboBox)(oForm.Items.Item("TipoDoc").Specific);
                Tipo = oComboBox.Value;
                Tipo = Tipo.Trim();
                sDocSubType = "";
                if (Tipo == "01") //Factura venta
                {
                    Tabla = "OINV";
                    sDocSubType = "'--'";
                    TipoDocElect = "01";
                    ObjType = "13";
                }
                else if (Tipo == "01A") //Factura anticipo
                {
                    Tabla = "ODPI";
                    sDocSubType = "'--'";
                    TipoDocElect = "01";
                    ObjType = "203";
                }
                else if (Tipo == "01X") //Factura Exportacion
                {
                    Tabla = "OINV";
                    sDocSubType = "'IX'";
                    TipoDocElect = "01";
                    ObjType = "13";
                }
                else if (Tipo == "08") //Nota de Debito
                {
                    Tabla = "OINV";
                    sDocSubType = "'DN'";
                    TipoDocElect = "08";
                    ObjType = "13";
                }
                else if (Tipo == "03") //Boleta
                {
                    Tabla = "OINV";
                    sDocSubType = "'IB','--'";
                    TipoDocElect = "03";
                    ObjType = "13";
                }
                else if (Tipo == "07") //nota de credito
                {
                    Tabla = "ORIN";
                    sDocSubType = "'--'";
                    TipoDocElect = "07";
                    ObjType = "14";
                }
                else if (Tipo == "09") //guia remision Entrega
                {
                    Tabla = "ODLN";
                    sDocSubType = "'--'";
                    TipoDocElect = "09";
                    ObjType = "15";
                }
                else if (Tipo == "09T") //guia remision Transferencia
                {
                    Tabla = "OWTR";
                    sDocSubType = "'--'";
                    TipoDocElect = "09";
                    ObjType = "67";
                }
                else if (Tipo == "09D") //guia remision Devolucion
                {
                    Tabla = "ORPD";
                    sDocSubType = "'--'";
                    TipoDocElect = "09";
                    ObjType = "21";
                }
                else if (Tipo == "20") //Comprobante Retencion
                {
                    Tabla = "OVPM";
                    sDocSubType = "'--'";
                    TipoDocElect = "20";
                    ObjType = "46";
                }


                if (TipoDocElect == "20")
                {
                    if (GlobalSettings.RunningUnderSQLServer)
                    {
                        s = @"SELECT 'N' 'Sel'
                                  ,T0.DocEntry
                                  ,T0.DocNum
		                          ,T0.ObjType
	                              ,T0.DocDate
		                          ,'20'	'TipoDoc'
		                          ,T0.U_BPP_PTSC	'Serie'
		                          ,T0.U_BPP_PTCC	'Folio'
		                          ,T0.CardCode		'CardCode'
		                          ,T0.CardName		'CardName'
		                          ,ISNULL(T0.DocCurr,'PEN')	'Moneda'
		                          ,CASE WHEN T0.DocTotalFC > 0 THEN T0.DocTotalFC
			                            ELSE T0.DocTotal
		                           END				'DocTotal'
                                  ,CASE WHEN LEFT(T0.U_BPP_PTSC,1) = 'R' THEN T0.U_BPP_PTSC + '-' + T0.U_BPP_PTCC ELSE 'R' + T0.U_BPP_PTSC + '-' + T0.U_BPP_PTCC END 'ExtFolio'
	                          FROM {0} T0
	                          JOIN NNM1 N0 ON N0.Series = T0.Series
	                          , OADM A0
	                         WHERE UPPER(LEFT(N0.BeginStr,1)) = 'E'
                               AND ISNULL(T0.U_BPP_PTSC,'') <> '' 
		                       AND ISNULL(T0.U_BPP_PTCC,'') <> '' 
	                           AND T0.Canceled = 'N'
	                           {1}
	                           {2}
	                           {3}
	                           {4}
	                           {5}";
                        s = String.Format(s, Tabla,
                            (FDesde != "" ? @" AND T0.DocDate >= '" + FDesde + "' " : ""),
                            (FHasta != "" ? @" AND T0.DocDate <= '" + FHasta + "' " : ""),
                            (Serie != "" ? @" AND T0.U_BPP_MDSD = '" + Serie + "' " : ""),
                            (CDesde != "" ? @" AND T0.U_BPP_MDCD >= '" + CDesde + "' " : ""),
                            (CHasta != "" ? @" AND T0.U_BPP_MDCD <= '" + CHasta + "' " : "")
                            );
                    }
                    else
                    {
                        s = @"SELECT 'N' ""Sel""
                                  ,T0.""DocEntry""
                                  ,T0.""DocNum""
		                          ,T0.""ObjType""
	                              ,T0.""DocDate""
		                          ,'20'	""TipoDoc""
		                          ,T0.""U_BPP_PTSC""	""Serie""
		                          ,T0.""U_BPP_PTCC""	""Folio""
		                          ,T0.""CardCode""		""CardCode""
		                          ,T0.""CardName""		""CardName""
		                          ,IFNULL(T0.""DocCurr"",'PEN')	""Moneda""
		                          ,CASE WHEN T0.""DocTotalFC"" > 0 THEN T0.""DocTotalFC""
			                            ELSE T0.""DocTotal""
		                           END				""DocTotal""
                                  ,CASE WHEN LEFT(T0.""U_BPP_PTSC"",1) = 'R' THEN T0.""U_BPP_PTSC"" || '-' || T0.""U_BPP_PTCC"" ELSE 'R' || T0.""U_BPP_PTSC"" || '-' || T0.""U_BPP_PTCC"" END ""ExtFolio""
	                          FROM ""{0}"" T0
	                          JOIN ""NNM1"" N0 ON N0.""Series"" = T0.""Series""
	                          , ""OADM"" A0
	                         WHERE UPPER(LEFT(N0.""BeginStr"",1)) = 'E'
                               AND IFNULL(T0.""U_BPP_PTSC"",'') <> '' 
		                       AND IFNULL(T0.""U_BPP_PTCC"",'') <> '' 
	                           AND T0.""Canceled"" = 'N'
	                           {1}
	                           {2}
	                           {3}
	                           {4}
	                           {5}";
                        s = String.Format(s, Tabla,
                            (FDesde != "" ? @" AND T0.""DocDate"" >= '" + FDesde + "' " : ""),
                            (FHasta != "" ? @" AND T0.""DocDate"" <= '" + FHasta + "' " : ""),
                            (Serie != "" ? @" AND T0.""U_BPP_MDSD"" = '" + Serie + "' " : ""),
                            (CDesde != "" ? @" AND T0.""U_BPP_MDCD"" >= '" + CDesde + "' " : ""),
                            (CHasta != "" ? @" AND T0.""U_BPP_MDCD"" <= '" + CHasta + "' " : "")
                            );
                    }
                }
                else //TipoDocElect diferente a 20
                {
                    if (GlobalSettings.RunningUnderSQLServer)
                    {
                        s = @"SELECT 'N' 'Sel'
                                  ,T0.DocEntry
                                  ,T0.DocNum
		                          ,T0.ObjType
	                              ,T0.DocDate
		                          ,T0.U_BPP_MDTD	'TipoDoc'
		                          ,T0.U_BPP_MDSD	'Serie'
		                          ,T0.U_BPP_MDCD	'Folio'
		                          ,T0.CardCode		'CardCode'
		                          ,T0.CardName		'CardName'
		                          ,ISNULL(T0.DocCur,'PEN')	'Moneda'
		                          ,CASE WHEN T0.DocTotalFC > 0 THEN T0.DocTotalFC
			                            ELSE T0.DocTotal
		                           END				'DocTotal'
                                  ,CASE T0.U_BPP_MDTD
								      WHEN '01' THEN CASE WHEN LEFT(T0.U_BPP_MDSD,1) = 'F' THEN T0.U_BPP_MDSD + '-' + T0.U_BPP_MDCD ELSE 'F' + T0.U_BPP_MDSD + '-' + T0.U_BPP_MDCD END
									  WHEN '03' THEN CASE WHEN LEFT(T0.U_BPP_MDSD,1) = 'B' THEN T0.U_BPP_MDSD + '-' + T0.U_BPP_MDCD ELSE 'B' + T0.U_BPP_MDSD + '-' + T0.U_BPP_MDCD END
									  WHEN '08' THEN CASE WHEN LEFT(T0.U_BPP_MDSD,1) = 'F' THEN T0.U_BPP_MDSD + '-' + T0.U_BPP_MDCD ELSE 'F' + T0.U_BPP_MDSD + '-' + T0.U_BPP_MDCD END
									  WHEN '09' THEN CASE WHEN LEFT(T0.U_BPP_MDSD,1) = 'T' THEN T0.U_BPP_MDSD + '-' + T0.U_BPP_MDCD ELSE 'T' + T0.U_BPP_MDSD + '-' + T0.U_BPP_MDCD END
									  WHEN '07' THEN 
									        CASE WHEN (SELECT  MIN(documentType) FROM VID_VW_FE_PE_ORIN_R WHERE DocEntry = T0.DocEntry AND ObjType = T0.ObjType) = '03'
													THEN CASE WHEN LEFT(T0.U_BPP_MDSD,1) = 'B' THEN T0.U_BPP_MDSD + '-' + T0.U_BPP_MDCD ELSE 'B' + T0.U_BPP_MDSD + '-' + T0.U_BPP_MDCD END
												 ELSE CASE WHEN LEFT(T0.U_BPP_MDSD,1) = 'F' THEN T0.U_BPP_MDSD + '-' + T0.U_BPP_MDCD ELSE 'F' + T0.U_BPP_MDSD + '-' + T0.U_BPP_MDCD END
									        END
								   END 'ExtFolio'
	                          FROM {0} T0
	                          JOIN NNM1 N0 ON N0.Series = T0.Series
	                          , OADM A0
	                         WHERE UPPER(LEFT(N0.BeginStr,1)) = 'E'
                               AND ISNULL(T0.U_BPP_MDTD,'') <> '' 
		                       AND ISNULL(T0.U_BPP_MDSD,'') <> '' 
		                       AND ISNULL(T0.U_BPP_MDCD,'') <> '' 
                               AND T0.DocStatus = 'O'
	                           AND T0.Canceled = 'N'
	                           AND T0.U_BPP_MDTD = '{1}'
                               AND T0.DocSubType IN ({2})
	                           {3}
	                           {4}
	                           {5}
	                           {6}
	                           {7}";
                        s = String.Format(s, Tabla, Tipo, sDocSubType,
                            (FDesde != "" ? @" AND T0.DocDate >= '" + FDesde + "' " : ""),
                            (FHasta != "" ? @" AND T0.DocDate <= '" + FHasta + "' " : ""),
                            (Serie != "" ? @" AND T0.U_BPP_MDSD = '" + Serie + "' " : ""),
                            (CDesde != "" ? @" AND T0.U_BPP_MDCD >= '" + CDesde + "' " : ""),
                            (CHasta != "" ? @" AND T0.U_BPP_MDCD <= '" + CHasta + "' " : "")
                            );
                    }
                    else
                    {
                        s = @"SELECT 'N' ""Sel""
                                  ,T0.""DocEntry""
                                  ,T0.""DocNum""
		                          ,T0.""ObjType""
	                              ,T0.""DocDate""
		                          ,T0.""U_BPP_MDTD""	""TipoDoc""
		                          ,T0.""U_BPP_MDSD""	""Serie""
		                          ,T0.""U_BPP_MDCD""	""Folio""
		                          ,T0.""CardCode""		""CardCode""
		                          ,T0.""CardName""		""CardName""
		                          ,IFNULL(T0.""DocCur"",'PEN')	""Moneda""
		                          ,CASE WHEN T0.""DocTotalFC"" > 0 THEN T0.""DocTotalFC""
			                            ELSE T0.""DocTotal""
		                           END				""DocTotal""
                                  ,CASE T0.""U_BPP_MDTD""
								      WHEN '01' THEN CASE WHEN LEFT(T0.""U_BPP_MDSD"",1) = 'F' THEN T0.""U_BPP_MDSD"" || '-' || T0.""U_BPP_MDCD"" ELSE 'F' || T0.""U_BPP_MDSD"" || '-' || T0.""U_BPP_MDCD"" END
									  WHEN '03' THEN CASE WHEN LEFT(T0.""U_BPP_MDSD"",1) = 'B' THEN T0.""U_BPP_MDSD"" || '-' || T0.""U_BPP_MDCD"" ELSE 'B' || T0.""U_BPP_MDSD"" || '-' || T0.""U_BPP_MDCD"" END
									  WHEN '08' THEN CASE WHEN LEFT(T0.""U_BPP_MDSD"",1) = 'F' THEN T0.""U_BPP_MDSD"" || '-' || T0.""U_BPP_MDCD"" ELSE 'F' || T0.""U_BPP_MDSD"" || '-' || T0.""U_BPP_MDCD"" END
									  WHEN '09' THEN CASE WHEN LEFT(T0.""U_BPP_MDSD"",1) = 'T' THEN T0.""U_BPP_MDSD"" || '-' || T0.""U_BPP_MDCD"" ELSE 'T' || T0.""U_BPP_MDSD"" || '-' || T0.""U_BPP_MDCD"" END
									  WHEN '07' THEN 
									        CASE WHEN (SELECT  MIN(""documentType"") FROM VID_VW_FE_PE_ORIN_R WHERE ""DocEntry"" = T0.""DocEntry"" AND ""ObjType"" = T0.""ObjType"") = '03'
													THEN CASE WHEN LEFT(T0.""U_BPP_MDSD"",1) = 'B' THEN T0.""U_BPP_MDSD"" || '-' || T0.""U_BPP_MDCD"" ELSE 'B' || T0.""U_BPP_MDSD"" || '-' || T0.""U_BPP_MDCD"" END
												 ELSE CASE WHEN LEFT(T0.""U_BPP_MDSD"",1) = 'F' THEN T0.""U_BPP_MDSD"" || '-' || T0.""U_BPP_MDCD"" ELSE 'F' || T0.""U_BPP_MDSD"" || '-' || T0.""U_BPP_MDCD"" END
									        END
								   END ""ExtFolio""
	                          FROM ""{0}"" T0
	                          JOIN ""NNM1"" N0 ON N0.""Series"" = T0.""Series""
	                          , ""OADM"" A0
	                         WHERE UPPER(LEFT(N0.""BeginStr"",1)) = 'E'
                               AND IFNULL(T0.""U_BPP_MDTD"",'') <> '' 
		                       AND IFNULL(T0.""U_BPP_MDSD"",'') <> '' 
		                       AND IFNULL(T0.""U_BPP_MDCD"",'') <> '' 
                               AND T0.""DocStatus"" = 'O'
	                           AND T0.""Canceled"" = 'N'
	                           AND T0.""U_BPP_MDTD"" = '{1}'
                               AND T0.""DocSubType"" IN ({2})
	                           {3}
	                           {4}
	                           {5}
	                           {6}
	                           {7}";
                        s = String.Format(s, Tabla, Tipo, sDocSubType,
                            (FDesde != "" ? @" AND T0.""DocDate"" >= '" + FDesde + "' " : ""),
                            (FHasta != "" ? @" AND T0.""DocDate"" <= '" + FHasta + "' " : ""),
                            (Serie != "" ? @" AND T0.""U_BPP_MDSD"" = '" + Serie + "' " : ""),
                            (CDesde != "" ? @" AND T0.""U_BPP_MDCD"" >= '" + CDesde + "' " : ""),
                            (CHasta != "" ? @" AND T0.""U_BPP_MDCD"" <= '" + CHasta + "' " : "")
                            );
                    }
                }

                oGrid = ((SAPbouiCOM.Grid)oForm.Items.Item("grid").Specific);
                oGrid.DataTable.ExecuteQuery(s);


                if (oGrid.DataTable.Rows.Count > 0)
                {
                    oGrid.Columns.Item("Sel").Type = BoGridColumnType.gct_CheckBox;
                    oGrid.Columns.Item("Sel").Editable = true;
                    oGrid.Columns.Item("Sel").Visible = true;

                    oGrid.Columns.Item("DocEntry").Type = BoGridColumnType.gct_EditText;
                    var col = (EditTextColumn)(oGrid.Columns.Item("DocEntry"));
                    col.Editable = false;
                    col.Visible = true;
                    col.LinkedObjectType = ObjType; // Link to Employee
                    col.TitleObject.Caption = "Llave Doc";
                    col.TitleObject.Sortable = true;

                    oGrid.Columns.Item("DocNum").Type = BoGridColumnType.gct_EditText;
                    col = (EditTextColumn)(oGrid.Columns.Item("DocNum"));
                    col.Editable = false;
                    col.Visible = true;
                    col.TitleObject.Caption = "DocNum";
                    col.TitleObject.Sortable = true;

                    oGrid.Columns.Item("ObjType").Type = BoGridColumnType.gct_EditText;
                    col = (EditTextColumn)(oGrid.Columns.Item("ObjType"));
                    col.Editable = false;
                    col.Visible = false;

                    oGrid.Columns.Item("DocDate").Type = BoGridColumnType.gct_EditText;
                    col = (EditTextColumn)(oGrid.Columns.Item("DocDate"));
                    col.Editable = false;
                    col.Visible = true;
                    col.TitleObject.Caption = "Fecha Contable";
                    col.TitleObject.Sortable = true;

                    oGrid.Columns.Item("TipoDoc").Type = BoGridColumnType.gct_EditText;
                    col = (EditTextColumn)(oGrid.Columns.Item("TipoDoc"));
                    col.Editable = false;
                    col.Visible = true;
                    col.TitleObject.Caption = "Tipo Doc";

                    oGrid.Columns.Item("Serie").Type = BoGridColumnType.gct_EditText;
                    col = (EditTextColumn)(oGrid.Columns.Item("Serie"));
                    col.Editable = false;
                    col.Visible = true;
                    col.TitleObject.Caption = "Serie";
                    col.TitleObject.Sortable = true;

                    oGrid.Columns.Item("Folio").Type = BoGridColumnType.gct_EditText;
                    col = (EditTextColumn)(oGrid.Columns.Item("Folio"));
                    col.Editable = false;
                    col.Visible = true;
                    col.RightJustified = true;
                    col.TitleObject.Caption = "Correlativo";
                    col.TitleObject.Sortable = true;

                    oGrid.Columns.Item("CardCode").Type = BoGridColumnType.gct_EditText;
                    col = (EditTextColumn)(oGrid.Columns.Item("CardCode"));
                    col.Editable = false;
                    col.Visible = true;
                    col.LinkedObjectType = "2";
                    col.TitleObject.Caption = "Código SN";
                    col.TitleObject.Sortable = true;

                    oGrid.Columns.Item("CardName").Type = BoGridColumnType.gct_EditText;
                    col = (EditTextColumn)(oGrid.Columns.Item("CardName"));
                    col.Editable = false;
                    col.Visible = true;
                    col.TitleObject.Caption = "Nombre Cliente";
                    col.TitleObject.Sortable = true;

                    oGrid.Columns.Item("Moneda").Type = BoGridColumnType.gct_EditText;
                    col = (EditTextColumn)(oGrid.Columns.Item("Moneda"));
                    col.Editable = false;
                    col.Visible = true;
                    col.TitleObject.Caption = "Moneda";
                    col.TitleObject.Sortable = true;

                    oGrid.Columns.Item("DocTotal").Type = BoGridColumnType.gct_EditText;
                    col = (EditTextColumn)(oGrid.Columns.Item("DocTotal"));
                    col.Editable = false;
                    col.Visible = true;
                    col.RightJustified = true;
                    col.TitleObject.Caption = "Total Doc";

                    oGrid.Columns.Item("ExtFolio").Type = BoGridColumnType.gct_EditText;
                    col = (EditTextColumn)(oGrid.Columns.Item("ExtFolio"));
                    col.Editable = false;
                    col.Visible = false;
                    col.TitleObject.Caption = "Folio Externo";
                }
                else
                    FSBOApp.StatusBar.SetText("No se ha encontrado el documento " + Tipo, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
            }
            catch (Exception e)
            {
                FSBOApp.StatusBar.SetText("BuscarRegistros: " + e.Message + " ** Trace: " + e.StackTrace, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                OutLog("BuscarRegistros: " + e.Message + " ** Trace: " + e.StackTrace);
            }
            finally
            {
                oForm.Freeze(false);
            }
        }


        private void GuardarRegistros()
        {
            SAPbouiCOM.Grid oGrid;
            Boolean Paso = false;
            String ObjType;
            Int32 DocEntry;
            String Tipo;
            String Serie;
            String Folio;
            Int32 lRetCode;
            String errMsg;
            Int32 errCode;
            XDocument miXMLDoc;
            XmlDocument oXml;
            String UserWS = "";
            String PassWS = "";
            String TaxIdNum = "";
            String URL = "";
            String ExternalFolio = "";
            XmlNode oNode;
            SAPbobsCOM.Documents oDocs;
            SAPbobsCOM.StockTransfer oStock;
            SAPbobsCOM.Payments oPay;
            try
            {
                 if (GlobalSettings.RunningUnderSQLServer)
                     s = @"SELECT T0.U_URLDarBaja 'URL', ISNULL(T0.U_UserWS,'') 'UserWS', ISNULL(T0.U_PassWS,'') 'PassWS', ISNULL(T0.U_MostrarXML,'N') 'MostrarXML', ISNULL(A0.TaxIdNum,'') 'TaxIdNum' 
                           FROM [@VID_FEPARAM] T0 , OADM A0";
                else
                     s = @"SELECT T0.""U_URLDarBaja"" ""URL"", IFNULL(T0.""U_UserWS"",'') ""UserWS"", IFNULL(T0.""U_PassWS"",'') ""PassWS"", IFNULL(T0.""U_MostrarXML"",'N') ""MostrarXML"", IFNULL(A0.""TaxIdNum"",'') ""TaxIdNum"" 
                           FROM ""@VID_FEPARAM"" T0, ""OADM"" A0";

                oRecordSet.DoQuery(s);
                if (oRecordSet.RecordCount == 0)
                    FSBOApp.StatusBar.SetText("No se ha ingresado URL", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                else if (((System.String)oRecordSet.Fields.Item("URL").Value).Trim() == "")
                    FSBOApp.StatusBar.SetText("No se ha ingresado URL", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                //else if (((System.String)oRecordSet.Fields.Item("UserWS").Value).Trim() == "")
                //    throw new Exception("No se encuentra usuario en Parametros");
                //else if (((System.String)oRecordSet.Fields.Item("PassWS").Value).Trim() == "")
                //    throw new Exception("No se encuentra password en Parametros");
                else if (((System.String)oRecordSet.Fields.Item("TaxIdNum").Value).Trim() == "")
                    throw new Exception("No se encuentra RUC de Emisor, Gestión -> Inicialización Sistema -> Detalle Sociedad -> Datos de Contabilidad -> ID fiscal general 1");
                else
                {
                    UserWS = Funciones.DesEncriptar((System.String)(oRecordSet.Fields.Item("UserWS").Value).ToString().Trim());
                    PassWS = Funciones.DesEncriptar((System.String)(oRecordSet.Fields.Item("PassWS").Value).ToString().Trim());
                    TaxIdNum = ((System.String)oRecordSet.Fields.Item("TaxIdNum").Value).Trim();
                    URL = ((System.String)oRecordSet.Fields.Item("URL").Value).Trim();
                }

                oGrid = ((SAPbouiCOM.Grid)oForm.Items.Item("grid").Specific);
                FCmpny.StartTransaction();
                for (Int32 iLinea = 0; iLinea <= oGrid.DataTable.Rows.Count - 1; iLinea++)
                {
                    if (((System.String)oGrid.DataTable.GetValue("Sel", iLinea)).Trim() == "Y")
                    {
                        try
                        {
                            Paso = true;
                            ObjType = ((System.String)oGrid.DataTable.GetValue("ObjType", iLinea));
                            DocEntry = ((System.Int32)oGrid.DataTable.GetValue("DocEntry", iLinea));
                            Tipo = ((System.String)oGrid.DataTable.GetValue("TipoDoc", iLinea));
                            Serie = ((System.String)oGrid.DataTable.GetValue("Serie", iLinea));
                            Folio = ((System.String)oGrid.DataTable.GetValue("Folio", iLinea));
                            ExternalFolio = ((System.String)oGrid.DataTable.GetValue("ExtFolio", iLinea));

                            if (ObjType == "67")
                            {
                                oStock = ((SAPbobsCOM.StockTransfer)FCmpny.GetBusinessObject(BoObjectTypes.oStockTransfer));
                                if (oStock.GetByKey(DocEntry))
                                {
                                    lRetCode = oStock.Cancel();
                                }
                            }
                            else if (ObjType == "46")
                            {
                                oPay = ((SAPbobsCOM.Payments)FCmpny.GetBusinessObject(BoObjectTypes.oVendorPayments));
                                if (oPay.GetByKey(DocEntry))
                                {
                                    lRetCode = oPay.Cancel();
                                }
                            }
                            else
                            {
                                if (ObjType == "21")
                                    oDocs = ((SAPbobsCOM.Documents)FCmpny.GetBusinessObject(BoObjectTypes.oPurchaseReturns));
                                else if (ObjType == "15")
                                    oDocs = ((SAPbobsCOM.Documents)FCmpny.GetBusinessObject(BoObjectTypes.oDeliveryNotes));
                                else if (ObjType == "14")
                                    oDocs = ((SAPbobsCOM.Documents)FCmpny.GetBusinessObject(BoObjectTypes.oCreditNotes));
                                else if (ObjType == "203")
                                    oDocs = ((SAPbobsCOM.Documents)FCmpny.GetBusinessObject(BoObjectTypes.oDownPayments));
                                else
                                    oDocs = ((SAPbobsCOM.Documents)FCmpny.GetBusinessObject(BoObjectTypes.oInvoices));
                                if (oDocs.GetByKey(DocEntry))
                                {
                                    var oDocCancel = oDocs.CreateCancellationDocument();
                                    lRetCode = oDocCancel.Add();// Cancel();
                                    if (lRetCode != 0)
                                    {
                                        FCmpny.GetLastError(out errCode, out errMsg);
                                        throw new Exception("No se ha encontrado Documento " + Tipo + " " + Serie + "-" + Folio + ": " + errMsg);
                                    }
                                    else
                                    {
                                        //enviar baja al portal
                                        miXMLDoc = new XDocument(
                                                    new XDeclaration("1.0", "utf-8", "yes")
                                                        , new XElement("documentoelectronico",
                                                        new XElement("DocNum", TaxIdNum),
                                                        new XElement("DocType", Tipo),
                                                        new XElement("IdDocumento", ExternalFolio.Trim()
                                               ))
                                       );
                                        oXml = new XmlDocument();
                                        using (var xmlReader = miXMLDoc.CreateReader())
                                        {
                                            oXml.Load(xmlReader);
                                        }                                 

                                        s = Funciones.UpLoadDocumentByUrl(oXml, null, GlobalSettings.RunningUnderSQLServer, URL, UserWS, PassWS, "D-" + ExternalFolio);
                                      
                                        oXml.LoadXml(s);

                                        oNode = oXml.DocumentElement.SelectSingleNode("/Error/ErrorCode");

                                        string ticket = oNode.InnerText;
                                        
                                        oNode = oXml.DocumentElement.SelectSingleNode("/Error/ErrorText");
                                        
                                        string errorText = oNode.InnerText;
                                        
                                        oNode = oXml.DocumentElement.SelectSingleNode("/Error/IdDocument");
                                        
                                        string idDocument = oNode.InnerText;

                                        if (errorText != "OK")
                                        {
                                            FSBOApp.StatusBar.SetText("Mensaje: " + errorText + " " + Tipo + " " + Serie + "-" + Folio, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                                        }
                                        else
                                        {  
                                            FSBOApp.StatusBar.SetText("Se ha creado cancelacion del documento " + Tipo + " " + Serie + "-" + Folio, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success);

                                            //insertar en la tabla  y consultar por el estado mismo codigo que insert de factura
                                        }

                                        
                                    }
                                }
                                else
                                    FSBOApp.StatusBar.SetText("No se ha encontrado Documento " + Tipo + " " + Serie + "-" + Folio, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                            }

                            oDocs = null;
                            oPay = null;
                            oStock = null;
                        }
                        catch (Exception ss)
                        {
                            FSBOApp.StatusBar.SetText("Dar de Baja: " + ss.Message + " ** Trace: " + ss.StackTrace, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                            OutLog("Dar de Baja: " + ss.Message + " ** Trace: " + ss.StackTrace);
                            if (FCmpny.InTransaction)
                                FCmpny.EndTransaction(BoWfTransOpt.wf_RollBack);
                        }
                    }
                }

                if (!Paso)
                    FSBOApp.StatusBar.SetText("Debe seleccionar un documento minimo", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Warning);
                else
                {
                    if (FCmpny.InTransaction)
                        FCmpny.EndTransaction(BoWfTransOpt.wf_Commit);
                }
            }
            catch (Exception e)
            {
                FSBOApp.StatusBar.SetText("GuardarRegistros: " + e.Message + " ** Trace: " + e.StackTrace, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                OutLog("GuardarRegistros: " + e.Message + " ** Trace: " + e.StackTrace);
                if (FCmpny.InTransaction)
                    FCmpny.EndTransaction(BoWfTransOpt.wf_RollBack);
            }
        }

    }//fin class
}
