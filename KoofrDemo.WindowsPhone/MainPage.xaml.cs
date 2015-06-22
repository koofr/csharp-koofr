using Koofr.Sdk.Api.V2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace KoofrDemo
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        KoofrClient client;

        public MainPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            client = new KoofrClient("https://app.koofr.net", new HttpClient());

            var authOk = await client.Authenticate("email", "password");

            if (!authOk)
            {
                throw new Exception("Invalid email or password");
            }

            var mounts = await client.GetMounts();
            var mount = mounts.Find(m => m.IsPrimary);

            var uploadBytes = Encoding.UTF8.GetBytes("test");
            await client.FilesUpload(mount.Id, "/", "example.txt", uploadBytes);

            var downloadBytes = await client.FilesDownloadBytes(mount.Id, "/example.txt", null);

            var files = await client.ListFiles(mount.Id, "/");

            var fileNames = files.Select(f => f.Name);

            listView.DataContext = fileNames;
        }
    }
}
