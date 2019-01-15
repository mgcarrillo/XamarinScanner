Scandit SDK Barcode Scanner
===========================================

Using the Scandit SDK Barcode Scanner component in your Xamarin app is as simple as:

* downloading and installing the component
* signing up for an API key at http://www.scandit.com (free for the enterprise trial)
* integrating the Scandit SDK component into your app by implementing the callbacks for successful scans and instantiating the ScanditSDKBarcodePicker as shown in the example code below.

Install the Scandit SDK component
---------------------------------

Add the Scandit Barcode Scanner to your project via the Component Manager.

Get an Scandit SDK App Key
--------------------------

[Sign up](https://ssl.scandit.com/account/customers/new?p=test) for a free enterprise trial license and copy your app key from within your Scandit web account.


iOS: Integrate the Scandit SDK into Your iOS App
---------------------------------------

If you don't want to type the whole namespace each time when using the scanner you should add a using directive at the top of the class you will use the scanner in:

```csharp
using ScanditBarcodeScanner.iOS;
```
Set the app key in your AppDelegate's FinishedLaunching method the following way:

```csharp
@implementation AppDelegate

public override bool FinishedLaunching (UIApplication app, NSDictionary options)
    // Provide the app key for your scandit license.
    License.SetAppKey(@"--- ENTER YOUR SCANDIT APP KEY HERE ---");

    // ...
}
```
Implement the ScanDelegate protocol to handle the successful barcode decoding. The API reference of the ScanDelegate protocol provides more details about what is returned.

**Careful:** The ScanDelegate method is invoked on a picker-internal queue. To perform any UI work, you must dispatch to the main UI thread.

```csharp
    public class PickerScanDelegate : ScanDelegate {

        UIViewController presentingViewController;

        public PickerScanDelegate(UIViewController controller) {
            presentingViewController = controller;
        }

        public override void DidScan (BarcodePicker picker, IScanSession session) {
            NSArray recognized = session.NewlyRecognizedCodes;
            Barcode code = recognized.GetItem<Barcode> (0);

            // Add your own code to handle the barcode result e.g.
            Console.WriteLine("scanned {0} barcode: {1}", code.Symbology, code.Data);
        }
    }
```

And then set the scan delegate on the picker.

**Careful:** Since a change around iOS 9 the delegates are only softly referenced and can be dereferenced and deallocated if the memory is low. To avoid this you should keep a reference to them until you no longer use the picker (for example by creating a class variable for the delegates in the presenting view controller).

```csharp
scanDelegate = new PickerScanDelegate(this);
picker.ScanDelegate = scanDelegate;
```

The scanning process is started by instantiating the  BarcodePicker, specifying the delegate that will receive the scan callback event and then starting the scanner.

```csharp
    // Configure the barcode picker through a scan settings instance by defining which
    // symbologies should be enabled.
    ScanSettings settings = ScanSettings.DefaultSettings ();

    // prefer backward facing camera over front-facing cameras.
    settings.CameraFacingDirection = CameraFacingDirection.Back;

    // Enable symbologies that you want to scan.
    settings.SetSymbologyEnabled (Symbology.EAN13, true);
    settings.SetSymbologyEnabled (Symbology.UPC12, true);
    settings.SetSymbologyEnabled (Symbology.QR, true);

    // Setup the barcode scanner.
    BarcodePicker picker = new BarcodePicker (settings);

    // Set the delegate to receive scan events.
    scanDelegate = new PickerScanDelegate(this);
    picker.ScanDelegate = scanDelegate;

   // Start the scanning process.
    picker.StartScanning ();

    // Show the scanner. The easiest way to do so is by presenting it modally.
    PresentViewController (picker, true, null);
```


Android: Integrate the Scandit SDK into Your Android App
---------------------------------------

If not already done, grant your application the right to access the camera. You will need to open the project settings and go to Build->AndroidApplication where you can find a list of "Required permissions". Look for "Camera" and select it if it is not yet selected.

If you don't want to type the whole namespace each time when using the scanner you should add a using directive at the top of the class you will use the scanner in:

```csharp
using ScanditBarcodeScanner.Android;
using ScanditBarcodeScanner.Android.Recognition;
```
To receive events from the Scandit Barcode Scanner SDK you have to implement the OnScanListener interface.

If your class is an activity called DemoActivity, it looks something like this:

```csharp
public class DemoActivity : Activity, IOnScanListener {
}
```

Add the following callback methods to the class (as defined by the interface):

```csharp
    public void DidScan(IScanSession session) {
        // This callback is called whenever a barcode is decoded.
    }
```

The scanning process is managed by the BarcodePicker. The barcode scanning is configured through an instance of ScanSettings that you pass to the BarcodePicker constructor.

```csharp
// Set your app key
ScanditLicense.AppKey("--- ENTER YOUR SCANDIT APP KEY HERE ---");

ScanSettings settings = ScanSettings.Create();
settings.SetSymbologyEnabled(Barcode.SymbologyEan13, true);
settings.SetSymbologyEnabled(Barcode.SymbologyUpca, true);

// Instantiate the barcode picker by using the settings defined above.
BarcodePicker picker = new BarcodePicker(this, settings);

// Set the on scan listener to receive barcode scan events.
picker.SetOnScanListener(this);
```

Show the scanner to the user. The easiest way to do so is by setting it as the content view of your activity:

```csharp
SetContentView(mPicker);
```

Starting and stopping should normally happen in the OnPause() and OnResume() methods of the same activity or fragment that contains the picker.

```csharp
private BarcodePicker picker;

protected override void onResume() {
    picker.StartScanning();
    base.OnResume();
}

protected override void OnPause() {
    picker.StopScanning();
    base.OnPause();
}
```

More information
----------------

Try our demo apps for Android and iOS: 

http://www.scandit.com/resources/demo-apps/

Access our documentation and getting started guides: 

http://docs.scandit.com


Support
-------

Questions? Contact `support@scandit.com`.
