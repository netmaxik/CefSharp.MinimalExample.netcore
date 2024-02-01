using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace CefSharp.MinimalExample.Wpf
{
    public partial class MainWindow : Window
    {
        public ICommand TestWork { get; set; }
        private bool Show_Coords = false;
        public MainWindow()
        {
            InitializeComponent();
            TestWork = new RelayCommand(TestBugCommand);
            DataContext = this; // Set the data context for binding
        }

        private void TestBugCommand()
        {
            Show_Coords = !Show_Coords;

        }

        /// <summary>
        /// Do script  - making EvaluateScriptAsync
        /// </summary>
        /// <param name="script"></param>
        /// <returns></returns>        
        private string Do_Script (string script)
        {
            int timeout = 10;
            string res = "";
            IFrame workframe = Browser.GetFocusedFrame();
            try
            {
                var task = workframe.EvaluateScriptAsync(script, "", 1, TimeSpan.FromSeconds(timeout));
                var complete = task.ContinueWith(t =>
                {
                    if (!t.IsFaulted)
                    {
                        if (t.Status != TaskStatus.RanToCompletion)
                        {
                            res= "not finished";
                        }
                        else
                        {
                            var response = t.Result;
                            res = Convert.ToString(response.Success ? (response.Result ?? "null") : response.Message);
                        }
                    }
                }, TaskScheduler.Default);

                complete.Wait();
            }
            catch (Exception)
            {

                res = "error";
            }
            return res;
        }

        /// <summary>
        /// Window_MouseMove - on mounse show coords
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (!Show_Coords)
                return;

            var X = e.GetPosition(Browser).X;
            var Y = e.GetPosition(Browser).Y;
            string scr = "(function() { return document.elementFromPoint(" + X.ToString() + "," + Y.ToString() + @").outerHTML })();";
            var res = Do_Script(scr);

            if (string.IsNullOrEmpty(res) || string.IsNullOrEmpty(res) || res == "error" || res == "not finished")
            {
                return;
            }
            

            Title = $"{X}:{Y} - {res}";
        }
    }

    public class RelayCommand : ICommand
    {
        private Action _execute;

        public RelayCommand(Action execute)
        {
            _execute = execute;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true; // Here you can put logic to enable or disable the command
        }

        public void Execute(object parameter)
        {
            _execute();
        }
    }
}
