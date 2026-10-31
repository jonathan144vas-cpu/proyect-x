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
        // Paleta por tipo de comprobante: azul para entregas, verde para recepciones.
        private sealed record Tema(string Principal, string Claro, string Borde);

        private static readonly Tema TemaEntrega =
            new(Colors.Blue.Darken3, Colors.Blue.Lighten5, Colors.Blue.Lighten3);

        private static readonly Tema TemaRecepcion =
            new(Colors.Green.Darken3, Colors.Green.Lighten5, Colors.Green.Lighten2);

        public byte[] GenerarComprobanteRecepcion(Pedido pedido)
        {
            var tema = TemaRecepcion;
            var ahora = DateTime.Now;

            return Document.Create(documento =>
            {
                documento.Page(pagina =>
                {
                    ConfigurarPagina(pagina);

                    pagina.Header().Element(c => Encabezado(c, tema,
                        "COMPROBANTE DE RECEPCIÓN DE BODEGA",
                        "Sistema de Gestión de Inventario",
                        $"CR-{pedido.Id:D5}", ahora));

                    pagina.Content().PaddingVertical(20).Column(columna =>
                    {
                        columna.Spacing(12);

                        columna.Item().Element(c => CuadroDatos(c, tema, "DATOS DE LA RECEPCIÓN", datos =>
                        {
                            datos.Add(("Pedido No.", pedido.Id.ToString()));
                            datos.Add(("Fecha del pedido", $"{pedido.FechaSolicitud:dd/MM/yyyy}"));
                            datos.Add(("Solicitado por", pedido.SolicitadoPor ?? "N/A"));
                            datos.Add(("Proveedor", string.IsNullOrWhiteSpace(pedido.Proveedor) ? "N/A" : pedido.Proveedor));
                            if (pedido.FechaVencimiento is not null)
                            {
                                datos.Add(("Vencimiento del lote", $"{pedido.FechaVencimiento:dd/MM/yyyy}"));
                            }
                        }));

                        columna.Item().Column(seccion =>
                        {
                            seccion.Item().Text("PRODUCTOS RECIBIDOS").FontSize(11).Bold().FontColor(tema.Principal);
                            seccion.Item().PaddingTop(4).Table(tabla =>
                            {
                                tabla.ColumnsDefinition(columnas =>
                                {
                                    columnas.RelativeColumn(4);
                                    columnas.RelativeColumn(2);
                                    columnas.RelativeColumn(2);
                                    columnas.RelativeColumn(2);
                                });

                                tabla.Header(encabezado =>
                                {
                                    encabezado.Cell().Element(c => CeldaEncabezado(c, tema)).Text("PRODUCTO");
                                    encabezado.Cell().Element(c => CeldaEncabezado(c, tema)).AlignCenter().Text("CANTIDAD");
                                    encabezado.Cell().Element(c => CeldaEncabezado(c, tema)).AlignCenter().Text("UNIDAD");
                                    encabezado.Cell().Element(c => CeldaEncabezado(c, tema)).AlignCenter().Text("ESTADO");
                                });

                                tabla.Cell().Element(Celda).Text(pedido.Articulo).FontSize(9);
                                tabla.Cell().Element(Celda).AlignCenter().Text(pedido.Cantidad.ToString()).FontSize(9);
                                tabla.Cell().Element(Celda).AlignCenter().Text(pedido.UnidadMedida).FontSize(9);
                                tabla.Cell().Element(Celda).AlignCenter().Text(pedido.Estado.ToString()).FontSize(9);
                            });
                        });

                        if (!string.IsNullOrWhiteSpace(pedido.Observaciones))
                        {
                            columna.Item().Element(c => CuadroObservaciones(c, pedido.Observaciones));
                        }
                    });

                    pagina.Footer().Element(c => PieFirmas(c,
                        "Firma y Sello de quien recibe", "(Bodega / Responsable)",
                        "Firma y Sello de conformidad", "(Proveedor / Entrega)",
                        "Control de Víveres - Sistema de Gestión de Inventario"));
                });
            }).GeneratePdf();
        }

        public byte[] GenerarComprobanteEntrega(EntregaProgramada entrega)
        {
            var tema = TemaEntrega;
            var fechaEntrega = entrega.FechaCompletada ?? DateTime.Now;

            return Document.Create(documento =>
            {
                documento.Page(pagina =>
                {
                    ConfigurarPagina(pagina);

                    pagina.Header().Element(c => Encabezado(c, tema,
                        "COMPROBANTE DE ENTREGA COMUNITARIA",
                        "Sistema de Gestión de Entregas",
                        $"CE-{entrega.Id:D5}", fechaEntrega));

                    pagina.Content().PaddingVertical(20).Column(columna =>
                    {
                        columna.Spacing(12);

                        columna.Item().Element(c => CuadroDatos(c, tema, "DATOS DE LA ENTREGA", datos =>
                        {
                            datos.Add(("Lugar / Comunidad", entrega.Lugar));
                            datos.Add(("Municipio", entrega.Municipio));
                            datos.Add(("Departamento", entrega.Departamento));
                            datos.Add(("Fecha programada", $"{entrega.FechaProgramada:dd/MM/yyyy}"));
                            datos.Add(("Familias beneficiadas", entrega.FamiliasBeneficiadas.ToString()));
                            datos.Add(("Registrado por", entrega.RegistradoPor ?? "N/A"));
                        }));

                        columna.Item().Column(seccion =>
                        {
                            seccion.Item().Text("PRODUCTOS ENTREGADOS").FontSize(11).Bold().FontColor(tema.Principal);
                            seccion.Item().PaddingTop(4).Table(tabla =>
                            {
                                tabla.ColumnsDefinition(columnas =>
                                {
                                    columnas.RelativeColumn(5);
                                    columnas.RelativeColumn(2);
                                    columnas.RelativeColumn(3);
                                });

                                tabla.Header(encabezado =>
                                {
                                    encabezado.Cell().Element(c => CeldaEncabezado(c, tema)).Text("PRODUCTO");
                                    encabezado.Cell().Element(c => CeldaEncabezado(c, tema)).AlignCenter().Text("CANTIDAD");
                                    encabezado.Cell().Element(c => CeldaEncabezado(c, tema)).AlignCenter().Text("UNIDAD");
                                });

                                foreach (var detalle in entrega.Detalles)
                                {
                                    tabla.Cell().Element(Celda).Text(detalle.Producto).FontSize(9);
                                    tabla.Cell().Element(Celda).AlignCenter().Text(detalle.Total.ToString("0.##")).FontSize(9);
                                    tabla.Cell().Element(Celda).AlignCenter().Text(detalle.SistemaMedida).FontSize(9);
                                }
                            });

                            seccion.Item().PaddingTop(4).AlignRight()
                                .Text($"Total de productos distintos: {entrega.Detalles.Count}")
                                .FontSize(8).FontColor(Colors.Grey.Darken1);
                        });

                        if (!string.IsNullOrWhiteSpace(entrega.Observaciones))
                        {
                            columna.Item().Element(c => CuadroObservaciones(c, entrega.Observaciones));
                        }
                    });

                    pagina.Footer().Element(c => PieFirmas(c,
                        "Firma y Sello de quien entrega", "(Bodega / Responsable)",
                        "Firma y Sello de conformidad", "(Comité Comunitario / Autoridad Local)",
                        "Control de Víveres - Sistema de Gestión de Entregas"));
                });
            }).GeneratePdf();
        }

        private static void ConfigurarPagina(PageDescriptor pagina)
        {
            pagina.Size(PageSizes.A4);
            pagina.Margin(40);
            pagina.DefaultTextStyle(estilo => estilo.FontSize(10).FontFamily(Fonts.Lato));
        }

        private static void Encabezado(IContainer contenedor, Tema tema, string titulo, string subtitulo,
            string folio, DateTime fecha)
        {
            contenedor.Row(fila =>
            {
                fila.ConstantItem(70).Height(70)
                    .Border(2).BorderColor(tema.Principal).Background(tema.Claro)
                    .AlignCenter().AlignMiddle()
                    .Width(36).Height(36)
                    .Svg(IconoCanasta(tema.Principal));

                fila.RelativeItem().PaddingLeft(15).Column(c =>
                {
                    c.Item().Text("CONTROL DE VÍVERES").FontSize(18).Bold().FontColor(tema.Principal);
                    c.Item().Text(titulo).FontSize(12).SemiBold().FontColor(Colors.Grey.Darken2);
                    c.Item().PaddingTop(2).Text(subtitulo).FontSize(9).FontColor(Colors.Grey.Medium);
                });

                fila.ConstantItem(150).Column(c =>
                {
                    c.Item().AlignRight().Text($"FOLIO: {folio}").FontSize(11).Bold();
                    c.Item().AlignRight().Text($"FECHA: {fecha:dd/MM/yyyy}").FontSize(9);
                    c.Item().AlignRight().Text($"HORA: {fecha:HH:mm}").FontSize(9);
                });
            });
        }

        // Ícono "basket2-fill" de Bootstrap Icons, el mismo de la pantalla de inicio de sesión.
        private static string IconoCanasta(string color) =>
            $"""
            <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="{color}" viewBox="0 0 16 16">
              <path d="M5.929 1.757a.5.5 0 1 0-.858-.514L2.217 6H.5a.5.5 0 0 0-.5.5v1a.5.5 0 0 0 .5.5h.623l1.844 6.456A.75.75 0 0 0 3.69 15h8.622a.75.75 0 0 0 .722-.544L14.877 8h.623a.5.5 0 0 0 .5-.5v-1a.5.5 0 0 0-.5-.5h-1.717L10.93 1.243a.5.5 0 1 0-.858.514L12.617 6H3.383zM4 10a1 1 0 0 1 2 0v2a1 1 0 1 1-2 0zm3 0a1 1 0 0 1 2 0v2a1 1 0 1 1-2 0zm4-1a1 1 0 0 1 1 1v2a1 1 0 1 1-2 0v-2a1 1 0 0 1 1-1"/>
            </svg>
            """;

        // Cuadro de datos en dos columnas; los pares se acomodan de izquierda a derecha.
        private static void CuadroDatos(IContainer contenedor, Tema tema, string titulo,
            Action<List<(string Etiqueta, string Valor)>> llenar)
        {
            var datos = new List<(string Etiqueta, string Valor)>();
            llenar(datos);

            contenedor.Background(tema.Claro).Border(1).BorderColor(tema.Borde).Padding(12).Column(columna =>
            {
                columna.Spacing(4);
                columna.Item().Text(titulo).FontSize(11).Bold().FontColor(tema.Principal);

                foreach (var par in datos.Chunk(2))
                {
                    columna.Item().Row(fila =>
                    {
                        foreach (var (etiqueta, valor) in par)
                        {
                            fila.RelativeItem().Text(texto =>
                            {
                                texto.Span($"{etiqueta}: ").FontSize(9).FontColor(Colors.Grey.Darken2);
                                texto.Span(valor).FontSize(9).SemiBold();
                            });
                        }

                        if (par.Length == 1)
                        {
                            fila.RelativeItem();
                        }
                    });
                }
            });
        }

        private static void CuadroObservaciones(IContainer contenedor, string observaciones)
        {
            contenedor.Background(Colors.Grey.Lighten4).Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10)
                .Column(c =>
                {
                    c.Item().Text("OBSERVACIONES").FontSize(10).Bold().FontColor(Colors.Grey.Darken2);
                    c.Item().Text(observaciones).FontSize(9);
                });
        }

        private static void PieFirmas(IContainer contenedor, string firma1, string detalle1,
            string firma2, string detalle2, string leyenda)
        {
            contenedor.Column(columna =>
            {
                columna.Item().PaddingTop(40).Row(fila =>
                {
                    fila.RelativeItem().Column(c => Firma(c, firma1, detalle1));
                    fila.ConstantItem(40);
                    fila.RelativeItem().Column(c => Firma(c, firma2, detalle2));
                });

                columna.Item().PaddingTop(15).AlignCenter().Text(leyenda)
                    .FontSize(7).FontColor(Colors.Grey.Medium);
            });
        }

        private static void Firma(ColumnDescriptor columna, string firma, string detalle)
        {
            columna.Item().PaddingTop(15).LineHorizontal(1).LineColor(Colors.Grey.Darken1);
            columna.Item().AlignCenter().Text(firma).FontSize(9);
            columna.Item().AlignCenter().Text(detalle).FontSize(8).FontColor(Colors.Grey.Medium);
        }

        private static IContainer CeldaEncabezado(IContainer contenedor, Tema tema) =>
            contenedor.Background(tema.Principal).Padding(6)
                .DefaultTextStyle(x => x.SemiBold().FontColor(Colors.White).FontSize(9));

        private static IContainer Celda(IContainer contenedor) =>
            contenedor.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5);
    }
}
