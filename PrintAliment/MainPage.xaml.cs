using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Printing;
using Windows.Graphics.Printing.OptionDetails;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Printing;
using Windows.UI.Xaml.Shapes;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace PrintAliment
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        PrintManager printmgr = PrintManager.GetForCurrentView();        
        PrintDocument printDoc = null;         
        PrintTask task = null; 
        public MainPage()
        {
            this.InitializeComponent(); 
            printmgr.PrintTaskRequested += Printmgr_PrintTaskRequested;
            MyWebView.LoadCompleted += MyWebView_LoadCompleted;
        }

        async void MyWebView_LoadCompleted(object sender, NavigationEventArgs e)
        {
            MyWebViewRectangle.Fill = await GetWebViewBrush(MyWebView);
            //  MyPrintPages.ItemsSource = await GetWebPages(MyWebView, new Windows.Foundation.Size(100d, 150d));

        }

        async Task<WebViewBrush> GetWebViewBrush(Windows.UI.Xaml.Controls.WebView webView)
        {
            // resize width to content
            var _OriginalWidth = webView.Width;
            var _WidthString = await webView.InvokeScriptAsync("eval",
                new[] { "document.body.scrollWidth.toString()" });
            int _ContentWidth;

            if (!int.TryParse(_WidthString, out _ContentWidth))
                throw new Exception(string.Format("failure/width:{0}", _WidthString));

            webView.Width = _ContentWidth;

            // resize height to content
            var _OriginalHeight = webView.Height;

            var _HeightString = await webView.InvokeScriptAsync("eval",
                new[] { "document.body.scrollHeight.toString()" });
            int _ContentHeight;

            if (!int.TryParse(_HeightString, out _ContentHeight))
                throw new Exception(string.Format("failure/height:{0}", _HeightString));

            webView.Height = _ContentHeight;

            // create brush
            var _OriginalVisibilty = webView.Visibility;

            webView.Visibility = Windows.UI.Xaml.Visibility.Visible;

            var _Brush = new WebViewBrush
            {
                Stretch = Stretch.Uniform,
                SourceName = webView.Name
            };


            _Brush.Redraw();


            // webView.Width = _OriginalWidth;
            //webView.Height = _OriginalHeight;
            webView.Visibility = _OriginalVisibilty;
            return _Brush;
        }


        private void Printmgr_PrintTaskRequested(PrintManager sender, PrintTaskRequestedEventArgs args)
        {  
            var deferral = args.Request.GetDeferral(); 
            task = args.Request.CreatePrintTask("Print", OnPrintTaskSourceRequrested);
            //task.Completed += PrintTask_Completed;
            PrintTaskOptionDetails printDetailedOptions = PrintTaskOptionDetails.GetFromPrintTaskOptions(task.Options);
            printDetailedOptions.OptionChanged += PrintDetailedOptions_OptionChanged;
            deferral.Complete();
        }
        private void PrintDetailedOptions_OptionChanged(PrintTaskOptionDetails sender, PrintTaskOptionChangedEventArgs args)
        {
            System.Diagnostics.Debug.WriteLine(args.OptionId);
        }
        //private void PrintTask_Completed(PrintTask sender, PrintTaskCompletedEventArgs args)
        //{
        //    //Completed
        //}
        private async void OnPrintTaskSourceRequrested(PrintTaskSourceRequestedArgs args)
        {
            var def = args.GetDeferral();
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
              () =>
              { 
                  args.SetSource(printDoc?.DocumentSource);

              });
            def.Complete();
        }
        private async void appbar_Printer_Click(object sender, RoutedEventArgs e)
        {
            if (printDoc != null)
            {
                printDoc.GetPreviewPage -= OnGetPreviewPage;
                printDoc.Paginate -= PrintDic_Paginate;
                printDoc.AddPages -= PrintDic_AddPages;
            }
            this.printDoc = new PrintDocument();          
            printDoc.GetPreviewPage += OnGetPreviewPage;        
            printDoc.Paginate += PrintDic_Paginate;   
            printDoc.AddPages += PrintDic_AddPages;        
            bool showPrint = await PrintManager.ShowPrintUIAsync();
        }     
        private void PrintDic_AddPages(object sender, AddPagesEventArgs e)
        {
            Rectangle page = (Rectangle)this.FindName("MyWebViewRectangle");
            printDoc.AddPage(page);
            printDoc.AddPagesComplete();
        }       
        private void PrintDic_Paginate(object sender, PaginateEventArgs e)
        {
            PrintTaskOptions opt = task.Options;
            PrintTaskOptionDetails printDetailedOptions = PrintTaskOptionDetails.GetFromPrintTaskOptions(e.PrintTaskOptions);
            
            printDoc.SetPreviewPageCount(1, PreviewPageCountType.Final);
        }
 

        private void OnGetPreviewPage(object sender, GetPreviewPageEventArgs e)
        {
              printDoc.SetPreviewPage(e.PageNumber, PrintArea); 
        }    
    }
 
}
