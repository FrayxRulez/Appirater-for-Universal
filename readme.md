Introduction
============
Appirater is a class that you can drop into any Windows or Windows Phone (8.1 or later) app that will help remind your users to review your app on the Store.  The code is released under the MIT/X11, so feel free to
modify and share your changes with the world. 

The source is based on [Appirater for Monotouch].

How to
======

* Declare ad object of type `Appirater` in your `App.xaml.cs`.
* Initialize it in `App()` constructor after `Window.Current.Activate()` (`new Appirater("<your app name>", bool debug)`).
* Customize behaviors using `.Settings` property.
* Call `.AppLaunched(true)` at the next line.
* Call `.AppEnteredForeground(true)` in your `Window.Current.Activated` event handler.
* (Optional) Call `.UserDidSignificantEvent(true)` when the user does something 'significant' in the app.

Install
=======
You can install the library via [NuGet]:

Install-Package [Appirater]
------------------------------

[Appirater for monotouch]:https://github.com/chebum/Appirater-for-MonoTouch
[NuGet]:http://nuget.org/
[Appirater]:http://nuget.org/packages/Appirater
