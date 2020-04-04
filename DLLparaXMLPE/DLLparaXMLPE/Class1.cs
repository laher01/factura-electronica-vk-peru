using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using SAPbobsCOM;
using SAPbouiCOM;
using VisualD.SBOFunctions;
using VisualD.untLog;

namespace DLLparaXMLPE
{
    public class TDLLparaXMLPE
    {
        public VisualD.SBOFunctions.CSBOFunctions SBO_f;
        private String s;

        public String GenerarXMLStringInvoice(ref SAPbobsCOM.Recordset ors, String TipoDocElec, ref XDocument miXML, String Sector)
        {
            Int32 i;
            XElement xNodo = null;
            String ExternalFolio;
            try
            {
                if (Sector == "E")
                {
                    var x1 = ((System.String)ors.Fields.Item("TipoDocumento").Value).Trim();
                    var x2 = ((System.String)ors.Fields.Item("EmisorNroDocumento").Value).Trim();
                    var x3 = ((System.String)ors.Fields.Item("EmisorTipoDocumento").Value).Trim();
                    var x4 = ((System.String)ors.Fields.Item("EmisorNombreLegal").Value).Trim();
                    var x5 = ((System.String)ors.Fields.Item("EmisorNombreComercial").Value).Trim();
                    var x6 = ((System.String)ors.Fields.Item("EmisorDireccion").Value).Trim();
                    var x7 = ((System.String)ors.Fields.Item("EmisorUrbanizacion").Value).Trim();
                    var x8 = ((System.String)ors.Fields.Item("EmisorDepartamento").Value).Trim();
                    var x9 = ((System.String)ors.Fields.Item("EmisorProvincia").Value).Trim();
                    var x10 = ((System.String)ors.Fields.Item("EmisorDistrito").Value).Trim();
                    var x11 = ((System.String)ors.Fields.Item("CamposExtrasCorreoReceptor").Value).Trim();
                    var x12 = ((System.String)ors.Fields.Item("ReceptorNroDocumento").Value).Trim();
                    var x13 = ((System.String)ors.Fields.Item("ReceptorTipoDocumento").Value).Trim();
                    var x14 = ((System.String)ors.Fields.Item("ReceptorNombreLegal").Value).Trim();
                    ExternalFolio = ((System.String)ors.Fields.Item("IdDocumento").Value).Trim();
                    var x16 = ((System.String)ors.Fields.Item("FechaEmision").Value).Trim();
                    var x17 = ((System.String)ors.Fields.Item("Moneda").Value).Trim();
                    var x18 = ((System.Double)ors.Fields.Item("Gravadas").Value);
                    var x19 = ((System.Double)ors.Fields.Item("Gratuitas").Value);
                    var x20 = ((System.Double)ors.Fields.Item("Inafectas").Value);
                    var x21 = ((System.Double)ors.Fields.Item("Exoneradas").Value);
                    var x22 = ((System.Double)ors.Fields.Item("DescuentoGlobal").Value);
                    var x23 = ((System.Double)ors.Fields.Item("TotalVenta").Value);
                    var x24 = ((System.Double)ors.Fields.Item("TotalIgv").Value);
                    var x25 = ((System.Double)ors.Fields.Item("TotalIsc").Value);
                    var x26 = ((System.Double)ors.Fields.Item("TotalOtrosTributos").Value);
                    var x27 = ((System.String)ors.Fields.Item("MontoEnLetras").Value).Trim();
                    var x28 = ((System.String)ors.Fields.Item("TipoOperacion").Value).Trim();
                    var x29 = ((System.Double)ors.Fields.Item("CalculoIgv").Value);
                    var x30 = ((System.Double)ors.Fields.Item("CalculoIsc").Value);
                    var x31 = ((System.Double)ors.Fields.Item("CalculoDetraccion").Value);
                    var x32 = ((System.Double)ors.Fields.Item("MontoPercepcion").Value);
                    var x33 = ((System.Double)ors.Fields.Item("MontoDetraccion").Value);
                    var x34 = ((System.Double)ors.Fields.Item("MontoAnticipo").Value);
                    var x35 = ((System.String)ors.Fields.Item("DatoAdicionales").Value).Trim();
                    var x36 = ((System.String)ors.Fields.Item("Relacionados").Value).Trim();    


                    //xNodo = new XElement("DocumentoElectronico",
                    miXML.Root.Add(
                                  new XElement("TipoDocumento", ((System.String)ors.Fields.Item("TipoDocumento").Value).Trim()),

                                      new XElement("Emisor",
                                        new XElement("NroDocumento", ((System.String)ors.Fields.Item("EmisorNroDocumento").Value).Trim()),
                                        new XElement("TipoDocumento", ((System.String)ors.Fields.Item("EmisorTipoDocumento").Value).Trim()),
                                        new XElement("NombreLegal", ((System.String)ors.Fields.Item("EmisorNombreLegal").Value).Trim()),
                                        new XElement("NombreComercial", ((System.String)ors.Fields.Item("EmisorNombreComercial").Value).Trim()),
                                        new XElement("Direccion", ((System.String)ors.Fields.Item("EmisorDireccion").Value).Trim()),
                                        new XElement("Urbanizacion", ((System.String)ors.Fields.Item("EmisorUrbanizacion").Value).Trim()),
                                        new XElement("Departamento", ((System.String)ors.Fields.Item("EmisorDepartamento").Value).Trim()),
                                        new XElement("Provincia", ((System.String)ors.Fields.Item("EmisorProvincia").Value).Trim()),
                                        new XElement("Distrito", ((System.String)ors.Fields.Item("EmisorDistrito").Value).Trim())
                                      ),

                                      new XElement("CamposExtras",
                                        new XElement("CorreoReceptor", ((System.String)ors.Fields.Item("CamposExtrasCorreoReceptor").Value).Trim())),

                                      new XElement("Receptor",
                                        new XElement("NroDocumento", ((System.String)ors.Fields.Item("ReceptorNroDocumento").Value).Trim()),
                                        new XElement("TipoDocumento", ((System.String)ors.Fields.Item("ReceptorTipoDocumento").Value).Trim()),
                                        new XElement("NombreLegal", ((System.String)ors.Fields.Item("ReceptorNombreLegal").Value).Trim())),
                                      new XElement("IdDocumento", ((System.String)ors.Fields.Item("IdDocumento").Value).Trim()),
                                      new XElement("FechaEmision", ((System.String)ors.Fields.Item("FechaEmision").Value).Trim()),
                                      new XElement("Moneda", ((System.String)ors.Fields.Item("Moneda").Value).Trim()),
                                      new XElement("Gravadas", ((System.Double)ors.Fields.Item("Gravadas").Value)),
                                      new XElement("Gratuitas", ((System.Double)ors.Fields.Item("Gratuitas").Value)),
                                      new XElement("Inafectas", ((System.Double)ors.Fields.Item("Inafectas").Value)),
                                      new XElement("Exoneradas", ((System.Double)ors.Fields.Item("Exoneradas").Value)),
                                      new XElement("DescuentoGlobal", ((System.Double)ors.Fields.Item("DescuentoGlobal").Value)),
                                      new XElement("TotalVenta", ((System.Double)ors.Fields.Item("TotalVenta").Value)),
                                      new XElement("TotalIgv", ((System.Double)ors.Fields.Item("TotalIgv").Value)),
                                      new XElement("TotalIsc", ((System.Double)ors.Fields.Item("TotalIsc").Value)),
                                      new XElement("TotalOtrosTributos", ((System.Double)ors.Fields.Item("TotalOtrosTributos").Value)),
                                      new XElement("MontoEnLetras", ((System.String)ors.Fields.Item("MontoEnLetras").Value).Trim()),
                                      new XElement("TipoOperacion", ((System.String)ors.Fields.Item("TipoOperacion").Value).Trim()),
                                      new XElement("CalculoIgv", ((System.Double)ors.Fields.Item("CalculoIgv").Value)),
                                      new XElement("CalculoIsc", ((System.Double)ors.Fields.Item("CalculoIsc").Value)),
                                      new XElement("CalculoDetraccion", ((System.Double)ors.Fields.Item("CalculoDetraccion").Value)),
                                      new XElement("MontoPercepcion", ((System.Double)ors.Fields.Item("MontoPercepcion").Value)),
                                      new XElement("MontoDetraccion", ((System.Double)ors.Fields.Item("MontoDetraccion").Value)),
                                      new XElement("MontoAnticipo", ((System.Double)ors.Fields.Item("MontoAnticipo").Value)),
                                      new XElement("DatoAdicionales", ((System.String)ors.Fields.Item("DatoAdicionales").Value).Trim()),
                                      new XElement("Relacionados", ((System.String)ors.Fields.Item("Relacionados").Value).Trim())
                                );
                    //miXML.Descendants("DocumentoElectronico").LastOrDefault().Add(xNodo);
                    //miXML.Root.Add(xNodo);
                }//fin Sector E
                else if (Sector == "R")
                {
                    while (!ors.EoF)
                    {
                        var result = (from nodo in miXML.Descendants("Discrepancias")
                                      //where nodo.Attribute("id").Value == "1234"
                                      select nodo).FirstOrDefault();

                        if (result == null)
                        {
                            xNodo = new XElement("Discrepancias",
                                                new XElement("Discrepancia",
                                                    new XElement("nroReferencia", ((System.String)ors.Fields.Item("nroReferencia").Value)),
                                                    new XElement("Tipo", ((System.String)ors.Fields.Item("Tipo").Value)),
                                                    new XElement("Descripcion", ((System.String)ors.Fields.Item("Descripcion").Value).Trim()))
                                                );
                            miXML.Root.Add(xNodo);

                            if (TipoDocElec == "08")
                            {   
                                xNodo = new XElement("relacionados",
                                                    new XElement("relacionado",
                                                        new XElement("NroDocumento", ((System.String)ors.Fields.Item("nroReferencia").Value)),
                                                        new XElement("TipoDocumento", ((System.String)ors.Fields.Item("Tipo").Value)))
                                                    );
                                miXML.Root.Add(xNodo);
                            }
                        }
                        else
                        {
                            xNodo = new XElement("Discrepancia",
                                                    new XElement("nroReferencia", ((System.String)ors.Fields.Item("nroReferencia").Value)),
                                                    new XElement("Tipo", ((System.String)ors.Fields.Item("Tipo").Value)),
                                                    new XElement("Descripcion", ((System.String)ors.Fields.Item("Descripcion").Value).Trim()));
                            miXML.Descendants("Discrepancias").LastOrDefault().Add(xNodo);

                            if (TipoDocElec == "08")
                            {
                                xNodo = new XElement("relacionado",
                                                        new XElement("NroDocumento", ((System.String)ors.Fields.Item("nroReferencia").Value)),
                                                        new XElement("TipoDocumento", ((System.String)ors.Fields.Item("Tipo").Value)));
                                miXML.Descendants("relacionados").LastOrDefault().Add(xNodo);
                            }
                        }
                        ors.MoveNext();
                    }
                }//fin Sector R
                else if (Sector == "D")
                {
                    while (!ors.EoF)
                    {
                        var result = (from nodo in miXML.Descendants("Items")
                                      //where nodo.Attribute("id").Value == "1234"
                                      select nodo).FirstOrDefault();

                        if (result == null)
                        {
                            xNodo = new XElement("Items",
                                                new XElement("DetalleDocumento",
                                                    new XElement("Id", ((System.Int32)ors.Fields.Item("Id").Value)),
                                                    new XElement("Cantidad", ((System.Double)ors.Fields.Item("Cantidad").Value)),
                                                    new XElement("UnidadMedida", ((System.String)ors.Fields.Item("UnidadMedida").Value).Trim()),
                                                    new XElement("Suma", ((System.Double)ors.Fields.Item("Suma").Value)),
                                                    new XElement("TotalVenta", ((System.Double)ors.Fields.Item("TotalVenta").Value)),
                                                    new XElement("PrecioUnitario", ((System.Double)ors.Fields.Item("PrecioUnitario").Value)),
                                                    new XElement("TipoPrecio", ((System.String)ors.Fields.Item("TipoPrecio").Value).Trim()),
                                                    new XElement("Impuesto", ((System.Double)ors.Fields.Item("Impuesto").Value)),
                                                    new XElement("TipoImpuesto", ((System.String)ors.Fields.Item("TipoImpuesto").Value).Trim()),
                                                    new XElement("ImpuestoSelectivo", ((System.Double)ors.Fields.Item("ImpuestoSelectivo").Value)),
                                                    new XElement("OtroImpuesto", ((System.Double)ors.Fields.Item("OtroImpuesto").Value)),
                                                    new XElement("Descripcion", ((System.String)ors.Fields.Item("Descripcion").Value).Trim()),
                                                    new XElement("CodigoItem", ((System.String)ors.Fields.Item("CodigoItem").Value).Trim()),
                                                    new XElement("PrecioReferencial", ((System.Double)ors.Fields.Item("PrecioReferencial").Value)))
                                                );
                            miXML.Root.Add(xNodo);
                        }
                        else
                        {
                            xNodo = new XElement("DetalleDocumento",
                                                    new XElement("Id", ((System.Int32)ors.Fields.Item("Id").Value)),
                                                    new XElement("Cantidad", ((System.Double)ors.Fields.Item("Cantidad").Value)),
                                                    new XElement("UnidadMedida", ((System.String)ors.Fields.Item("UnidadMedida").Value).Trim()),
                                                    new XElement("Suma", ((System.Double)ors.Fields.Item("Suma").Value)),
                                                    new XElement("TotalVenta", ((System.Double)ors.Fields.Item("TotalVenta").Value)),
                                                    new XElement("PrecioUnitario", ((System.Double)ors.Fields.Item("PrecioUnitario").Value)),
                                                    new XElement("TipoPrecio", ((System.String)ors.Fields.Item("TipoPrecio").Value).Trim()),
                                                    new XElement("Impuesto", ((System.Double)ors.Fields.Item("Impuesto").Value)),
                                                    new XElement("TipoImpuesto", ((System.String)ors.Fields.Item("TipoImpuesto").Value).Trim()),
                                                    new XElement("ImpuestoSelectivo", ((System.Double)ors.Fields.Item("ImpuestoSelectivo").Value)),
                                                    new XElement("OtroImpuesto", ((System.Double)ors.Fields.Item("OtroImpuesto").Value)),
                                                    new XElement("Descripcion", ((System.String)ors.Fields.Item("Descripcion").Value).Trim()),
                                                    new XElement("CodigoItem", ((System.String)ors.Fields.Item("CodigoItem").Value).Trim()),
                                                    new XElement("PrecioReferencial", ((System.Double)ors.Fields.Item("PrecioReferencial").Value)));
                            miXML.Descendants("Items").LastOrDefault().Add(xNodo);
                        }
                        ors.MoveNext();
                    }
                }

                return miXML.ToString();
            }
            catch (Exception x)
            {
                SBO_f.oLog.OutLog("Error GenerarXMLStringInvoice, Sector " + Sector + " -> " + x.Message + ", TRACE " + x.StackTrace);
                return "";
            }
        }

