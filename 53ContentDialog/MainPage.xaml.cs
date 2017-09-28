using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Printing;
using Windows.Graphics.Printing.OptionDetails;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Printing;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace _53ContentDialog
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
        private async void PrintDetailedOptions_OptionChanged(PrintTaskOptionDetails sender, PrintTaskOptionChangedEventArgs args)
        {
            System.Diagnostics.Debug.WriteLine(args.OptionId);
            CoreApplicationView newView = CoreApplication.CreateNewView();
            int newViewId = 0;
            await newView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                Frame frame = new Frame();
                frame.Navigate(typeof(SecondaryPage), null);
                Window.Current.Content = frame;
                // You have to activate the window in order to show it later.
                Window.Current.Activate();

                newViewId = ApplicationView.GetForCurrentView().Id;
            });
            //bool viewShown = await ApplicationViewSwitcher.TryShowAsStandaloneAsync(newViewId);

            //await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
            //    async  () =>
            //      {
            //          //ContentDialogPage dialog = new ContentDialogPage();                
            //          //await dialog.ShowAsync();  
                    
            //      }); 
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
            printDoc.AddPage(this);
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

        private async void btn_Click(object sender, RoutedEventArgs e)
        {
            ContentDialogPage dialog = new ContentDialogPage();
            await dialog.ShowAsync();
        }

        private async void btn2_Click(object sender, RoutedEventArgs e)
        {
            SecondDialog dialog = new   SecondDialog();
            await dialog.ShowAsync();
        }
    }
}
