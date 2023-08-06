using iText.Kernel.Pdf.Extgstate;
using iText.Kernel.Pdf;
using iText.Layout.Element;
using iText.Kernel.Events;
using iText.Kernel.Geom;
using iText.Kernel.Pdf.Canvas;
using iText.Layout;

namespace TAUpload.Models
{
    public class TransparentImage : IEventHandler
    {
        protected PdfExtGState gState;
        protected Image img;
        public TransparentImage(Image img)
        {
            this.img = img;
            gState = new PdfExtGState().SetFillOpacity(0.2f);
        }

        public void HandleEvent(Event @event)
        {
            PdfDocumentEvent docEvent = (PdfDocumentEvent)@event;
            PdfDocument pdf = docEvent.GetDocument();
            PdfPage page = docEvent.GetPage();
            Rectangle pageSize = page.GetPageSize();
            var pdfCanvas = new PdfCanvas(page.GetLastContentStream(), page.GetResources(), pdf);
            pdfCanvas.SaveState().SetExtGState(gState);
            var canvas = new Canvas(pdfCanvas, pageSize);
            canvas.Add(img.ScaleAbsolute(pageSize.GetWidth(), pageSize.GetHeight()));
            pdfCanvas.RestoreState();
            pdfCanvas.Release();
        }
    }
}