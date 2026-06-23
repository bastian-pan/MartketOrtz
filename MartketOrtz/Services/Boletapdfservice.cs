using MartketOrtz.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace MartketOrtz.Services
{

    public class BoletaPdfService
    {
        private readonly IWebHostEnvironment _env;

        public BoletaPdfService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public byte[] GenerarBoletaPdf(int idVenta, DateTime fecha, decimal total, decimal iva, List<DetalleVenta> detalles)
        {
            decimal neto = total - iva;

            // logo
            string logoPath = Path.Combine(_env.WebRootPath, "images", "logo.png");
            byte[]? logoBytes = File.Exists(logoPath) ? File.ReadAllBytes(logoPath) : null;

            var documento = Document.Create(container =>
            {
                container.Page(page =>
                {
                    // medidas de la boleta
                    page.Size(226, 1200, Unit.Point);
                    page.Margin(15);
                    page.DefaultTextStyle(x => x.FontFamily("Courier New").FontSize(9));

                    page.Content().Column(column =>
                    {
                        column.Spacing(4);

                        if (logoBytes != null)
                        {
                            column.Item().AlignCenter().Width(50).Image(logoBytes);
                        }

                        column.Item().AlignCenter().Text("MiniMarket Ortz")
                            .FontSize(14).Bold();

                        column.Item().AlignCenter().Text("Boleta Electrónica")
                            .FontSize(8).FontColor(Colors.Grey.Darken1);

                        column.Item().LineHorizontal(1).LineDashPattern([2f, 2f]).LineColor(Colors.Grey.Medium);

                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Text("N° Venta:");
                            row.RelativeItem().AlignRight().Text(idVenta.ToString()).Bold();
                        });

                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Text("Fecha:");
                            row.RelativeItem().AlignRight().Text(fecha.ToString("dd/MM/yyyy HH:mm"));
                        });

                        column.Item().LineHorizontal(1).LineDashPattern([2f, 2f]).LineColor(Colors.Grey.Medium);

                        // encabezado de la boleta
                        column.Item().Row(row =>
                        {
                            row.RelativeItem(3).Text("Producto").Bold();
                            row.RelativeItem(1).AlignRight().Text("Cant.").Bold();
                            row.RelativeItem(2).AlignRight().Text("Subtotal").Bold();
                        });

                        if (detalles != null && detalles.Count > 0)
                        {
                            foreach (var item in detalles)
                            {
                                column.Item().Row(row =>
                                {
                                    row.RelativeItem(3).Text(item.NombreProducto);
                                    row.RelativeItem(1).AlignRight().Text(item.Cantidad.ToString());
                                    row.RelativeItem(2).AlignRight().Text("$" + item.SubTotal.ToString("N0"));
                                });
                            }
                        }
                        else
                        {
                            column.Item().AlignCenter().Text("(Venta sin detalles registrados)")
                                .FontSize(8).Italic().FontColor(Colors.Grey.Darken1);
                        }

                        column.Item().LineHorizontal(1).LineDashPattern([2f, 2f]).LineColor(Colors.Grey.Medium);

                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Text("Neto:");
                            row.RelativeItem().AlignRight().Text("$" + neto.ToString("N0"));
                        });

                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Text("IVA (19%):");
                            row.RelativeItem().AlignRight().Text("$" + iva.ToString("N0"));
                        });

                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Text("TOTAL:").FontSize(11).Bold();
                            row.RelativeItem().AlignRight().Text("$" + total.ToString("N0")).FontSize(11).Bold();
                        });

                        column.Item().LineHorizontal(1).LineDashPattern([2f, 2f]).LineColor(Colors.Grey.Medium);

                        column.Item().AlignCenter().PaddingTop(5).Text("¡Gracias por su compra!")
                            .FontSize(8).FontColor(Colors.Grey.Darken1);
                    });
                });
            });

            return documento.GeneratePdf();
        }
    }
}