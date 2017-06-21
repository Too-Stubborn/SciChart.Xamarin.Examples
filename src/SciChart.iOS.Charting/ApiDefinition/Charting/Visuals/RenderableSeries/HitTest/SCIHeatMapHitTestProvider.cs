﻿using System;
using CoreGraphics;
using Foundation;
using ObjCRuntime;
using UIKit;
using System.Runtime.InteropServices;

namespace SciChart.iOS.Charting
{
    // @interface SCIUniformHeatmapHitTestProvider : NSObject <ISCIHitTestProviderProtocol>
    [BaseType(typeof(NSObject))]
    interface SCIUniformHeatmapHitTestProvider : SCIHitTestProviderProtocol
    {
    }
}