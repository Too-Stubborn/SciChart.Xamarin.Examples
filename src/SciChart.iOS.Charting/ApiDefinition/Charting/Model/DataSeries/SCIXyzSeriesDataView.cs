﻿using System;
using CoreGraphics;
using Foundation;
using ObjCRuntime;
using UIKit;
using System.Runtime.InteropServices;

namespace SciChart.iOS.Charting
{
    // @interface SCIXyzSeriesDataView : SCIXySeriesDataView
    [BaseType(typeof(SCIXySeriesDataView))]
    interface SCIXyzSeriesDataView
    {
    }
}