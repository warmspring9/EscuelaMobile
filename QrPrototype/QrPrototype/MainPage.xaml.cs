using Rg.Plugins.Popup.Extensions;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using ZXing.Net.Mobile.Forms;
using Newtonsoft.Json.Linq;
using System.IO;
using Newtonsoft.Json;

namespace QrPrototype
{
    public class Person
    {

        public string name;
        public string last_name;
        public string photo;
        public string id;
        public string phone_num;
        public Boolean is_driver;

        public Person(string name, string last_name, string id, string phone_num, string photo)
        {
            this.name = name;
            this.last_name = last_name;
            this.id = id;
            this.phone_num = phone_num;
            this.photo = photo;
        }
    };

    public class AuthRequest
    {
        public string idGuardian;
        public string idStudent;
    }

    public partial class MainPage : ContentPage
    {
        static HttpClient client = new HttpClient();
        // API conection string
        static string path = "http://192.168.100.5/api/mobilguardian";

        private IDictionary<string, Person> guardians = new Dictionary<string, Person>();
        private IDictionary<string, string> permits = new Dictionary<string, string>();
        private string current_id = null;
        private string current_phone = "";

        public MainPage()
        {
            InitializeComponent();
        }

        private void Scan_for_qr(object sender, EventArgs e)
        {
            var scan = new ZXingScannerPage();
            Navigation.PushAsync(scan);

            scan.OnScanResult += (result) =>
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await Navigation.PopAsync();

                    //api call
                    string parameter = $"?id={result.Text}";
                    
                    try
                    {
                        HttpResponseMessage response = await client.GetAsync(path+parameter);
                        if (response.IsSuccessStatusCode)
                        {
                            var json = JObject.Parse(await response.Content.ReadAsStringAsync());
                            current_id = (string)json["id"];
                            name.Text = "";
                            name.Text = json["name"] + " " + json["last_name"];
                            current_phone = (string)json["phone_num"];
                            id.Text = current_id;

                            string img64 = (string)json["image"];
                            img64 = img64.Substring(0, img64.Length - 5);
                            
                            var byteArray = Convert.FromBase64String(img64);
                            Stream stream = new MemoryStream(byteArray);
                            var imageSource = ImageSource.FromStream(() => stream);
                            image.Source = imageSource;

                            change_visibility(sender, e);
                        } else
                        {
                            await PopupNavigation.Instance.PushAsync(new NotFoundDialog(), true);
                        }
                    } 
                    catch (Exception excp)
                    {
                        // TODO have to make api error page
                        return;
                    }
                    
                });
            };
        }

        private void scan_for_student(object sender, EventArgs e)
        {
            var scan = new ZXingScannerPage();
            Navigation.PushAsync(scan);

            scan.OnScanResult += (result) =>
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await Navigation.PopAsync();
                    AuthRequest inputs = new AuthRequest();
                    inputs.idGuardian = current_id;
                    inputs.idStudent = result.Text;
                    var content = new StringContent(JsonConvert.SerializeObject(inputs), Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await client.PostAsync(path+"/authorization", content);
                    if (response.IsSuccessStatusCode)
                    {
                        string res = await response.Content.ReadAsStringAsync();
                        if (res.Equals("true")){
                            await PopupNavigation.Instance.PushAsync(new ApproveDialog(), true);
                        } else
                        {
                            await PopupNavigation.Instance.PushAsync(new DeniedDialog(), true);
                        }
                    } else
                    {
                        await PopupNavigation.Instance.PushAsync(new NotFoundDialog(), true);
                    }
                });
            };
        }

        private void change_visibility(Object sender, EventArgs e)
        {
            guardian_info.IsVisible = !guardian_info.IsVisible;
            scan_btn.IsVisible = !scan_btn.IsVisible;

            if (!guardian_info.IsVisible)
            {
                image.Source = "qr_placeholder.jpg";
            }
        }

        private void call_number(Object sender, EventArgs e)
        {
            if (current_id != null && guardians.ContainsKey(current_id))
            {
                Launcher.OpenAsync(new Uri("tel:" + current_phone));
            }
        }
    }
}
