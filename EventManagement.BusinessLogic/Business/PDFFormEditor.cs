using System.Collections.Generic;
using System.IO;
using System.Linq;
using iText.Forms;
using iText.Forms.Fields;
using iText.Kernel.Pdf;

namespace EventManagement.BusinessLogic.Business
{
    public class PDFFormEditor
    {
        private string TemplateForm;
        
        public PDFFormEditor(string templateForm)
        {
            TemplateForm = templateForm;
        }

        public void GeneratePDFFromTemplate(string destFile, IDictionary<string, string> data)
        {
            GeneratePDF(data, new PdfWriter(destFile));
        }

        public Stream GeneratePDFFromTemplate(IDictionary<string, string> data)
        {
            var stream = new MemoryStream();
            GeneratePDF(data, new PdfWriter(stream));
            return stream;
        }

        private void GeneratePDF(IDictionary<string, string> mappingWithActuals, PdfWriter pdfWriter)
        {
            PdfDocument pdfDoc = new PdfDocument(new PdfReader(TemplateForm), pdfWriter);
            PdfAcroForm form = PdfAcroForm.GetAcroForm(pdfDoc, true);
            IDictionary<string, PdfFormField> fields = form.GetFormFields();
            PdfFormField toSet;
            foreach (var item in mappingWithActuals)
            {
                var success = fields.TryGetValue(item.Key, out toSet);
                if (success)
                {
                    {
                        toSet.SetValue(item.Value);
                    }
                }
            }
            pdfDoc.Close();
        }

        public string[] GetKeyTemplateKeys()
        {
            PdfDocument pdfDoc = new PdfDocument(new PdfReader(TemplateForm));
            PdfAcroForm form = PdfAcroForm.GetAcroForm(pdfDoc, true);
            IDictionary<string, PdfFormField> fields = form.GetFormFields();
            return fields.Keys.ToArray();
        }
    }
}
