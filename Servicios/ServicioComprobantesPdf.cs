using System;
using System.Linq;
using ControlViveresApp.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ControlViveresApp.Servicios
{
    /// <summary>
    /// Genera los comprobantes en PDF de recepción de pedidos (bodega) y de
    /// entregas completadas (a las comunidades).
    /// </summary>
    public class ServicioComprobantesPdf
    {
        public byte[] GenerarComprobanteRecepcion(Pedido pedido)
        {
            return Document.Create(documento =>
            {
                documento.Page(pagina =>
                {
                    pagina.Size(PageSizes.A5);
                    pagina.Margin(30);
                    pagina.DefaultTextStyle(estilo => estilo.FontSize(10));

                    pagina.Header().Column(columna =>
                    {
                        columna.Item().Text("Control de Víveres").FontSize(18).Bold();
                        columna.Item().Text("Comprobante de Recepción de Bodega").FontSize(12).SemiBold();
                        columna.Item().PaddingTop(4).LineHorizontal(1);
                    });

                    pagina.Content().PaddingVertical(10).Column(columna =>
                    {
                        columna.Spacing(6);

                        columna.Item().Text($"Fecha y hora de recepción: {DateTime.Now:dd/MM/yyyy HH:mm}");
                        columna.Item().Text($"Fecha emitida (registro del pedido): {pedido.FechaSolicitud:dd/MM/yyyy HH:mm}");
                        columna.Item().Text($"Pedido No.: {pedido.Id}");
                        columna.Item().Text($"Solicitado por: {pedido.SolicitadoPor ?? "N/A"}");
                        if (!string.IsNullOrWhiteSpace(pedido.Proveedor))
                        {
                            columna.Item().Text($"Proveedor: {pedido.Proveedor}");
                        }

                        columna.Item().PaddingTop(10).Table(tabla =>
                        {
                            tabla.ColumnsDefinition(columnas =>
                            {
                                columnas.RelativeColumn(3);
                                columnas.RelativeColumn(2);
                                columnas.RelativeColumn(2);
                                columnas.RelativeColumn(2);
                            });

                            tabla.Header(encabezado =>
                            {
                                encabezado.Cell().Element(CeldaEncabezado).Text("Producto");
                                encabezado.Cell().Element(CeldaEncabezado).Text("Cantidad");
                                encabezado.Cell().Element(CeldaEncabezado).Text("Unidad");
                                encabezado.Cell().Element(CeldaEncabezado).Text("Estado");
                            });

                            tabla.Cell().Element(Celda).Text(pedido.Articulo);
                            tabla.Cell().Element(Celda).Text(pedido.Cantidad.ToString());
                            tabla.Cell().Element(Celda).Text(pedido.UnidadMedida);
                            tabla.Cell().Element(Celda).Text(pedido.Estado.ToString());
                        });

                        if (pedido.FechaVencimiento is not null)
                        {
                            columna.Item().PaddingTop(6)
                                .Text($"Fecha de vencimiento del lote: {pedido.FechaVencimiento:dd/MM/yyyy}");
                        }

                        if (!string.IsNullOrWhiteSpace(pedido.Observaciones))
                        {
                            columna.Item().PaddingTop(6).Text($"Observaciones: {pedido.Observaciones}");
                        }
                    });

                    pagina.Footer().PaddingTop(30).Column(columna =>
                    {
                        columna.Item().AlignRight().Width(200).Column(firma =>
                        {
                            firma.Item().PaddingTop(20).LineHorizontal(1);
                            firma.Item().AlignCenter().Text("Firma y sello de conformidad de bodega");
                        });
                    });
                });
            }).GeneratePdf();
        }

        public byte[] GenerarComprobanteEntrega(EntregaProgramada entrega)
        {
            return Document.Create(documento =>
            {
                documento.Page(pagina =>
                {
                    pagina.Size(PageSizes.A5);
                    pagina.Margin(30);
                    pagina.DefaultTextStyle(estilo => estilo.FontSize(10));

                    pagina.Header().Column(columna =>
                    {
                        columna.Item().Text("Control de Víveres").FontSize(18).Bold();
                        columna.Item().Text("Comprobante de Entrega Comunitaria").FontSize(12).SemiBold();
                        columna.Item().PaddingTop(4).LineHorizontal(1);
                    });

                    pagina.Content().PaddingVertical(10).Column(columna =>
                    {
                        columna.Spacing(6);

                        columna.Item().Text($"Fecha de entrega: {(entrega.FechaCompletada ?? DateTime.Now):dd/MM/yyyy HH:mm}");
                        columna.Item().Text($"Fecha emitida (registro de la entrega programada): {entrega.FechaRegistro:dd/MM/yyyy HH:mm}");
                        columna.Item().Text($"Lugar: {entrega.Lugar}");
                        columna.Item().Text($"Municipio, Departamento: {entrega.Municipio}, {entrega.Departamento}");
                        columna.Item().Text($"Familias beneficiadas: {entrega.FamiliasBeneficiadas}");
                        columna.Item().Text($"Registrado por: {entrega.RegistradoPor ?? "N/A"}");

                        columna.Item().PaddingTop(10).Text("Lo que se entregó").SemiBold();

                        columna.Item().Table(tabla =>
                        {
                            tabla.ColumnsDefinition(columnas =>
                            {
                                columnas.RelativeColumn(3);
                                columnas.RelativeColumn(2);
                                columnas.RelativeColumn(2);
                            });

                            tabla.Header(encabezado =>
                            {
                                encabezado.Cell().Element(CeldaEncabezado).Text("Producto");
                                encabezado.Cell().Element(CeldaEncabezado).Text("Sistema de medida");
                                encabezado.Cell().Element(CeldaEncabezado).Text("Total");
                            });

                            foreach (var detalle in entrega.Detalles)
                            {
                                tabla.Cell().Element(Celda).Text(detalle.Producto);
                                tabla.Cell().Element(Celda).Text(detalle.SistemaMedida);
                                tabla.Cell().Element(Celda).Text(detalle.Total.ToString("0.##"));
                            }
                        });

                        if (!string.IsNullOrWhiteSpace(entrega.Observaciones))
                        {
                            columna.Item().PaddingTop(6).Text($"Observaciones: {entrega.Observaciones}");
                        }
                    });

                    pagina.Footer().PaddingTop(30).Column(columna =>
                    {
                        columna.Item().AlignRight().Width(200).Column(firma =>
                        {
                            firma.Item().PaddingTop(20).LineHorizontal(1);
                            firma.Item().AlignCenter().Text("Firma de conformidad del lugar de destino");
                        });
                    });
                });
            }).GeneratePdf();
        }

        private static IContainer CeldaEncabezado(IContainer contenedor) =>
            contenedor.Background(Colors.Grey.Lighten2).Padding(4).DefaultTextStyle(x => x.SemiBold());

        private static IContainer Celda(IContainer contenedor) =>
            contenedor.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4);
    }
}
