Camera Explorer
===============

Camera Explorer application demonstrates the use of the new advanced
Windows Phone 8 camera API, the Windows.Phone.Media.Capture.PhotoCaptureDevice
and the related classes and enumerations on Lumia devices. The updated 
application adds tap-to-focus and Lens Picker integration.

The example has been developed with Silverlight for Windows Phone devices
and tested to work on Nokia Lumia devices with Windows Phone 8.

This example application is hosted in GitHub:
https://github.com/Microsoft/camera-explorer

For more information on implementation and porting, visit Lumia
Developer's Library:
http://developer.nokia.com/Resources/Library/Lumia/#!imaging/advanced-photo-capturing/camera-explorer.html

This project is compatible with Windows Phone 8. Tested to work on Nokia Lumia
520, Nokia Lumia 820, Nokia Lumia 920 and Nokia Lumia 1020. Developed with
Microsoft Visual Studio Express for Windows Phone 2012.


What's new
----------

* Version 1.3.1.0: Minor bug fixes.
* Version 1.3.0.0: Support for devices without front camera, flash setting
  fixed, half-pressing camera key now reactivates auto-focus after tap-to-focus,
  settings (for each sensor) are now persistent.


1. Instructions
-------------------------------------------------------------------------------

This is a simple build-and-run solution. Learn about Windows Phone 8
camera features by trying out the application. 

**Building and deploying to phone**

In Windows Phone 8 SDK:

1. Open the SLN file: File > Open Project, select the file `CameraExplorer.sln`.
2. Select the target 'Device'.
3. Press F5 to build the project and run it on the device.

Please see official documentation for deploying and testing applications on
Windows Phone devices:
http://msdn.microsoft.com/en-us/library/gg588378%28v=vs.92%29.aspx


2. Implementation
-------------------------------------------------------------------------------

**Folders:**

* The root folder contains the project file, the license information and this
  file (release_notes.txt).
* `CameraExplorer`: Root folder for the implementation files.
 * `Assets`: Graphic assets like icons and tiles.
 * `Properties`: Application property files.
 * `Resources`: Application resources.


**Important files and classes:**

| File | Description |
| ---- | ----------- |
| MainPage.xaml(.cs) | The main page with viewfinder and overlays. |
| SettingsPage.xaml(.cs) | The page that is used to modify camera parameters. |
| Parameter.cs | Implementations for binding. |
| RangeParameter.cs | PhotoCameraDevice API properties to XAML. |
| ArrayParameter.cs | controls in the SettingsPage UI. |


**Required Capabilities:**


* `ID_CAP_ISV_CAMERA`
* `ID_CAP_MEDIALIB_PHOTO`


3. License
-------------------------------------------------------------------------------

See the license text file delivered with this project. The license file is also
available online at 
https://github.com/Microsoft/camera-explorer/blob/master/Licence.txt


4. Related documentation
-------------------------------------------------------------------------------

An article "Advanced Photo Capturing" published on Lumia Developer's Library
(http://www.developer.nokia.com/Resources/Library/Lumia/#!advanced-photo-capturing.html) 
describes the usage of PhotoCaptureDevice properties in more detail.


5. Version history
-------------------------------------------------------------------------------

* Version 1.3.1: Minor bug fixes.
* Version 1.3: Support for devices without front camera, flash setting fixed,
  half-pressing camera key now reactivates auto-focus after tap-to-focus,
  settings (for each sensor) are now persistent.
* Version 1.2: Bug fix to tap-to-focus (Ticket #5).
* Version 1.1: Tap-to-focus and Lens Picker integration added.
* Version 1.0: The first release.
