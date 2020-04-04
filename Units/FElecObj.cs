using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using SAPbouiCOM;
using SAPbobsCOM; 
using VisualD.MainObjBase;
using VisualD.MenuConfFr; 
//using VisualD.GlobalVid;
using VisualD.SBOFunctions;
using VisualD.vkBaseForm;
using VisualD.vkFormInterface;
using VisualD.MultiFunctions;
using System.Xml;
using Factura_Electronica_VK.ReImprimir;
using Factura_Electronica_VK.Invoice;
using Factura_Electronica_VK.CreditNotes;
using Factura_Electronica_VK.DeliveryNote;
using Factura_Electronica_VK.ConfigFE;
using Factura_Electronica_VK.Monitor;
using Factura_Electronica_VK.Functions;
using Factura_Electronica_VK.ProcedimientosFE;
using Factura_Electronica_VK.ConfiguracionImpuestoPE;
using Factura_Electronica_VK.TipoNotasPE;
using Factura_Electronica_VK.UnidadMedidasISOPE;
using Factura_Electronica_VK.MenuConfiguracionHANA;
using Factura_Electronica_VK.PagoEfectuado;
using Factura_Electronica_VK.DardeBaja;


namespace Factura_Electronica_VK.FElecObj
{
    public class TFacturaElec : TMainObjBase //class(TMainObjBase)
    {
        String s;
        public override void AddMenus()
        {
            base.AddMenus();
            System.Xml.XmlDocument oXMLDoc;
            //String sImagePath;
            try
            {
                //inherited addMenus;
                oXMLDoc = new System.Xml.XmlDocument();
                //try
                    //sImagePath := TMultiFunctions.ExtractFilePath(TMultiFunctions.ParamStr(0)) + '\Menus\Menu.xml';
                    //oXMLDoc.Load(sImagePath);
                    //StrAux := oXMLDoc.InnerXml;
                    //SBOApplication.LoadBatchActions(var StrAux);
                //except
                //on e: exception do
                    //SBOFunctions.oLog.OutLog('AddMenus err: ' + e.Message + ' ** Trace: ' + e.  StackTrace);
                //end;
            }
            finally
            {
                oXMLDoc = null;
            }
        } //fin AddMenus

