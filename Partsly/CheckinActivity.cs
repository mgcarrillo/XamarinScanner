using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Text;
using Android.Views.InputMethods;
using Android.Widget;
using ZXing.Mobile;
using String = System.String;

namespace Partsly
{
    [Activity(Label = "Check In Parts")]
    public class CheckinActivity : Activity
    {
        static Dictionary<string, string> partsMapping = new Dictionary<string, string>()
        {
            { "12630223", "01079-PCKT" },
            { "12637105", "01069-CT" },
            { "12657499", "34900B" }, //"01069-BPCKT" },  // mapped to a real part in DMS
            { "12660159", "01079-BPCKT" },
        };

        Button buttonQuickScan;
        Button buttonQtyScan;
        MobileBarcodeScanner scanner;

        private String m_qtyString = "1";  // use string for input // TODO

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);


            // Create your application here
            this.SetContentView(Resource.Layout.Checkin);

            this.FindViewById<Button>(Resource.Id.buttonBack).Click += this.Back;

            // Initialize scanner
            MobileBarcodeScanner.Initialize(Application);
            scanner = new ZXing.Mobile.MobileBarcodeScanner();

            // set up scanning options
            var scanOptions = new ZXing.Mobile.MobileBarcodeScanningOptions();
            scanOptions.PossibleFormats = new List<ZXing.BarcodeFormat>() {
                ZXing.BarcodeFormat.EAN_8,
                ZXing.BarcodeFormat.EAN_13,
                ZXing.BarcodeFormat.UPC_A,
                ZXing.BarcodeFormat.UPC_E,
                ZXing.BarcodeFormat.UPC_EAN_EXTENSION
            };

            // quick scan code
            buttonQuickScan = this.FindViewById<Button>(Resource.Id.buttonQuickScan);
            buttonQuickScan.Click += async delegate
            {
                scanner.UseCustomOverlay = false;
                scanner.TopText = "Scanning for barcode";
                var result = await scanner.Scan();
                await AddOneToInventory(result);
                DisplayConfirm(1);
            };

            // quantities scan code
            buttonQtyScan = this.FindViewById<Button>(Resource.Id.buttonQtyScan);
            buttonQtyScan.Click += async delegate
            {
                scanner.UseCustomOverlay = false;
                scanner.TopText = "Scanning for barcode";
                var result = await scanner.Scan();

                await GetQuantityDialog(result);
                //DisplayConfirm();
                //await AddQtyToInventory(result, Int32.Parse(m_qtyString));
            };
        }

        async Task AddOneToInventory(ZXing.Result result)
        {
            await AddQtyToInventory(result, 1);
        }

        async Task AddQtyToInventory(ZXing.Result result, int qty)
        {
            string message = "";
            var barcodenumber = result?.Text;
            if (result != null && !string.IsNullOrEmpty(barcodenumber))
                message = "Barcode: " + barcodenumber;
            else
                message = "Could not scan.";

            this.RunOnUiThread(() => Toast.MakeText(this, message, ToastLength.Short).Show());

            // make an async call to open track
            // "GM /12657499"
            // parse number and lookup in dictionary
            var partkey = barcodenumber.Substring(barcodenumber.IndexOf("/") + 1).Trim();
            var posturl = "http://partsapi.us-east-1.elasticbeanstalk.com/api/parts?partName=" + partsMapping[partkey].Trim() + "&quantity=" + qty;

            await CallOpenTrackAddPart(posturl);
        }

        private async Task CallOpenTrackAddPart(string url)
        {
            try
            {

                (new WebClient()).UploadString(url, string.Empty); // .NET 4.7

                //using (WebClient client = new WebClient())
                //{
                //    client.Headers[HttpRequestHeader.ContentType] = "application/json";
                //    client.UploadString(url, string.Empty);
                //}
            }
            catch  // TODO for now, swallow exception, cause I dunno what to do here
            { }
        }

        async Task /*void*/ GetQuantityDialog(ZXing.Result result)
        {
            using (var builder = new AlertDialog.Builder(this))
            {
                builder.SetTitle("Parts Quantity To Check In");

                // Set up the input
                EditText input = new EditText(this);
                // Specify the type of input expected; this, for example, sets the input as a password, and will mask the text
                input.SetRawInputType(InputTypes.ClassNumber);
                builder.SetView(input);

                input.Text = "1"; //GetSavedInput(input, out selectedInput);
                //SetEditTextStylings(input);
                input.InputType =  Android.Text.InputTypes.ClassNumber;
                builder.SetTitle("Enter a Part Quantity");
                builder.SetView(input);
                builder.SetPositiveButton(
                    "Ok",
                    async (see, ess) =>
                    {
                        if (input.Text != string.Empty && input.Text != "0")
                        {
                            int parsedInput = Int32.Parse(input.Text, NumberStyles.Integer, CultureInfo.InvariantCulture);
                            m_qtyString = parsedInput.ToString();
                            //SaveUserInputToClass(input.Text, input);
                            DisplayConfirm(parsedInput);
                            await AddQtyToInventory(result, Int32.Parse(m_qtyString));
                        }
                        else
                        {
                            //RemoveInputFromClass(input);
                            m_qtyString = string.Empty;
                        }
                        HideKeyboard(input);
                    });
                builder.SetNegativeButton("Cancel", (afk, kfa) => { HideKeyboard(input); });
                builder.Show();
                ShowKeyboard(input);

                //// confirm quantity entered
                //input.TextChanged += /*async*/ (object sender, Android.Text.TextChangedEventArgs e) => {

                //    DisplayConfirm();
                //    /*await*/ AddQtyToInventory(result, Int32.Parse(m_qtyString));
                //};
            }
        }

        private void DisplayConfirm(int qty)
        {
            this.RunOnUiThread(() => Toast.MakeText(this, "Added " + qty + " to inventory for this part.", ToastLength.Short).Show());
        }

        public void Back(object sender, EventArgs e)
        {
            this.StartActivity(typeof(MainActivity));
        }

        private void ShowKeyboard(EditText userInput)
        {
            userInput.RequestFocus();
            InputMethodManager imm = (InputMethodManager)this.GetSystemService(Context.InputMethodService);
            imm.ToggleSoftInput(ShowFlags.Forced, 0);
        }

        private void HideKeyboard(EditText userInput)
        {
            InputMethodManager imm = (InputMethodManager)this.GetSystemService(Context.InputMethodService);
            imm.HideSoftInputFromWindow(userInput.WindowToken, 0);
        }
    }
}