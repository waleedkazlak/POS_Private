using ClosedXML.Excel;
using POS_Private.Application.DTOs;

namespace POS_Private.Infrastructure.Services;

public interface IExcelExportService
{
    byte[] ExportInvoicesToExcel(IEnumerable<InvoiceDto> invoices);
    byte[] ExportInventoryToExcel(IEnumerable<BranchStockDto> stocks);
}

public class ExcelExportService : IExcelExportService
{
    public byte[] ExportInvoicesToExcel(IEnumerable<InvoiceDto> invoices)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Invoices");

        worksheet.Cell(1, 1).Value = "Invoice #";
        worksheet.Cell(1, 2).Value = "Date";
        worksheet.Cell(1, 3).Value = "Branch";
        worksheet.Cell(1, 4).Value = "Cashier";
        worksheet.Cell(1, 5).Value = "Customer";
        worksheet.Cell(1, 6).Value = "SubTotal (EGP)";
        worksheet.Cell(1, 7).Value = "Discount (EGP)";
        worksheet.Cell(1, 8).Value = "VAT 14% (EGP)";
        worksheet.Cell(1, 9).Value = "Total (EGP)";
        worksheet.Cell(1, 10).Value = "Status";

        var headerRow = worksheet.Row(1);
        headerRow.Style.Font.Bold = true;
        headerRow.Style.Fill.BackgroundColor = XLColor.FromHtml("#3B82F6");
        headerRow.Style.Font.FontColor = XLColor.White;

        int row = 2;
        foreach (var inv in invoices)
        {
            worksheet.Cell(row, 1).Value = inv.InvoiceNumber;
            worksheet.Cell(row, 2).Value = inv.InvoiceDate.ToString("yyyy-MM-dd HH:mm");
            worksheet.Cell(row, 3).Value = inv.BranchName;
            worksheet.Cell(row, 4).Value = inv.CashierName;
            worksheet.Cell(row, 5).Value = inv.CustomerName ?? "Walk-in";
            worksheet.Cell(row, 6).Value = inv.SubTotal;
            worksheet.Cell(row, 7).Value = inv.DiscountAmount;
            worksheet.Cell(row, 8).Value = inv.TaxAmount;
            worksheet.Cell(row, 9).Value = inv.TotalAmount;
            worksheet.Cell(row, 10).Value = inv.Status.ToString();
            row++;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public byte[] ExportInventoryToExcel(IEnumerable<BranchStockDto> stocks)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Inventory");

        worksheet.Cell(1, 1).Value = "Branch";
        worksheet.Cell(1, 2).Value = "Product Name";
        worksheet.Cell(1, 3).Value = "Variant";
        worksheet.Cell(1, 4).Value = "SKU";
        worksheet.Cell(1, 5).Value = "Barcode";
        worksheet.Cell(1, 6).Value = "Price (EGP)";
        worksheet.Cell(1, 7).Value = "Qty on Hand";
        worksheet.Cell(1, 8).Value = "Min Alert Level";
        worksheet.Cell(1, 9).Value = "Status";

        var headerRow = worksheet.Row(1);
        headerRow.Style.Font.Bold = true;
        headerRow.Style.Fill.BackgroundColor = XLColor.FromHtml("#10B981");
        headerRow.Style.Font.FontColor = XLColor.White;

        int row = 2;
        foreach (var s in stocks)
        {
            worksheet.Cell(row, 1).Value = s.BranchName;
            worksheet.Cell(row, 2).Value = s.ProductName;
            worksheet.Cell(row, 3).Value = s.VariantName ?? "-";
            worksheet.Cell(row, 4).Value = s.SKU ?? "-";
            worksheet.Cell(row, 5).Value = s.Barcode ?? "-";
            worksheet.Cell(row, 6).Value = s.Price;
            worksheet.Cell(row, 7).Value = s.QuantityOnHand;
            worksheet.Cell(row, 8).Value = s.MinAlertLevel;
            worksheet.Cell(row, 9).Value = s.IsLowStock ? "LOW STOCK" : "In Stock";
            row++;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