        public String GenerarXMLStringCreditNote(ref SAPbobsCOM.Recordset ors, String TipoDocElec, ref XDocument miXML, String Sector)
        {
            Int32 i;
            XElement xNodo = null;
            String ExternalFolio;
            try
            {
                if (Sector == "E")
                {
                    var x1 = ((System.String)ors.Fields.Item("TipoDocumento").Value).Trim();
                    var x2 = ((System.String)ors.Fields.Item("EmisorNroDocumento").Value).Trim();
                    var x3 = ((System.String)ors.Fields.Item("EmisorTipoDocumento").Value).Trim();
                    var x4 = ((System.String)ors.Fields.Item("EmisorNombreLegal").Value).Trim();
                    var x5 = ((System.String)ors.Fields.Item("EmisorNombreComercial").Value).Trim();
                    var x6 = ((System.String)ors.Fields.Item("EmisorDireccion").Value).Trim();
                    var x7 = ((System.String)ors.Fields.Item("EmisorUrbanizacion").Value).Trim();
                    var x8 = ((System.String)ors.Fields.Item("EmisorDepartamento").Value).Trim();
                    var x9 = ((System.String)ors.Fields.Item("EmisorProvincia").Value).Trim();
                    var x10 = ((System.String)ors.Fields.Item("EmisorDistrito").Value).Trim();
                    var x11 = ((System.String)ors.Fields.Item("CamposExtrasCorreoReceptor").Value).Trim();
                    var x12 = ((System.String)ors.Fields.Item("ReceptorNroDocumento").Value).Trim();
                    var x13 = ((System.String)ors.Fields.Item("ReceptorTipoDocumento").Value).Trim();
                    var x14 = ((System.String)ors.Fields.Item("ReceptorNombreLegal").Value).Trim();
                    ExternalFolio = ((System.String)ors.Fields.Item("IdDocumento").Value).Trim();
                    var x16 = ((System.String)ors.Fields.Item("FechaEmision").Value).Trim();
                    var x17 = ((System.String)ors.Fields.Item("Moneda").Value).Trim();
                    var x18 = ((System.Double)ors.Fields.Item("Gravadas").Value);
                    var x19 = ((System.Double)ors.Fields.Item("Gratuitas").Value);
                    var x20 = ((System.Double)ors.Fields.Item("Inafectas").Value);
                    var x21 = ((System.Double)ors.Fields.Item("Exoneradas").Value);
                    var x22 = ((System.Double)ors.Fields.Item("DescuentoGlobal").Value);
                    var x23 = ((System.Double)ors.Fields.Item("TotalVenta").Value);
                    var x24 = ((System.Double)ors.Fields.Item("TotalIgv").Value);
                    var x25 = ((System.Double)ors.Fields.Item("TotalIsc").Value);
                    var x26 = ((System.Double)ors.Fields.Item("TotalOtrosTributos").Value);
                    var x27 = ((System.String)ors.Fields.Item("MontoEnLetras").Value).Trim();
                    var x28 = ((System.String)ors.Fields.Item("TipoOperacion").Value).Trim();
                    var x29 = ((System.Double)ors.Fields.Item("CalculoIgv").Value);
                    var x30 = ((System.Double)ors.Fields.Item("CalculoIsc").Value);
                    var x31 = ((System.Double)ors.Fields.Item("CalculoDetraccion").Value);
                    var x32 = ((System.Double)ors.Fields.Item("MontoPercepcion").Value);
                    var x33 = ((System.Double)ors.Fields.Item("MontoDetraccion").Value);
                    var x34 = ((System.Double)ors.Fields.Item("MontoAnticipo").Value);
                    var x35 = ((System.String)ors.Fields.Item("DatoAdicionales").Value).Trim();
                    var x36 = ((System.String)ors.Fields.Item("Relacionados").Value).Trim();


                    //xNodo = new XElement("DocumentoElectronico",
                    miXML.Root.Add(
                                    new XElement("TipoDocumento", ((System.String)ors.Fields.Item("TipoDocumento").Value).Trim()),
                                      new XElement("Emisor",
                                        new XElement("NroDocumento", ((System.String)ors.Fields.Item("EmisorNroDocumento").Value).Trim()),
                                        new XElement("TipoDocumento", ((System.String)ors.Fields.Item("EmisorTipoDocumento").Value).Trim()),
                                        new XElement("NombreLegal", ((System.String)ors.Fields.Item("EmisorNombreLegal").Value).Trim()),
                                        new XElement("NombreComercial", ((System.String)ors.Fields.Item("EmisorNombreComercial").Value).Trim()),
                                        new XElement("Direccion", ((System.String)ors.Fields.Item("EmisorDireccion").Value).Trim()),
                                        new XElement("Urbanizacion", ((System.String)ors.Fields.Item("EmisorUrbanizacion").Value).Trim()),
                                        new XElement("Departamento", ((System.String)ors.Fields.Item("EmisorDepartamento").Value).Trim()),
                                        new XElement("Provincia", ((System.String)ors.Fields.Item("EmisorProvincia").Value).Trim()),
                                        new XElement("Distrito", ((System.String)ors.Fields.Item("EmisorDistrito").Value).Trim())
                                      ),
                                      new XElement("CamposExtras",
                                        new XElement("CorreoReceptor", ((System.String)ors.Fields.Item("CamposExtrasCorreoReceptor").Value).Trim())),
                                      new XElement("Receptor",
                                        new XElement("NroDocumento", ((System.String)ors.Fields.Item("ReceptorNroDocumento").Value).Trim()),
                                        new XElement("TipoDocumento", ((System.String)ors.Fields.Item("ReceptorTipoDocumento").Value).Trim()),
                                        new XElement("NombreLegal", ((System.String)ors.Fields.Item("ReceptorNombreLegal").Value).Trim())),
                                      new XElement("IdDocumento", ((System.String)ors.Fields.Item("IdDocumento").Value).Trim()),
                                      new XElement("FechaEmision", ((System.String)ors.Fields.Item("FechaEmision").Value).Trim()),
                                      new XElement("Moneda", ((System.String)ors.Fields.Item("Moneda").Value).Trim()),
                                      new XElement("Gravadas", ((System.Double)ors.Fields.Item("Gravadas").Value)),
                                      new XElement("Gratuitas", ((System.Double)ors.Fields.Item("Gratuitas").Value)),
                                      new XElement("Inafectas", ((System.Double)ors.Fields.Item("Inafectas").Value)),
                                      new XElement("Exoneradas", ((System.Double)ors.Fields.Item("Exoneradas").Value)),
                                      new XElement("DescuentoGlobal", ((System.Double)ors.Fields.Item("DescuentoGlobal").Value)),
                                      new XElement("TotalVenta", ((System.Double)ors.Fields.Item("TotalVenta").Value)),
                                      new XElement("TotalIgv", ((System.Double)ors.Fields.Item("TotalIgv").Value)),
                                      new XElement("TotalIsc", ((System.Double)ors.Fields.Item("TotalIsc").Value)),
                                      new XElement("TotalOtrosTributos", ((System.Double)ors.Fields.Item("TotalOtrosTributos").Value)),
                                      new XElement("MontoEnLetras", ((System.String)ors.Fields.Item("MontoEnLetras").Value).Trim()),
                                      new XElement("TipoOperacion", ((System.String)ors.Fields.Item("TipoOperacion").Value).Trim()),
                                      new XElement("CalculoIgv", ((System.Double)ors.Fields.Item("CalculoIgv").Value)),
                                      new XElement("CalculoIsc", ((System.Double)ors.Fields.Item("CalculoIsc").Value)),
                                      new XElement("CalculoDetraccion", ((System.Double)ors.Fields.Item("CalculoDetraccion").Value)),
                                      new XElement("MontoPercepcion", ((System.Double)ors.Fields.Item("MontoPercepcion").Value)),
                                      new XElement("MontoDetraccion", ((System.Double)ors.Fields.Item("MontoDetraccion").Value)),
                                      new XElement("MontoAnticipo", ((System.Double)ors.Fields.Item("MontoAnticipo").Value)),
                                      new XElement("DatoAdicionales", ((System.String)ors.Fields.Item("DatoAdicionales").Value).Trim()),
                                      new XElement("Relacionados", ((System.String)ors.Fields.Item("Relacionados").Value).Trim())
                                );
                    //miXML.Root.Add(xNodo);
                }//fin Sector E
                else if (Sector == "R")
                {
                    while (!ors.EoF)
                    {
                        var result = (from nodo in miXML.Descendants("Discrepancias")
                                      //where nodo.Attribute("id").Value == "1234"
                                      select nodo).FirstOrDefault();

                        if (result == null)
                        {
                            xNodo = new XElement("Discrepancias",
                                                new XElement("Discrepancia",
                                                    new XElement("nroReferencia", ((System.String)ors.Fields.Item("nroReferencia").Value)),
                                                    new XElement("Tipo", ((System.String)ors.Fields.Item("Tipo").Value)),
                                                    new XElement("Descripcion", ((System.String)ors.Fields.Item("Descripcion").Value).Trim()))
                                                );
                            miXML.Root.Add(xNodo);

                            xNodo = new XElement("relacionados",
                                                new XElement("relacionado",
                                                    new XElement("NroDocumento", ((System.String)ors.Fields.Item("nroReferencia").Value)),
                                                    new XElement("TipoDocumento", ((System.String)ors.Fields.Item("Tipo").Value)))
                                                );
                            miXML.Root.Add(xNodo);
                        }
                        else
                        {
                            xNodo = new XElement("Discrepancia",
                                                    new XElement("nroReferencia", ((System.String)ors.Fields.Item("nroReferencia").Value)),
                                                    new XElement("Tipo", ((System.String)ors.Fields.Item("Tipo").Value)),
                                                    new XElement("Descripcion", ((System.String)ors.Fields.Item("Descripcion").Value).Trim()));
                            miXML.Descendants("Discrepancias").LastOrDefault().Add(xNodo);

                            xNodo = new XElement("relacionado",
                                                    new XElement("NroDocumento", ((System.String)ors.Fields.Item("nroReferencia").Value)),
                                                    new XElement("TipoDocumento", ((System.String)ors.Fields.Item("Tipo").Value)));
                            miXML.Descendants("relacionados").LastOrDefault().Add(xNodo);
                        }
                        ors.MoveNext();
                    }
                }//fin Sector R
                else if (Sector == "D")
                {
                    while (!ors.EoF)
                    {
                        var result = (from nodo in miXML.Descendants("Items")
                                      //where nodo.Attribute("id").Value == "1234"
                                      select nodo).FirstOrDefault();

                        if (result == null)
                        {
                            xNodo = new XElement("Items",
                                                new XElement("DetalleDocumento",
                                                    new XElement("Id", ((System.Int32)ors.Fields.Item("Id").Value)),
                                                    new XElement("Cantidad", ((System.Double)ors.Fields.Item("Cantidad").Value)),
                                                    new XElement("UnidadMedida", ((System.String)ors.Fields.Item("UnidadMedida").Value).Trim()),
                                                    new XElement("Suma", ((System.Double)ors.Fields.Item("Suma").Value)),
                                                    new XElement("TotalVenta", ((System.Double)ors.Fields.Item("TotalVenta").Value)),
                                                    new XElement("PrecioUnitario", ((System.Double)ors.Fields.Item("PrecioUnitario").Value)),
                                                    new XElement("TipoPrecio", ((System.String)ors.Fields.Item("TipoPrecio").Value).Trim()),
                                                    new XElement("Impuesto", ((System.Double)ors.Fields.Item("Impuesto").Value)),
                                                    new XElement("TipoImpuesto", ((System.String)ors.Fields.Item("TipoImpuesto").Value).Trim()),
                                                    new XElement("ImpuestoSelectivo", ((System.Double)ors.Fields.Item("ImpuestoSelectivo").Value)),
                                                    new XElement("OtroImpuesto", ((System.Double)ors.Fields.Item("OtroImpuesto").Value)),
                                                    new XElement("Descripcion", ((System.String)ors.Fields.Item("Descripcion").Value).Trim()),
                                                    new XElement("CodigoItem", ((System.String)ors.Fields.Item("CodigoItem").Value).Trim()),
                                                    new XElement("PrecioReferencial", ((System.Double)ors.Fields.Item("PrecioReferencial").Value)))
                                                );
                            miXML.Root.Add(xNodo);
                        }
                        else
                        {
                            xNodo = new XElement("DetalleDocumento",
                                                    new XElement("Id", ((System.Int32)ors.Fields.Item("Id").Value)),
                                                    new XElement("Cantidad", ((System.Double)ors.Fields.Item("Cantidad").Value)),
                                                    new XElement("UnidadMedida", ((System.String)ors.Fields.Item("UnidadMedida").Value).Trim()),
                                                    new XElement("Suma", ((System.Double)ors.Fields.Item("Suma").Value)),
                                                    new XElement("TotalVenta", ((System.Double)ors.Fields.Item("TotalVenta").Value)),
                                                    new XElement("PrecioUnitario", ((System.Double)ors.Fields.Item("PrecioUnitario").Value)),
                                                    new XElement("TipoPrecio", ((System.String)ors.Fields.Item("TipoPrecio").Value).Trim()),
                                                    new XElement("Impuesto", ((System.Double)ors.Fields.Item("Impuesto").Value)),
                                                    new XElement("TipoImpuesto", ((System.String)ors.Fields.Item("TipoImpuesto").Value).Trim()),
                                                    new XElement("ImpuestoSelectivo", ((System.Double)ors.Fields.Item("ImpuestoSelectivo").Value)),
                                                    new XElement("OtroImpuesto", ((System.Double)ors.Fields.Item("OtroImpuesto").Value)),
                                                    new XElement("Descripcion", ((System.String)ors.Fields.Item("Descripcion").Value).Trim()),
                                                    new XElement("CodigoItem", ((System.String)ors.Fields.Item("CodigoItem").Value).Trim()),
                                                    new XElement("PrecioReferencial", ((System.Double)ors.Fields.Item("PrecioReferencial").Value)));
                            miXML.Descendants("Items").LastOrDefault().Add(xNodo);
                        }
                        ors.MoveNext();
                    }
                }//fin Sector D

                return miXML.ToString();
            }
            catch (Exception x)
            {
                SBO_f.oLog.OutLog("Error GenerarXMLStringCreditNote, Sector " + Sector + " -> " + x.Message + ", TRACE " + x.StackTrace);
                return "";
            }
        }

