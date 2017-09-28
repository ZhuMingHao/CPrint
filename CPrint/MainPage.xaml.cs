using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace CPrint
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        PrintManager printmgr = PrintManager.GetForCurrentView();
        /// <summary>
        /// PrintDocument 将打印流发送到打印机的一个可重用对象
        /// </summary>
        PrintDocument printDoc = null;
        /// <summary>
        /// RotateTransform 是来旋转打印元素的，如果设备是横着的则需要旋转90°
        /// </summary>
        RotateTransform rottrf = null;
        /// <summary>
        /// 表示包括要打印的内容以及提供对描述将如何打印内容的信息的访问的打印操作
        /// </summary>
        PrintTask task = null;

        //ObservableCollection<Employee> Employeeslist = new ObservableCollection<Employee>
        //{          

        //    new Employee() {Id=10,Name="employee4"},
        //    new Employee() {Id=6,Name="G"},
        //    new Employee() {Id=9,Name="F"},
        //    new Employee() {Id=10,Name="E"},
        //    new Employee() {Id=7,Name="D"},
        //    new Employee() {Id=8,Name="C"},
        //    new Employee() {Id=9,Name="B"},
        //    new Employee() {Id=10,Name="A"}

        //};


        public MainPage()
        {
            this.InitializeComponent();
            //当发生打印的请求时，通过 ShowPrintUIAsync方法通过编程方式调用打印，或通过用户操作可能会触发此事件
            //ApplicationView.PreferredLaunchViewSize = new Size(612, 792);
            //ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;
            printmgr.PrintTaskRequested += Printmgr_PrintTaskRequested;

 

        }
        private void Printmgr_PrintTaskRequested(PrintManager sender, PrintTaskRequestedEventArgs args)
        {
        

            //从参数的Request属性中获取与PrintTaskRequest的任务关联
            //创建好打印内容和任务后 在调用Complete方法进行打印
            var deferral = args.Request.GetDeferral();
            // 创建打印任务
            task = args.Request.CreatePrintTask("Print", OnPrintTaskSourceRequrested);
            task.Completed += PrintTask_Completed;

            PrintTaskOptionDetails printDetailedOptions = PrintTaskOptionDetails.GetFromPrintTaskOptions(task.Options);
            IList<string> displayedOptions = printDetailedOptions.DisplayedOptions;
            //displayedOptions.Add(Windows.Graphics.Printing.StandardPrintTaskOptions.Copies);
            //displayedOptions.Add(Windows.Graphics.Printing.StandardPrintTaskOptions.Orientation);
            //displayedOptions.Add(Windows.Graphics.Printing.StandardPrintTaskOptions.ColorMode);

            // Create a new list option
            PrintCustomItemListOptionDetails pageFormat = printDetailedOptions.CreateItemListOption("PageContent", "Pictures");
            pageFormat.AddItem("PicturesText", "Pictures and text");
            pageFormat.AddItem("PicturesOnly", "Pictures only");
            pageFormat.AddItem("TextOnly", "Text only");

            // Add the custom option to the option list
            displayedOptions.Add("PageContent");

            printDetailedOptions.OptionChanged += printDetailedOptions_OptionChanged;


            deferral.Complete();
        }

        private async void printDetailedOptions_OptionChanged(PrintTaskOptionDetails sender, PrintTaskOptionChangedEventArgs args)
        {
            string optionId = args.OptionId as string;
            if (string.IsNullOrEmpty(optionId))
            {
                return;
            }
            //await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            //{
            //    printDoc.InvalidatePreview();
            //});

            if (optionId == "PageContent")
            {
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
               {
                   printDoc.InvalidatePreview();
               });
            }
        }

        private void PrintTask_Completed(PrintTask sender, PrintTaskCompletedEventArgs args)
        {
            //打印完成
        }
        private async void OnPrintTaskSourceRequrested(PrintTaskSourceRequestedArgs args)
        {       

            var def = args.GetDeferral();
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
              () =>
              {
                  // 设置打印源
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
            //订阅预览事件
            printDoc.GetPreviewPage += OnGetPreviewPage;
            //订阅预览时 打印参数发生变化事件 比如文档方向
            printDoc.Paginate += PrintDic_Paginate;
            //添加页面处理事件
            printDoc.AddPages += PrintDic_AddPages;
            // 显示打印对话框
            bool showPrint = await PrintManager.ShowPrintUIAsync();
        }
        //添加打印页面的内容
        private void PrintDic_AddPages(object sender, AddPagesEventArgs e)
        {
            //增加一个页要打印的元素
            printDoc.AddPage(this);
            //完成对打印页面的增加
            printDoc.AddPagesComplete();
        }
        DisplayContent imageText = DisplayContent.TextAndImages;
        private void PrintDic_Paginate(object sender, PaginateEventArgs e)
        {
          

            PrintTaskOptions opt = task.Options;
            // 根据页面的方向来调整打印内容的旋转方向
            //switch (opt.Orientation)
            //{
            //    case PrintOrientation.Default:
            //        rottrf.Angle = 0d;
            //        break;
            //    case PrintOrientation.Portrait:
            //        rottrf.Angle = 0d;
            //        break;
            //    case PrintOrientation.Landscape:
            //        rottrf.Angle = 90d;
            //        break;
            //}
            // 设置预览页面的总页数
            PrintTaskOptionDetails printDetailedOptions = PrintTaskOptionDetails.GetFromPrintTaskOptions(e.PrintTaskOptions);
            string pageContent = (printDetailedOptions.Options["PageContent"].Value as string).ToLowerInvariant();
          
        // Set the text & image display flag
           imageText = (DisplayContent)((Convert.ToInt32(pageContent.Contains("pictures")) << 1) | Convert.ToInt32(pageContent.Contains("text")));


            printDoc.SetPreviewPageCount(1, PreviewPageCountType.Final);


        }
        private void OnGetPreviewPage(object sender, GetPreviewPageEventArgs e)
        {
            // 设置要预览的页面
            // printDoc.SetPreviewPage(e.PageNumber, sp_PrintArea);
            switch(imageText)
            {
                case DisplayContent.Images:
                    printDoc.SetPreviewPage(e.PageNumber, imgtest);break;
                case DisplayContent.Text:
                    printDoc.SetPreviewPage(e.PageNumber, Txtblock); break;
                case DisplayContent.TextAndImages:
                    printDoc.SetPreviewPage(e.PageNumber, this);break;
                    
            }
           



        }

        private async void BtnListboxheight_Click(object sender, RoutedEventArgs e)
        {
            //await new Windows.UI.Popups.MessageDialog(ListEmployee.Height.ToString()).ShowAsync();
        }

        private void GetCurrentView_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            ApplicationView.GetForCurrentView().TryResizeView(new Size { Width = 210, Height = 297 });
            // Txtblock.Text = "Now width=210 height=297";
        }
    }

    public class Employee
    {
        public int Id { get; set; }
        public string Name { get; set; }

    }
    [Flags]
    internal enum DisplayContent : int
    {
        /// <summary>
        /// Show only text
        /// </summary>
        Text = 1,

        /// <summary>
        /// Show only images
        /// </summary>
        Images = 2,

        /// <summary>
        /// Show a combination of images and text
        /// </summary>
        TextAndImages = 3
    }
}