        public override void MenuEventExt(List<object> oForms, ref MenuEvent pVal, ref Boolean BubbleEvent)
        {
            IvkFormInterface oForm;
            base.MenuEventExt(oForms, ref pVal, ref BubbleEvent);
            try
            {
                //Inherited MenuEventExt(oForms,var pVal,var BubbleEvent);
                oForm = null;
                if (! pVal.BeforeAction)
                {
                    switch (pVal.MenuUID)
                    {
                        case "VID_FERImpFE":    
                            {
                                oForm = (IvkFormInterface)(new TReImprimir());
                                //(TReImprimir)(oForm).ooForms = oForms;
                                break;
                            }
                        case "VID_FEConf":
                            {
                                oForm = (IvkFormInterface)(new TConfigFE());
                                //(TConfigFE)(oForm).ooForms = oForms;
                                break;
                            }
                        case "VID_FEMonitor": //Menu para Monitor
                            {
                                oForm = (IvkFormInterface)(new TMonitor());
                                //(TMonitor)(oForm).ooForms = oForms;
                                break;
                            }
                        case "VID_FEDardeBaja": //Menu para dar de baja
                            {
                                oForm = (IvkFormInterface)(new TDardeBaja());
                                break;
                            }
                        case "VID_FEPROCED": //Menu para Procedimiento FE
                            {
                                oForm = (IvkFormInterface)(new TProcedimientosFE());
                                //(TMultiplesBases)(oForm).ooForms = oForms;
                                break;
                            }
                        case "VID_FEPEIVA": //Menu para Configuracion de Impuestos PE
                            {
                                oForm = (IvkFormInterface)(new TConfiguracionImpuestoPE());
                                break;
                            }
                        case "VID_FEPENOTES": //Menu para Tipo de Notas
                            {
                                oForm = (IvkFormInterface)(new TTipoNotasPE());
                                break;
                            }
                        case "VID_FEPEUMISO": //Menu para Unidad de medida ISO
                            {
                                oForm = (IvkFormInterface)(new TUnidadMedidasISOPE());
                                break;
                            }
                        case "VID_RHSQL":
                            {
                                //oForm                       := IvkFormInterface(New TCredencialesBD);
                                //TCredencialesBD(oForm).ooForms :=oForms;
                                if (GlobalSettings.RunningUnderSQLServer)
                                    oForm = (IvkFormInterface)(new TMenuConfFr());
                                else
                                    oForm = (IvkFormInterface)(new TMenuConfiguracionHANA());
                                //oForm1 := SBOApplication.Forms.ActiveForm;
                                //EditText(oForm1.Items.Item("Pw").Specific).IsPassword := true;
                                break;
                            }
                    }  
            
                    if (oForm != null) 
                    {
                        SAPbouiCOM.Application App = SBOApplication;
                        SAPbobsCOM.Company Cmpny = SBOCompany;
                        VisualD.SBOFunctions.CSBOFunctions SboF = SBOFunctions;
                        VisualD.GlobalVid.TGlobalVid Glob = GlobalSettings;
                        
                        if (oForm.InitForm(SBOFunctions.generateFormId(GlobalSettings.SBOSpaceName, GlobalSettings), "forms\\",ref  App,ref  Cmpny,ref SboF, ref Glob)) 
                        {   oForms.Add(oForm); }
                        else 
                        {
                            SBOApplication.Forms.Item(oForm.getFormId()).Close();
                            oForm = null;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                SBOApplication.MessageBox(e.Message + " ** Trace: " + e.StackTrace, 1, "Ok","","");  // Captura errores no manejados
                oLog.OutLog("MenuEventExt: " + e.Message + " ** Trace: " + e.StackTrace);
            }
        } //MenuEventExt

        public override IvkFormInterface ItemEventExt(IvkFormInterface oIvkForm, List<object> oForms, String LstFrmUID, String FormUID, ref ItemEvent pVal, ref Boolean BubbleEvent)
        {
            SAPbouiCOM.Form oForm;
            SAPbouiCOM.Form oFormParent;
            IvkFormInterface result = null;
            result = base.ItemEventExt(oIvkForm, oForms, LstFrmUID, FormUID, ref pVal, ref BubbleEvent);
            try
            {
                //inherited ItemEventExt(oIvkForm,oForms,LstFrmUID, FormUID, var pVal, var BubbleEvent);   

                result = base.ItemEventExt(oIvkForm, oForms, LstFrmUID, FormUID, ref pVal, ref BubbleEvent);

                if (result != null)
                {
                    return result;
                }
                else
                {
                    if (oIvkForm != null)
                    {
                        return oIvkForm;
                    }
                }

                // CFL Extendido (Enmascara el CFL estandar)
                if ((pVal.BeforeAction) && (pVal.EventType == BoEventTypes.et_FORM_LOAD) && (!string.IsNullOrEmpty(LstFrmUID)))
                {
                    try
                    {
                        oForm = SBOApplication.Forms.Item(LstFrmUID);
                    }
                    catch
                    {
                        oForm = null;
                    }
                }


                if ((!pVal.BeforeAction) && (pVal.FormTypeEx == "0"))
                {
                    if ((oIvkForm == null) && (GlobalSettings.UsrFldsFormActive) && (GlobalSettings.UsrFldsFormUid != "") && (pVal.EventType == BoEventTypes.et_FORM_LOAD))
                    {
                        oForm = SBOApplication.Forms.Item(pVal.FormUID);
                        oFormParent = SBOApplication.Forms.Item(GlobalSettings.UsrFldsFormUid);
                        try
                        {
                            //SBO_App.StatusBar.SetText(oFormParent.Title,BoMessageTime.bmt_Short,BoStatusBarMessageType.smt_Warning);
                            SBOFunctions.FillListUserFieldForm(GlobalSettings.ListFormsUserField, oFormParent, oForm);
                        }
                        finally
                        {
                            GlobalSettings.UsrFldsFormUid = "";
                            GlobalSettings.UsrFldsFormActive = false;
                        }
                    }
                    else
                    {
                        if ((pVal.EventType == BoEventTypes.et_FORM_ACTIVATE) || (pVal.EventType == BoEventTypes.et_COMBO_SELECT) || (pVal.EventType == BoEventTypes.et_FORM_RESIZE))
                        {
                            oForm = SBOApplication.Forms.Item(pVal.FormUID);
                            SBOFunctions.DisableListUserFieldsForm(GlobalSettings.ListFormsUserField, oForm);
                        }
                    }

                }


                if ((!pVal.BeforeAction) && (pVal.EventType == BoEventTypes.et_FORM_LOAD) && (oIvkForm == null))
                {
                    switch (pVal.FormTypeEx)
                    {
                        case "133": //Factura
                            {
                                result = (IvkFormInterface)(new TInvoice());
                                //(TInvoice)(result).ooForms = oForms;
                                TInvoice.DocSubType = "--";
                                TInvoice.bFolderAdd = false;
                                TInvoice.ObjType = "13";
                                TInvoice.ReservaExp = false;
                                break;
                            }
                        case "65307": //Factura Exportacion
                            {
                                result = (IvkFormInterface)(new TInvoice());
                                //(TInvoice)(result).ooForms = oForms;
                                TInvoice.DocSubType = "IX";
                                TInvoice.bFolderAdd = true;
                                TInvoice.ObjType = "13";
                                TInvoice.ReservaExp = false;
                                break;
                            }
                        case "60090": //Factura + pago venta
                            {
                                result = (IvkFormInterface)(new TInvoice());
                                //(TInvoice)(result).ooForms = oForms;
                                TInvoice.DocSubType = "--";
                                TInvoice.bFolderAdd = false;
                                TInvoice.ReservaExp = false;
                                TInvoice.ObjType = "13";
                                break;
                            }
                        case "60091": //Factura Reserva
                            {
                                result = (IvkFormInterface)(new TInvoice());
                                //(TInvoice)(result).ooForms = oForms;
                                TInvoice.DocSubType = "--";
                                TInvoice.bFolderAdd = true;
                                TInvoice.ObjType = "13";
                                TInvoice.ReservaExp = true;
                                break;
                            }
                        case "65302": //Factura exenta 
                            {
                                result = (IvkFormInterface)(new TInvoice());
                                //(TInvoice)(result).ooForms = oForms;
                                TInvoice.DocSubType = "IE";
                                TInvoice.bFolderAdd = false;
                                TInvoice.ObjType = "13";
                                TInvoice.ReservaExp = false;
                                break;
                            }
                        case "65300": //Factura Anticipo
                            {
                                result = (IvkFormInterface)(new TInvoice());
                                //(TInvoice)(result).ooForms = oForms;
                                TInvoice.DocSubType = "--";
                                TInvoice.bFolderAdd = false;
                                TInvoice.ObjType = "203";
                                TInvoice.ReservaExp = false;
                                break;
                            }
                        case "65304": //Boleta 
                            {
                                result = (IvkFormInterface)(new TInvoice());
                                //(TInvoice)(result).ooForms = oForms;
                                TInvoice.DocSubType = "IB";
                                TInvoice.bFolderAdd = true;
                                TInvoice.ObjType = "13";
                                TInvoice.ReservaExp = false;
                                break;
                            }
                        case "65305": //Boleta Exenta
                            {
                                result = (IvkFormInterface)(new TInvoice());
                                //(TInvoice)(result).ooForms = oForms;
                                TInvoice.DocSubType = "EB";
                                TInvoice.bFolderAdd = true;
                                TInvoice.ObjType = "13";
                                TInvoice.ReservaExp = false;
                                break;
                            }
                        case "65303": //Nota de debito 
                            {
                                result = (IvkFormInterface)(new TInvoice());
                                //(TInvoice)(result).ooForms = oForms;
                                TInvoice.DocSubType = "DN";
                                TInvoice.bFolderAdd = true;
                                TInvoice.ObjType = "13";
                                TInvoice.ReservaExp = false;
                                break;
                            }
                        case "179": //Nota de Credito 
                            {
                                result = (IvkFormInterface)(new TCreditNotes());
                                //(TCreditNotes)(result).ooForms = oForms;
                                TCreditNotes.DocSubType = "--";
                                TCreditNotes.bFolderAdd = true;
                                TCreditNotes.ObjType = "14";
                                break;
                            }
                        case "426": //Pago efectuado
                            {
                                result = (IvkFormInterface)(new TPagoEfectuado());
                                //(TCreditNotes)(result).ooForms = oForms;
                                TPagoEfectuado.bFolderAdd = true;
                                TPagoEfectuado.ObjType = "46";
                                break;
                            }
                        case "140": //Entrega
                            {
                                result = (IvkFormInterface)(new TDeliveryNote());
                                //(TDeliveryNote)(result).ooForms = oForms;
                                TDeliveryNote.Transferencia = false;
                                TDeliveryNote.bFolderAdd = true;
                                TDeliveryNote.Devolucion = false;
                                TDeliveryNote.ObjType = "15";
                                break;
                            }
                        case "940": //Transferencia Stock
                            {
                                result = (IvkFormInterface)(new TDeliveryNote());
                                //(TDeliveryNote)(result).ooForms = oForms;
                                TDeliveryNote.Transferencia = true;
                                TDeliveryNote.bFolderAdd = true;
                                TDeliveryNote.Devolucion = false;
                                TDeliveryNote.ObjType = "67";
                                break;
                            }
                        case "182": //Devolucion mercancia compra
                            {
                                result = (IvkFormInterface)(new TDeliveryNote());
                                //(TDeliveryNote)(result).ooForms = oForms;
                                TDeliveryNote.Transferencia = false;
                                TDeliveryNote.bFolderAdd = true;
                                TDeliveryNote.Devolucion = true;
                                TDeliveryNote.ObjType = "21";
                                break;
                            }
                    } //fin  switch
                }


                if (result != null)
                {
                    SAPbouiCOM.Application App = SBOApplication;
                    SAPbobsCOM.Company Cmpny = SBOCompany;
                    VisualD.SBOFunctions.CSBOFunctions SboF = SBOFunctions;
                    VisualD.GlobalVid.TGlobalVid Glob = GlobalSettings;
                    if (result.InitForm(pVal.FormUID, @"forms\\", ref App, ref Cmpny, ref SboF, ref Glob))
                    {
                        oForms.Add(result);
                    }
                    else
                    {
                        SBOApplication.Forms.Item(result.getFormId()).Close();
                        result = null;
                    }
                }

                return result;
            }// fin try
            catch (Exception e)
            {
                return null;
                oLog.OutLog("ItemEventExt: " + e.Message + " ** Trace: " + e.StackTrace);
                SBOApplication.MessageBox(e.Message + " ** Trace: " + e.StackTrace, 1, "Ok","","");  // Captura errores no manejados
            }
    
        } //fin ItemEventExt
    }
}