        public String GenerarXMLStringPayment(ref SAPbobsCOM.Recordset ors, String TipoDocElec, ref XDocument miXML, String Sector)
        {
            Int32 i;
            XElement xNodo = null;
            String ExternalFolio;
            try
            {
                if (Sector == "E")
                {
                    //xNodo = new XElement("DocumentoElectronico",
                    miXML.Root.Add(
                                    new XElement("IdDocumento", ((System.String)ors.Fields.Item("IdDocumento").Value).Trim()),
                                              new XElement("FechaEmision", ((System.String)ors.Fields.Item("FechaEmision").Value).Trim()),

                                              new XElement("Emisor",
                                                new XElement("NroDocumento", ((System.String)ors.Fields.Item("EmisorNroDocumento").Value).Trim()),
                                                new XElement("TipoDocumento", ((System.String)ors.Fields.Item("EmisorTipoDocumento").Value).Trim()),
                                                new XElement("NombreLegal", ((System.String)ors.Fields.Item("EmisorNombreLegal").Value).Trim()),
                                                new XElement("NombreComercial", ((System.String)ors.Fields.Item("EmisorNombreComercial").Value).Trim()),
                                                new XElement("Ubigeo", ((System.String)ors.Fields.Item("EmisorUbigeo").Value).Trim()),
                                                new XElement("Direccion", ((System.String)ors.Fields.Item("EmisorDireccion").Value).Trim()),
                                                new XElement("Urbanizacion", ((System.String)ors.Fields.Item("EmisorUrbanizacion").Value).Trim()),
                                                new XElement("Departamento", ((System.String)ors.Fields.Item("EmisorDepartamento").Value).Trim()),
                                                new XElement("Provincia", ((System.String)ors.Fields.Item("EmisorProvincia").Value).Trim()),
                                                new XElement("Distrito", ((System.String)ors.Fields.Item("EmisorDistrito").Value).Trim())
                                              ),

                                              new XElement("CamposExtras",
                                                new XElement("CorreoReceptor", ((System.String)ors.Fields.Item("CamposExtrasCorreoReceptor").Value).Trim())),

                                              new XElement("Receptor",
                                                new XElement("NroDocumento", ((System.String)ors.Fields.Item("ReceptorNroDocumento").Value).Trim()),
                                                new XElement("TipoDocumento", ((System.String)ors.Fields.Item("ReceptorTipoDocumento").Value).Trim()),
                                                new XElement("NombreComercial", ((System.String)ors.Fields.Item("ReceptorNombreComercial").Value).Trim()),
                                                new XElement("NombreLegal", ((System.String)ors.Fields.Item("ReceptorNombreLegal").Value).Trim())),
                                              new XElement("Moneda", ((System.String)ors.Fields.Item("Moneda").Value).Trim()),
                                              new XElement("Obervaciones", ((System.String)ors.Fields.Item("Observaciones").Value).Trim()),
                                              new XElement("RegimenRetencion", ((System.String)ors.Fields.Item("RegimenRetencion").Value).Trim()),
                                              new XElement("TasaRetencion", ((System.String)ors.Fields.Item("TasaRetencion").Value).Trim()),
                                              new XElement("ImporteTotalRetenido", ((System.Double)ors.Fields.Item("ImporteTotalRetenido").Value)),
                                              new XElement("ImporteTotalPagado", ((System.Double)ors.Fields.Item("ImporteTotalPagado").Value))
                                        );
                    //miXML.Root.Add(xNodo);
                }//fin Sector E
                else if (Sector == "D")
                {
                    while (!ors.EoF)
                    {
                        var result = (from nodo in miXML.Descendants("DocumentosRelacionados")
                                      //where nodo.Attribute("id").Value == "1234"
                                      select nodo).FirstOrDefault();

                        if (result == null)
                        {
                            xNodo = new XElement("DocumentosRelacionados",
                                                    new XElement("NroDocumento", ((System.String)ors.Fields.Item("NroDocumento").Value).Trim()),
                                                    new XElement("TipoDocumento", ((System.String)ors.Fields.Item("TipoDocumento").Value).Trim()),
                                                    new XElement("FechaEmision", ((System.String)ors.Fields.Item("FechaEmision").Value).Trim()),
                                                    new XElement("ImporteTotal", ((System.Double)ors.Fields.Item("ImporteTotal").Value)),
                                                    new XElement("MonedaDocumentoRelacionado", ((System.String)ors.Fields.Item("MonedaDocumentoRelacionado").Value).Trim()),
                                                    new XElement("NumeroPago", ((System.Int32)ors.Fields.Item("NumeroPago").Value)),
                                                    new XElement("ImporteTotalNeto", ((System.Double)ors.Fields.Item("ImporteTotalNeto").Value)),
                                                    new XElement("ImporteSinRetencion", ((System.Double)ors.Fields.Item("ImporteSinRetencion").Value)),
                                                    new XElement("FechaPago", ((System.String)ors.Fields.Item("FechaPago").Value).Trim()),
                                                    new XElement("ImporteRetencion", ((System.Double)ors.Fields.Item("ImporteRetencion").Value)),
                                                    new XElement("FechaRetencion", ((System.String)ors.Fields.Item("FechaRetencion").Value).Trim()),
                                                    new XElement("TipoCambio", ((System.Double)ors.Fields.Item("TipoCambio").Value)),
                                                    new XElement("FechaTipoCambio", ((System.String)ors.Fields.Item("FechaTipoCambio").Value).Trim())
                                                );
                            //miXML.Root.Add(xNodo);
                            miXML.Descendants("DocumentoElectronico").LastOrDefault().Add(xNodo);
                        }
                        else
                        {
                            xNodo = new XElement("DocumentosRelacionados",
                                                    new XElement("NroDocumento", ((System.String)ors.Fields.Item("NroDocumento").Value).Trim()),
                                                    new XElement("TipoDocumento", ((System.String)ors.Fields.Item("TipoDocumento").Value).Trim()),
                                                    new XElement("FechaEmision", ((System.String)ors.Fields.Item("FechaEmision").Value).Trim()),
                                                    new XElement("ImporteTotal", ((System.Double)ors.Fields.Item("ImporteTotal").Value)),
                                                    new XElement("MonedaDocumentoRelacionado", ((System.String)ors.Fields.Item("MonedaDocumentoRelacionado").Value).Trim()),
                                                    new XElement("NumeroPago", ((System.Int32)ors.Fields.Item("NumeroPago").Value)),
                                                    new XElement("ImporteTotalNeto", ((System.Double)ors.Fields.Item("ImporteTotalNeto").Value)),
                                                    new XElement("ImporteSinRetencion", ((System.Double)ors.Fields.Item("ImporteSinRetencion").Value)),
                                                    new XElement("FechaPago", ((System.String)ors.Fields.Item("FechaPago").Value).Trim()),
                                                    new XElement("ImporteRetencion", ((System.Double)ors.Fields.Item("ImporteRetencion").Value)),
                                                    new XElement("FechaRetencion", ((System.String)ors.Fields.Item("FechaRetencion").Value).Trim()),
                                                    new XElement("TipoCambio", ((System.Double)ors.Fields.Item("TipoCambio").Value)),
                                                    new XElement("FechaTipoCambio", ((System.String)ors.Fields.Item("FechaTipoCambio").Value).Trim())
                                                );
                            miXML.Descendants("DocumentoElectronico").LastOrDefault().Add(xNodo);
                        }
                        ors.MoveNext();
                    }
                }//fin Sector D

                return miXML.ToString();
            }
            catch (Exception x)
            {
                SBO_f.oLog.OutLog("Error GenerarXMLStringPayment, Sector " + Sector + " -> " + x.Message + ", TRACE " + x.StackTrace);
                return "";
            }
        }

