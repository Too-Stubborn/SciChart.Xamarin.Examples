using System;
using CoreGraphics;
using Foundation;
using ObjCRuntime;
using UIKit;
using System.Runtime.InteropServices;

namespace SciChart.iOS.Charting
{
    // @interface SCIOpenGLRenderSurface : SCIRenderSurfaceBase
    [BaseType(typeof(SCIRenderSurfaceBase))]
    interface SCIOpenGLRenderSurface
    {
        // @property (nonatomic) CGRect projectionFrame;
        [Export("projectionFrame", ArgumentSemantic.Assign)]
        CGRect ProjectionFrame { get; set; }

        // @property (nonatomic) UIColor * backgroundColor;
        [Export("backgroundColor", ArgumentSemantic.Assign)]
        UIColor BackgroundColor { get; set; }

        // -(void)free;
        [Export("free")]
        void Free();

        // @property (nonatomic) int drawingBufferSize;
        [Export("drawingBufferSize")]
        int DrawingBufferSize { get; set; }
    }
}