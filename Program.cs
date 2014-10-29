//----------------------------------------------------------------------------
//  Copyright (C) 2004-2012 by EMGU. All rights reserved.       
//----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace IFGPro
{
   static class Program
   {
      /// <summary>
      /// The main entry point for the application.
      /// </summary>
      /// public Form
      [STAThread]
      static void Main()
      {
              Application.EnableVisualStyles();
              Application.SetCompatibleTextRenderingDefault(false);
              Application.Run(new MainWindow());
      }
   }
}