        public String GenerarXMLStringDelivery(ref SAPbobsCOM.Recordset ors, String TipoDocElec, ref XDocument miXML, String Sector)
        {
            Int32 i;
            XElement xNodo = null;
            String ExternalFolio;
            try
            {
                if (Sector == "E")
                {
                    //xNodo = new XElement("DocumentoElectronico",
                    miXML.Root.Add(
                                    new XElement("IdDocumento", ((System.String)ors.Fields.Item("IdDocumento").Value).Trim()),
                                      new XElement("FechaEmision", ((System.String)ors.Fields.Item("FechaEmision").Value).Trim()),
                                      new XElement("TipoDocumento", ((System.String)ors.Fields.Item("TipoDocumento").Value).Trim()),
                                      new XElement("Glosa", ((System.String)ors.Fields.Item("Glosa").Value).Trim()),
                                      new XElement("CodigoMotivoTraslado", ((System.String)ors.Fields.Item("CodigoMotivoTraslado").Value).Trim()),
                                      new XElement("DescripcionMotivo", ((System.String)ors.Fields.Item("DescripcionMotivo").Value).Trim()),
                                      new XElement("Transbordo", ((System.String)ors.Fields.Item("Transbordo").Value).Trim()),
                                      new XElement("PesoBrutoTotal", ((System.Double)ors.Fields.Item("PesoBrutoTotal").Value)),
                                      new XElement("NroPallets", ((System.Double)ors.Fields.Item("NroPallets").Value)),
                                      new XElement("ModalidadTraslado", ((System.String)ors.Fields.Item("ModalidadTraslado").Value).Trim()),
                                      new XElement("FechaInicioTraslado", ((System.String)ors.Fields.Item("FechaInicioTraslado").Value).Trim()),
                                      new XElement("RazonSocialTransportista", ((System.String)ors.Fields.Item("RazonSocialTransportista").Value).Trim()),
                                      new XElement("RucTransportista", ((System.String)ors.Fields.Item("RucTransportista").Value).Trim()),
                                      new XElement("NroPlacaVehiculo", ((System.String)ors.Fields.Item("NroPlacaVehiculo").Value).Trim()),
                                      new XElement("NroDocumentoConductor", ((System.String)ors.Fields.Item("NroDocumentoConductor").Value).Trim()),

                                      new XElement("DireccionPartida",
                                        new XElement("DireccionCompleta", ((System.String)ors.Fields.Item("DireccionPartidaCompleta").Value).Trim()),
                                        new XElement("Ubigeo", ((System.String)ors.Fields.Item("DireccionPartidaUbigeo").Value).Trim())),

                                      new XElement("DireccionLlegada",
                                        new XElement("DireccionCompleta", ((System.String)ors.Fields.Item("DireccionLlegadaCompleta").Value).Trim()),
                                        new XElement("Ubigeo", ((System.String)ors.Fields.Item("DireccionLlegadaUbigeo").Value).Trim())),

                                      new XElement("NumeroContenedor", ((System.String)ors.Fields.Item("NumeroContenedor").Value).Trim()),
                                      new XElement("CodigoPuerto", ((System.String)ors.Fields.Item("CodigoPuerto").Value).Trim()),

                                      new XElement("Remitente",
                                        new XElement("NroDocumento", ((System.String)ors.Fields.Item("RemitenteNroDocumento").Value).Trim()),
                                        new XElement("TipoDocumento", ((System.String)ors.Fields.Item("RemitenteTipoDocumento").Value).Trim()),
                                        new XElement("Direccion", ((System.String)ors.Fields.Item("RemitenteDireccion").Value).Trim()),
                                        new XElement("Urbanizacion", ((System.String)ors.Fields.Item("RemitenteUrbanizacion").Value).Trim()),
                                        new XElement("Departamento", ((System.String)ors.Fields.Item("RemitenteDepartamento").Value).Trim()),
                                        new XElement("Provincia", ((System.String)ors.Fields.Item("RemitenteProvincia").Value).Trim()),
                                        new XElement("Distrito", ((System.String)ors.Fields.Item("RemitenteDistrito").Value).Trim()),
                                        new XElement("NombreComercial", ((System.String)ors.Fields.Item("RemitenteNombreComercial").Value).Trim()),
                                        new XElement("NombreLegal", ((System.String)ors.Fields.Item("RemitenteNombreLegal").Value).Trim()),
                                        new XElement("Ubigeo", ((System.String)ors.Fields.Item("RemitenteUbigeo").Value).Trim())
                                      ),

                                      new XElement("Destinatario",
                                        new XElement("NroDocumento", ((System.String)ors.Fields.Item("DestinatarioNroDocumento").Value).Trim()),
                                        new XElement("TipoDocumento", ((System.String)ors.Fields.Item("DestinatarioTipoDocumento").Value).Trim()),
                                        new XElement("NombreLegal", ((System.String)ors.Fields.Item("DestinatarioNombreLegal").Value).Trim()),
                                        new XElement("NombreComercial", ((System.String)ors.Fields.Item("DestinatarioNombreComercial").Value).Trim())),

                                      new XElement("CamposExtras",
                                        new XElement("CorreoReceptor", ((System.String)ors.Fields.Item("CamposExtrasCorreoReceptor").Value).Trim()))
                                );
                    //miXML.Add(xNodo);
                }//fin Sector E
                else if (Sector == "R")
                {
                    while (!ors.EoF)
                    {
                        var result = (from nodo in miXML.Descendants("DocumentosRelacionados")
                                      //where nodo.Attribute("id").Value == "1234"
                                      select nodo).FirstOrDefault();

                        if (result == null)
                        {
                            xNodo = new XElement("DocumentoRelacionados",
                                                new XElement("DocumentoRelacionado",
                                                    new XElement("NroDocumento", ((System.String)ors.Fields.Item("NroDocumento").Value)),
                                                    new XElement("TipoDocumento", ((System.String)ors.Fields.Item("TipoDocumento").Value)))
                                                );
                            miXML.Root.Add(xNodo);
                        }
                        else
                        {
                            xNodo = new XElement("DocumentoRelacionado",
                                                    new XElement("NroDocumento", ((System.String)ors.Fields.Item("NroDocumento").Value)),
                                                    new XElement("TipoDocumento", ((System.String)ors.Fields.Item("TipoDocumento").Value)));
                            miXML.Descendants("DocumentoRelacionados").LastOrDefault().Add(xNodo);
                        }
                        ors.MoveNext();
                    }
                }//fin Sector R
                else if (Sector == "D")
                {
                    while (!ors.EoF)
                    {
                        //**var result = (from nodo in miXML.Descendants("Items")
                                      //where nodo.Attribute("id").Value == "1234"
                        //**              select nodo).FirstOrDefault();

                        //**if (result == null)
                        //**{
                            //**xNodo = new XElement("Items",
                            xNodo = new XElement("BienesaTransportar",
                                                    new XElement("Cantidad", ((System.Double)ors.Fields.Item("Cantidad").Value)),
                                                    new XElement("CodigoItem", ((System.String)ors.Fields.Item("CodigoItem").Value).Trim()),
                                                    new XElement("Correlativo", ((System.String)ors.Fields.Item("Correlativo").Value).Trim()),
                                                    new XElement("Descripcion", ((System.String)ors.Fields.Item("Descripcion").Value).Trim()),
                                                    new XElement("LineaReferencia", ((System.String)ors.Fields.Item("LineaReferencia").Value).Trim()),
                                                    new XElement("UnidadMedida", ((System.String)ors.Fields.Item("UnidadMedida").Value).Trim())
                                                    //**)
                                                );
                            miXML.Root.Add(xNodo);
                        //**}
                        //**else
                        //**{
                        //**    xNodo = new XElement("BienesaTransportar",
                        //**                            new XElement("Cantidad", ((System.Double)ors.Fields.Item("Cantidad").Value)),
                        //**                            new XElement("CodigoItem", ((System.String)ors.Fields.Item("CodigoItem").Value).Trim()),
                        //**                            new XElement("Correlativo", ((System.String)ors.Fields.Item("Correlativo").Value).Trim()),
                        //**                            new XElement("Descripcion", ((System.String)ors.Fields.Item("Descripcion").Value).Trim()),
                        //**                            new XElement("LineaReferencia", ((System.String)ors.Fields.Item("LineaReferencia").Value).Trim()),
                        //**                            new XElement("UnidadMedida", ((System.String)ors.Fields.Item("UnidadMedida").Value).Trim())
                        //**                            );
                        //**    miXML.Descendants("Items").LastOrDefault().Add(xNodo);
                            //**}
                        ors.MoveNext();
                    }
                }

                return miXML.ToString();
            }
            catch (Exception x)
            {
                SBO_f.oLog.OutLog("Error GenerarXMLStringInvoice, Sector " + Sector + " -> " + x.Message + ", TRACE " + x.StackTrace);
                return "";
            }
        }
    }
}
