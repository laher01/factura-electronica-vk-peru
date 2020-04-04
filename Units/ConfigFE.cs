using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Configuration;
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
using Factura_Electronica_VK.Functions;

namespace Factura_Electronica_VK.ConfigFE
{
    public class TConfigFE : TvkBaseForm, IvkFormInterface
    {
        private List<string> Lista;
        private SAPbouiCOM.DataTable dt;
        private SAPbobsCOM.Recordset oRecordSet;
        private SAPbouiCOM.Matrix oMtx;
        private SAPbouiCOM.Form oForm;
        private SAPbouiCOM.Item oItem;
        private SAPbouiCOM.DBDataSource oDBDSHeader;
        private SAPbouiCOM.Column oColumn;
        private CultureInfo _nf = new System.Globalization.CultureInfo("en-US");
        private String s;

        public new bool InitForm(string uid, string xmlPath, ref Application application, ref SAPbobsCOM.Company company, ref CSBOFunctions SBOFunctions, ref TGlobalVid _GlobalSettings)
        {
            //SAPbouiCOM.ComboBox oCombo;
            TFunctions Param;
            SAPbouiCOM.CheckBox oCheckBox;
            SAPbouiCOM.EditText oEditText;

            //
            //  obetener recurso
            //  try
            //  .....
            //  finally
            //  liberar recurso
            //  end

            bool Result = base.InitForm(uid, xmlPath, ref application, ref company, ref SBOFunctions, ref _GlobalSettings);

            oRecordSet = (SAPbobsCOM.Recordset)(FCmpny.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset));
            try
            {

                //Lista    := New list<string>;

                FSBOf.LoadForm(xmlPath, "strCnn.srf", uid);
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

                oDBDSHeader = (DBDataSource)(oForm.DataSources.DBDataSources.Item("@VID_FEPARAM"));

                if (!GlobalSettings.RunningUnderSQLServer)
                    oForm.Items.Item("btnProcFE").Visible = false;
                else
                    oForm.Items.Item("btnProcFE").Visible = true;

                //s := 'Select count(*) cant from [@VID_FEPARAM]';
                if (GlobalSettings.RunningUnderSQLServer)
                {
                    s = @"Select count(*) CANT
                                ,U_UserED
                                ,U_PwdED
                                ,U_UserWS
                                ,U_PassWS
                            from [@VID_FEPARAM] 
                            group by U_UserED
                                ,U_PwdED
                                ,U_UserWS
                                ,U_PassWS";
                }
                else
                {
                    s = @"Select count(*) ""CANT"" 
                           ,""U_UserED""
                           ,""U_PwdED""
                           ,""U_UserWS""
                           ,""U_PassWS""
                      from ""@VID_FEPARAM"" 
                     group by ""U_UserED""
                             ,""U_PwdED""
                             ,""U_UserWS""
                             ,""U_PassWS""";
                }
                oRecordSet.DoQuery(s);
                if ((System.Int32)(oRecordSet.Fields.Item("CANT").Value) > 0)
                {
                    Param = new TFunctions();
                    Param.SBO_f = FSBOf;

                    oForm.SupportedModes = 1;
                    oForm.Mode = BoFormMode.fm_UPDATE_MODE;
                    oDBDSHeader.Query(null);

                    s = Param.DesEncriptar((System.String)(oRecordSet.Fields.Item("U_UserED").Value).ToString().Trim());
                    oDBDSHeader.SetValue("U_UserED", 0, s);

                    s = Param.DesEncriptar((System.String)(oRecordSet.Fields.Item("U_PwdED").Value).ToString().Trim());
                    oDBDSHeader.SetValue("U_PwdED", 0, s);

                    s = Param.DesEncriptar((System.String)(oRecordSet.Fields.Item("U_UserWS").Value).ToString().Trim());
                    oDBDSHeader.SetValue("U_UserWS", 0, s);

                    s = Param.DesEncriptar((System.String)(oRecordSet.Fields.Item("U_PassWS").Value).ToString().Trim());
                    oDBDSHeader.SetValue("U_PassWS", 0, s);
                }
                else
                {
                    oForm.SupportedModes = 3;
                    oForm.Mode = BoFormMode.fm_ADD_MODE;
                    oForm.PaneLevel = 106;
                }

            }
            catch (Exception e)
            {
                OutLog("InitForm: " + e.Message + " ** Trace: " + e.StackTrace);
                FSBOApp.MessageBox(e.Message + " ** Trace: " + e.StackTrace, 1, "Ok", "", "");
            }
            finally
            {
                oForm.Freeze(false);
            }


            return Result;
        }//fin InitForm


        public new void FormEvent(String FormUID, ref SAPbouiCOM.ItemEvent pVal, ref Boolean BubbleEvent)
        {
            //SAPbouiCOM.DataTable oDataTable;
            //inherited FormEvent(FormUID,var pVal,var BubbleEvent);
            String Local;
            SAPbouiCOM.CheckBox oCheckBox;
            base.FormEvent(FormUID, ref pVal, ref BubbleEvent);


            try
            {

                if ((pVal.EventType == BoEventTypes.et_ITEM_PRESSED) && (pVal.BeforeAction == true))
                {
                    if ((pVal.ItemUID == "1") && ((oForm.Mode == BoFormMode.fm_ADD_MODE) || (oForm.Mode == BoFormMode.fm_UPDATE_MODE)))
                    {
                        s = "1";
                        oDBDSHeader.SetValue("Code", 0, s);
                        if (1 != FSBOApp.MessageBox("¿ Desea actualizar los parametros ?", 1, "Ok", "Cancelar", ""))
                        { BubbleEvent = false; }
                        else
                        {
                            BubbleEvent = false;

                            if (oForm.SupportedModes == 1)
                                s = "1";
                            else
                                s = "3";

                            if (AddDatos(s))
                            {
                                FSBOApp.StatusBar.SetText("Datos actualizados satisfactoriamente", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success);
                                oForm.Mode = BoFormMode.fm_OK_MODE;
                                //Remover menu y colocar los nuevos segun parametros


                                System.Xml.XmlDocument oXmlDoc = null;
                                oXmlDoc = new System.Xml.XmlDocument();
                                oXmlDoc.Load(System.IO.Path.GetDirectoryName(TMultiFunctions.ParamStr(0)) + "\\Menus\\RemoveMenuPE.xml");

                                string sXML = oXmlDoc.InnerXml.ToString();
                                FSBOApp.LoadBatchActions(ref sXML);

                                oXmlDoc.Load(System.IO.Path.GetDirectoryName(TMultiFunctions.ParamStr(0)) + "\\Menus\\Menu.xml");

                                sXML = oXmlDoc.InnerXml.ToString();
                                FSBOApp.LoadBatchActions(ref sXML);
                            }
                        }
                    }
                }

                if ((pVal.EventType == BoEventTypes.et_ITEM_PRESSED) && (!pVal.BeforeAction))
                {

                    if (pVal.ItemUID == "btnProcFE")
                        CargarProcedimientos();
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


        private Boolean AddDatos(String Tipo)
        {
            TFunctions Param; ;
            Boolean _return;
            String UserED;
            String PwdED;
            String UserWS;
            String PassWS;

            try
            {
                _return = false;
                if (ValidacionFinal())
                {
                    UserED = (System.String)(oDBDSHeader.GetValue("U_UserED", 0)).Trim();
                    PwdED = (System.String)(oDBDSHeader.GetValue("U_PwdED", 0)).Trim();
                    UserWS = (System.String)(oDBDSHeader.GetValue("U_UserWS", 0)).Trim();
                    PassWS = (System.String)(oDBDSHeader.GetValue("U_PassWS", 0)).Trim();

                    Param = new TFunctions();
                    Param.SBO_f = FSBOf;

                    if (UserED != "")
                    {
                        s = Param.Encriptar(UserED);
                        oDBDSHeader.SetValue("U_UserED", 0, s);
                    }
                    else
                        oDBDSHeader.SetValue("U_UserED", 0, "");

                    if (PwdED != "")
                    {
                        s = Param.Encriptar(PwdED);
                        oDBDSHeader.SetValue("U_PwdED", 0, s);
                    }
                    else
                        oDBDSHeader.SetValue("U_PwdED", 0, "");

                    if (UserWS != "")
                    {
                        s = Param.Encriptar(UserWS);
                        oDBDSHeader.SetValue("U_UserWS", 0, s);
                    }
                    else
                        oDBDSHeader.SetValue("U_UserWS", 0, "");

                    if (PassWS != "")
                    {
                        s = Param.Encriptar(PassWS);
                        oDBDSHeader.SetValue("U_PassWS", 0, s);
                    }
                    else
                        oDBDSHeader.SetValue("U_PassWS", 0, "");

                    if (Tipo == "1")
                        _return = Param.ParamUpd(oDBDSHeader);
                    else
                        _return = Param.ParamAdd(oDBDSHeader);

                    oDBDSHeader.SetValue("U_UserED", 0, UserED);
                    oDBDSHeader.SetValue("U_PwdED", 0, PwdED);
                    oDBDSHeader.SetValue("U_UserWS", 0, UserWS);
                    oDBDSHeader.SetValue("U_PassWS", 0, PassWS);

                    _return = true;
                }
                return _return;
            }
            catch (Exception e)
            {
                OutLog("AddDatos : " + e.Message + " ** Trace: " + e.StackTrace);
                FSBOApp.StatusBar.SetText("AddDatos : " + e.Message + " ** Trace: " + e.StackTrace, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                return false;
            }
        }//fin AddDatos


        private void CargarProcedimientos()
        {
            String SQLFile;
            System.IO.StreamReader sr;
            String ruta;
            System.String[] awords;
            String[] charArray;

            try
            {

                charArray = new String[] { "GO--" };
                if (GlobalSettings.RunningUnderSQLServer)
                {//cargar procedimiento SQL
                    //ruta = TMultiFunctions.ExtractFilePath(TMultiFunctions.ParamStr(0)) + "\\SQLs\\SQLServer\\";
                    ruta = Directory.GetCurrentDirectory() + "\\SQLs\\SQLServer\\";
                    DirectoryInfo oDirectorio = new DirectoryInfo(ruta);

                    //obtengo ls ficheros contenidos en la ruta
                    foreach (FileInfo file in oDirectorio.GetFiles())
                    {
                        try
                        {
                            if (file.Extension == ".sql")
                            {
                                SQLFile = file.FullName;
                                sr = new System.IO.StreamReader(SQLFile, System.Text.Encoding.GetEncoding("ISO8859-1"));
                                s = sr.ReadToEnd();
                                sr.Close();
                                if (s != "")
                                {
                                    awords = s.Split(charArray, StringSplitOptions.None);
                                    foreach (String aword in awords)
                                    {
                                        oRecordSet.DoQuery(aword);
                                    }
                                    FSBOApp.StatusBar.SetText("Cargado exitosamente, " + file.Name, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            FSBOApp.StatusBar.SetText(ex.Message + " ** Trace: " + ex.StackTrace, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                            OutLog("CargarProcedimientos: " + ex.Message + " ** Trace: " + ex.StackTrace);
                        }

                    }

                }
                else
                {//cargar procedimiento HANA
                    ruta = Directory.GetCurrentDirectory() + "\\SQLs\\HANA\\";
                    DirectoryInfo oDirectorio = new DirectoryInfo(ruta);

                    //obtengo ls ficheros contenidos en la ruta
                    foreach (FileInfo file in oDirectorio.GetFiles())
                    {
                        try
                        {
                            if (file.Extension == ".sql")
                            {
                                SQLFile = file.FullName;
                                sr = new System.IO.StreamReader(SQLFile, System.Text.Encoding.GetEncoding("ISO8859-1"));
                                s = sr.ReadToEnd();
                                sr.Close();
                                if (s != "")
                                {
                                    //OutLog(s);
                                    awords = s.Split(charArray, StringSplitOptions.None);
                                    foreach (String aword in awords)
                                    {
                                        try
                                        {
                                            //OutLog(aword.Replace("GO--", ""));
                                            oRecordSet.DoQuery(aword.Replace("GO--", ""));
                                            FSBOApp.StatusBar.SetText("Cargado exitosamente, " + file.Name, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success);
                                        }
                                        catch (Exception ej1)
                                        {
                                            //FSBOApp.StatusBar.SetText(ej1.Message + " ** Trace: " + ej1.StackTrace, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                                            OutLog("CargarProcedimientos :" + SQLFile + " - " + ej1.Message + " ** Trace: " + ej1.StackTrace);
                                        }
                                    }

                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            FSBOApp.StatusBar.SetText(ex.Message + " ** Trace: " + ex.StackTrace, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                            OutLog("CargarProcedimientos: " + ex.Message + " ** Trace: " + ex.StackTrace);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                FSBOApp.StatusBar.SetText(e.Message + " ** Trace: " + e.StackTrace, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                OutLog("CargarProcedimientos: " + e.Message + " ** Trace: " + e.StackTrace);
            }
        }

        private Boolean ValidacionFinal()
        {
            try
            {
                return true;
            }
            catch (Exception e)
            {
                OutLog("ValidacionFinal : " + e.Message + " ** Trace: " + e.StackTrace);
                FSBOApp.StatusBar.SetText("ValidacionFinal : " + e.Message + " ** Trace: " + e.StackTrace, BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Error);
                return false;
            }
        }
    }//fin class
}
